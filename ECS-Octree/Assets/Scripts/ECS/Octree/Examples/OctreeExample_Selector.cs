using UnityEngine ;

namespace Antypodish.ECS.Octree.Examples
{
    
    class OctreeExample_Selector
    { 

        // Rendering bottleneck (see RenderMeshSystemV2)
        // Be careful, when rendering multiple octrees. Consider disabling culling or implement own rendering.
                
        // Edit this manually and rebuild project, to select relevant example.
        // static public Selector selector = Selector.IsRayCollidingSystem_Rays2Octree ; // Ok
        // static public Selector selector = Selector.IsRayCollidingSystem_Octrees2Ray ; // Ok 
        // static public Selector selector = Selector.IsBoundsCollidingSystem_Octrees2Bounds ; // Ok     
        // static public Selector selector = Selector.IsBoundsCollidingSystem_Bounds2Octrees ; // Ok
        static public Selector selector = Selector.GetCollidingRayInstancesSystem_Rays2Octree ; // Ok // Required for block highlighting.
        // static public Selector selector = Selector.GetCollidingRayInstancesSystem_Octrees2Ray ; // Ok
        // static public Selector selector = Selector.GetCollidingBoundsInstancesSystem_Octrees2Bounds ; // Ok // Rendering bottleneck (see RenderMeshSystemV2)
        // static public Selector selector = Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree ; // Ok
                 
        /// <summary>
        /// Example of x octrees instances / entities to added.
        /// </summary>
        static public int i_generateInstanceInOctreeCount = 5000 ; // = 1000 ;
        /// <summary>
        /// Example of x octrees instances / entities to deleted.
        /// </summary>
        static public int i_deleteInstanceInOctreeCount = 0 ; // = 5 ; // = 750 ;

        /// <summary>        
        /// Creates x octrees with same amount of instances.
        /// </summary>
        static public int i_octreesCount = 1 ;
        static public int i_raysCount    = 1000 ;
        static public int i_boundsCount  = 1 ;

        /*
        public void Start ( )
        {
            Debug.LogError ( "ZZZ" ) ;

            selector = setSelector ;
            i_generateInstanceInOctreeCount = setGenerateInstanceInOctreeCount ;
            i_deleteInstanceInOctreeCount = setDeleteInstanceInOctreeCount ;
        }
        */        

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
        IsBoundsCollidingSystem_Bounds2Octrees = 6,

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
