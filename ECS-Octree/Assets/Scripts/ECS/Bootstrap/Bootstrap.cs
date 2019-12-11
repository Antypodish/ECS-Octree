// This script initializes the game with ECS

using UnityEngine ;
using UnityEngine.SceneManagement ;
using Unity.Rendering ;


namespace Antypodish.ECS
{

    public sealed class Bootstrap
    {
            
        // meshes prefabs
        static public MeshInstanceRenderer blockPrefabDefault ;
        static public MeshInstanceRenderer blockPrefab01 ;        
        static public MeshInstanceRenderer highlightRenderer ;
        
        
        [ RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.BeforeSceneLoad ) ]
        public static void Initialize ()
        {
            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            
            Debug.Log ( "Bootstrap Initialization" ) ;
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
            
            blockPrefabDefault = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefabDefault" ) ; // OOP.Prefabs.Default
            blockPrefab01 = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefab01" ) ; // OOP.Prefabs.Prefab01    
           
            highlightRenderer = _GetRendererFromPrefab ( "ECS Prefabs/PrefabHighlight01" );
            
        }

        private static MeshInstanceRenderer _GetRendererFromPrefab ( string s_goName )
        {
            GameObject prefab = GameObject.Find ( s_goName );
            var result = prefab.GetComponent <MeshInstanceRendererComponent> ().Value ;
            Object.Destroy (prefab) ;
            return result;
        }
    }
}
