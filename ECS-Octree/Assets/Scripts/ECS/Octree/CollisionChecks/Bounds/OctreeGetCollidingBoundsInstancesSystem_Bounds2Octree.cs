using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
               
    /// <summary>
    /// Bounds to octree system, checks one or more bounds, against its paired target octree entity.
    /// </summary>
    // [UpdateAfter ( typeof ( OctreeForceCollisionCheckSystem ) ) ]   
    // [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]  
    class GetCollidingBoundsInstancesSystem_Bounds2Octree : JobComponentSystem
    {
            
        EndInitializationEntityCommandBufferSystem eiecb ;

        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;

            group = GetEntityQuery 
            ( 
                typeof ( IsActiveTag ),
                typeof ( GetCollidingBoundsInstancesTag ),
                typeof ( OctreeEntityPair4CollisionData ),
                typeof ( BoundsData ),
                typeof ( IsCollidingData ),
                typeof ( CollisionInstancesBufferElement )
                // typeof (RootNodeData) // Unused in ray
            ) ;
            
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.TempJob ) ;     

            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> ( true ) ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> ( true ) ;
                                

            // Test bounds 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;
            GetCollidingBoundsInstances_Common._DebugBounds ( ref ecb, ref na_collisionChecksEntities, ref a_isCollidingData, ref collisionInstancesBufferElement, false ) ;
            
            na_collisionChecksEntities.Dispose () ;

            eiecb.AddJobHandleForProducer ( inputDeps ) ;

            // Test bounds            
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 3 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;


            // int i_groupLength = group.CalculateLength () ;

            JobHandle setBoundsTestJobHandle = new SetBoundsTestJob 
            {
                
                // a_collisionChecksEntities           = na_collisionChecksEntities,

                checkBounds                         = checkBounds,
                // a_boundsData                        = a_boundsData,
                // a_rayMaxDistanceData             = a_rayMaxDistanceData,

            }.Schedule ( group, inputDeps ) ;

            JobHandle jobHandle = new Job 
            {
                
                //ecb                                 = ecb,                
                // na_collisionChecksEntities           = na_collisionChecksEntities,
                                
                // a_octreeEntityPair4CollisionData    = a_octreeEntityPair4CollisionData,
                // a_boundsData                        = a_boundsData,
                // a_isCollidingData                   = a_isCollidingData,
                // collisionInstancesBufferElement     = collisionInstancesBufferElement,

                
                // Octree entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),

                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true )

            }.Schedule ( group, setBoundsTestJobHandle ) ;

            return jobHandle ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobForEach <BoundsData>
        // struct SetBoundsTestJob : IJobParallelFor 
        {
            
            [ReadOnly] 
            public Bounds checkBounds ;

            // [ReadOnly] 
            // public EntityArray a_collisionChecksEntities ;

            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( ref BoundsData bounds )
            {
                bounds = new BoundsData () { bounds = checkBounds } ;

                // Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                // BoundsData boundsData = new BoundsData () { bounds = checkBounds } ;                
                // a_boundsData [octreeRayEntity] = boundsData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobForEach_BCCC <CollisionInstancesBufferElement, IsCollidingData, OctreeEntityPair4CollisionData, BoundsData> 
        {
            
            // [ReadOnly] public NativeArray <Entity> na_collisionChecksEntities ;

            // [ReadOnly] 
            // public ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData ;  

            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <BoundsData> a_boundsData ;  
            
            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;
            //[NativeDisableParallelForRestriction]
            // public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 


            // Octree entity pair, for collision checks

            // Check if octree is active
            [ReadOnly] 
            public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] 
            public ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData ;
                            
            [ReadOnly] 
            public BufferFromEntity <NodeBufferElement> nodeBufferElement ;            
            [ReadOnly] 
            public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;            
            [ReadOnly] 
            public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;            
            [ReadOnly] 
            public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;


            public void Execute ( DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer, ref IsCollidingData isColliding, [ReadOnly] ref OctreeEntityPair4CollisionData octreeEntityPair4Collision, [ReadOnly] ref BoundsData checkBounds )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeBoundsEntity = na_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeBoundsEntity] ;   

                // Stores reference to detected colliding instance.
                // DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer          = collisionInstancesBufferElement [octreeBoundsEntity] ;  
                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                isColliding.i_collisionsCount                   = 0 ; // Reset colliding instances counter.
                // isCollidingData.i_nearestInstanceCollisionIndex  = 0 ; // Unused
                // isCollidingData.f_nearestDistance                = float.PositiveInfinity ; // Unused

                // OctreeEntityPair4CollisionData octreeEntityPair4CollisionData                       = a_octreeEntityPair4CollisionData [octreeBoundsEntity] ;
                // BoundsData checkBounds                                                              = a_boundsData [octreeBoundsEntity] ;
            

                // Octree entity pair, for collision checks
                    
                Entity octreeRootNodeEntity                                                         = octreeEntityPair4Collision.octree2CheckEntity ;

                // Is target octree active
                if ( a_isActiveTag.Exists (octreeRootNodeEntity) )
                {

                    RootNodeData octreeRootNode                                                     = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                    DynamicBuffer <NodeBufferElement> a_nodesBuffer                                 = nodeBufferElement [octreeRootNodeEntity] ;
                    DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer      = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                    DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                  = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                    DynamicBuffer <InstanceBufferElement> a_instanceBuffer                          = instanceBufferElement [octreeRootNodeEntity] ;   
                
                    
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNode.i_totalInstancesCountInTree > 0 )
                    {
                    
                        if ( GetCollidingBoundsInstances_Common._GetNodeColliding ( ref octreeRootNode, octreeRootNode.i_rootNodeIndex, checkBounds.bounds, ref a_collisionInstancesBuffer, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer ) )
                        {   
                            /*
                            // Debug
                            Debug.Log ( "Is colliding." ) ;  
                            */                          
                        }

                    }
                
                }

                // a_isCollidingData [octreeBoundsEntity] = isCollidingData ; // Set back.
                    
            }

        }

    }

}
