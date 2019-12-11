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
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]   
    class IsBoundsCollidingSystem_Octrees2Bounds : JobComponentSystem
    {
            
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag),
                typeof (IsBoundsCollidingTag),
                typeof (BoundsEntityPair4CollisionData),
                // typeof (BoundsData), // Not used by octree entity
                typeof (IsCollidingData),
                // typeof (CollisionInstancesBufferElement),
                typeof (RootNodeData) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;    
            
            ComponentDataFromEntity <BoundsEntityPair4CollisionData> a_boundsEntityPair4CollisionData = GetComponentDataFromEntity <BoundsEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
                        
            ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData                               = GetComponentDataFromEntity <RootNodeData> () ;
       
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                    = GetBufferFromEntity <NodeBufferElement> () ;         
            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement        = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;            
            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                    = GetBufferFromEntity <NodeChildrenBufferElement> () ;        
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                            = GetBufferFromEntity <InstanceBufferElement> () ;


            // Ray entity pair, for collision checks
                        
            ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;
            
            ComponentDataFromEntity <BoundsData> a_boundsData                                         = GetComponentDataFromEntity <BoundsData> () ;
                        




            // Test bounds 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            IsBoundsColliding_Common._DebugBounds ( a_collisionChecksEntities, a_isCollidingData, false ) ;
                        

            
            // Test bounds                        
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;


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
                    
                        if ( IsBoundsColliding_Common._IsNodeColliding ( octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, checkBounds.bounds, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) )
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
