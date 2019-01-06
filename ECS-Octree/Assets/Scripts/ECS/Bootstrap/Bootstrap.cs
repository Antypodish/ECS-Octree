/* This script initializes the game with ECS
 * Based on Pure TwoStick example, reduced to minimum
 */

using Unity.Entities ;
// using Unity.Mathematics ;
using UnityEngine ;
using UnityEngine.SceneManagement ;
using Unity.Rendering ;
using Unity.Transforms ;
using Unity.Mathematics ;

namespace ECS
{
    public sealed class Bootstrap
    {
        //public static TwoStickExampleSettings Settings;

        // archetypes
        
            
        // meshes prefabs
        static public MeshInstanceRenderer blockPrefabDefault ;
//        static public MeshInstanceRenderer blockPrefabHover ;
//        static public MeshInstanceRenderer blockPrefab01 ;
        /*
        static public MeshInstanceRenderer highlightRenderer ;
        static public MeshInstanceRenderer gravitySourceRenderer ;

        static public MeshInstanceRenderer octreeCenter01 ;
        static public MeshInstanceRenderer octreeCenter02 ;
        static public MeshInstanceRenderer octreeCenter03 ;
        static public MeshInstanceRenderer octreeCenter04 ;
        static public MeshInstanceRenderer octreeCenter05 ;
        static public MeshInstanceRenderer octreeCenter06 ;
        static public MeshInstanceRenderer octreeCenter07 ;

        static public MeshInstanceRenderer octreeNode ;
        */
        
        [ RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.BeforeSceneLoad ) ]
        public static void Initialize ()
        {
            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            
            Debug.Log ( "Bootstrap Initialization" ) ;
        }

        /// <summary>
        /// Trigger with relevant input
        /// </summary>
        public static void _NewGame ()
        {
            
            // AddBlockSystem._AddBlockRequest ( new float3 (-1,0,0), new float3 (), new Entity () ) ;
            //var player = Object.Instantiate(Settings.PlayerPrefab);
            //player.GetComponent<Position2D>().Value = new float2(0, 0);
            //player.GetComponent<Heading2D>().Value = new float2(0, 1);

            // Access the ECS entity manager
           // var entityManager = World.Active.GetOrCreateManager <EntityManager> () ;

                       
            // Create an entity based on the archetype. It will get default-constructed
            // defaults for all the component types we listed.
            // Entity entity = entityManager.CreateEntity ( objectArchetype ) ;
            //Entity entity = entityManager.CreateEntity ( typeof ( AddBlockTag ), typeof ( EntityComponent ) ) ;
            //entityManager.SetComponentData ( entity, new EntityComponent { entity = entity } ) ;

            /*
            // We can tweak a few components to make more sense like this.
            //entityManager.SetComponentData(entity, new Position2D {Value = new float2(0.0f, 0.0f)});
            //entityManager.SetComponentData(entity, new Heading2D  {Value = new float2(0.0f, 1.0f)});
            //entityManager.SetComponentData(entity, new Health { Value = Settings.playerInitialHealth });
            
            // set default position/rotation/scale matrix
            float4x4 f4x4 = float4x4.identity ;
            entityManager.SetComponentData ( entity, new TransformMatrix { Value = f4x4 } ) ;
            entityManager.SetComponentData ( entity, new Position { Value = new float3 (5,3,1) } ) ;

            // Finally we add a shared component which dictates the rendered mesh
            entityManager.AddSharedComponentData ( entity, playerRenderer ) ;
            
            */
        }
        
        
        [ RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.AfterSceneLoad ) ]
        public static void InitializeAfterSceneLoad ()
        {
            //var settingsGO = GameObject.Find("Settings");
            //if (settingsGO == null)
            //{
            //    SceneManager.sceneLoaded += OnSceneLoaded;
            //    return;
            //}
            
            _InitializeWithScene ();

            _NewGame () ;
        }

        private static void OnSceneLoaded ( Scene scene, LoadSceneMode arg1 )
        {
            _InitializeWithScene () ;
        }

        public static void _InitializeWithScene ()
        {
            Debug.Log ("Bootstrap ini") ;
            //var settingsGO = GameObject.Find("Settings");
            //Settings = settingsGO?.GetComponent<TwoStickExampleSettings>();
            //if (!Settings)
            //    return;

            //EnemySpawnSystem.SetupComponentData();
            
            //World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();

            
            blockPrefabDefault = _GetRendererFromPrefab ( "ECS Prefabs/BlockPrefabDefault" ) ; // OOP.Prefabs.Default
//            blockPrefabHover = _GetRendererFromPrefab ( "Prefabs/BlockPrefabHover" ) ; // OOP.Prefabs.Prefab01   
//            blockPrefab01 = _GetRendererFromPrefab ( "Prefabs/BlockPrefab01" ) ; // OOP.Prefabs.Prefab01    
            /*
            highlightRenderer = _GetRendererFromPrefab ( "PrefabHighlight01" );
            gravitySourceRenderer = _GetRendererFromPrefab ( "PrefabGravitySource" );

            octreeCenter01 = _GetRendererFromPrefab ( "PrefabOctreeCenter01" ) ;
            octreeCenter02 = _GetRendererFromPrefab ( "PrefabOctreeCenter02" ) ;
            octreeCenter03 = _GetRendererFromPrefab ( "PrefabOctreeCenter03" ) ;
            octreeCenter04 = _GetRendererFromPrefab ( "PrefabOctreeCenter04" ) ;
            octreeCenter05 = _GetRendererFromPrefab ( "PrefabOctreeCenter05" ) ;
            octreeCenter06 = _GetRendererFromPrefab ( "PrefabOctreeCenter06" ) ;
            octreeCenter07 = _GetRendererFromPrefab ( "PrefabOctreeCenter07" ) ;
            octreeNode = _GetRendererFromPrefab ( "Prefab02" );
            */
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
