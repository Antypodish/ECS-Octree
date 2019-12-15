// using Unity.Entities ;
// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


namespace Antypodish.ECS.Octree
{

    public class OctreeExample_StartupMonoBehaviour : MonoBehaviour
    {
        
        [TextArea]
        public string readme = "See OctreeExampleCommon.cs for more details about options." +
            "Block highlighting example GetCollidingRayInstancesSystem_Rays2Octree" ;

        public bool manualInitialize             = false ;

        public Examples.Selector exampleSelector = Examples.Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree ;
        public int generateInstanceInOctreeCount = 100 ;
        public int deleteInstanceInOctreeCount   = 0 ;

        // [TextArea]
        public string readme1 = "Only applicable to _Octrees2 selection." ;
        public int octreesCount                  = 1 ;
        // [TextArea]
        public string readme2 = "Only applicable to _Bounds2Octree selection." ;
        public int boundsCount                   = 1 ;
        // [TextArea]
        public string readme3 = "Only applicable to _Rays2Octree selection." ;
        public int raysCount                     = 1000 ;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
            if ( manualInitialize )
            {
                Examples.OctreeExample_Selector.selector                        = exampleSelector ;
                Examples.OctreeExample_Selector.i_generateInstanceInOctreeCount = generateInstanceInOctreeCount ;
                Examples.OctreeExample_Selector.i_deleteInstanceInOctreeCount   = deleteInstanceInOctreeCount ;
                
                Examples.OctreeExample_Selector.i_octreesCount                  = octreesCount ;
                Examples.OctreeExample_Selector.i_boundsCount                   = boundsCount ;
                Examples.OctreeExample_Selector.i_raysCount                     = raysCount ;
                

                OctreeExamples_SystemsExecutor.canInitialize                    = manualInitialize ;
                manualInitialize                                                = false ;
            }
        }
    }

}