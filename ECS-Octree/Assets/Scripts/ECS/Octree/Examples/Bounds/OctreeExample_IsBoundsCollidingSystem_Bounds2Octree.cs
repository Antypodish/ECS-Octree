using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine;

namespace ECS.Octree
{

    class OctreeExample_IsBoundsCollidingBarrier_Bounds2Octrees : BarrierSystem { }

    class OctreeExample_IsBoundsCollidingSystem_Bounds2Octrees : JobComponentSystem
    {

        [Inject] private OctreeExample_IsBoundsCollidingBarrier_Bounds2Octrees barrier ;
        
        EntityArchetype octreeArchetype ;        
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test bounds
            // Many bounds, to many octrees
            // Where each bounds has one entity target.
            // Results return, weather collision with an instance occured.


            // Toggle manually only one example systems at the time
            if ( true ) return ; // Early exit


            Debug.Log ( "Start Test Is Bounds Colliding Octree System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            
            EntityManager.AddComponent ( newOctreeEntity, typeof ( IsBoundsCollidingTag ) ) ;



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

                Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

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
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;
                                         
                RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                {
                    i_instanceID = i_instanceID,                    
                };
                
                a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;

            }




            // ***** Example Bounds Components For Collision Checks ***** //

            // Create test bounds
            // Many bounds, to many octrees
            // Where each bounds has one entity target.
            for ( int i = 0; i < 10; i ++ ) 
            {
                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                     
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new IsBoundsCollidingTag () ) ; 
                ecb.AddComponent ( new BoundsData ()
                {
                    bounds = new Bounds () { center = float3.zero, size = new float3 ( 5, 5, 5 ) }
                } ) ; 
                // Check bounds collision with octree and return colliding instances.
                ecb.AddComponent ( new OctreeEntityPair4CollisionData () 
                {
                    octree2CheckEntity = newOctreeEntity
                } ) ;
                ecb.AddComponent ( new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                // ecb.AddBuffer <CollisionInstancesBufferElement> () ; // Not required in this system
            } // for
                            
        }
        
    }
}


