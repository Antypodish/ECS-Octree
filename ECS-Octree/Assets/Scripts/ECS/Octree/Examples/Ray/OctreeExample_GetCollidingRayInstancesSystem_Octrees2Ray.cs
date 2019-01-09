using Unity.Collections ;
using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine;

namespace ECS.Octree
{


    public class OctreeExample_GetCollidingRayInstancesBarrier_Octrees2Ray : BarrierSystem {} ;
 

    class OctreeExample_GetCollidingRayInstancesSystem_Octrees2Ray : JobComponentSystem
    {

        [Inject] private OctreeExample_GetCollidingRayInstancesBarrier_Octrees2Ray barrier ;
             
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test octrees
            // Many octrees, to ray pair
            // Where each octree has one ray entity target.
            // Results return number of colliding instance
            // index to list of the colliding instances IDs,
            // and distance to the nearest instance.

            // Toggle manually only one example systems at the time
            if ( false ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Ray Instances System" ) ;


            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            
            
            // Many octrees, to single, or many rays
            // Where each octree has one ray entity target.

            // ***** Example Ray Components For Collision Checks ***** //

            // Test ray entity 
            // for each octree
            Entity rayEntity = EntityManager.CreateEntity () ;
                   
            EntityManager.AddComponentData ( rayEntity, new IsActiveTag () ) ; 
            EntityManager.AddComponentData ( rayEntity, new RayData () ) ; 
            EntityManager.AddComponentData ( rayEntity, new RayMaxDistanceData ()
            {
                f = 100f
            } ) ; 


            // ***** Initialize Octree ***** //







            int i_octreesCount = 1 ;

            NativeArray <Entity> a_entities = new NativeArray<Entity> ( i_octreesCount, Allocator.Temp ) ;

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;
                //Entity newOctreeEntity = a_entities [i_octreeEntityIndex] ;
                AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            
                EntityManager.AddComponent ( newOctreeEntity, typeof ( GetCollidingRayInstancesTag ) ) ;
                
                EntityManager.AddComponentData ( newOctreeEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                EntityManager.AddBuffer <CollisionInstancesBufferElement> ( newOctreeEntity ) ;

                
                // Assign target ray entity, to octree entity
                Entity octreeEntity = newOctreeEntity ;    
                
                // Check bounds collision with octree and return colliding instances.
                EntityManager.AddComponentData ( octreeEntity, new RayEntityPair4CollisionData () 
                {
                    ray2CheckEntity = rayEntity
                } ) ;




                
                // ***** Example Components To Add / Remove Instance ***** //
            

                
                Entity newBlockEntity = new Entity () ; // empty entity
                NativeArray <Entity> a_instanceEntities = new NativeArray <Entity> ( 100, Allocator.Temp ) ;
                int i_instanceEntityIndex = 0 ;
            
                for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
                {  
                    newBlockEntity = EntityManager.CreateEntity () ;
                    a_instanceEntities [i_instanceID] = newBlockEntity ;                  
                    // i_instanceEntityIndex ++ ;
                }

            


                // Request to add 100 instances
                // User is responsible to ensure, that instances IDs are unique in the octrtree.
                EntityManager.AddBuffer <AddInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.         

                BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;
                DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [newOctreeEntity] ;  

                for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
                {  

                     newBlockEntity = a_instanceEntities [i_instanceEntityIndex] ;
                    i_instanceEntityIndex ++ ;
                
                    int x = i_instanceID % 10 ;
                    int y = (int) math.floor ( i_instanceID / 10 ) ;
                    float3 f3_blockCenter = new float3 ( x, 0, y ) + new float3 ( 1, 1, 1 )  * 0.5f ;

                    Blocks.Methods._AddBlockRequestViaCustomBufferWithEntity ( ecb, newBlockEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 1 ) ;



     //               Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

                    Bounds bounds = new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ;
                
                    AddInstanceBufferElement addInstanceBuffer = new AddInstanceBufferElement () 
                    {
                        i_instanceID = newBlockEntity.Index,
                        i_version = newBlockEntity.Version,
                        instanceBounds = bounds
                    };

                    a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
                }


                

                // Request to remove 53 instances.
                // User is responsible to ensure, that requested instance ID to delete exists in the octree.            
                EntityManager.AddBuffer <RemoveInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were removed, tag will be deleted.

                BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [newOctreeEntity] ;  
            
                i_instanceEntityIndex = 0 ;

                for ( int i_instanceID = 0; i_instanceID < 53; i_instanceID ++ )
                {            

                    // i_instanceEntityIndex -- ;
                    Entity removeEntity = a_instanceEntities [i_instanceEntityIndex] ;                 
                    i_instanceEntityIndex ++ ;


                    int x = i_instanceID % 10 ;
                    int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;
     //               Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;
                                         
                    RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                    {
                        i_instanceID = removeEntity.Index          
                        // i_instanceID = i_instanceID        
                    };
                
                    a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;


                    Blocks.Methods._RemoveBlockRequestWithEntity ( ecb, removeEntity ) ;

                }
                
                
            a_instanceEntities.Dispose () ;


            } // for



        }

    }
}


