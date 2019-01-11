using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using UnityEngine ;


namespace ECS.Octree.Examples
{


    public class OctreeExample_IsBoundsCollidingBarrier_Octrees2Bounds : BarrierSystem {} ;
 

    class OctreeExample_IsBoundsCollidingSystem_Octrees2Bounds : JobComponentSystem
    {

        [Inject] private OctreeExample_IsBoundsCollidingBarrier_Octrees2Bounds barrier ;
             
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test octrees
            // Many octrees, to bounds pair
            // Where each octree has one bounds entity target.
            // Results return, weather collision with an instance occured.

            // Toggle manually only one example systems at the time
            if ( !( ExampleSelector.selector == Selector.IsBoundsCollidingSystem_Octrees2Bounds ) ) return ; // Early exit


            Debug.Log ( "Start Test Is Bounds Colliding Octree System" ) ;
            
            
            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;

            
            
            // Many octrees, to single, or many rays
            // Where each octree has one ray entity target.

            // ***** Example Ray Components For Collision Checks ***** //

            // Test bounds entity 
            // for each octree
            Entity boundsEntity = EntityManager.CreateEntity () ;
                   
            EntityManager.AddComponentData ( boundsEntity, new IsActiveTag () ) ; 

            // This may be overritten by, other system. Check corresponding collision check system.
            EntityManager.AddComponentData ( boundsEntity, new BoundsData ()
            {                
                bounds = new Bounds () { center = float3.zero, size = new float3 ( 1, 1, 1 ) * 5}
            } ) ; 
                        
            
            Debug.Log ( "Octree: create dummy boundary box, to test for collision." ) ;
            float3 f3_blockCenter = new float3 ( 10, 2, 10 ) ;
            // Only test
            Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, boundsEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 5 ) ;


            // ***** Initialize Octree ***** //

            int i_octreesCount = 1 ; // Example of x octrees.
            // int i_octreesCount = 100 ; // Example of x octrees.

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

                AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;
            
                EntityManager.AddComponent ( newOctreeEntity, typeof ( IsBoundsCollidingTag ) ) ;
                
                EntityManager.AddComponentData ( newOctreeEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.

                
                // Assign target bounds entity, to octree entity
                Entity octreeEntity = newOctreeEntity ;    
                
                // Check bounds collision with octree and return colliding instances.
                EntityManager.AddComponentData ( octreeEntity, new BoundsEntityPair4CollisionData () 
                {
                    bounds2CheckEntity = boundsEntity
                } ) ;



                
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

            } // for


        }

    }
}



