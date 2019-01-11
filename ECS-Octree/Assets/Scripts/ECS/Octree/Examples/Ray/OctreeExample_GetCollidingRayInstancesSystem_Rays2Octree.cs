using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using UnityEngine ;


namespace ECS.Octree.Examples
{


    public class OctreeExample_GetCollidingRayInstancesBarrier_Rays2Octree : BarrierSystem { }
  

    class OctreeExample_GetCollidingRayInstancesSystem_Rays2Octre : JobComponentSystem
    {

        [Inject] private OctreeExample_GetCollidingRayInstancesBarrier_Rays2Octree barrier ;
              
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test rays
            // Many rays, to octree pair
            // Where each ray entty has one octree entity target.


            // Toggle manually only one example systems at the time
            if ( !( ExampleSelector.selector == Selector.GetCollidingRayInstancesSystem_Rays2Octree ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Ray Instances System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            
            // EntityManager.AddComponent ( newOctreeEntity, typeof ( GetCollidingRayInstancesTag ) ) ;



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
            int i_instances2RemoveCount = ExampleSelector.i_deleteInstanceInOctreeCount = 53 ; // Example of x octrees instances / entities to delete. // 53
            Common._RequestRemoveInstances ( ecb, octreeEntity, removeInstanceBufferElement, ref a_instanceEntities, i_instances2RemoveCount ) ;
                
                
            // Ensure example array is disposed.
            a_instanceEntities.Dispose () ;




            // ***** Example Ray Components For Collision Checks ***** //

            // Create test rays
            // Many rays, to single or many octrees
            // Where each ray has one octree entity target.
            for ( int i = 0; i < 1000; i ++ ) 
            {
                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new GetCollidingRayInstancesTag () ) ; 
                ecb.AddComponent ( new RayData () ) ; 
                ecb.AddComponent ( new RayMaxDistanceData ()
                {
                    f = 100f
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


