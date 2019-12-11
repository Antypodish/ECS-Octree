﻿using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    

    public class GetCollidingBoundsInstancesBarrier_Bounds2Octree : BarrierSystem {} ;

    
    /// <summary>
    /// Bounds to octree system, checks one or more bounds, against its paired target octree entity.
    /// </summary>
    // [UpdateAfter ( typeof ( OctreeForceCollisionCheckSystem ) ) ]   
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]  
    class GetCollidingBoundsInstancesSystem_Bounds2Octree : JobComponentSystem
    {
            
        [Inject] private GetCollidingBoundsInstancesBarrier_Bounds2Octree barrier ;
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;


            group = GetComponentGroup ( 
                typeof (IsActiveTag),
                typeof (GetCollidingBoundsInstancesTag),
                typeof (OctreeEntityPair4CollisionData),
                typeof (BoundsData),
                typeof (IsCollidingData)
                // typeof (CollisionInstancesBufferElement)
                // typeof (RootNodeData) // Unused in ray
            ) ;
            
            base.OnCreateManager ( );
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;     
            ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData = GetComponentDataFromEntity <OctreeEntityPair4CollisionData> () ;
            ComponentDataFromEntity <BoundsData> a_boundsData                                         = GetComponentDataFromEntity <BoundsData> () ;

            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;


            ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;


            // Octree entity pair, for collision checks
                        
            ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData                               = GetComponentDataFromEntity <RootNodeData> () ;
                                
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                    = GetBufferFromEntity <NodeBufferElement> () ;         
            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement        = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;            
            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                    = GetBufferFromEntity <NodeChildrenBufferElement> () ;        
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                            = GetBufferFromEntity <InstanceBufferElement> () ;
            



            // Test bounds 
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            GetCollidingBoundsInstances_Common._DebugBounds ( barrier.CreateCommandBuffer (), a_collisionChecksEntities, a_isCollidingData, collisionInstancesBufferElement, false ) ;
                                  
            

            // Test bounds            
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;


            int i_groupLength = group.CalculateLength () ;

            var setBoundsTestJob = new SetBoundsTestJob 
            {
                
                a_collisionChecksEntities           = a_collisionChecksEntities,

                checkBounds                         = checkBounds,
                a_boundsData                        = a_boundsData,
                // a_rayMaxDistanceData             = a_rayMaxDistanceData,

            }.Schedule ( i_groupLength, 8, inputDeps ) ;

            var job = new Job 
            {
                
                //ecb                                 = ecb,                
                a_collisionChecksEntities           = a_collisionChecksEntities,
                                
                a_octreeEntityPair4CollisionData    = a_octreeEntityPair4CollisionData,
                a_boundsData                        = a_boundsData,
                a_isCollidingData                   = a_isCollidingData,
                collisionInstancesBufferElement     = collisionInstancesBufferElement,


                
                // Octree entity pair, for collision checks
                
                a_isActiveTag                       = a_isActiveTag,

                a_octreeRootNodeData                = a_octreeRootNodeData,

                nodeBufferElement                   = nodeBufferElement,
                nodeInstancesIndexBufferElement     = nodeInstancesIndexBufferElement,
                nodeChildrenBufferElement           = nodeChildrenBufferElement,
                instanceBufferElement               = instanceBufferElement

            }.Schedule ( i_groupLength, 8, setBoundsTestJob ) ;

            return job ;
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetBoundsTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Bounds checkBounds ;

            [ReadOnly] public EntityArray a_collisionChecksEntities ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <BoundsData> a_boundsData ;           
            
            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRayEntity = a_collisionChecksEntities [i_arrayIndex] ;

                BoundsData boundsData = new BoundsData () { bounds = checkBounds } ;                
                a_boundsData [octreeRayEntity] = boundsData ;
            }
            
        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct Job : IJobParallelFor 
        {
            
            [ReadOnly] public EntityArray a_collisionChecksEntities ;

            [ReadOnly] public ComponentDataFromEntity <OctreeEntityPair4CollisionData> a_octreeEntityPair4CollisionData ;  

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <BoundsData> a_boundsData ;  
            
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

                Entity octreeBoundsEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeBoundsEntity] ;   
                // Stores reference to detected colliding instance.
                DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer          = collisionInstancesBufferElement [octreeBoundsEntity] ;  
                
                isCollidingData.i_collisionsCount                   = 0 ; // Reset colliding instances counter.
                // isCollidingData.i_nearestInstanceCollisionIndex  = 0 ; // Unused
                // isCollidingData.f_nearestDistance                = float.PositiveInfinity ; // Unused

                


                OctreeEntityPair4CollisionData octreeEntityPair4CollisionData                       = a_octreeEntityPair4CollisionData [octreeBoundsEntity] ;
                BoundsData checkBounds                                                              = a_boundsData [octreeBoundsEntity] ;
            

                // Octree entity pair, for collision checks
                    
                Entity octreeRootNodeEntity                                                         = octreeEntityPair4CollisionData.octree2CheckEntity ;

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
                    
                        if ( GetCollidingBoundsInstances_Common._GetNodeColliding ( octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, checkBounds.bounds, ref a_collisionInstancesBuffer, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) )
                        {   
                            /*
                            // Debug
                            Debug.Log ( "Is colliding." ) ;  
                            */                          
                        }

                    }
                
                }

                a_isCollidingData [octreeBoundsEntity] = isCollidingData ; // Set back.
                    
            }

        }

    }

}
