using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine;


namespace Antypodish.ECS.Octree
{
       
    /// <summary>
    /// Bounds to octree system, checks one or more bounds, against its paired target octree entity.
    /// </summary>
    // [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]   
    class GetCollidingBoundsInstancesSystem_Octrees2Bounds : JobComponentSystem
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
                typeof ( BoundsEntityPair4CollisionData ),
                // typeof (BoundsData), // Not used by octree entity
                typeof ( IsCollidingData ),
                typeof ( CollisionInstancesBufferElement ),
                typeof ( RootNodeData ) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
                        
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.TempJob ) ;    
            
            ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData = GetComponentDataFromEntity <BoundsEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;
            
            
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
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;

            // int i_groupLength = group.CalculateLength () ;

            
            JobHandle setBoundsTestJobHandle = new SetBoundsTestJob 
            {
                
                checkBounds                          = checkBounds,

                // a_collisionChecksEntities           = na_collisionChecksEntities,
                // a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,
                
                // a_boundsData                        = a_boundsData,

            }.Schedule ( group, inputDeps ) ;
            

            JobHandle jobHandle = new Job 
            {
                
                //ecb                                 = ecb,                
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                                
                // a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,

                // a_isCollidingData                   = a_isCollidingData,
                // collisionInstancesBufferElement     = collisionInstancesBufferElement,
                
                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true ),

                
                // Ray entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),
                
                a_boundsData                        = GetComponentDataFromEntity <BoundsData> ( true ),


            }.Schedule ( group, setBoundsTestJobHandle ) ;
            
            return jobHandle ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobForEach <BoundsEntityPair4CollisionData>
        // struct SetBoundsTestJob : IJobParallelFor 
        {
            
            [ReadOnly] 
            public Bounds checkBounds ;

            // [ReadOnly] 
            // public EntityArray a_collisionChecksEntities ;

            // [ReadOnly] 
            // public ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( [ReadOnly] ref BoundsEntityPair4CollisionData boundsEntityPair4Collision )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                // BoundsEntityPair4CollisionData boundsEntityPair4CollisionData = a_boundsEntityPair4CollisionData [octreeEntity] ;
                Entity octreeBoundsEntity = boundsEntityPair4Collision.bounds2CheckEntity ;

                a_boundsData [octreeBoundsEntity] = new BoundsData () { bounds = checkBounds } ;                
                // a_boundsData [octreeBoundsEntity] = boundsData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobForEachWithEntity_EBCC <CollisionInstancesBufferElement, IsCollidingData, BoundsEntityPair4CollisionData> 
        // struct Job : IJobParallelFor 
        {
            
            // [ReadOnly] public EntityArray a_collisionChecksEntities ;

                        
            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;               
            // [NativeDisableParallelForRestriction]
            // public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 
            
            // [ReadOnly] public ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData ; 

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
            
            
            
            // Ray entity pair, for collision checks
            
            // Check if ray is active
            [ReadOnly] 
            public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] 
            public ComponentDataFromEntity <BoundsData> a_boundsData ;           

            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer, ref IsCollidingData isColliding, [ReadOnly] ref BoundsEntityPair4CollisionData boundsEntityPair4Collision )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRootNodeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                            = a_isCollidingData [octreeRootNodeEntity] ;
                // Stores reference to detected colliding instance.
                // DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer = collisionInstancesBufferElement [octreeRootNodeEntity] ;    
                
                
                isColliding.i_nearestInstanceCollisionIndex                            = 0 ;
                isColliding.f_nearestDistance                                          = float.PositiveInfinity ;

                isColliding.i_collisionsCount                                          = 0 ; // Reset colliding instances counter.

                
                RootNodeData rootNode                                                      = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                            = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer             = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                     = instanceBufferElement [octreeRootNodeEntity] ;   

                
                // BoundsEntityPair4CollisionData rayEntityPair4CollisionData                 = a_boundsEntityPair4CollisionData [octreeRootNodeEntity] ;
                // BoundsEntityPair4CollisionData rayEntityPair4CollisionData                 = a_boundsEntityPair4CollisionData [octreeRootNodeEntity] ;

                // Ray entity pair, for collision checks
                                                                        
                Entity bounds2CheckEntity                                                  = boundsEntityPair4Collision.bounds2CheckEntity ;


                // Is target octree active
                if ( a_isActiveTag.Exists (bounds2CheckEntity) )
                {

                    BoundsData checkBounds = a_boundsData [bounds2CheckEntity] ;
                
                
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( rootNode.i_totalInstancesCountInTree > 0 )
                    {
                    
                        if ( GetCollidingBoundsInstances_Common._GetNodeColliding ( ref rootNode, rootNode.i_rootNodeIndex, checkBounds.bounds, ref a_collisionInstancesBuffer, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer ) )
                        {   
                            /*
                            // Debug
                            Debug.Log ( "Is colliding." ) ;  
                            */                          
                        }
                    }
                
                }

                // a_isCollidingData [octreeRootNodeEntity] = isCollidingData ; // Set back.
                    
            }

        }

    }

}
