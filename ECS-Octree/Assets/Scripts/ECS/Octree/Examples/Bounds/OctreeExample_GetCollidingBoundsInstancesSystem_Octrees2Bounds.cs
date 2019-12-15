using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace Antypodish.ECS.Octree.Examples
{

    [DisableAutoCreation]
    class OctreeExample_GetCollidingBoundsInstancesSystem_Octrees2Bounds : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        protected override void OnCreate ( )
        {

            // Test octrees
            // Many octrees, to bounds pair
            // Where each octree has one bounds entity target.
            // Results return number of colliding instance
            // index to list of the colliding instances IDs,
            // and distance to the nearest instance.

            // Toggle manually only one example systems at the time
            // if ( !( OctreeExample_Selector.selector == Selector.GetCollidingBoundsInstancesSystem_Octrees2Bounds ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Bounds Instances System" ) ;


            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;

            
            
            // Many octrees, to single, or many bounds
            // Where each octree has one bounds entity target.

            // ***** Example bounds Components For Collision Checks ***** //
            
            // RenderMeshTypesData renderMeshTypes = EntityManager.GetComponentData <RenderMeshTypesData> ( Bootstrap.renderMeshTypesEntity ) ;

            // Test bounds entity 
            // for each octree
            Entity boundsEntity = EntityManager.Instantiate ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.boundingBoxEntity ) ;
                   
            EntityManager.AddComponentData ( boundsEntity, new IsActiveTag () ) ;             
            // This may be overritten by, other system. Check corresponding collision check system.
            EntityManager.AddComponentData ( boundsEntity, new BoundsData ()            
            {
                bounds = new Bounds () { center = float3.zero, size = new float3 ( 5, 5, 5 ) }
            } ) ; 
            
            Debug.Log ( "Octree: create dummy (for visualization only) boundary box, to test for collision." ) ;
            float3 f3_blockCenter = new float3 ( 10, 2, 3 ) ;
            // Only test
            Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ref ecb, boundsEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 5, MeshType.BoundingBox ) ;
            // Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, boundsEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 5 ) ;

            // ***** Initialize Octree ***** //

            // Creates x octrees with same amount of instances.
            int i_octreesCount = OctreeExample_Selector.i_octreesCount ; // Example of x octrees.

            NativeArray <Entity> a_entities = new NativeArray<Entity> ( i_octreesCount, Allocator.Temp ) ;

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                Entity newOctreeEntity = EntityManager.CreateEntity ( AddNewOctreeSystem.octreeArchetype ) ;

                AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 4, float3.zero - new float3 ( 1, 1, 1 ) * 2, 2, 1 ) ; // ok // Minimum node size of 2 -> up to 8 instances per node.

                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 1, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1 ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 2, float3.zero - new float3 ( 1, 1, 1 ), 1, 1 ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 4, float3.zero - new float3 ( 1, 1, 1 ) * 2, 1, 1 ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 4, 1, 1 ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 16, float3.zero - new float3 ( 1, 1, 1 ) * 8, 1, 1 ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1 ) ; // Faulty
                // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero, 1, 1 ) ; // Faulty
            
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
            
                // Example of adding and removing some instanceses, hence entity blocks.


                // Add
                
                int i_instances2AddCount                      = OctreeExample_Selector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 100
                NativeArray <Entity> na_instanceEntities      = Common._CreateInstencesArray ( EntityManager, i_instances2AddCount ) ;
                
                // Request to add n instances.
                // User is responsible to ensure, that instances IDs are unique in the octrtree.
                ecb.AddComponent <AddInstanceTag> ( octreeEntity ) ; // Once system executed and instances were added, tag component will be deleted.  
                // EntityManager.AddBuffer <AddInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.        
                BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;

                Common._RequesAddInstances ( ref ecb, octreeEntity, addInstanceBufferElement, ref na_instanceEntities, i_instances2AddCount ) ;



                // Remove
                
                ecb.AddComponent <RemoveInstanceTag> ( octreeEntity ) ; // Once system executed and instances were removed, tag component will be deleted.
                // EntityManager.AddBuffer <RemoveInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were removed, component will be deleted.
                BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                
                // Request to remove some instances
                // Se inside method, for details
                int i_instances2RemoveCount = OctreeExample_Selector.i_deleteInstanceInOctreeCount ; // Example of x octrees instances / entities to delete. // 53
                Common._RequestRemoveInstances ( ref ecb, octreeEntity, removeInstanceBufferElement, ref na_instanceEntities, i_instances2RemoveCount ) ;
                
                
                // Ensure example array is disposed.
                na_instanceEntities.Dispose () ;

            } // for
            
        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
             return inputDeps ;
        }

    }

}


