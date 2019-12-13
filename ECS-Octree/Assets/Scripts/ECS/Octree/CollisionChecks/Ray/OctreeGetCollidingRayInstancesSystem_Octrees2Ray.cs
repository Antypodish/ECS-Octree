using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    
    /// <summary>
    /// Octree to ray system, checks one or more octress, against its paired target ray entity.
    /// </summary>
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]    
    class GetCollidingRayInstancesSystem_Octrees2Ray : JobComponentSystem
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
                typeof ( GetCollidingRayInstancesTag ),
                typeof ( RayEntityPair4CollisionData ),
                // typeof (RayData), // Not used by octree entity
                // typeof (RayMaxDistanceData), // Not used by octree entity
                typeof ( IsCollidingData ),
                typeof ( CollisionInstancesBufferElement ),
                typeof ( RootNodeData ) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.Temp ) ;    
            
            ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData       = GetComponentDataFromEntity <RayEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;
                        
       
            ComponentDataFromEntity <RayData> a_rayData                                               = GetComponentDataFromEntity <RayData> () ;
            ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData                         = GetComponentDataFromEntity <RayMaxDistanceData> () ;
            


            // Test ray 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;
            GetCollidingRayInstances_Common._DebugRays ( ref ecb, ref na_collisionChecksEntities, ref a_rayData, ref a_rayMaxDistanceData, ref a_isCollidingData, ref collisionInstancesBufferElement, ref a_rayEntityPair4CollisionData, false, false ) ;
            
            na_collisionChecksEntities.Dispose () ;

            eiecb.AddJobHandleForProducer ( inputDeps ) ;
            
            // Test ray            
            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;
            
            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;

            int i_groupLength = group.CalculateEntityCount () ;

            
            JobHandle setRayTestJobHandle = new SetRayTestJob 
            {
                
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                // a_rayEntityPair4CollisionData       = a_rayEntityPair4CollisionData,

                ray                                 = ray,
                a_rayData                           = a_rayData,
                // a_rayMaxDistanceData                = a_rayMaxDistanceData,

            }.Schedule ( group, inputDeps ) ;
            

            JobHandle jobHandle = new Job 
            {
                
                //ecb                                 = ecb,                
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                                
                // a_rayEntityPair4CollisionData       = a_rayEntityPair4CollisionData,

                // a_isCollidingData                   = a_isCollidingData,
                // collisionInstancesBufferElement     = collisionInstancesBufferElement,
                
                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true ),

                
                // Ray entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),
                
                a_rayData                           = a_rayData,
                a_rayMaxDistanceData                = a_rayMaxDistanceData,

            }.Schedule ( group, setRayTestJobHandle ) ;


            return jobHandle ;
        }

        
        [BurstCompile]
        struct SetRayTestJob : IJobForEach <RayEntityPair4CollisionData>
        // struct SetRayTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Ray ray ;

            // [ReadOnly] public EntityArray a_collisionChecksEntities ;
            // [ReadOnly] public ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData ;
            
             
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <RayData> a_rayData ;           

            
            public void Execute ( [ReadOnly] ref RayEntityPair4CollisionData rayEntityPair4Collision )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                // RayEntityPair4CollisionData rayEntityPair4CollisionData =  a_rayEntityPair4CollisionData [octreeEntity] ;
                Entity octreeRayEntity = rayEntityPair4Collision.ray2CheckEntity ;

                RayData rayData = new RayData () { ray = ray } ;                
                a_rayData [octreeRayEntity] = rayData ;

            }
                        
        }
        


        [BurstCompile]    
        struct Job : IJobForEachWithEntity_EBCC <CollisionInstancesBufferElement, IsCollidingData, RayEntityPair4CollisionData>     
        // struct Job : IJobParallelFor 
        {
            
            // [ReadOnly] public EntityArray a_collisionChecksEntities ;

                        
            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;               
            // [NativeDisableParallelForRestriction]
            // public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 
            
            // [ReadOnly] public ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData ; 

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
            public ComponentDataFromEntity <RayData> a_rayData ;           
            [ReadOnly] 
            public ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData ;
            


            
            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer, ref IsCollidingData isColliding, [ReadOnly] ref RayEntityPair4CollisionData rayEntityPair4Collision )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRootNodeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRootNodeEntity] ;
                // Stores reference to detected colliding instance.
                // DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer          = collisionInstancesBufferElement [octreeRootNodeEntity] ;    
                
                
                isColliding.i_nearestInstanceCollisionIndex                                 = 0 ;
                isColliding.f_nearestDistance                                               = float.PositiveInfinity ;

                isColliding.i_collisionsCount                                               = 0 ; // Reset colliding instances counter.

                
                RootNodeData octreeRootNode                                                 = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                             = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer  = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer              = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                      = instanceBufferElement [octreeRootNodeEntity] ;   

                
                // RayEntityPair4CollisionData rayEntityPair4CollisionData                     = a_rayEntityPair4CollisionData [octreeRootNodeEntity] ;

                // Ray entity pair, for collision checks
                                                                        
                Entity ray2CheckEntity                                                      = rayEntityPair4Collision.ray2CheckEntity ;

                // Is target octree active
                if ( a_isActiveTag.Exists ( ray2CheckEntity ) )
                {

                    RayData rayData                       = a_rayData [ray2CheckEntity] ;
                    RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [ray2CheckEntity] ;
            

                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNode.i_totalInstancesCountInTree > 0 )
                    {
                    
                        
                        if ( GetCollidingRayInstances_Common._GetNodeColliding ( ref octreeRootNode, octreeRootNode.i_rootNodeIndex, rayData.ray, ref a_collisionInstancesBuffer, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer, rayMaxDistanceData.f ) )
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

                // a_isCollidingData [octreeRootNodeEntity] = isCollidingData ; // Set back.
                    
            }

        }


        


    }

}