using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace Antypodish.ECS.Octree
{

    class OctreeExamples_SystemsExecutor : JobComponentSystem
    {
                
        // EndInitializationEntityCommandBufferSystem eiecb ;

        static public bool canInitialize = false ;

        protected override void OnCreate ( )
        {
            
        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            if ( canInitialize )
            {
                     
                canInitialize = false ;

                Debug.LogWarning ( "Example selector: " + Examples.OctreeExample_Selector.selector.ToString () + "(" + (int) Examples.OctreeExample_Selector.selector + ")" ) ;

                switch ( Examples.OctreeExample_Selector.selector )
                {
                    case Examples.Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree :
                    
                        var octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree = World.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree> () ;
                        octreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree.Update () ;
                        break ;

                    case Examples.Selector.GetCollidingBoundsInstancesSystem_Octrees2Bounds :
                    
                        var octreeExample_GetCollidingBoundsInstancesSystem_Octrees2Bounds = World.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingBoundsInstancesSystem_Octrees2Bounds> () ;
                        octreeExample_GetCollidingBoundsInstancesSystem_Octrees2Bounds.Update () ;
                        break ;

                    case Examples.Selector.GetCollidingRayInstancesSystem_Octrees2Ray :
                    
                        var octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray = World.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingRayInstancesSystem_Octrees2Ray> () ;
                        octreeExample_GetCollidingRayInstancesSystem_Octrees2Ray.Update () ;
                        break ;

                    case Examples.Selector.GetCollidingRayInstancesSystem_Rays2Octree :
                    
                        var octreeExample_GetCollidingRayInstancesSystem_Rays2Octree = World.GetOrCreateSystem <Octree.Examples.OctreeExample_GetCollidingRayInstancesSystem_Rays2Octree> () ;
                        octreeExample_GetCollidingRayInstancesSystem_Rays2Octree.Update () ;
                        break ;

                    case Examples.Selector.IsBoundsCollidingSystem_Bounds2Octrees :
                    
                        var octreeExample_IsBoundsCollidingSystem_Bounds2Octrees = World.GetOrCreateSystem <Octree.Examples.OctreeExample_IsBoundsCollidingSystem_Bounds2Octrees> () ;
                        octreeExample_IsBoundsCollidingSystem_Bounds2Octrees.Update () ;
                        break ;

                    case Examples.Selector.IsBoundsCollidingSystem_Octrees2Bounds :
                    
                        var octreeExample_IsBoundsCollidingSystem_Octrees2Bounds = World.GetOrCreateSystem <Octree.Examples.OctreeExample_IsBoundsCollidingSystem_Octrees2Bounds> () ;
                        octreeExample_IsBoundsCollidingSystem_Octrees2Bounds.Update () ;
                        break ;

                    case Examples.Selector.IsRayCollidingSystem_Octrees2Ray :
                    
                        var octreeExample_IsRayCollidingSystem_Octrees2Ray = World.GetOrCreateSystem <Octree.Examples.OctreeExample_IsRayCollidingSystem_Octrees2Ray> () ;
                        octreeExample_IsRayCollidingSystem_Octrees2Ray.Update () ;
                        break ;

                    case Examples.Selector.IsRayCollidingSystem_Rays2Octree :
                    
                        var octreeExample_IsRayCollidingSystem_Rays2Octrees = World.GetOrCreateSystem <Octree.Examples.OctreeExample_IsRayCollidingSystem_Rays2Octrees> () ;
                        octreeExample_IsRayCollidingSystem_Rays2Octrees.Update () ;
                        break ;

                    default :
                        break ;
                }

            }
            return inputDeps ;
        }

    }
}


