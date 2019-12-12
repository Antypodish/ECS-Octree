using Unity.Mathematics ;
using Unity.Transforms ;
using Unity.Rendering ;
using Unity.Entities ;
using Unity;
using UnityEngine;
using Unity.Jobs ;

namespace Antypodish.ECS.Octree
{

    class BlocksArchetypes : JobComponentSystem
    {
        
        static public EntityArchetype blockArchetype ;


        protected override void OnCreate ( )
        {

            blockArchetype = EntityManager.CreateArchetype 
            (
                typeof ( AddInstanceTag ),
                typeof ( MeshTypeData ),
                typeof ( RenderMesh ),
                typeof ( Translation ),
                typeof ( Rotation ),
                typeof ( NonUniformScale )              
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            return inputDeps ;
        }

    }

}
