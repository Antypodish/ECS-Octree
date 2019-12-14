using System.Collections.Generic ;
using Unity.Mathematics ;
using Unity.Transforms ;
using Unity.Rendering ;
using Unity.Entities ;

using UnityEngine ;

namespace Antypodish.ECS
{    

    public struct SpawnerEntityPrefabsData : IComponentData
    {
        public Entity defaultEntity ;
        public Entity higlightEntity ;
        public Entity prefab01Entity ;
    }

    public struct SpawnerMeshData
    {
        public RenderMesh defaultMesh ;
        public RenderMesh higlightMesh ;
        public RenderMesh prefab01Mesh ;
    }
    
    public enum MeshType
    {
        Highlight = -1,
        Default   = 0,
        Prefab01  = 1
    }

    [RequiresEntityConversion]
    public class PrefabsSpawner_FromEntity : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        static public SpawnerEntityPrefabsData spawnerEntitiesPrefabs ;
        static public SpawnerMeshData spawnerMesh ;

        // static public bool isConverted = false ;
               
        public GameObject prefabDefault ;
        public GameObject prefabHiglight ;
        public GameObject prefabBlock01 ;
        

        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs ( List<GameObject> gameObjects )
        {                       
            gameObjects.Add ( prefabDefault ) ;  
            gameObjects.Add ( prefabHiglight ) ; 
            gameObjects.Add ( prefabBlock01 ) ;          
        }

        public void Convert ( Entity spawnerEntity, EntityManager em, GameObjectConversionSystem conversionSystem )
        {

            spawnerEntitiesPrefabs = new SpawnerEntityPrefabsData
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                defaultEntity  = conversionSystem.GetPrimaryEntity ( prefabDefault ),
                higlightEntity = conversionSystem.GetPrimaryEntity ( prefabHiglight ),
                prefab01Entity = conversionSystem.GetPrimaryEntity ( prefabBlock01 ),
            };

            SpawnerMeshData spawnerMeshData = new SpawnerMeshData ()
            {
                defaultMesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.defaultEntity ),
                higlightMesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.higlightEntity ),
                prefab01Mesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.prefab01Entity )
            } ;

            em.AddComponentData ( spawnerEntitiesPrefabs.defaultEntity, new MeshTypeData () { type = MeshType.Default } ) ;
            em.AddComponentData ( spawnerEntitiesPrefabs.defaultEntity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponent <Prefab> ( spawnerEntitiesPrefabs.defaultEntity ) ;

            em.AddComponentData ( spawnerEntitiesPrefabs.higlightEntity, new MeshTypeData () { type = MeshType.Highlight } ) ;
            em.AddComponentData ( spawnerEntitiesPrefabs.higlightEntity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponent <Prefab> ( spawnerEntitiesPrefabs.higlightEntity ) ;

            em.AddComponentData ( spawnerEntitiesPrefabs.prefab01Entity, new MeshTypeData () { type = MeshType.Prefab01 } ) ;
            em.AddComponentData ( spawnerEntitiesPrefabs.prefab01Entity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponent <Prefab> ( spawnerEntitiesPrefabs.prefab01Entity ) ;


            em.AddComponentData ( spawnerEntity, spawnerEntitiesPrefabs );

            /*
            // This wont render yet, as prefab mesh is not generated yet. Must be created after Convert method.
            Entity testEntity = em.Instantiate ( spawnerEntitiesPrefabs.prefab01Entity ) ;

            RenderMesh renderer = Bootstrap._SelectRenderMesh ( MeshType.Prefab01 ) ;
            em.SetSharedComponentData ( testEntity, renderer ) ;
            
            
            em.AddComponentData ( spawnerEntitiesPrefabs.prefab01Entity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponentData ( testEntity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponentData ( testEntity, new LocalToWorld () { Value = float4x4.identity } ) ;
            em.AddComponentData ( testEntity, new WorldRenderBounds () { Value = new AABB () { Extents = 0.5f } } ) ;
            */

            /*
            var spawnerData = new SpawnerEntityPrefabsData
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                
                defaultEntity = conversionSystem.GetPrimaryEntity ( prefabDefault )                
                
                //CountX = CountX,
                //CountY = CountY
            } ;
            */
                        
            // _SetPrefabComponents ( em, spawnerData.prefabCellMeshEntity, MeshTypes.Cell ) ;
            
            // float4x4 f4x4 = float4x4.identity ;

            
            /*
            em.SetName ( spawnerData.defaultEntity, "Prefab_" + MeshType.Prefab01.ToString () ) ;
            em.AddComponent <Prefab> ( spawnerData.defaultEntity ) ;

            em.SetComponentData ( spawnerData.defaultEntity, new Translation () { Value = 0 } ) ;
            em.SetComponentData ( spawnerData.defaultEntity, new Rotation () { Value = quaternion.identity } ) ;
            // em.AddComponent ( spawnerData.prefabCellMeshEntity, typeof ( Rotation ) ) ;
            em.AddComponentData ( spawnerData.defaultEntity, new NonUniformScale () { Value = 1 }  ) ; 
            // em.AddComponent ( spawnerData.prefabCellMeshEntity, typeof ( Parent ) ) ;            
            // em.SetComponentData ( spawnerData.prefabCellMeshEntity, new LocalToParent () { Value = f4x4 } ) ;
            // em.SetComponentData ( spawnerData.prefabCellMeshEntity, new LocalToWorld () { Value = f4x4 } ) ;
            
           
            em.AddComponentData ( spawnerEntity, spawnerData ) ;

            isConverted = true ;
            */
            
            var postBootstrapSystem = World.Active.GetOrCreateSystem <Octree.PostBootstrapSystem> () ;
            postBootstrapSystem.Update () ;
            
        }

        /*
        private void _SetPrefabComponents ( EntityManager em, Entity entity, MeshTypes meshTypes )
        {            
//            Debug.Log ( "Set prefab components: " + entity ) ;
//            em.SetName ( entity, "Prefab" + meshTypes.ToString () + "Mesh_" + entity.Index ) ;
            em.AddComponent ( entity, typeof ( Translation ) ) ;
            em.AddComponent ( entity, typeof ( Rotation ) ) ;
            em.AddComponent ( entity, typeof ( NonUniformScale ) ) ; 

            // em.AddComponent ( entity, typeof ( ParentMeshEntityComponent ) ) ;
            // em.AddComponent ( entity, typeof ( ChildMeshLocalPositionComponent ) ) ;            
            // em.AddComponent ( entity, typeof ( MeshTypeComponent ) ) ;
            // em.SetComponentData ( entity, new MeshTypeComponent () { i_reference = (int) meshTypes } ) ;

            em.SetName ( entity, "Prefab_" + meshTypes.ToString () ) ;
                        
        }
        */
    }

}
