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
    class IsBoundsCollidingSystem_Bounds2Octree : JobComponentSystem
    {
            
        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;
            
            group = GetEntityQuery 
            ( 
                typeof ( IsActiveTag ),
                typeof ( IsBoundsCollidingTag ),
                typeof ( OctreeEntityPair4CollisionData ),
                typeof ( BoundsData ),
                typeof ( IsCollidingData )
                // typeof (CollisionInstancesBufferElement)
                // typeof (RootNodeData) // Unused in bounds
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            NativeArray <Entity> na_collisionChecksEntities                                           = group.ToEntityArray ( Allocator.Temp ) ; 
            
            // ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData = GetComponentDataFromEntity <OctreeEntityPair4CollisionData> () ;
            // ComponentDataFromEntity <BoundsData> a_boundsData                                         = GetComponentDataFromEntity <BoundsData> () ;

            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> ( true ) ;
            

            // Test bounds 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            IsBoundsColliding_Common._DebugBounds ( ref na_collisionChecksEntities, ref a_isCollidingData, false ) ;
            
            // int i_groupLength = na_collisionChecksEntities.Length ;
            na_collisionChecksEntities.Dispose () ;
            

            // Test bounds            
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;



            var setBoundsTestJob = new SetBoundsTestJob 
            {
                //a_collisionChecksEntities           = na_collisionChecksEntities,

                checkBounds                         = checkBounds,
                // a_boundsData                        = a_boundsData,

            }.Schedule ( group, inputDeps ) ;

            var job = new Job 
            {
                // a_collisionChecksEntities           = na_collisionChecksEntities,
                                
                //a_octreeEntityPair4CollisionData    = a_octreeEntityPair4CollisionData,
                //a_boundsData                        = a_boundsData,
                //a_isCollidingData                   = a_isCollidingData,


                
                // Octree entity pair, for collision checks
                
                a_isActiveTag                       = GetComponentDataFromEntity <IsActiveTag> ( true ),

                a_octreeRootNodeData                = GetComponentDataFromEntity <RootNodeData> ( true ),

                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> ( true ),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> ( true ),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> ( true ),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> ( true )

            }.Schedule ( group, setBoundsTestJob ) ;


            return job ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobForEach <BoundsData>
        // struct SetBoundsTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Bounds checkBounds ;

            // [ReadOnly] public EntityArray a_collisionChecksEntities ;

            // [NativeDisableParallelForRestriction]
            //public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( ref BoundsData bounds )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                bounds = new BoundsData () { bounds = checkBounds } ;                
                // a_boundsData [octreeRayEntity] = boundsData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobForEach_CCC <IsCollidingData, OctreeEntityPair4CollisionData, BoundsData> 
        // struct Job : IJobParallelFor 
        {
            
            // [ReadOnly] public EntityArray a_collisionChecksEntities ;


            
            // [ReadOnly] public ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData ;  
            // [ReadOnly] public ComponentDataFromEntity <BoundsData> a_boundsData ;  
            
            // [NativeDisableParallelForRestriction]
            // public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;


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


            public void Execute ( ref IsCollidingData isColliding, [ReadOnly] ref OctreeEntityPair4CollisionData octreeEntityPair4Collision, [ReadOnly] ref BoundsData checkBounds )
            // public void Execute ( int i_arrayIndex )
            {

                // Entity octreeBoundsEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                // IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeBoundsEntity] ;   
                
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
                        // To even allow instances collision checks, octree must have at least one instance.
                        if ( IsBoundsColliding_Common._IsNodeColliding ( ref octreeRootNode, octreeRootNode.i_rootNodeIndex, checkBounds.bounds, ref isColliding, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer ) )
                        {
                            // Debug.Log ( "Is colliding." ) ;
                        }
                
                    }

                }

                // a_isCollidingData [octreeBoundsEntity] = isColliding ; // Set back.
                    
            }

        }

    }

}
