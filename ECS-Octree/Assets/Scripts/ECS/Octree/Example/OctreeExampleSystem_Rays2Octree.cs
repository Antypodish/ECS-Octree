using Unity.Collections ;
using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine;

namespace ECS.Octree
{


    public class OctreeExampleBarrier_Rays2Octree : BarrierSystem {} ;
 

    class OctreeExampleSystem_Rays2Octree : JobComponentSystem
    {

        [Inject] private OctreeExampleBarrier_Rays2Octree barrier ;
        
        // EntityArchetype octreeArchetype ;        
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test octrees
            // Many octrees, to single, or many rays
            // Where each octree has one ray entity target.

            // Toggle manually only one example systems at the time
            if ( false ) return ; // Early exit


            Debug.Log ( "Start Test Octree System" ) ;


            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            
            
            // Many octrees, to single, or many rays
            // Where each octree has one ray entity target.

            // Test ray entity 
            // for each octree
            Entity rayEntity = EntityManager.CreateEntity () ;
                   
            EntityManager.AddComponentData ( rayEntity, new IsActiveTag () ) ; 
            EntityManager.AddComponentData ( rayEntity, new RayData () ) ; 
            EntityManager.AddComponentData ( rayEntity, new RayMaxDistanceData ()
            {
                f = 100f
            } ) ; 


            int i_octreesCount = 1000 ;

            NativeArray <Entity> a_entities = new NativeArray<Entity> ( i_octreesCount, Allocator.Temp ) ;

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;
                //Entity newOctreeEntity = a_entities [i_octreeEntityIndex] ;
                AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            


                // ***** Instance Optional Components ***** //
            
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

                
                // Assign target ray entity, to octree entity
                Entity octreeEntity = newOctreeEntity ;    
                
                // Check bounds collision with octree and return colliding instances.
                EntityManager.AddComponentData ( octreeEntity, new RayEntityPair4CollisionData () 
                {
                    ray2CheckEntity = rayEntity
                } ) ;

                EntityManager.AddComponentData ( octreeEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                EntityManager.AddBuffer <CollisionInstancesBufferElement> ( octreeEntity ) ;



            

                // ***** Collision Detection Components, Check List ***** //

    // TODO: replace instance ID with entity?
                // ecb.AddComponent ( newOctreeEntity, new IsBoundsCollidingTag () ) ; // Check boundary collision with octree instances.
                // ecb.AddComponent ( newOctreeEntity, new IsRayCollidingTag () ) ; // Check ray collision with octree instances.
                // ecb.AddComponent ( newOctreeEntity, new GetCollidingBoundsInstancesTag () ) ; // Check bounds collision with octree and return colliding instances.
                // ecb.AddComponent ( newOctreeEntity, new GetCollidingRayInstancesTag () ) ; // Check bounds collision with octree and return colliding instances.

    // TODO: incomplete Get max bounds
                // ecb.AddComponent ( newOctreeEntity, new GetMaxBoundsTag () ) ;


            } // for


        }

    }
}


