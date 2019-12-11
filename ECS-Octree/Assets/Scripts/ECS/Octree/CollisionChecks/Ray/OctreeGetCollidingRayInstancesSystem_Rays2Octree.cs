using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ; 
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    
    
    public class GetCollidingRayInstancesBarrier_Rays2Octree : BarrierSystem {} ;


    /// <summary>
    /// Ray to octree system, checks one or more rays, against its paired target octree entity.
    /// </summary>
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]    
    class GetCollidingRayInstancesSystem_Rays2Octree : JobComponentSystem
    {
        
        [Inject] private GetCollidingRayInstancesBarrier_Rays2Octree barrier ;
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Ray Colliding Instances System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag),
                typeof (GetCollidingRayInstancesTag),
                typeof (OctreeEntityPair4CollisionData),
                typeof (RayData),
                typeof (RayMaxDistanceData),
                typeof (IsCollidingData),
                typeof (CollisionInstancesBufferElement)
                // typeof (RootNodeData) // Unused in ray
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;     
            ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData = GetComponentDataFromEntity <OctreeEntityPair4CollisionData> () ;
            ComponentDataFromEntity <RayData> a_rayData                                               = GetComponentDataFromEntity <RayData> () ;
            ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData                         = GetComponentDataFromEntity <RayMaxDistanceData> () ;

            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;


            ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;


            // Octree entity pair, for collision checks
                        
            ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData                               = GetComponentDataFromEntity <RootNodeData> () ;
                                
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                    = GetBufferFromEntity <NodeBufferElement> () ;         
            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement        = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;            
            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                    = GetBufferFromEntity <NodeChildrenBufferElement> () ;        
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                            = GetBufferFromEntity <InstanceBufferElement> () ;
            

            // Test ray 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData = new ComponentDataFromEntity<RayEntityPair4CollisionData> () ; // As empty.
            GetCollidingRayInstances_Common._DebugRays ( barrier.CreateCommandBuffer (), a_collisionChecksEntities, a_rayData, a_rayMaxDistanceData, a_isCollidingData, collisionInstancesBufferElement, a_rayEntityPair4CollisionData, false, false ) ;

            
            // Test ray 
            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;
            
            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;

            int i_groupLength = group.CalculateLength () ;

            var setRayTestJob = new SetRayTestJob 
            {
                
                a_collisionChecksEntities           = a_collisionChecksEntities,

                ray                                 = ray,
                a_rayData                           = a_rayData,
                // a_rayMaxDistanceData                = a_rayMaxDistanceData,

            }.Schedule ( i_groupLength, 8, inputDeps ) ;

            var job = new Job 
            {
                
                //ecb                                 = ecb,                
                a_collisionChecksEntities           = a_collisionChecksEntities,
                                
                a_octreeEntityPair4CollisionData    = a_octreeEntityPair4CollisionData,
                a_rayData                           = a_rayData,
                a_rayMaxDistanceData                = a_rayMaxDistanceData,
                a_isCollidingData                   = a_isCollidingData,
                collisionInstancesBufferElement     = collisionInstancesBufferElement,


                
                // Octree entity pair, for collision checks
                
                a_isActiveTag                       = a_isActiveTag,

                a_octreeRootNodeData                = a_octreeRootNodeData,

                nodeBufferElement                   = nodeBufferElement,
                nodeInstancesIndexBufferElement     = nodeInstancesIndexBufferElement,
                nodeChildrenBufferElement           = nodeChildrenBufferElement,
                instanceBufferElement               = instanceBufferElement

            }.Schedule ( i_groupLength, 8, setRayTestJob ) ;

            return job ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetRayTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Ray ray ;

            [ReadOnly] public EntityArray a_collisionChecksEntities ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <RayData> a_rayData ;           
            
            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                RayData rayData = new RayData () { ray = ray } ;                
                a_rayData [octreeRayEntity] = rayData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobParallelFor 
        {
            
            [ReadOnly] public EntityArray a_collisionChecksEntities ;


            
            [ReadOnly] public ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData ;  
            [ReadOnly] public ComponentDataFromEntity <RayData> a_rayData ;           
            [ReadOnly] public ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData ;
            
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;
            
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 


            // Octree entity pair, for collision checks

            // Check if octree is active
            [ReadOnly] public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] public ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData ;
                            
            [ReadOnly] public BufferFromEntity <NodeBufferElement> nodeBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;            
            [ReadOnly] public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;


            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRayEntity] ;
                // Stores reference to detected colliding instance.
                DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer          = collisionInstancesBufferElement [octreeRayEntity] ;    
                
                
                isCollidingData.i_nearestInstanceCollisionIndex = 0 ;
                isCollidingData.f_nearestDistance               = float.PositiveInfinity ;

                isCollidingData.i_collisionsCount               = 0 ; // Reset colliding instances counter.


                OctreeEntityPair4CollisionData octreeEntityPair4CollisionData                       = a_octreeEntityPair4CollisionData [octreeRayEntity] ;
                RayData rayData                                                                     = a_rayData [octreeRayEntity] ;
                RayMaxDistanceData rayMaxDistanceData                                               = a_rayMaxDistanceData [octreeRayEntity] ;
            

                // Octree entity pair, for collision checks
                    
                Entity octreeRootNodeEntity                                                         = octreeEntityPair4CollisionData.octree2CheckEntity ;

                // Is target octree active
                if ( a_isActiveTag.Exists (octreeRootNodeEntity) )
                {

                    RootNodeData octreeRootNodeData                                                           = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                    DynamicBuffer <NodeBufferElement> a_nodesBuffer                                     = nodeBufferElement [octreeRootNodeEntity] ;
                    DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer          = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                    DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                      = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                    DynamicBuffer <InstanceBufferElement> a_instanceBuffer                              = instanceBufferElement [octreeRootNodeEntity] ;   
                


                
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNodeData.i_totalInstancesCountInTree > 0 )
                    {
                    

                        if ( GetCollidingRayInstances_Common._GetNodeColliding ( octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, rayData.ray, ref a_collisionInstancesBuffer, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, rayMaxDistanceData.f ) )
                        {   
                            /*
                            // Debug
                            string s_collidingIDs = "" ;
                            int i_collisionsCount = isCollidingData.i_collisionsCount ;

                            for ( int i = 0; i < i_collisionsCount; i ++ )
                            {
                                CollisionInstancesBufferElement collisionInstancesBuffer = a_collisionInstancesBuffer [i] ;
                                s_collidingIDs += collisionInstancesBuffer.i_ID + ", " ;
                            }

                            Debug.Log ( "Is colliding with #" + i_collisionsCount + " instances of IDs: " + s_collidingIDs + "; Nearest collided instance is at " + isCollidingData.f_nearestDistance + "m, with index #" + isCollidingData.i_nearestInstanceCollisionIndex ) ;
                           */ 

                        }
                    }
                
                }

                a_isCollidingData [octreeRayEntity] = isCollidingData ; // Set back.
                    
            }
        }
        

    }

}