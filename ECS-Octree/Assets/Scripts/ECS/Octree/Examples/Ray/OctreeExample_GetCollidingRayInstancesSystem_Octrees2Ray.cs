using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

using Unity.Rendering ;
namespace Antypodish.ECS.Octree.Examples
{ 
    
    [DisableAutoCreation]
    class OctreeExample_GetCollidingRayInstancesSystem_Octrees2Ray : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        protected override void OnCreate ( )
        {

            // Test octrees
            // Many octrees, to ray pair
            // Where each octree has one ray entity target.
            // Results return number of colliding instance
            // index to list of the colliding instances IDs,
            // and distance to the nearest instance.

            // Toggle manually only one example systems at the time
            // if ( !( OctreeExample_Selector.selector == Selector.GetCollidingRayInstancesSystem_Octrees2Ray ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Ray Instances System" ) ;

          
            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            // EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;

            
            
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
                f = 1000f
            } ) ; 




            // ***** Initialize Octree ***** //
            
            // Creates x octrees with same amount of instances.
            int i_octreesCount = 1 ; // Example of x octrees.
            // int i_octreesCount = 100 ; // Example of x octrees.

            NativeArray <Entity> a_entities = new NativeArray <Entity> ( i_octreesCount, Allocator.Temp ) ;

            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;

            for ( int i_octreeEntityIndex = 0; i_octreeEntityIndex < i_octreesCount; i_octreeEntityIndex ++ ) 
            {

                // ecb = barrier.CreateCommandBuffer () ;
                Entity newOctreeEntity = EntityManager.CreateEntity ( AddNewOctreeSystem.octreeArchetype ) ;
                
                AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1.01f ) ;
            
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
            
                // Example of adding and removing some instanceses, hence entity blocks.


                // Add

                int i_instances2AddCount                 = OctreeExample_Selector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 10000
                NativeArray <Entity> na_instanceEntities = Common._CreateInstencesArray ( EntityManager, i_instances2AddCount ) ;
                
                // Request to add n instances.
                // User is responsible to ensure, that instances IDs are unique in the octrtree.
                ecb.AddComponent <AddInstanceTag> ( octreeEntity ) ; // Once system executed and instances were added, tag component will be deleted.  
                BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;

                Common._RequesAddInstances ( ref ecb, octreeEntity, addInstanceBufferElement, ref na_instanceEntities, i_instances2AddCount ) ;


                
                // Remove
                
                ecb.AddComponent <RemoveInstanceTag> ( octreeEntity ) ; // Once system executed and instances were removed, tag component will be deleted.
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


