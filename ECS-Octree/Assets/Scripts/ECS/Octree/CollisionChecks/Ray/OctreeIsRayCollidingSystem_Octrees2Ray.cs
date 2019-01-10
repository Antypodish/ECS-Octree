using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;

namespace ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class IsRayCollidingSystem_Octrees2Ray : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree IS Ray Colliding System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag), 
                typeof (IsRayCollidingTag),
                typeof (RayEntityPair4CollisionData),
                // typeof (RayData), // Not used by octree entity
                // typeof (RayMaxDistanceData), // Not used by octree entity
                typeof (IsCollidingData),
                // typeof (CollisionInstancesBufferElement), // Not required in this system
                typeof (RootNodeData) 
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {


            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;    
            
            ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData       = GetComponentDataFromEntity <RayEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
                        
            ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData                               = GetComponentDataFromEntity <RootNodeData> () ;
          
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                    = GetBufferFromEntity <NodeBufferElement> () ;         
            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement        = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;            
            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                    = GetBufferFromEntity <NodeChildrenBufferElement> () ;        
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                            = GetBufferFromEntity <InstanceBufferElement> () ;


            // Ray entity pair, for collision checks
                        
            ComponentDataFromEntity <IsActiveTag> a_isActiveTag                                       = GetComponentDataFromEntity <IsActiveTag> () ;
            
            ComponentDataFromEntity <RayData> a_rayData                                               = GetComponentDataFromEntity <RayData> () ;
            ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData                         = GetComponentDataFromEntity <RayMaxDistanceData> () ;



            // Test ray  
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            IsRayColliding_Common._DebugRays ( a_collisionChecksEntities, a_rayData, a_rayMaxDistanceData, a_isCollidingData, a_rayEntityPair4CollisionData, false, false ) ;

            /*
            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.
            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < 1; i_collisionChecksIndex ++ )
            // for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < a_collisionChecksEntities.Length; i_collisionChecksIndex ++ )
            {                  
                Entity octreeEntity = a_collisionChecksEntities [i_collisionChecksIndex] ;
                IsCollidingData isCollidingData = a_isCollidingData [octreeEntity] ;

                if ( isCollidingData.i_collisionsCount > 0 ) Debug.Log ( "Is colliding." ) ;                
            }
            */
            


            int i_groupLength = group.CalculateLength () ;

            
            // Test ray            
            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;

            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;

            var setRayTestJob = new SetRayTestJob 
            {
                
                a_collisionChecksEntities           = a_collisionChecksEntities,
                a_rayEntityPair4CollisionData       = a_rayEntityPair4CollisionData,

                ray                                 = ray,
                a_rayData                           = a_rayData,
                // a_rayMaxDistanceData                = a_rayMaxDistanceData,

            }.Schedule ( i_groupLength, 8, inputDeps ) ;
            




            var job = new Job 
            {
                
                //ecb                                 = ecb,                
                a_collisionChecksEntities           = a_collisionChecksEntities,
                                
                a_rayEntityPair4CollisionData       = a_rayEntityPair4CollisionData,

                a_isCollidingData                   = a_isCollidingData,
                
                a_octreeRootNodeData                = a_octreeRootNodeData,

                nodeBufferElement                   = nodeBufferElement,
                nodeInstancesIndexBufferElement     = nodeInstancesIndexBufferElement,
                nodeChildrenBufferElement           = nodeChildrenBufferElement,
                instanceBufferElement               = instanceBufferElement,

                
                // Ray entity pair, for collision checks
                
                a_isActiveTag                       = a_isActiveTag,
                
                a_rayData                           = a_rayData,
                a_rayMaxDistanceData                = a_rayMaxDistanceData,


            }.Schedule ( i_groupLength, 8, setRayTestJob ) ;

            return job ;

        }


        [BurstCompile]
        // [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct SetRayTestJob : IJobParallelFor 
        {
            
            [ReadOnly] public Ray ray ;

            [ReadOnly] public EntityArray a_collisionChecksEntities ;
            [ReadOnly] public ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <RayData> a_rayData ;           

            
            public void Execute ( int i_arrayIndex )
            {

                Entity octreeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                RayEntityPair4CollisionData rayEntityPair4CollisionData = a_rayEntityPair4CollisionData [octreeEntity] ;
                Entity octreeRayEntity = rayEntityPair4CollisionData.ray2CheckEntity ;

                RayData rayData = new RayData () { ray = ray } ;                
                a_rayData [octreeRayEntity] = rayData ;

            }
                        
        }



        [BurstCompile]        
        struct Job : IJobParallelFor 
        {
            
            [ReadOnly] public EntityArray a_collisionChecksEntities ;

                        
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <IsCollidingData> a_isCollidingData ;   
            
 //           [NativeDisableParallelForRestriction]
 //           public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 
            
            [ReadOnly] public ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData ; 

            [ReadOnly] public ComponentDataFromEntity <RootNodeData> a_octreeRootNodeData ;
                                   
            [ReadOnly] public BufferFromEntity <NodeBufferElement> nodeBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;            
            [ReadOnly] public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;            
            [ReadOnly] public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;
            
            
            
            // Ray entity pair, for collision checks
            
            // Check if ray is active
            [ReadOnly] public ComponentDataFromEntity <IsActiveTag> a_isActiveTag ;

            [ReadOnly] public ComponentDataFromEntity <RayData> a_rayData ;           
            [ReadOnly] public ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData ;
            



            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRootNodeEntity = a_collisionChecksEntities [i_arrayIndex] ;

                
                // Its value should be 0, if no collision is detected.
                // And >= 1, if instance collision is detected, or there is more than one collision, 
                // indicating number of collisions. 
                IsCollidingData isCollidingData                                                     = a_isCollidingData [octreeRootNodeEntity] ;                   
                isCollidingData.i_collisionsCount                   = 0 ; // Reset colliding instances counter.
                //isCollidingData.i_nearestInstanceCollisionIndex   = 0 ; // Unused
                //isCollidingData.f_nearestDistance                 = float.PositiveInfinity ; // Unused

                
                RootNodeData octreeRootNodeData                                                           = a_octreeRootNodeData [octreeRootNodeEntity] ;
                
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                                     = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer          = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                      = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                              = instanceBufferElement [octreeRootNodeEntity] ;   

                
                RayEntityPair4CollisionData rayEntityPair4CollisionData                             = a_rayEntityPair4CollisionData [octreeRootNodeEntity] ;

                // Ray entity pair, for collision checks
                                                                        
                Entity ray2CheckEntity                                                              = rayEntityPair4CollisionData.ray2CheckEntity ;

                // Is target octree active
                if ( a_isActiveTag.Exists (ray2CheckEntity) )
                {

                    RayData rayData                                                                     = a_rayData [ray2CheckEntity] ;
                    RayMaxDistanceData rayMaxDistanceData                                               = a_rayMaxDistanceData [ray2CheckEntity] ;
            

                    // To even allow instances collision checks, octree must have at least one instance.
                    if ( octreeRootNodeData.i_totalInstancesCountInTree > 0 )
                    {
                    
                        
                        if ( IsRayColliding_Common._IsNodeColliding ( octreeRootNodeData, octreeRootNodeData.i_rootNodeIndex, rayData.ray, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, rayMaxDistanceData.f ) )                        
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
