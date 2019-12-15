using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
               
    class CollisionChecksSystem : JobComponentSystem
    {
            
        EndInitializationEntityCommandBufferSystem eiecb ;

        protected override void OnCreate ( )
        {
                      
        }

        int i_frameIndex = 0 ;

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            // Debug.LogWarning ( "Coll." ) ;

            if ( i_frameIndex == 0 )
            {
                // var getCollidingRayInstancesSystem_Rays2Octree = World.GetOrCreateSystem <GetCollidingRayInstancesSystem_Rays2Octree> () ;
                // getCollidingRayInstancesSystem_Rays2Octree.Update () ;
            }

            i_frameIndex ++ ;

            if ( i_frameIndex > 100 ) i_frameIndex = 0 ;

            return inputDeps ;
        }

    }

}
