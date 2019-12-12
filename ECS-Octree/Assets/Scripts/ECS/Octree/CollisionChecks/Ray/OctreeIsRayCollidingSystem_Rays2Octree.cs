using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
        
    /// <summary>
    /// Ray to octree system, checks one or more rays, against its paired target octree entity.
    /// </summary>
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]    
    class IsRayCollidingSystem_Rays2Octree : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Ray Colliding Instances System" ) ;
            
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;

            group = GetEntityQuery 
            ( 
                typeof ( IsActiveTag ),
                typeof ( IsRayCollidingTag ),
                typeof ( OctreeEntityPair4CollisionData ),
                typeof ( RayData ),
                typeof ( RayMaxDistanceData ),
                typeof ( IsCollidingData )
                // typeof (CollisionInstancesBufferElement)
                // typeof (RootNodeData) // Unused in ray
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.Temp ) ;     

            ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData = GetComponentDataFromEntity <OctreeEntityPair4CollisionData> () ;
            ComponentDataFromEntity <RayData> a_rayData                                               = GetComponentDataFromEntity <RayData> () ;
            ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData                         = GetComponentDataFromEntity <RayMaxDistanceData> () ;

            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
                                    
            

            // Test ray  
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData = new ComponentDataFromEntity<RayEntityPair4CollisionData> () ; // As empty.
            IsRayColliding_Common._DebugRays ( ref na_collisionChecksEntities, ref a_rayData, ref a_rayMaxDistanceData, ref a_isCollidingData, ref a_rayEntityPair4CollisionData, false, false ) ;
            
            na_collisionChecksEntities.Dispose () ;
            
            // Test ray
            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;

            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;

            // int i_groupLength = group.CalculateLength () ;

            JobHandle setRayTestJobHandle = new SetRayTestJob 
            {
                
                // a_collisionChecksEntities           = na_collisionChecksEntities,

                ray                                 = ray,
                // a_rayData                           = a_rayData,
                // a_rayMaxDistanceData                = a_rayMaxDistanceData,

            }.Schedule ( group, inputDeps ) ;
            

            JobHandle jobHandle = new Job 
            {
                      
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                
                // Octree entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),

                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true )

            }.Schedule ( group, setRayTestJobHandle ) ;


            return jobHandle ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetRayTestJob : IJobForEach <RayData>
        // struct SetRayTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Ray ray ;

            // [ReadOnly] public EntityArray a_collisionChecksEntities ;

            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <RayData> a_rayData ;           
            
            public void Execute ( ref RayData rayData )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                rayData = new RayData () { ray = ray } ;                
                // a_rayData [octreeRayEntity] = rayData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobForEach <IsCollidingData, OctreeEntityPair4CollisionData, RayData, RayMaxDistanceData>  
        // struct Job : IJobParallelFor 
        {
            
            // [ReadOnly] public EntityArray a_collisionChecksEntities ;


            // Octree entity pair, for collision checks

            // Check if octree is active
            [ReadOnly] public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] public ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData ;
                            
            [ReadOnly] public BufferFromEntity <NodeBufferElement> nodeBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;            
            [ReadOnly] public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;


            public void Execute ( ref IsCollidingData isColliding, [ReadOnly] ref OctreeEntityPair4CollisionData octreeEntityPair4Collision, [ReadOnly] ref RayData rayData, [ReadOnly] ref RayMaxDistanceData rayMaxDistance )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRayEntity] ;   
                
                isColliding.i_collisionsCount = 0 ; // Reset colliding instances counter.
                // isCollidingData.i_nearestInstanceCollisionIndex  = 0 ; // Unused
                // isCollidingData.f_nearestDistance                = float.PositiveInfinity ; // Unused

                


                // OctreeEntityPair4CollisionData octreeEntityPair4CollisionData                       = a_octreeEntityPair4CollisionData [octreeRayEntity] ;
                // RayData rayData                                                                     = a_rayData [octreeRayEntity] ;
                // RayMaxDistanceData rayMaxDistanceData   = a_rayMaxDistanceData [octreeRayEntity] ;
            

                // Octree entity pair, for collision checks
                    
                Entity octreeRootNodeEntity  = octreeEntityPair4Collision.octree2CheckEntity ;

                // Is target octree active
                if ( a_isActiveTag.Exists (octreeRootNodeEntity) )
                {

                    RootNodeData octreeRootNodeData                                                 = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                    DynamicBuffer <NodeBufferElement> a_nodesBuffer                                 = nodeBufferElement [octreeRootNodeEntity] ;
                    DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer      = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                    DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                  = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                    DynamicBuffer <InstanceBufferElement> a_instanceBuffer                          = instanceBufferElement [octreeRootNodeEntity] ;   
                


                
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNodeData.i_totalInstancesCountInTree > 0 )
                    {
                    
                        if ( IsRayColliding_Common._IsNodeColliding ( ref octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, rayData.ray, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer, rayMaxDistance.f ) )                          
                        {   
                            /*
                            // Debug
                            Debug.Log ( "Is colliding." ) ;  
                            */                          
                        }
                    }
                
                }

                // a_isCollidingData [octreeRayEntity] = isCollidingData ; // Set back.
                    
            }

        }
        

    }

}