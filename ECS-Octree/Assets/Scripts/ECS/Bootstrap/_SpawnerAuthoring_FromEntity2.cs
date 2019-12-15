/*
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Antypodish.ECS
{  

    // ReSharper disable once InconsistentNaming
    public struct Spawner_FromEntity : IComponentData
    {
        public int CountX;
        public int CountY;
        public Entity Prefab;
    }

    // ReSharper disable once InconsistentNaming
    // [RequiresEntityConversion]
    public class _SpawnerAuthoring_FromEntity2 : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject Prefab;
        public int CountX;
        public int CountY;


        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Prefab);
        }

        // Lets you convert the editor data representation to the entity optimal runtime representation
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new Spawner_FromEntity
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                Prefab = conversionSystem.GetPrimaryEntity(Prefab),
                CountX = CountX,
                CountY = CountY
            };
            dstManager.AddComponentData(entity, spawnerData);
        }
    }

}
*/