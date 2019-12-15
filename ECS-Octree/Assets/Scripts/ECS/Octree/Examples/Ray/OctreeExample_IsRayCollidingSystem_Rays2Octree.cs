using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace Antypodish.ECS.Octree.Examples
{
    
    [DisableAutoCreation]
    class OctreeExample_IsRayCollidingSystem_Rays2Octrees : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;
        
        protected override void OnCreate ( )
        {

            // Test rays
            // Many rays, to many octrees
            // Where each ray has one entity target.
            // Results return, weather collision with an instance occured.


            // Toggle manually only one example systems at the time
            // if ( !( OctreeExample_Selector.selector == Selector.IsRayCollidingSystem_Rays2Octree ) ) return ; // Early exit
            
            Debug.Log ( "Start Test Is Ray Colliding Octree System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            
            Entity newOctreeEntity = EntityManager.CreateEntity ( AddNewOctreeSystem.octreeArchetype ) ;

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;
            
            AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 4, float3.zero - new float3 ( 1, 1, 1 ) * 2, 2, 1.01f ) ; // ok // Minimum node size of 2 -> up to 8 instances per node.

            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 1, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1.01f ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 2, float3.zero - new float3 ( 1, 1, 1 ), 1, 1.01f ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 4, float3.zero - new float3 ( 1, 1, 1 ) * 2, 1, 1.01f ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 4, 1, 1.01f ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 16, float3.zero - new float3 ( 1, 1, 1 ) * 8, 1, 1.01f ) ; // ok // Minimum node size of 1 -> up to 1 instances per node.
            // AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero - new float3 ( 1, 1, 1 ) * 0.5f, 1, 1.01f ) ; // Faulty
            
            // EntityManager.AddComponent ( newOctreeEntity, typeof ( IsRayCollidingTag ) ) ;



            // Assign target ray entity, to octree entity
            Entity octreeEntity = newOctreeEntity ;    


            // ***** Example Components To Add / Remove Instance ***** //
            
            // Example of adding and removing some instanceses, hence entity blocks.


            // Add  

            // RenderMeshTypesData renderMeshTypes = EntityManager.GetComponentData <RenderMeshTypesData> ( Bootstrap.renderMeshTypesEntity ) ;
            // Bootstrap.EntitiesPrefabsData entitiesPrefabs = EntityManager.GetComponentData <Bootstrap.EntitiesPrefabsData> ( Bootstrap.entitiesPrefabsEntity ) ;

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




            // ***** Example Ray Components For Collision Checks ***** //

            // Create test rays
            // Many rays, to many octrees
            // Where each ray has one octree entity target.
            for ( int i = 0; i < 1000; i ++ ) 
            {
                Entity testEntity = ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                

                ecb.AddComponent ( testEntity, new IsActiveTag () ) ;                 
                ecb.AddComponent ( testEntity, new IsRayCollidingTag () ) ;
                ecb.AddComponent ( testEntity, new RayData () ) ; 
                ecb.AddComponent ( testEntity, new RayMaxDistanceData ()
                {
                    f = 100f
                } ) ; 
                // Check bounds collision with octree and return colliding instances.
                ecb.AddComponent ( testEntity, new OctreeEntityPair4CollisionData () 
                {
                    octree2CheckEntity = newOctreeEntity
                } ) ;
                ecb.AddComponent ( testEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                // ecb.AddBuffer <CollisionInstancesBufferElement> () ; // Not required in this system
            } // for
                            
        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            return inputDeps ;
        }
        
    }
}


