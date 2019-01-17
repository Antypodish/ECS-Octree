using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using UnityEngine ;


namespace ECS.Octree.Examples
{


    public class OctreeExample_IsRayCollidingBarrier_Octrees2Ray : BarrierSystem {} ;
 

    class OctreeExample_IsRayCollidingSystem_Octrees2Ray : JobComponentSystem
    {

        [Inject] private OctreeExample_IsRayCollidingBarrier_Octrees2Ray barrier ;
             
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test octrees
            // Many octrees, to ray pair
            // Where each octree has one ray entity target.
            // Results return, weather collision with an instance occured.

            // Toggle manually only one example systems at the time
            if ( !( ExampleSelector.selector == Selector.IsRayCollidingSystem_Octrees2Ray ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Is Ray Colliding Octree System" ) ;


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

            int i_octreesCount = ExampleSelector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 1000
            
            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

                AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1.01f ) ;
            
                EntityManager.AddComponent ( newOctreeEntity, typeof ( IsRayCollidingTag ) ) ;
                
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
            
                // Example of adding and removing some instanceses, hence entity blocks.


                // Add

                int i_instances2AddCount = 100 ;
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



