using Unity.Entities;
using Unity.Jobs;


namespace Antypodish.ECS.Octree
{
    

    public struct ForceCollisionCheckTag : IComponentData { }

    
    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]   
    class ForceCollisionCheckSystem : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreate ( )
        {

            group = GetComponentGroup ( 
                typeof ( ForceCollisionCheckTag )
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            EntityArray a_entities = group.GetEntityArray () ;
            
            Entity entity = a_entities [0] ;

            EntityManager.RemoveComponent ( entity, typeof ( ForceCollisionCheckTag ) ) ;


            return base.OnUpdate ( inputDeps );
        }
    }

}
