using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using UnityEngine ;


namespace ECS.Octree.Examples
{


    public class OctreeExample_GetCollidingBoundsInstancesBarrier_Bounds2Octree : BarrierSystem { }
  

    class OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree : JobComponentSystem
    {

        [Inject] private OctreeExample_GetCollidingBoundsInstancesBarrier_Bounds2Octree barrier ;
              
        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );

            

            // Test bounds
            // Many bounds, to octree pair
            // Where each bounds entty has one octree entity target.


            // Toggle manually only one example systems at the time
            if ( !( ExampleSelector.selector == Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Bounds Instances System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1 ) ;




            // Assign target bounds entity, to octree entity
            Entity octreeEntity = newOctreeEntity ;    



            // ***** Example Components To Add / Remove Instance ***** //
            
            // Example of adding and removing some instanceses, hence entity blocks.


            // Add

            int i_instances2AddCount = ExampleSelector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 1000
            NativeArray <Entity> a_instanceEntities = Common._CreateInstencesArray ( EntityManager, i_instances2AddCount ) ;
                
            // Request to add n instances.
            // User is responsible to ensure, that instances IDs are unique in the octrtree.
            EntityManager.AddBuffer <AddInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.        
            BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;

            Common._RequesAddInstances ( ecb, octreeEntity, addInstanceBufferElement, ref a_instanceEntities, i_instances2AddCount ) ;



            // Remove
                
            EntityManager.AddBuffer <RemoveInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were removed, component will be deleted.
            BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                
            // Request to remove some instances
            // Se inside method, for details
            int i_instances2RemoveCount = ExampleSelector.i_deleteInstanceInOctreeCount ; // Example of x octrees instances / entities to delete. // 53
            Common._RequestRemoveInstances ( ecb, octreeEntity, removeInstanceBufferElement, ref a_instanceEntities, i_instances2RemoveCount ) ;
                
                
            // Ensure example array is disposed.
            a_instanceEntities.Dispose () ;





            // ***** Example Bounds Components For Collision Checks ***** //
                        
            
            Debug.Log ( "Octree: create dummy boundary box, to test for collision." ) ;
            float3 f3_blockCenter = new float3 ( 10, 2, 10 ) ;
            // Only test
            Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, EntityManager.CreateEntity ( ), f3_blockCenter, new float3 ( 1, 1, 1 ) * 5 ) ;

            // Create test bounds
            // Many bounds, to single or many octrees
            // Where each bounds has one octree entity target.
            for ( int i = 0; i < 10; i ++ ) 
            {

                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new GetCollidingBoundsInstancesTag () ) ;                  
                // This may be overritten by, other system. Check corresponding collision check system.
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
                ecb.AddBuffer <CollisionInstancesBufferElement> () ;

            } // for
                
        }
        
    }
}


