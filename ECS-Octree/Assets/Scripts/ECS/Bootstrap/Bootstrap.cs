// This script initializes the game with ECS
using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Transforms ;
using Unity.Rendering ;
using Unity.Entities ;

using UnityEngine ;
using UnityEngine.SceneManagement ;


namespace Antypodish.ECS
{
    
    public struct RenderMeshTypesData // : IComponentData
    {
        public RenderMesh highlight ;
        public RenderMesh prefab01 ;
        public RenderMesh defualt ;
    }


    public sealed class Bootstrap
    {
            
        static public RenderMeshTypesData renderMeshTypes ;
        
            
        static EntityManager em ; // = World.Active.EntityManager ;
        
        //static EntityArchetype renderMeshTypesArchetype ;

        // static public Entity renderMeshTypesEntity ;

        
        // static public EntitiesPrefabsData entitiesPrefabs ;
        
        static public Entity entitiesPrefabsEntity ;

        public struct EntitiesPrefabsData : IComponentData
        {
            public Entity blockEntity ;
        }

        

        // meshes prefabs
        // static public MeshInstanceRenderer blockPrefabDefault ;
        // static public MeshInstanceRenderer blockPrefab01 ;        
        // static public MeshInstanceRenderer highlightRenderer ;
        
        
        [RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.BeforeSceneLoad ) ]
        public static void Initialize ()
        {
            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            
            Debug.Log ( "Bootstrap Initialization" ) ;

            em = World.Active.EntityManager ;
            
            //renderMeshTypesArchetype = em.CreateArchetype
            //(
            //    typeof ( RenderMeshTypesData )
            //) ;

            // renderMeshTypesEntity = em.CreateEntity ( renderMeshTypesArchetype ) ;
            // em.AddComponent <RenderMeshTypesData> ( renderMeshTypesEntity ) ;
            // em.SetName ( renderMeshTypesEntity, "renderMeshTypes" ) ;

            entitiesPrefabsEntity = em.CreateEntity ( typeof ( EntitiesPrefabsData )  ) ;
            em.SetName ( entitiesPrefabsEntity, "entitiesPrefabs" ) ;

            // renderMeshTypes = new RenderMeshTypesData () ;
            // entitiesPrefabs = new EntitiesPrefabsData () ;
        }

        
        
        [RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.AfterSceneLoad ) ]
        public static void InitializeAfterSceneLoad ()
        {            
            _InitializeWithScene ();

            _NewGame () ;
        }

        private static void OnSceneLoaded ( Scene scene, LoadSceneMode arg1 )
        {
            _InitializeWithScene () ;
        }

        
        /// <summary>
        /// Trigger with relevant input
        /// </summary>
        public static void _NewGame ()
        {

        }

        public static void _InitializeWithScene ()
        {

            Debug.Log ("Bootstrap init inihibited.") ;
            
            
            /*
            // RenderMeshTypesData renderMeshTypes = new RenderMeshTypesData () ;

            renderMeshTypes.highlight = _GetRendererFromPrefab ( "ECS Prefabs/PrefabHighlight01" );

            renderMeshTypes.defualt   = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefabDefault" ) ; // OOP.Prefabs.Default
            renderMeshTypes.prefab01  = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefab01" ) ; // OOP.Prefabs.Prefab01    

            // em.SetComponentData <RenderMeshTypesData> ( renderMeshTypesEntity, renderMeshTypes ) ;

            EntitiesPrefabsData entitiesPrefabs = new EntitiesPrefabsData () ; 
            entitiesPrefabs.blockEntity = em.CreateEntity () ;
            
            em.SetComponentData <EntitiesPrefabsData> ( entitiesPrefabsEntity, entitiesPrefabs ) ;

            Entity blockEntity = entitiesPrefabs.blockEntity ;
            
            em.AddComponent <MeshTypeData> ( blockEntity ) ;

            // AddBlockData
            em.AddComponentData ( blockEntity, new Translation { Value = float3.zero } ) ; // Default unset.
            em.AddComponentData ( blockEntity, new Rotation { Value = quaternion.identity} ) ; // Default unset.
            em.AddComponentData ( blockEntity, new NonUniformScale { Value = float3.zero } ) ; // Default unset.

            RenderMesh renderMesh = new RenderMesh () ;
            em.AddSharedComponentData ( blockEntity, renderMesh ) ;

            // ecb.AddComponent ( newEntity, new AddBlockData { }
                        
            var octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree = World.Active.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree> () ;
            octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree.Update () ;
            
            var octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray = World.Active.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingRayInstancesSystem_Octrees2Ray> () ;
            octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray.Update () ;
            */
        }

        private static RenderMesh _GetRendererFromPrefab ( string s_goName )
        {
            GameObject prefab = GameObject.Find ( s_goName ) ;
            RenderMeshProxy renderMeshProxy = prefab.GetComponent <RenderMeshProxy> () ;
            RenderMesh renderMesh = new RenderMesh () ;
            /*
            renderMesh.castShadows = UnityEngine.Rendering.ShadowCastingMode.Off ;
            renderMesh.layer = 0 ;
            renderMesh.receiveShadows = false ;
            renderMesh.mesh = renderMeshProxy.Value.mesh ;
            renderMesh.material = renderMeshProxy.Value.material ;
            */

            renderMesh = renderMeshProxy.Value ;

            // var result = prefab.GetComponent <RenderMesh> ().Value ;
            Object.Destroy (prefab) ;
            return renderMesh ;
        }
        
        static public RenderMesh _SelectRenderMesh ( MeshType meshType )
        {
            switch ( meshType )
            {
                case MeshType.Highlight:
                    return PrefabsSpawner_FromEntity.spawnerMesh.higlightMesh ;
                    // return Bootstrap.renderMeshTypes.highlight ;
                    // break ;
                case MeshType.Prefab01:
                    return PrefabsSpawner_FromEntity.spawnerMesh.prefab01Mesh ;
                    // return Bootstrap.renderMeshTypes.prefab01 ;
                    // break ;
                default :
                    return PrefabsSpawner_FromEntity.spawnerMesh.defaultMesh ;
                    // return Bootstrap.renderMeshTypes.defualt ;
                    // break ;
            }
        }
    }
}
