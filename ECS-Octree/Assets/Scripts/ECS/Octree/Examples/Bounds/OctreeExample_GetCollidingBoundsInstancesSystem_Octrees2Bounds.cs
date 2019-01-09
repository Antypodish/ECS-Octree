using Unity.Collections ;
using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine;

namespace ECS.Octree
{


    public class OctreeExample_GetCollidingBoundsInstancesBarrier_Octrees2Bounds : BarrierSystem {} ;
 

    class OctreeExample_GetCollidingBoundsInstancesSystem_Octrees2Bounds : JobComponentSystem
    {

        [Inject] private OctreeExample_GetCollidingBoundsInstancesBarrier_Octrees2Bounds barrier ;
             
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test octrees
            // Many octrees, to bounds pair
            // Where each octree has one bounds entity target.
            // Results return number of colliding instance
            // index to list of the colliding instances IDs,
            // and distance to the nearest instance.

            // Toggle manually only one example systems at the time
            if ( true ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Bounds Instances System" ) ;


            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            
            
            // Many octrees, to single, or many bounds
            // Where each octree has one bounds entity target.

            // ***** Example bounds Components For Collision Checks ***** //

            // Test bounds entity 
            // for each octree
            Entity boundsEntity = EntityManager.CreateEntity () ;
                   
            EntityManager.AddComponentData ( boundsEntity, new IsActiveTag () ) ; 
            EntityManager.AddComponentData ( boundsEntity, new BoundsData ()            
            {
                bounds = new Bounds () { center = float3.zero, size = new float3 ( 5, 5, 5 ) }
            } ) ; 


            // ***** Initialize Octree ***** //

            int i_octreesCount = 1000 ;

            NativeArray <Entity> a_entities = new NativeArray<Entity> ( i_octreesCount, Allocator.Temp ) ;

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;
                //Entity newOctreeEntity = a_entities [i_octreeEntityIndex] ;
                AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            
                EntityManager.AddComponent ( newOctreeEntity, typeof ( GetCollidingBoundsInstancesTag ) ) ;
                
                EntityManager.AddComponentData ( newOctreeEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                EntityManager.AddBuffer <CollisionInstancesBufferElement> ( newOctreeEntity ) ;

                
                // Assign target bounds entity, to octree entity
                Entity octreeEntity = newOctreeEntity ;    
                
                // Check bounds collision with octree and return colliding instances.
                EntityManager.AddComponentData ( octreeEntity, new BoundsEntityPair4CollisionData () 
                {
                    bounds2CheckEntity = boundsEntity
                } ) ;




                
                // ***** Example Components To Add / Remove Instance ***** //
            
                // Request to add 100 instances
                // User is responsible to ensure, that instances IDs are unique in the octrtree.
                EntityManager.AddBuffer <AddInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.         

                BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;
                DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [newOctreeEntity] ;  

                for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
                {  

                    int x = i_instanceID % 10 ;
                    int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;

     //               Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

                    Bounds bounds = new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ;
                
                    AddInstanceBufferElement addInstanceBuffer = new AddInstanceBufferElement () 
                    {
                        i_instanceID = i_instanceID,
                        instanceBounds = bounds
                    };

                    a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
                }


                

                // Request to remove 53 instances.
                // User is responsible to ensure, that requested instance ID to delete exists in the octree.            
                EntityManager.AddBuffer <RemoveInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were removed, tag will be deleted.

                BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [newOctreeEntity] ;  
            
                for ( int i_instanceID = 0; i_instanceID < 53; i_instanceID ++ )
                {            
                    int x = i_instanceID % 10 ;
                    int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;
     //               Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;
                                         
                    RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                    {
                        i_instanceID = i_instanceID,                    
                    };
                
                    a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;

                }

            } // for


        }

    }

}


