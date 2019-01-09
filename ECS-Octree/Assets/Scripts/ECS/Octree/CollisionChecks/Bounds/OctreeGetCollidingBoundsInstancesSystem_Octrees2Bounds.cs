using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using UnityEngine;
using Unity.Burst ;
using Unity.Mathematics ;


namespace ECS.Octree
{


    /// <summary>
    /// Bounds to octree system, checks one or more bounds, against its paired target octree entity.
    /// </summary>
    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]   
    class GetCollidingBoundsInstancesSystem_Octrees2Bounds : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag),
                typeof (GetCollidingBoundsInstancesTag),
                typeof (BoundsEntityPair4CollisionData),
                // typeof (BoundsData), // Not used by octree entity
                typeof (IsCollidingData),
                typeof (CollisionInstancesBufferElement),
                typeof (RootNodeData) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;    
            
            ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData = GetComponentDataFromEntity <BoundsEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;
                        
            ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData                               = GetComponentDataFromEntity <RootNodeData> () ;
       
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                    = GetBufferFromEntity <NodeBufferElement> () ;         
            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement        = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;            
            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                    = GetBufferFromEntity <NodeChildrenBufferElement> () ;        
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                            = GetBufferFromEntity <InstanceBufferElement> () ;


            // Ray entity pair, for collision checks
                        
            ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;
            
            ComponentDataFromEntity <BoundsData> a_boundsData                                         = GetComponentDataFromEntity <BoundsData> () ;
                        


            // Test bounds                        
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;

