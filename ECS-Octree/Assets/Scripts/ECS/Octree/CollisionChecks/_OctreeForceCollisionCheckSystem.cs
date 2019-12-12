using Unity.Collections ;
using Unity.Entities;
using Unity.Jobs;


namespace Antypodish.ECS.Octree
{
    

    public struct ForceCollisionCheckTag : IComponentData { }

    
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]   
    class ForceCollisionCheckSystem : JobComponentSystem
    {

        EntityQuery group ;

        protected override void OnCreate ( )
        {

            group = GetEntityQuery ( 
                typeof ( ForceCollisionCheckTag )
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            NativeArray <Entity> na_entities = group.ToEntityArray ( Allocator.Temp ) ;
            
            Entity entity = na_entities [0] ;
            na_entities.Dispose () ;

            EntityManager.RemoveComponent ( entity, typeof ( ForceCollisionCheckTag ) ) ;

            return inputDeps ;
        }
    }

}
