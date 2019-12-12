using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace Antypodish.ECS.Octree.Examples
{
           
    class OctreeExample_IsBoundsCollidingSystem_Bounds2Octrees : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;
        
        protected override void OnCreate ( )
        {

            // Test bounds
            // Many bounds, to many octrees
            // Where each bounds has one entity target.
            // Results return, weather collision with an instance occured.


            // Toggle manually only one example systems at the time
            if ( !( ExampleSelector.selector == Selector.IsBoundsCollidingSystem_Bounds2Octree ) ) return ; // Early exit


            Debug.Log ( "Start Test Is Bounds Colliding Octree System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1 ) ;
            
            EntityManager.AddComponent ( newOctreeEntity, typeof ( IsBoundsCollidingTag ) ) ;



            // Assign target bounds entity, to octree entity
            Entity octreeEntity = newOctreeEntity ;    



            // ***** Example Components To Add / Remove Instance ***** //
            
            // Example of adding and removing some instanceses, hence entity blocks.


            // Add

            int i_instances2AddCount = ExampleSelector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 100
            NativeArray <Entity> a_instanceEntities = Common._CreateInstencesArray ( EntityManager, i_instances2AddCount ) ;
                
            // Request to add n instances.
            // User is responsible to ensure, that instances IDs are unique in the octrtree.
            EntityManager.AddBuffer <AddInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.        
            BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;

            Common._RequesAddInstances ( ref ecb, octreeEntity, addInstanceBufferElement, ref a_instanceEntities, i_instances2AddCount, ref Bootstrap.entitiesPrefabs, ref Bootstrap.renderMeshTypes ) ;



            // Remove
                
            EntityManager.AddBuffer <RemoveInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were removed, component will be deleted.
            BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                
            // Request to remove some instances
            // Se inside method, for details
            int i_instances2RemoveCount = ExampleSelector.i_deleteInstanceInOctreeCount ; // Example of x octrees instances / entities to delete. // 53
            Common._RequestRemoveInstances ( ref ecb, octreeEntity, removeInstanceBufferElement, ref a_instanceEntities, i_instances2RemoveCount ) ;
                
                
            // Ensure example array is disposed.
            a_instanceEntities.Dispose () ;




            // ***** Example Bounds Components For Collision Checks ***** //
            
            Debug.Log ( "Octree: create dummy boundary box, to test for collision." ) ;
            float3 f3_blockCenter = new float3 ( 10, 2, 10 ) ;
            // Only test
            Entity boundsEntity = EntityManager.CreateEntity ( ) ;
            Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ref ecb, boundsEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 5, MeshType.Prefab01, ref Bootstrap.entitiesPrefabs, ref Bootstrap.renderMeshTypes ) ;
            // Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, EntityManager.CreateEntity (), f3_blockCenter, new float3 ( 1, 1, 1 ) * 5 ) ;

            // Create test bounds
            // Many bounds, to many octrees
            // Where each bounds has one octree entity target.
            for ( int i = 0; i < 10; i ++ ) 
            {
                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                     
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new IsBoundsCollidingTag () ) ; 
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
                // ecb.AddBuffer <CollisionInstancesBufferElement> () ; // Not required in this system
            } // for
                            
        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            return inputDeps ;
        }

    }
}


