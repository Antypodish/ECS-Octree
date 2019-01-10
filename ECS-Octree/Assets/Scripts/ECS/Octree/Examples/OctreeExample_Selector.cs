using Unity.Collections ;
using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine ;


namespace ECS.Octree.Examples
{
    
    internal class ExampleSelector
    { 
        // Edit this manually and rebuild project, to select relevant example.
        static public Selector selector = Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree ;
    }

    /// <summary>
    /// Select example.
    /// Each octree, ray and bounds, are represented by own entity.
    /// Node can store instances. Intstance can represent ID, or entity, for which ID (optional entity index) must be ensured, that is unique in the tree.
    /// Check individual examples.cs files, to see more details, how to derive relevant octree, how many octrees is generated, and how many instances(or entities) in octree is added.
    /// Warrning, some exmaples may take moment to load, if they have many octrees to test. To speed up, either reduce number of octrees / rays / bounds to test, or disable debug, in relevant octree system.
    /// </summary>
    public enum Selector
    {
        none = 0,

        // **** Rayc(cast) base collisons checks, are testing, if bounds of instances (optional entities) inside octree nodes, are intersecting with target ray(s).

        /// <summary>
        /// Check if target ray intersects instances bounds, inside octree nodes.
        /// Optimised with multithreaded burst job system, when having many octrees, to test against one, or few rays.
        /// </summary>
        IsRayCollidingSystem_Octrees2Ray = 1,
        /// <summary>
        /// Check if target ray intersects instances bounds, inside octree nodes.
        /// Optimised with multithreaded burst job system, when having many rays, to test against one, or few octrees.
        /// </summary>
        IsRayCollidingSystem_Rays2Octree = 2,
        
        /// <summary>
        /// Check if target ray intersects instances bounds, inside octree nodes.
        /// Returns number of instance (optional entities), which are intersecting target ray(s).
        /// Optimised with multithreaded burst job system, when having many octrees, to test against one, or few rays.
        /// </summary>
        GetCollidingRayInstancesSystem_Octrees2Ray = 3,
        /// <summary>
        /// Check if target ray intersects instances bounds, inside octree nodes.
        /// Returns number of instance (optional entities), which are intersecting target ray(s).
        /// Optimised with multithreaded burst job system, when having many rays, to test against one, or few octrees.
        /// </summary>
        GetCollidingRayInstancesSystem_Rays2Octree = 4,


        // **** Bounds base collisons checks, are testing, if bounds of instances (optional entities) inside octree nodes, are intersecting with target bounds.

        /// <summary>
        /// Check if target bounds intersects instances bounds, inside octree nodes.
        /// Optimised with multithreaded burst job system, when having many octrees, to test against one, or few bounds.
        /// </summary>
        IsBoundsCollidingSystem_Octrees2Bounds = 5,
        /// <summary>
        /// Check if target bounds intersects instances bounds, inside octree nodes.
        /// Optimised with multithreaded burst job system, when having many bounds, to test against one, or few octrees.
        /// </summary>
        IsBoundsCollidingSystem_Bounds2Octree = 6,

        /// <summary>
        /// Check if target bounds intersects instances bounds, inside octree nodes.
        /// Returns number of instance (optional entities), which are intersecting target bounds.
        /// Optimised with multithreaded burst job system, when having many octrees, to test against one, or few bounds.
        /// </summary>
        GetCollidingBoundsInstancesSystem_Octrees2Bounds = 7,
        /// <summary>
        /// Check if target bounds intersects instances bounds, inside octree nodes.
        /// Returns number of instance (optional entities), which are intersecting target bounds.
        /// Optimised with multithreaded burst job system, when having many bounds, to test against one, or few octrees.
        /// </summary>
        GetCollidingBoundsInstancesSystem_Bounds2Octree = 8,
    }

}
