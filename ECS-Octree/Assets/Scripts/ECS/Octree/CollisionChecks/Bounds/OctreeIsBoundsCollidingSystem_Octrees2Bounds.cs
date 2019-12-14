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
    // [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]   
    class IsBoundsCollidingSystem_Octrees2Bounds : JobComponentSystem
    {
            
        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;
            
            group = GetEntityQuery ( 
                typeof ( IsActiveTag ),
                typeof ( IsBoundsCollidingTag ),
                typeof ( BoundsEntityPair4CollisionData ),
                // typeof (BoundsData), // Not used by octree entity
                typeof ( IsCollidingData ),
                // typeof (CollisionInstancesBufferElement),
                typeof ( RootNodeData ) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.Temp ) ;    
            
            // ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData = GetComponentDataFromEntity <BoundsEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> ( true ) ;
                        

            // Ray entity pair, for collision checks
                        
            //ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;            
            ComponentDataFromEntity <BoundsData> a_boundsData                                         = GetComponentDataFromEntity <BoundsData> () ;
                        

            // Test bounds 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            IsBoundsColliding_Common._DebugBounds ( ref na_collisionChecksEntities, ref a_isCollidingData, false ) ;
                    
            na_collisionChecksEntities.Dispose () ;    

            
            // Test bounds                        
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;


            // int i_groupLength = group.CalculateLength () ;

            
            JobHandle setBoundsTestJobHandle = new SetBoundsTestJob 
            {
                
                checkBounds                         = checkBounds,

                // a_collisionChecksEntities           = na_collisionChecksEntities,
                // a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,
                
                a_boundsData                        = a_boundsData,

            }.Schedule ( group, inputDeps ) ;
            

            JobHandle jobHandle = new Job 
            {
                
                //ecb                                 = ecb,                
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                                
                // a_boundsEntityPair4CollisionData    = a_boundsEntityPair4CollisionData,

                // a_isCollidingData                   = a_isCollidingData,

                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true ),

                
                // Ray entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),
                
                a_boundsData                        = a_boundsData,


            }.Schedule ( group, setBoundsTestJobHandle ) ;


            return jobHandle ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobForEach <BoundsEntityPair4CollisionData>
        {
            
            [ReadOnly] public Bounds checkBounds ;

            // [ReadOnly] public EntityArray a_collisionChecksEntities ;
            // [ReadOnly] public ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( ref BoundsEntityPair4CollisionData boundsEntityPair4Collision )
            {

                // Entity octreeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                // BoundsEntityPair4CollisionData boundsEntityPair4Collision =  a_boundsEntityPair4CollisionData [octreeEntity] ;
                Entity octreeBoundsEntity = boundsEntityPair4Collision.bounds2CheckEntity ;
           
                a_boundsData [octreeBoundsEntity] = new BoundsData () { bounds = checkBounds } ;   
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobForEachWithEntity_ECC <IsCollidingData, BoundsEntityPair4CollisionData> 
        {
            
            // [ReadOnly] public EntityArray a_collisionChecksEntities ;

                        
            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;               
            
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


            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, ref IsCollidingData isColliding, [ReadOnly] ref BoundsEntityPair4CollisionData rayEntityPair4Collision )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRootNodeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRootNodeEntity] ;
                
                
                isColliding.i_nearestInstanceCollisionIndex = 0 ;
                isColliding.f_nearestDistance               = float.PositiveInfinity ;

                isColliding.i_collisionsCount               = 0 ; // Reset colliding instances counter.

                
                RootNodeData octreeRootNode                                                     = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                                 = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer      = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                  = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                          = instanceBufferElement [octreeRootNodeEntity] ;   

                
                // BoundsEntityPair4CollisionData rayEntityPair4CollisionData                      = a_boundsEntityPair4CollisionData [octreeRootNodeEntity] ;

                // Ray entity pair, for collision checks
                                                                        
                Entity bounds2CheckEntity                                                       = rayEntityPair4Collision.bounds2CheckEntity ;


                // Is target octree active
                if ( a_isActiveTag.Exists ( bounds2CheckEntity ) )
                {

                    BoundsData checkBounds = a_boundsData [bounds2CheckEntity] ;
                
                
                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNode.i_totalInstancesCountInTree > 0 )
                    {
                    
                        if ( IsBoundsColliding_Common._IsNodeColliding ( ref octreeRootNode, octreeRootNode.i_rootNodeIndex, checkBounds.bounds, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer ) )
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
