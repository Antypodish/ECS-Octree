// This script initializes the game with ECS
using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Rendering ;
using Unity.Entities ;

using UnityEngine ;
using UnityEngine.SceneManagement ;


namespace Antypodish.ECS
{
    
    public enum MeshType
    {
        Highlight = -1,
        Default   = 0,
        Prefab01  = 1
            
    }

    public sealed class Bootstrap
    {
            
        static public RenderMeshTypes renderMeshTypes ;

        public struct RenderMeshTypes
        {
            public RenderMesh highlight ;
            public RenderMesh prefab01 ;
            public RenderMesh defualt ;
        }
        
        static public EntitiesPrefabs entitiesPrefabs ;

        public struct EntitiesPrefabs
        {
            public Entity blockEntity ;
        }

        // meshes prefabs
        // static public MeshInstanceRenderer blockPrefabDefault ;
        // static public MeshInstanceRenderer blockPrefab01 ;        
        // static public MeshInstanceRenderer highlightRenderer ;
        
        
        [ RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.BeforeSceneLoad ) ]
        public static void Initialize ()
        {
            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            
            Debug.Log ( "Bootstrap Initialization" ) ;

            renderMeshTypes = new RenderMeshTypes () ;
            entitiesPrefabs = new EntitiesPrefabs () ;
        }

        
        
        [ RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.AfterSceneLoad ) ]
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

            Debug.Log ("Bootstrap ini") ;
            
            renderMeshTypes.highlight = _GetRendererFromPrefab ( "ECS Prefabs/PrefabHighlight01" );

            renderMeshTypes.defualt   = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefabDefault" ) ; // OOP.Prefabs.Default
            renderMeshTypes.prefab01  = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefab01" ) ; // OOP.Prefabs.Prefab01    
           
            entitiesPrefabs.blockEntity = em.CreateEntity () ;
            AddBlockData
            ecb.AddComponent ( jobIndex, blockEntity, new Translation { Value = float3.zero } ) ; // Default unset.
            ecb.AddComponent ( jobIndex, blockEntity, new Rotation { Value = quaternion.identity} ) ; // Default unset.
            ecb.AddComponent ( jobIndex, blockEntity, new NonUniformScale { Value = float3.zero } ) ; // Default unset.
            ecb.AddSharedComponent ( jobIndex, blockEntity, renderer ) ;

            ecb.AddComponent ( newEntity, new AddBlockData { }
        }

        private static RenderMesh _GetRendererFromPrefab ( string s_goName )
        {
            GameObject prefab = GameObject.Find ( s_goName ) ;
            var result = prefab.GetComponent <RenderMesh> () ;
            // var result = prefab.GetComponent <RenderMesh> ().Value ;
            Object.Destroy (prefab) ;
            return result ;
        }
        
        static public RenderMesh _SelectRenderMesh ( MeshType meshType, [ReadOnly] ref Bootstrap.RenderMeshTypes renderMeshTypes )
        {
            switch ( meshType )
            {
                case MeshType.Highlight:
                    return renderMeshTypes.highlight ;
                    // break ;
                default :
                    return renderMeshTypes.defualt ;
                    // break ;
            }
        }
    }
}