// ...             Bray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;
            

            //Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;
            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * rayMaxDistanceData.f, Color.red )  ;
            
            
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < 1; i_collisionChecksIndex ++ )
            // for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < a_collisionChecksEntities.Length; i_collisionChecksIndex ++ )
            {
                  
                Entity octreeEntity                = a_collisionChecksEntities [i_collisionChecksIndex] ;

                // RayData rayData                       = new RayData () { ray = ray } ;
                // a_rayData [octreeRayEntity]           = rayData ;
                

                // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * rayMaxDistanceData.f, Color.red )  ;

                // Last known instances collisions count.
                IsCollidingData isCollidingData = a_isCollidingData [octreeEntity] ;

                if ( isCollidingData.i_collisionsCount > 0 )
                {
                    
                    
                    // RayEntityPair4CollisionData rayEntityPair4CollisionData = a_rayEntityPair4CollisionData [octreeEntity] ;
                    // Entity octreeRayEntity = rayEntityPair4CollisionData.ray2CheckEntity ;

                    // RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [octreeRayEntity] ;

                    // Debug.Log ( "Octree: Last known instances collisions count #" + isCollidingData.i_collisionsCount ) ;

                    // Stores reference to detected colliding instance.
                    DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer = collisionInstancesBufferElement [octreeEntity] ;    

                    
                    string s_collidingIDs = "" ;

                    CollisionInstancesBufferElement collisionInstancesBuffer ;

                    for ( int i = 0; i < isCollidingData.i_collisionsCount; i ++ )
                    {
                        collisionInstancesBuffer = a_collisionInstancesBuffer [i] ;
                        s_collidingIDs += collisionInstancesBuffer.i_ID + ", " ;
                    }

                    Debug.Log ( "Is colliding with #" + isCollidingData.i_collisionsCount + " instances of IDs: " + s_collidingIDs ) ;
                    // Debug.Log ( "Is colliding with #" + isCollidingData.i_collisionsCount + " instances of IDs: " + s_collidingIDs + "; Nearest collided instance is at " + isCollidingData.f_nearestDistance + "m, with ID #" + a_collisionInstancesBuffer [isCollidingData.i_nearestInstanceCollisionIndex].i_ID ) ;
                    
                }
                
            }
            
            


            int i_groupLength = group.CalculateLength () ;

            
            var setBoundsTestJob = new SetBoundsTestJob 
            {
                
                checkBounds                         = checkBounds,

                a_collisionChecksEntities           = a_collisionChecksEntities,
                a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,
                
                a_boundsData                        = a_boundsData,

            }.Schedule ( i_groupLength, 8, inputDeps ) ;
            




            var job = new Job 
            {
                
                //ecb                                 = ecb,                
                a_collisionChecksEntities           = a_collisionChecksEntities,
                                
                a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,

                a_isCollidingData                   = a_isCollidingData,
                collisionInstancesBufferElement     = collisionInstancesBufferElement,
                
                a_octreeRootNodeData                = a_octreeRootNodeData,

                nodeBufferElement                   = nodeBufferElement,
                nodeInstancesIndexBufferElement     = nodeInstancesIndexBufferElement,
                nodeChildrenBufferElement           = nodeChildrenBufferElement,
                instanceBufferElement               = instanceBufferElement,

                
                // Ray entity pair, for collision checks
                
                a_isActiveTag                       = a_isActiveTag,
                
                a_boundsData                        = a_boundsData,


            }.Schedule ( i_groupLength, 8, setBoundsTestJob ) ;

            return job ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Bounds checkBounds ;

            [ReadOnly] public EntityArray a_collisionChecksEntities ;
            [ReadOnly] public ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( int i_arrayIndex )
            {

                Entity octreeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                BoundsEntityPair4CollisionData boundsEntityPair4CollisionData =  a_boundsEntityPair4CollisionData [octreeEntity] ;
                Entity octreeBoundsEntity = boundsEntityPair4CollisionData.bounds2CheckEntity ;

                BoundsData boundsData = new BoundsData () { bounds = checkBounds } ;                
                a_boundsData [octreeBoundsEntity] = boundsData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobParallelFor 
        {
            
            [ReadOnly] public EntityArray a_collisionChecksEntities ;

                        
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;               
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 
            
            [ReadOnly] public ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData ; 

            [ReadOnly] public ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData ;
          
            [ReadOnly] public BufferFromEntity <NodeBufferElement> nodeBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;            
            [ReadOnly] public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;
            
            
            
            // Ray entity pair, for collision checks
            
            // Check if ray is active
            [ReadOnly] public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] public ComponentDataFromEntity <BoundsData> a_boundsData ;           


            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRootNodeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRootNodeEntity] ;
                // Stores reference to detected colliding instance.
                DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer          = collisionInstancesBufferElement [octreeRootNodeEntity] ;    
                
                
                isCollidingData.i_nearestInstanceCollisionIndex = 0 ;
                isCollidingData.f_nearestDistance               = float.PositiveInfinity ;

                isCollidingData.i_collisionsCount               = 0 ; // Reset colliding instances counter.

                
                RootNodeData octreeRootNodeData                                                     = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                                     = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer          = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                      = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                              = instanceBufferElement [octreeRootNodeEntity] ;   

                
                BoundsEntityPair4CollisionData rayEntityPair4CollisionData                          = a_boundsEntityPair4CollisionData [octreeRootNodeEntity] ;

                // Ray entity pair, for collision checks
                                                                        
                Entity bounds2CheckEntity                                                           = rayEntityPair4CollisionData.bounds2CheckEntity ;


                // Is target octree active
                if ( a_isActiveTag.Exists (bounds2CheckEntity) )
                {

                    BoundsData checkBounds                                                          = a_boundsData [bounds2CheckEntity] ;
                
                
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNodeData.i_totalInstancesCountInTree > 0 )
                    {
                    
                        if ( GetCollidingBoundsInstances_Common._GetNodeColliding ( octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, checkBounds.bounds, ref a_collisionInstancesBuffer, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) )
                        {   
                            /*
                            // Debug
                            Debug.Log ( "Is colliding." ) ;  
                            */                          
                        }
                    }
                
                }

                a_isCollidingData [octreeRootNodeEntity] = isCollidingData ; // Set back.
                    
            }

        }

    }

}
