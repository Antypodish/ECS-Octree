using Unity.Rendering;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace Antypodish.ECS.Octree
{ 
    
    [DisableAutoCreation]
    class PostBootstrapSystem : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        protected override void OnCreate ( )
        {

            Debug.Log ( "Start Post Bootstrap System." ) ;

            
            PrefabsSpawner_FromEntity.spawnerMesh = new SpawnerMeshData ()
            {
                defaultMesh = EntityManager.GetSharedComponentData <RenderMesh> ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.defaultEntity ),
                higlightMesh = EntityManager.GetSharedComponentData <RenderMesh> ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.higlightEntity ),
                prefab01Mesh = EntityManager.GetSharedComponentData <RenderMesh> ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.prefab01Entity )
            } ;

            // Debug.Log ( PrefabsSpawner_FromEntity.spawnerMesh.prefab01Mesh.mesh ) ;

            
            var octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree = World.Active.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree> () ;
            octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree.Update () ;
            
            var octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray = World.Active.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingRayInstancesSystem_Octrees2Ray> () ;
            octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray.Update () ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            return inputDeps ;
        }

    }
}


