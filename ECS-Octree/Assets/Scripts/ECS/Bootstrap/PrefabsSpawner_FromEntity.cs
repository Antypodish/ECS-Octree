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
        public Entity boundingBoxEntity ;
        public Entity prefab01Entity ;
    }

    public struct SpawnerMeshData
    {
        public RenderMesh defaultMesh ;
        public RenderMesh higlightMesh ;
        public RenderMesh boundingBoxMesh ;
        public RenderMesh prefab01Mesh ;
    }
    
    public enum MeshType
    {
        Highlight   = -1,
        Default     = 0,
        BoundingBox = 1,
        Prefab01    = 2
    }

    [RequiresEntityConversion]
    public class PrefabsSpawner_FromEntity : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        
        static public SpawnerEntityPrefabsData spawnerEntitiesPrefabs ;
        static public SpawnerMeshData spawnerMesh ;

        // static public bool isConverted = false ;
               
        public GameObject prefabDefault ;
        public GameObject prefabHiglight ;
        public GameObject boundingBox ;
        public GameObject prefabBlock01 ;

        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs ( List<GameObject> gameObjects )
        {                       
            gameObjects.Add ( prefabDefault ) ;  
            gameObjects.Add ( prefabHiglight ) ; 
            gameObjects.Add ( boundingBox ) ;   
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
                boundingBoxEntity = conversionSystem.GetPrimaryEntity ( boundingBox ),
                prefab01Entity = conversionSystem.GetPrimaryEntity ( prefabBlock01 )
            };

            SpawnerMeshData spawnerMeshData = new SpawnerMeshData ()
            {
                defaultMesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.defaultEntity ),
                higlightMesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.higlightEntity ),
                boundingBoxMesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.boundingBoxEntity ),
                prefab01Mesh = em.GetSharedComponentData <RenderMesh> ( spawnerEntitiesPrefabs.prefab01Entity )
            } ;


            _SetPrefabComponents ( em, spawnerEntitiesPrefabs.defaultEntity, MeshType.Default ) ;
            _SetPrefabComponents ( em, spawnerEntitiesPrefabs.higlightEntity, MeshType.Highlight ) ;
            _SetPrefabComponents ( em, spawnerEntitiesPrefabs.boundingBoxEntity, MeshType.BoundingBox ) ;
            _SetPrefabComponents ( em, spawnerEntitiesPrefabs.prefab01Entity, MeshType.Prefab01 ) ;
            

            em.AddComponentData ( spawnerEntity, spawnerEntitiesPrefabs );

                        
            var postBootstrapSystem = World.Active.GetOrCreateSystem <Octree.PostBootstrapSystem> () ;
            postBootstrapSystem.Update () ;
            
        }

        
        private void _SetPrefabComponents ( EntityManager em, Entity entity, MeshType meshTypes )
        {            
            em.AddComponentData ( entity, new MeshTypeData () { type = meshTypes } ) ;
            em.AddComponentData ( entity, new NonUniformScale () { Value = 1 } ) ;
            em.AddComponent <Prefab> ( entity ) ;
                        
        }
        
    }

}
