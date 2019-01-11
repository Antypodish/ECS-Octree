using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace ECS.Octree
{
    
    
    public class GetCollidingRayInstancesBarrier_Octrees2Ray : BarrierSystem {} ;


    /// <summary>
    /// Octree to ray system, checks one or more octress, against its paired target ray entity.
    /// </summary>
    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class GetCollidingRayInstancesSystem_Octrees2Ray : JobComponentSystem
    {
        
        [Inject] private GetCollidingRayInstancesBarrier_Octrees2Ray barrier ;
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Ray Colliding Instances System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag),
                typeof (GetCollidingRayInstancesTag),
                typeof (RayEntityPair4CollisionData),
                // typeof (RayData), // Not used by octree entity
                // typeof (RayMaxDistanceData), // Not used by octree entity
                typeof (IsCollidingData),
                typeof (CollisionInstancesBufferElement),
                typeof (RootNodeData) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            
            // EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            EntityArray a_collisionChecksEntities                                                     = group.GetEntityArray () ;    
            
            ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData       = GetComponentDataFromEntity <RayEntityPair4CollisionData> () ;
            
            ComponentDataFromEntity <IsCollidingData> a_isCollidingData                               = GetComponentDataFromEntity <IsCollidingData> () ;
            BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement        = GetBufferFromEntity <CollisionInstancesBufferElement> () ;
                        
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
            GetCollidingRayInstances_Common._DebugRays ( barrier.CreateCommandBuffer (), a_collisionChecksEntities, a_rayData, a_rayMaxDistanceData, a_isCollidingData, collisionInstancesBufferElement, a_rayEntityPair4CollisionData, false, false ) ;

            
            // Test ray            
            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;
            
            // Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 100, Color.red )  ;

            int i_groupLength = group.CalculateLength () ;

            
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
                collisionInstancesBufferElement     = collisionInstancesBufferElement,
                
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

                RayEntityPair4CollisionData rayEntityPair4CollisionData =  a_rayEntityPair4CollisionData [octreeEntity] ;
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
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement ; 
            
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

                a_isCollidingData [octreeRootNodeEntity] = isCollidingData ; // Set back.
                    
            }

        }


        


    }

}