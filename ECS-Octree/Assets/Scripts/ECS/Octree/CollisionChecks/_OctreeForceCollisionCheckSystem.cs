using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Octree
{
    
    public struct ForceCollisionCheckTag : IComponentData { }

    
    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]   
    class ForceCollisionCheckSystem : JobComponentSystem
    {


        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {

            group = GetComponentGroup ( 
                typeof ( ForceCollisionCheckTag )
            ) ;


           // EntityManager.CreateEntity ( typeof ( ForceCollisionCheckTag ) ) ;

            base.OnCreateManager ( );
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
