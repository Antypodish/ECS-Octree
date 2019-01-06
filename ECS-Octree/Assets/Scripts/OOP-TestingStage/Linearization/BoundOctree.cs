using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Test.OOP.Octree
{

    // A Dynamic, Loose Octree for storing any objects that can be described with AABB bounds
    // See also: PointOctree, where objects are stored as single points and some code can be simplified
    // Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes)
    //			and places objects into the appropriate nodes. This allows fast access to objects
    //			in an area of interest without having to check every object.
    // Dynamic: The octree grows or shrinks as required when objects as added or removed
    //			It also splits and merges nodes as appropriate. There is no maximum depth.
    //			Nodes have a constant - numObjectsAllowed - which sets the amount of items allowed in a node before it splits.
    // Loose:	The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
    //			This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
    //			A looseness value of 1.0 will make it a "normal" octree.

    // Code based on https://github.com/Unity-Technologies/UnityOctree
    // There can be found many similarities in terms of structure and comments.
    // However, this code has been converted into linearized form, 
    // to be more appropriate for into ECS conversion.

    // By Dobromil K Duda
    // 2019 January 04

    public class BoundOctree : MonoBehaviour
    {

        // The total amount of objects currently in the tree
        public int i_totalInstancesCountInTree ;
     
        // Root node of the octree
        // Initial node root index 0
        int i_rootNodeIndex ;
    
        // Should be a value between 1 and 2. A multiplier for the base size of a node.
	    // 1.0 is a "normal" octree, while values > 1 have overlap
        private float f_looseness ;

        // Size that the octree was on creation
	    float f_initialSize ;
    
	    // Minimum side length that a node can be - essentially an alternative to having a max depth
	    float f_minSize;




        private List <float> l_nodeBaseLength ;
        private List <float> l_nodeAdjLength ;
        private List <float> l_nodeMinSize ;
        private List <Vector3> l_nodeCenters ;            
        private List <Bounds> l_nodeBounds ;
    
        
        private List <int> l_nodeChildrenCount ;

        /// <summary>
        /// Group of 8 children per node
        /// </summary>
        private List <Bounds> l_nodeChildrenBounds ;
        /// <summary>
        /// Group of 8 children per node
        /// Reference to child node by index, if any
        /// </summary>
        private List <int> l_nodeChildrenNodesIndex ;    
    
        private List <int> l_nodeSpares ;


        const int numInstancesAllowed = 1 ;
        /// <summary>
        /// Count of instances per node.
        /// Each node instance stores index to appropriate instance lists.
        /// Same idex refers to the instance Boundry
        /// </summary>
        private List <int> l_nodeInstancesCount ;
        private int i_nodeSpareLastIndex ;
    
        /// <summary>
        /// Replacement for the object
        /// It references index to desired instances.
        /// Number of elements per node, should not exceed maximum allowed number of instances per node.
        /// If unused, value should be set to -1, as defualt.
        /// Index of instance is pointing at any desired instance list, where first three lists of instance bounds, spares and list of instance spares are mandatory.
        /// Further lists are optional. For example list of enitities, which are accessed with same index as bounds instance.
        /// Index from list should be returned, when testing for collision, new element is added, or removed.
        /// </summary>
        private List <int> l_nodeInstancesIndex ;

        /// <summary>
        /// Accessed by index from l_nodeInstancesIndex
        /// </summary>
        private List <Bounds> l_instancesBounds ;
        /// <summary>
        /// Store instance ID's which must be unique.
        /// </summary>
        private List <int> l_instancesID ;
        // You can add more lists with desired properties, wich maching list size of total instance bounds count (list size)
    
        /// <summary>
        /// Mandatory
        /// </summary>
        private List <int> l_instancesSpare ;
        /// <summary>
        /// Mandatory
        /// </summary>
        private int i_instancesSpareLastIndex ;

        const int numOfSpareInstances2Add = 100 ;

        /// <summary>
        /// Result of GetCollision, which outputs number instances, that boundery, or raycas has interact with.
        /// Should be read only. Is reset, every time GeCollision is executed.
        /// </summary>
        public List <int> l_collidingWith ;
        public int i_collisionRaycastDistance ;


        // Start is called before the first frame update
        void Start ()
        {

    	    /// Constructor for the bounds octree.
            // Is advised, to keep initial size, of minimum size multiply factor to power of 2
            // For minimum size of 1 initial size could be for example 1, 2, 4, 8, 16 ect
            // For minimum size of 3 initial size could be for example 3, 9, 27, 81 etc
            _Initialize ( 8, Vector3.zero, 1, 1 ) ;

            /*
            _OctreeAddInstance ( 1, new Bounds () { center = Vector3.one * 0, size = Vector3.one * 1 } ) ;
            _OctreeAddInstance ( 2, new Bounds () { center = Vector3.one * 0 + Vector3.forward, size = Vector3.one * 1 } ) ;
            _OctreeAddInstance ( 3, new Bounds () { center = Vector3.one * 0 + Vector3.forward * 2, size = Vector3.one * 1 } ) ;
            _OctreeAddInstance ( 4, new Bounds () { center = Vector3.one * 0 + Vector3.forward * 3, size = Vector3.one * 1 } ) ;
            _OctreeAddInstance ( 5, new Bounds () { center = Vector3.one * 0 + Vector3.forward * 4, size = Vector3.one * 1 } ) ;
            _OctreeAddInstance ( 6, new Bounds () { center = Vector3.one * 0 + Vector3.forward * 5, size = Vector3.one * 1 } ) ;
            */

            // _OctreeAddInstance ( 11, new Bounds () { center = Vector3.one * 15, size = Vector3.one * 5 } ) ;
     //       _OctreeAddInstance ( 12, new Bounds () { center = Vector3.one * 25, size = Vector3.one * 5 } ) ;
     //       _OctreeAddInstance ( 13, new Bounds () { center = Vector3.one * 5, size = Vector3.one * 2 } ) ;
     //       _OctreeAddInstance ( 14, new Bounds () { center = Vector3.one * 8, size = Vector3.one * 3 } ) ;
     //       _OctreeAddInstance ( 15, new Bounds () { center = Vector3.one * 1, size = Vector3.one * 1 } ) ;
     //       _OctreeAddInstance ( 16, new Bounds () { center = Vector3.one * -1, size = Vector3.one * 2 } ) ;

            // _OctreeAddInstance ( 13, new Bounds () { center = Vector3.one * 30, size = Vector3.one * 5 } ) ;
            //_OctreeAddInstance ( 14, new Bounds () { center = Vector3.one * 30, size = Vector3.one * 4 } ) ;

        
     //       _OctreeRemoveInstance ( 11 ) ;
    //        _OctreeRemoveInstance ( 12 ) ;
    //        _OctreeRemoveInstance ( 16 ) ;
    //        _OctreeRemoveInstance ( 14 ) ;
    //        _OctreeRemoveInstance ( 13 ) ;        
    //        _OctreeRemoveInstance ( 12 ) ;
          // _OctreeRemoveInstance ( 11 ) ;
        
        
            for ( int i = 0; i < 100; i ++ )
            {            
                int x = i % 10 ;
                int y = Mathf.FloorToInt ( i / 10 ) ;
                Debug.Log ( "Test instance spawn #" + i + " x: " + x + " y: " + y ) ;
                _OctreeAddInstance ( i, new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
            }
        
        
            for ( int i = 0; i < 95; i ++ )
            {            
                int x = i % 10 ;
                int y = Mathf.FloorToInt ( i / 10 ) ;
                Debug.Log ( "Test instance remove #" + i + " x: " + x + " y: " + y ) ;
                _OctreeRemoveInstance ( i ) ;
            }

        
            for ( int i = 0; i < 50; i ++ )
            {            
                int x = i % 10 ;
                int y = Mathf.FloorToInt ( i / 10 ) ;
                _OctreeAddInstance ( i, new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
            }
        

            Bounds checkBounds = new Bounds () { center = Vector3.one * 30, size = Vector3.one * 4 } ;
            bool isColliding   = _IsOctreeColliding ( checkBounds ) ;

            Debug.Log ( "Colliding: " + ( isColliding ? "T" : "F") ) ;

            checkBounds        = new Bounds () { center = Vector3.one * 0, size = Vector3.one * 30 } ;
            isColliding        = _IsOctreeColliding ( checkBounds ) ;

            Debug.Log ( "Colliding: " + ( isColliding ? "T" : "F") ) ;

        }

     
        // Update is called once per frame
        void Update ()
        {
            // Test

            Ray ray = new Ray () ;
            ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ; // .ViewportToWorldPoint ( Input.mousePosition ) ;
            Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 1000, Color.red ) ;
                
            float f_nearestDistance ;
            int i_nearestIndex ;

            _GetOctreeColliding ( ref l_collidingWith, ray, out i_nearestIndex, out f_nearestDistance, 1000 ) ;

            if ( l_collidingWith.Count > 0 )
            {
                string s_colliding = "" ;
                for ( int i = 0; i < l_collidingWith.Count; i ++ )
                {
                    int i_ID = l_collidingWith [i] ;
                    s_colliding += i_ID + "; " ;
                }
                    
                Debug.Log ( "Is Colliding with: " + s_colliding ) ;
                Debug.Log ( "Nearest: " + l_collidingWith [i_nearestIndex] + " at distance " + f_nearestDistance ) ;

            
            }
            // Debug.Log ( "Colliding: " + ( isColliding ? "T" : "F") ) ;
        }

    


	    /// <summary>
	    /// Constructor for the bounds octree.
        /// For minimum size of 1 initial size could be for example 1, 2, 4, 8, 16 ect
        /// For minimum size of 3 initial size could be for example 3, 9, 27, 81 etc
	    /// </summary>
	    /// <param name="f_initialSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
	    /// <param name="V3_initialPosition">Position of the centre of the initial node.</param>
	    /// <param name="f_minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
	    /// <param name="f_looseness">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
        public void _Initialize ( float f_initialSize, Vector3 V3_initialPosition, float f_minNodeSize, float f_looseness )
        {

            i_rootNodeIndex                = 0 ;

            this.f_initialSize             = f_initialSize ;
            this.f_minSize                 = f_minNodeSize ;
            this.f_looseness               = f_looseness ;

            l_nodeBaseLength               = new List <float> () ;
            l_nodeAdjLength                = new List <float> () ;
            // l_nodeMinSize               = new List <float> () ;
            // l_nodeLooseness             = new List <float> () ;
            l_nodeCenters                  = new List <Vector3> () ;
            l_nodeBounds                   = new List <Bounds> () ;
            // Size should be always 8 times size of the node elements.
            l_nodeChildrenBounds               = new List <Bounds> () ;

            l_nodeChildrenNodesIndex       = new List <int> () ;
            l_nodeChildrenCount            = new List <int> () ;
            l_nodeSpares                   = new List <int> () ;
            l_nodeInstancesCount           = new List <int> () ;

            l_nodeInstancesIndex           = new List <int> () ;
            l_instancesBounds              = new List <Bounds> () ;
            l_instancesID                  = new List <int> () ;
            l_instancesSpare               = new List <int> () ;
                

            l_collidingWith                = new List <int> () ;
            i_collisionRaycastDistance     = 1000 ; // default distance


            i_totalInstancesCountInTree    = 0 ;

            if ( f_minNodeSize > f_initialSize ) 
            {
			    Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + f_initialSize + " Adjusted to: " + f_minNodeSize );
			    f_minNodeSize = f_initialSize;
		    }

            f_looseness                  = Mathf.Clamp ( f_looseness, 1.0f, 2.0f );

            i_rootNodeIndex              = 0 ;  
            i_nodeSpareLastIndex -- ;   

            _CreateNewNode ( i_rootNodeIndex, f_initialSize, V3_initialPosition ) ;

            i_nodeSpareLastIndex -- ; 
                

            i_instancesSpareLastIndex    = 0 ;

            // Add some spares if needed.
            _AddInstanceSpares ( ) ;   

        }

    


    





        // ************** Instances *******************

        // #### PUBLIC METHODS ####
        // This method can be called by other mehtods, or systems.


        /// <summary>
	    /// Add an Instance.
	    /// </summary>
	    /// <param name="i_instanceID">External instance to add.</param>
	    /// <param name="instanceBounds">External instance 3D bounding box around the instance.</param>
	    public void _OctreeAddInstance ( int i_instanceID, Bounds instanceBounds ) 
        {

		    // Add object or expand the octree until it can be added
		    int i_count = 0; // Safety check against infinite/excessive growth

		    while ( !_AddNodeInstance ( i_rootNodeIndex, i_instanceID, instanceBounds ) ) 
            {

			    _GrowOctree ( instanceBounds.center - l_nodeCenters [i_rootNodeIndex] ) ;

			    if ( ++i_count > 20 ) 
                {
				    Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (i_count - 1) + ") attempts at growing the octree.");
				    return;
			    }
		    }

		    i_totalInstancesCountInTree ++ ;

	    }


        /// <summary>
	    /// Remove an instance. Makes the assumption that the instance only exists once in the tree.
	    /// </summary>
	    /// <param name="i_instanceID">External instance to remove.</param>
	    /// <returns>True if the object was removed successfully.</returns>
	    public bool _OctreeRemoveInstance ( int i_instanceID ) 
        {
		
            bool removed = _NodeRemoveInstance ( i_rootNodeIndex, i_instanceID );

		    // See if we can shrink the octree down now that we've removed the item
		    if ( removed ) 
            {            
			    i_totalInstancesCountInTree -- ;

                // Shrink if possible.
                i_rootNodeIndex = _ShrinkIfPossible ( i_rootNodeIndex, f_initialSize ) ;
			    // _OctreeShrink () ;
		    }

		    return removed ;
	    }


        /// <summary>
	    /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
	    /// </summary>
	    /// <param name="checkBounds">bounds to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    public bool _IsOctreeColliding ( Bounds checkBounds ) 
        {	
		    return _IsNodeColliding ( i_rootNodeIndex, ref checkBounds ) ;
	    }


        /// <summary>
	    /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
	    /// </summary>
	    /// <param name="checkRay">ray to check.</param>
	    /// <param name="f_maxDistance">distance to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    public bool _IsOctreeColliding ( Ray checkRay, float f_maxDistance ) 
        {		
		    return _IsNodeColliding ( i_rootNodeIndex, ref checkRay, f_maxDistance );
	    }


        /// <summary>
	    /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
	    /// <param name="l_collidingWith">list to store intersections.</param>
	    /// <param name="checkBounds">bounds to check.</param>
	    /// <returns>Objects that intersect with the specified bounds.</returns>
	    public void _GetOctreeColliding ( ref List <int> l_collidingWith, Bounds checkBounds ) 
        {        
            l_collidingWith.Clear () ; // Output
		    _GetNodeColliding ( i_rootNodeIndex, checkBounds, ref l_collidingWith ) ;
	    }


        /// <summary>
	    /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
	    /// <param name="l_collidingWith">list to store intersections.</param>
	    /// <param name="checkRay">ray to check.</param>
	    /// <param name="i_nearestIndex">Nerest collision index from the lits.</param>
	    /// <param name="f_nearestDistance">Nerest collision distance.</param>
	    /// <param name="f_maxDistance">distance to check.</param>
	    /// <returns>Objects that intersect with the specified ray.</returns>
	    public void _GetOctreeColliding ( ref List <int> l_collidingWith, Ray checkRay, out int i_nearestIndex, out float f_nearestDistance, float f_maxDistance = float.PositiveInfinity ) 
        {        
            l_collidingWith.Clear () ; // Output
            f_nearestDistance = float.PositiveInfinity ;
            i_nearestIndex = -1 ;
            _GetNodeColliding ( i_rootNodeIndex, checkRay, ref l_collidingWith, ref i_nearestIndex, ref f_nearestDistance, f_maxDistance ) ;        
	    }


        /// <summary>
        /// Get total octree bounds (boundary box).
        /// </summary>
        /// <returns></returns>
	    public Bounds _GetOctreeMaxBounds ()
	    {
		    // return _GetNodeBounds ( i_rootNodeIndex ) ;
            return l_nodeBounds [i_rootNodeIndex] ;
	    }



    





        // ************** Nodes *******************

        // #### PIVATE METHODS ####
        // This methods should not be accessed directly by other methods, or systems.
        
        
        /// <summary>
	    /// Node Constructor.
	    /// </summary>
        private void _CreateNewNode ( int i_nodeIndex, float f_baseLength, Vector3 V3_center )
        {

            // Expand storage if needed
            if ( i_nodeSpareLastIndex <= 9 )
            {

                int i_baseAddElementsCount = 10 ;
                   
                int i_initialSparesCount = l_nodeSpares.Count + i_baseAddElementsCount - 1 ;

                // Populate with defualt data
                for ( int i = 0; i < i_baseAddElementsCount; i ++ )
                {
                    // Add spares in reversed order, from higher index, to lower index.
                    l_nodeSpares.Add ( -1 ) ;
                    i_nodeSpareLastIndex ++ ;
                    l_nodeSpares [i_nodeSpareLastIndex] = i_initialSparesCount - i ;

                    l_nodeBaseLength.Add (-1) ;
                    l_nodeCenters.Add ( Vector3.zero ) ;
                    l_nodeAdjLength.Add (-1) ;
                    l_nodeBounds.Add ( new Bounds () ) ;

                    l_nodeChildrenCount.Add ( 0 ) ;
//                     l_nodeChildrenNodesIndex.Add ( -1 ) ;
                    l_nodeInstancesCount.Add ( 0 ) ;

                    // Add empty slotes, for assigning future instances.
                    for ( int j = 0; j < numInstancesAllowed; j ++ )
                    {
                        l_nodeInstancesIndex.Add ( -1 ) ;                    
                    }
                
                    for ( int j = 0; j < 8; j ++ )
                    {
                        l_nodeChildrenBounds.Add ( new Bounds () ) ;        
                        l_nodeChildrenNodesIndex.Add ( -1 ) ;
                    }
                }
            }


		    _SetValues ( i_nodeIndex, f_baseLength, V3_center ) ;
        
        }


        /// <summary>
	    /// Set values for this node. 
	    /// </summary>
	    /// <param name="f_baseLength">Length of this node, not taking looseness into account.</param>
	    /// <param name="f_minSize">Minimum size of nodes in this octree.</param>
	    /// <param name="f_looseness">Multiplier for baseLengthVal to get the actual size.</param>
	    /// <param name="V3_center">Centre position of this node.</param>
	    private void _SetValues ( int i_nodeIndex, float f_baseLength, Vector3 V3_center ) 
        {
        
    /*                
    GameObject go = GameObject.Find ( "Node " + i_nodeIndex.ToString () ) ;

    if ( go != null ) 
    {
        go.transform.position = V3_center ;
        go.transform.localScale = Vector3.one * f_baseLength ;
        go.name = "Node " + i_nodeIndex ;
                        
        Debug.Log ( "Node: Change scale #" + i_nodeIndex ) ;
    }
    else
    {
        GameObject newGameObject = GameObject.Instantiate ( GameObject.Find ( "TempNode" ), V3_center, Quaternion.identity ) ;
        newGameObject.transform.localScale = Vector3.one * f_baseLength ;
        newGameObject.name = "Node " + i_nodeIndex ;

        Debug.Log ( "Node: New game object #" + i_nodeIndex ) ;
    }
    */


		    l_nodeBaseLength [i_nodeIndex]  = f_baseLength;
		    l_nodeCenters [i_nodeIndex]     = V3_center ;
            float f_adjustLength            = f_looseness * f_baseLength ;
		    l_nodeAdjLength [i_nodeIndex]   = f_adjustLength ;

		    // Create the bounding box.
		    Vector3 size                    = new Vector3 ( f_adjustLength, f_adjustLength, f_adjustLength ) ;

		    l_nodeBounds [i_nodeIndex]      = new Bounds ( V3_center, size );

		    float f_quarter                 = f_baseLength / 4f ;
		    float f_childActualLength       = ( f_baseLength / 2) * f_looseness ;


		    Vector3 V3_childActualSize         = new Vector3 ( f_childActualLength, f_childActualLength, f_childActualLength ) ;

            int i_childrenIndexOffset       = i_nodeIndex * 8 ;


		    // Set 8 children for this node
		    l_nodeChildrenBounds [i_childrenIndexOffset]        = new Bounds ( V3_center + new Vector3 (-f_quarter, f_quarter, -f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 1]    = new Bounds ( V3_center + new Vector3 (f_quarter, f_quarter, -f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 2]    = new Bounds ( V3_center + new Vector3 (-f_quarter, f_quarter, f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 3]    = new Bounds ( V3_center + new Vector3 (f_quarter, f_quarter, f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 4]    = new Bounds ( V3_center + new Vector3 (-f_quarter, -f_quarter, -f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 5]    = new Bounds ( V3_center + new Vector3 (f_quarter, -f_quarter, -f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 6]    = new Bounds ( V3_center + new Vector3 (-f_quarter, -f_quarter, f_quarter), V3_childActualSize ) ;
		    l_nodeChildrenBounds [i_childrenIndexOffset + 7]    = new Bounds ( V3_center + new Vector3 (f_quarter, -f_quarter, f_quarter), V3_childActualSize ) ;

	    }


        /// <summary>
	    /// Grow the octree to fit in all objects.
	    /// </summary>
	    /// <param name="direction">Direction to grow.</param>
	    private void _GrowOctree ( Vector3 direction ) 
        {

		    int xDirection = direction.x >= 0 ? 1 : -1 ;
		    int yDirection = direction.y >= 0 ? 1 : -1 ;
		    int zDirection = direction.z >= 0 ? 1 : -1 ;

            int i_oldRootNodeIndex = i_rootNodeIndex ;
            float f_baseLength = l_nodeBaseLength [i_oldRootNodeIndex] ;
		    float f_half = f_baseLength / 2 ;
		    float f_newBaseLength = f_baseLength * 2 ;

		    Vector3 V3_newCenter = l_nodeCenters [i_oldRootNodeIndex] + new Vector3(xDirection * f_half, yDirection * f_half, zDirection * f_half);

		    // Create a new, bigger octree root node
                
		    if ( !_HasAnyInstances ( i_oldRootNodeIndex ) )
		    {
                _CreateNewNode ( i_rootNodeIndex, f_newBaseLength, V3_newCenter ) ;
            }
            else
            {
            
                i_rootNodeIndex = l_nodeSpares [i_nodeSpareLastIndex] ;
            
                i_nodeSpareLastIndex -- ;   
            
                _CreateNewNode ( i_rootNodeIndex, f_newBaseLength, V3_newCenter ) ;


			    // Create 7 new octree children to go with the old root as children of the new root
			    int i_rootPos = _GetRootPosIndex ( xDirection, yDirection, zDirection ) ;
			
                l_nodeChildrenCount [i_rootNodeIndex] = 8 ;
                int i_newRootNodeChildrenIndexOffset = i_rootNodeIndex * 8 ;

			    for (int i = 0; i < 8; i++)
			    {

                    int i_childIndexOffset = i_newRootNodeChildrenIndexOffset + i ;

				    if ( i == i_rootPos )
				    {
                        // Assign old root node as a child.
                        l_nodeChildrenNodesIndex [i_childIndexOffset]   = i_oldRootNodeIndex ;    
                        l_nodeChildrenBounds [i_childIndexOffset]           = l_instancesBounds [i_oldRootNodeIndex] ;
					
				    }
				    else
				    {
                        // Assign rest 7 children
					    xDirection                      = i % 2 == 0 ? -1 : 1;
					    yDirection                      = i > 3 ? -1 : 1;
					    zDirection                      = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;

                        int i_newNodeIndex              = l_nodeSpares [i_nodeSpareLastIndex] ; // Expected output 0 at initialization        
                        i_nodeSpareLastIndex -- ;  

                        Vector3 V3_childVector          = V3_newCenter + new Vector3 ( xDirection * f_half, yDirection * f_half, zDirection * f_half ) ;
                    
                        _CreateNewNode ( 
                            i_newNodeIndex,
                            f_newBaseLength,                                                   
                            V3_childVector
                        ) ; 
                    
                        l_nodeChildrenNodesIndex [i_childIndexOffset]       = i_newNodeIndex ; 

					    l_nodeChildrenBounds [i_childIndexOffset]               = new Bounds ( ) { 
                            center      = V3_childVector,
                            size        = Vector3.one * f_newBaseLength
                        } ;
				    }
			    } // for

		    }
	    }


	    /// <summary>
	    /// Shrink the octree if possible, else leave it the same.
	    /// </summary>
	    //void _OctreeShrink () 
        //{
		//    i_rootNodeIndex = _ShrinkIfPossible ( i_rootNodeIndex, f_initialSize ) ;
	    //}
    

	    /// <summary>
	    /// Used when growing the octree. Works out where the old root node would fit inside a new, larger root node.
	    /// </summary>
	    /// <param name="xDir">X direction of growth. 1 or -1.</param>
	    /// <param name="yDir">Y direction of growth. 1 or -1.</param>
	    /// <param name="zDir">Z direction of growth. 1 or -1.</param>
	    /// <returns>Octant where the root node should be.</returns>
	    static int _GetRootPosIndex ( int xDir, int yDir, int zDir ) 
        {
		    int result = xDir > 0 ? 1 : 0 ;

		    if (yDir < 0) result += 4 ;
		    if (zDir > 0) result += 2 ;

		    return result ;
	    }


	    /// <summary>
	    /// Add an object.
	    /// </summary>
        /// <param name="i_rootNodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance.</param>
	    /// <returns>True if the object fits entirely within this node.</returns>
	    private bool _AddNodeInstance ( int i_rootNodeIndex, int i_instanceID, Bounds instanceBounds ) 
        {

		    if ( !_Encapsulates ( l_nodeBounds [i_rootNodeIndex], instanceBounds ) ) return false ; // Early exit
			    
		    _NodeInstanceSubAdd ( i_rootNodeIndex, i_instanceID, instanceBounds ) ;

		    return true;
	    }


        /// <summary>
	    /// Remove an instace. Makes the assumption that the instance only exists once in the tree.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>
	    /// <returns>True if the object was removed successfully.</returns>
	    private bool _NodeRemoveInstance ( int i_nodeIndex, int i_instanceID ) 
        {

		    bool removed = false;

            int i_nodeInstanceCount = l_nodeInstancesCount [i_nodeIndex] ;
                
            int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

            if ( i_nodeInstanceCount > 0 )
            {

                // Try remove instance from this node
                for (int i = 0; i < numInstancesAllowed; i++) 
		        // for (int i = 0; i < l_nodeInstancesCount [i_nodeIndex]; i++) 
                {

                    int i_existingInstanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;

                    // If instance exists
                    if ( i_existingInstanceIndex >= 0 )
                    {
                 
                        if ( l_instancesID [i_existingInstanceIndex] == i_instanceID ) 			        
                        {   
                            removed = true ;
                            _PutBackSpareInstance ( i_existingInstanceIndex, i_nodeIndex ) ;
                            l_nodeInstancesCount [i_nodeIndex] -- ;
                            l_instancesID [i_existingInstanceIndex] = -1 ;
                    
                            
    Debug.LogWarning ( "Node: Remove #" + i_nodeIndex ) ;
    GameObject.Destroy ( GameObject.Find ( "Node " + i_nodeIndex.ToString () ) ) ;

				            break;
			            }
                
                    }           

		        } // for

            }

            int i_nodeChildrenCount = l_nodeChildrenCount [i_nodeIndex] ;
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

            // Try remove instance from this node children, if node don't have this instance
		    if ( !removed && i_nodeChildrenCount > 0 ) 
            {
			    for (int i = 0; i < 8; i++) 
                {
                    // Get children index of this node
                    int i_childNodeIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;

                    // Ignore negative index
                    if ( i_childNodeIndex >= 0 )
                    {
                        removed = _NodeRemoveInstance ( i_childNodeIndex, i_instanceID ) ;
				   
				        if ( removed ) break ;
                    }
			    }
		    }

		    if ( removed && i_nodeChildrenCount > 0 )
            {
			    // Check if we should merge nodes now that we've removed an item
			    if ( _ShouldMerge ( i_nodeIndex ) ) 
                {
				    _MergeNodes ( i_nodeIndex ) ;
			    }
		    }

		    return removed;
	    }


	    /// <summary>
	    /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    private bool _IsNodeColliding ( int i_nodeIndex, ref Bounds checkBounds ) 
        {
            Bounds nodeBounds = l_nodeBounds [i_nodeIndex] ;

		    // Are the input bounds at least partially in this node?
		    if ( !nodeBounds.Intersects ( checkBounds ) ) 
            {
			    return false;
		    }


            if ( l_nodeInstancesCount [i_nodeIndex] >= 0 ) 
            {            
                int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

		        // Check against any objects in this node
                for (int i = 0; i < numInstancesAllowed; i++) 
		        // for (int i = 0; i < l_nodeInstancesCount [i_nodeIndex]; i++) 
                {
            
                    // Get index of instance
                    int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                    
                        // Check if instance exists, and if has intersecting bounds.
			            if ( l_instancesBounds [i_instanceIndex].Intersects (checkBounds) ) 
                        {
				            return true;
			            }

                    }
            
		        }
            }

		    // Check children for collisions
            // Check if having children
		    if ( l_nodeChildrenCount [i_nodeIndex] > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {                
                    int i_nodeChildIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;

                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
				        if ( _IsNodeColliding ( i_nodeChildIndex, ref checkBounds ) ) 
                        {
					        return true ;
				        }

                    }
			    }
		    }

		    return false ;
	    }

        /// <summary>
	    /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check.</param>
	    /// <param name="maxDistance">Distance to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    private bool _IsNodeColliding ( int i_nodeIndex, ref Ray checkRay, float maxDistance = float.PositiveInfinity ) 
        {
		    // Is the input ray at least partially in this node?
		
            float distance ;

            Bounds nodeBounds = l_nodeBounds [i_nodeIndex] ;

		    if ( !nodeBounds.IntersectRay ( checkRay, out distance ) || distance > maxDistance ) 
            {
			    return false ;
		    }
        
            if ( l_nodeInstancesCount [i_nodeIndex] >= 0 ) 
            {            
                int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

		        // Check against any objects in this node
                for (int i = 0; i < numInstancesAllowed; i++) 
                {
            
                    // Get index of instance
                    int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                        nodeBounds = l_instancesBounds [i_instanceIndex] ;

			            if ( nodeBounds.IntersectRay (checkRay, out distance) && distance <= maxDistance ) 
                        {
				            return true;
			            }
                    }
            
		        }
            }

            // Check children for collisions
            // Check if having children
		    if ( l_nodeChildrenCount.Count > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {                
                    int i_nodeChildIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;
                    
                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
				        if ( _IsNodeColliding ( i_nodeChildIndex, ref checkRay, maxDistance ) ) 
                        {
					        return true ;
				        }
                    }
			    }
		    }

		    return false;
	    }

        
        /// <summary>
	    /// Returns an collection of objects, that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check. Passing by ref as it improves performance with structs.</param>
	    /// <param name="l_resultInstanceIDs">List result.</param>
	    private void _GetNodeColliding ( int i_nodeIndex, Bounds checkBounds, ref List <int> l_resultInstanceIDs ) 
        {
            Bounds nodeBounds = l_nodeBounds [i_nodeIndex] ;

            // Are the input bounds at least partially in this node?
		    if ( !nodeBounds.Intersects ( checkBounds ) ) 
            {
			    return ;
		    }
        

            if ( l_nodeInstancesCount [i_nodeIndex] >= 0 ) 
            {            
                int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

		        // Check against any objects in this node
                for (int i = 0; i < numInstancesAllowed; i++) 
                {
            
                    // Get index of instance
                    int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                        nodeBounds = l_instancesBounds [i_instanceIndex] ;

			            if ( nodeBounds.Intersects ( checkBounds ) ) 
                        {
				            l_resultInstanceIDs.Add ( l_instancesID [i_instanceIndex] ) ;
			            }
                    }
            
		        }
            }

            // Check children for collisions
            // Check if having children
		    if ( l_nodeChildrenCount [i_nodeIndex] > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {        
                    int i_nodeChildIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;
                    
                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
                        _GetNodeColliding ( i_nodeChildIndex, checkBounds, ref l_resultInstanceIDs ) ;
                    }
			    }
		    }

	    }


	    /// <summary>
	    /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check. Passing by ref as it improves performance with structs.</param>
	    /// <param name="l_resultInstanceIDs">List result.</param>
	    /// <param name="i_nearestIndex">Nerest collision index from the lits.</param>
	    /// <param name="f_nearestDistance">Nerest collision distance.</param>
	    /// <param name="maxDistance">Distance to check.</param>
	    /// <returns>Instances index, that intersect with the specified ray.</returns>
	    private void _GetNodeColliding ( int i_nodeIndex, Ray checkRay, ref List <int> l_resultInstanceIDs, ref int i_nearestIndex, ref float f_nearestDistance, float maxDistance = float.PositiveInfinity ) 
        {
		
            float f_distance;

            Bounds nodeBounds = l_nodeBounds [i_nodeIndex] ;

            // Is the input ray at least partially in this node?
		    if ( !nodeBounds.IntersectRay ( checkRay, out f_distance) || f_distance > maxDistance ) 
            {
			    return ;
		    }

            if ( l_nodeInstancesCount [i_nodeIndex] >= 0 ) 
            {            
                int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

		        // Check against any objects in this node
                for (int i = 0; i < numInstancesAllowed; i++) 
                {
            
                    // Get index of instance
                    int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                        nodeBounds = l_instancesBounds [i_instanceIndex] ;

			            if ( nodeBounds.IntersectRay ( checkRay, out f_distance) && f_distance <= maxDistance ) 
                        {
                            if ( f_distance < f_nearestDistance )
                            {
                                f_nearestDistance = f_distance ;
                                i_nearestIndex = l_resultInstanceIDs.Count ;
                            }

				            l_resultInstanceIDs.Add ( l_instancesID [i_instanceIndex] ) ;

			            }
                    }
            
		        }
            }


            // Check children for collisions
            // Check if having children
		    if ( l_nodeChildrenCount [i_nodeIndex] > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {        
                    int i_nodeChildIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;
                    
                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
                        _GetNodeColliding ( i_nodeChildIndex, checkRay, ref l_resultInstanceIDs, ref i_nearestIndex, ref f_nearestDistance, maxDistance ) ;

                    }
			    }
		    }

	    }


        /*
        /// <summary>
        /// Get Bounds.
        /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
        /// <returns>Bounds</returns>
	    private Bounds _GetNodeBounds ( int i_nodeIndex )
	    {
		    return l_nodeBounds [i_nodeIndex] ;
	    }
        */

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
	    /// We can shrink the octree if:
	    /// - This node is >= double minLength in length
	    /// - All objects in the root node are within one octant
	    /// - This node doesn't have children, or does but 7/8 children are empty
	    /// We can also shrink it if there are no objects left at all!
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="minLength">Minimum dimensions of a node in this octree.</param>
	    /// <returns>The new root index, or the existing one if we didn't shrink.</returns>
	    private int _ShrinkIfPossible ( int i_nodeIndex, float minLength ) 
        {

            float f_baseLength = l_nodeBaseLength [i_nodeIndex] ;

		    if ( f_baseLength < ( 2 * minLength ) ) 
            {
			    return i_nodeIndex ;
		    }
        
            int i_nodeChildrenCount = l_nodeChildrenCount [i_nodeIndex] ;
            int i_nodeInstanceCount = l_nodeInstancesCount [i_nodeIndex] ;

		    if ( i_nodeInstanceCount == 0 && i_nodeChildrenCount == 0 ) 
            {
			    return i_nodeIndex ;
		    }

            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;

		
            // -1 to 7, where -1 is no result found
		    int i_bestFit = -1;

            // Check objects in root
		    for (int i = 0; i < numInstancesAllowed; i++) 
            {

                if ( i_nodeInstanceCount == 0 )  break ;

                int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;

                if ( i_instanceIndex >= 0 )
                {

                    Bounds instanceBounds = l_instancesBounds [i_instanceIndex] ;

			        int newBestFit = _BestFitChild ( i_nodeIndex, instanceBounds ) ;
			
                    if (i == 0 || newBestFit == i_bestFit) 
                    {

				        // In same octant as the other(s). Does it fit completely inside that octant?
                        if ( _Encapsulates ( l_nodeChildrenBounds [i_nodeChildrenIndexOffset + newBestFit], instanceBounds ) ) 
                        {
					        if ( i_bestFit < 0 ) 
                            {
						        i_bestFit = newBestFit ;
					        }

                            break ;
				        }
				        else 
                        {
					        // Nope, so we can't reduce. Otherwise we continue
					        return i_nodeIndex ;
				        }
			        }
			        else 
                    {
				        return i_nodeIndex ; // Can't reduce - objects fit in different octants
			        }

                }

		    } // for

		    // Check instances in children if there are any
		    if ( i_nodeChildrenCount > 0 ) 
            {
			    bool childHadContent = false ;
            
                for (int i = 0; i < 8; i++) 
                {
                    // Has child any instances
				    if ( _HasAnyInstances ( l_nodeChildrenNodesIndex [ i_nodeChildrenIndexOffset + i ] ) ) 
                    {
                    
                        if ( childHadContent ) 
                        {
						    return i_nodeIndex ; // Can't shrink - another child had content already
					    }
					    if (i_bestFit >= 0 && i_bestFit != i) 
                        {
						    return i_nodeIndex ; // Can't reduce - objects in root are in a different octant to objects in child
					    }

					    childHadContent = true;
					    i_bestFit = i;
				    }
			    }
		    }

		    // Can reduce
		    if ( i_nodeChildrenCount == 0 ) 
            {
                Bounds childBounds = l_nodeChildrenBounds [i_nodeChildrenIndexOffset + i_bestFit] ;

			    // We don't have any children, so just shrink this node to the new size
			    // We already know that everything will still fit in it
			    _SetValues ( i_nodeIndex, f_baseLength / 2, childBounds.center ) ;

			    return i_nodeIndex ;
		    }

		    // No objects in entire octree
		    if ( i_bestFit == -1 ) 
            {
			    return i_nodeIndex ;
		    }

		    // We have children. Use the appropriate child as the new root node
            return l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i_bestFit] ;
	    }


        /// <summary>
	    /// Private counterpart to the public Add method.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceIndex">External instance ID to add.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance to add.</param>
	    private void _NodeInstanceSubAdd ( int i_nodeIndex, int i_instanceID, Bounds instanceBounds ) 
        {

		    // We know it fits at this level if we've got this far
		    // Just add if few objects are here, or children would be below min size
            int i_objectsCount = l_nodeInstancesCount [i_nodeIndex] ;

            if ( i_objectsCount < numInstancesAllowed || ( l_nodeBaseLength [i_nodeIndex] / 2) < f_minSize)         
            {
            
                _AssingInstance2Node ( i_nodeIndex, i_instanceID, instanceBounds ) ;
            
                if ( i_instancesSpareLastIndex == 0 )
                {
                    // Add some spares if needed.
                    _AddInstanceSpares ( ) ;                
                }
                else
                {
                    i_instancesSpareLastIndex -- ;
                }


    // Debugging
    GameObject go = GameObject.Find ( "Instance " + i_instanceID.ToString () ) ;

    if ( go != null ) 
    {
        Debug.Log ( "Instance: New game object #" + i_instanceID.ToString () ) ;
        go.SetActive ( true ) ;
        go.transform.localScale = instanceBounds.size ;
    }
    else
    {
        Debug.Log ( "Instance: New game object #" + i_instanceID.ToString () ) ;

        GameObject newGameObject = GameObject.Instantiate ( GameObject.Find ( "TempInstance" ), instanceBounds.center, Quaternion.identity ) ;
        newGameObject.transform.localScale = instanceBounds.size ;

        newGameObject.name = "Instance " + i_instanceID.ToString () ;
    }

		    }
		    else 
            {
			    // Fits at this level, but we can go deeper. Would it fit there?

			    // Create the 8 children
			    int i_bestFitChildLocalIndex ;
                int i_bestChildIndex ;

                int i_childrenCount       = l_nodeChildrenCount [i_nodeIndex] ;            
                int i_childrenIndexOffset = i_nodeIndex * 8 ;

			    if ( i_childrenCount == 0) 
                {
                    // Split Octree node, into 8 new smaller nodes as children nodex.
				    _Split ( i_nodeIndex );
                
                    int i_nodeInstanceIndexOffset = l_nodeInstancesIndex [i_nodeIndex] ;

				    // Now that we have the new children, see if this node's existing objects would fit there
				    for (int i = i_objectsCount - 1; i >= 0; i--) 
                    {
                        int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                        if ( i_instanceIndexOffset >= 0 )
                        {
                            Bounds existingInstanceBounds = l_instancesBounds [i_instanceIndexOffset] ;
                            int i_existingInsanceID = l_instancesID [i_instanceIndexOffset] ;
                        					    
					        // Find which child the object is closest to based on, where the
					        // object's center is located in relation to the octree's center.
					        i_bestFitChildLocalIndex = _BestFitChild ( i_nodeIndex, existingInstanceBounds ) ;

                            i_bestChildIndex = i_childrenIndexOffset + i_bestFitChildLocalIndex ;
                            Bounds childBounds = l_nodeChildrenBounds [i_bestChildIndex] ;

					        // Does it fit?
					        if ( _Encapsulates ( childBounds, existingInstanceBounds ) ) 
                            {                            
						        _NodeInstanceSubAdd ( l_nodeChildrenNodesIndex [i_bestChildIndex], i_existingInsanceID, existingInstanceBounds ) ; // Go a level deeper				
                            
                                // Remove from here
                                _PutBackSpareInstance ( i_instanceIndexOffset, i_nodeIndex ) ;
                                l_nodeInstancesCount [i_nodeIndex] -- ;
					        }
                        }
                    
				    }
			    }

			    // Now handle the new object we're adding now.
			    i_bestFitChildLocalIndex    = _BestFitChild ( i_nodeIndex, instanceBounds ) ;
                i_bestChildIndex            = i_childrenIndexOffset + i_bestFitChildLocalIndex ;

			    if ( _Encapsulates ( l_nodeChildrenBounds [i_bestChildIndex], instanceBounds ) ) 
                {                         
				    _NodeInstanceSubAdd ( l_nodeChildrenNodesIndex [i_bestChildIndex], i_instanceID, instanceBounds );
			    }
			    else 
                {
                
                    _AssingInstance2Node ( i_nodeIndex, i_instanceID, instanceBounds ) ;

                    if ( i_instancesSpareLastIndex == 0 )
                    {
                        // Add some spares if needed.
                        _AddInstanceSpares ( ) ;                
                    }
                    else
                    {
                        i_instancesSpareLastIndex -- ;
                    }
                
    // Debugging
    Debug.Log ( "Instance: New game object #" + i_instanceID.ToString () ) ;

    GameObject newGameObject = GameObject.Instantiate ( GameObject.Find ( "TempInstance" ), instanceBounds.center, Quaternion.identity ) ;
    newGameObject.transform.localScale = instanceBounds.size ;
    newGameObject.name = i_instanceID.ToString () ;

			    }
		    }

	    }

	    /// <summary>
	    /// Splits the octree into eight children.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    private void _Split ( int i_nodeIndex ) 
        {
            float f_nodeBaseLength      = l_nodeBaseLength [i_nodeIndex] ;
		    float f_quarter             = f_nodeBaseLength / 4f ;
		    float f_newBaseLength       = f_nodeBaseLength / 2 ;
            Vector3 V3_center           = l_nodeCenters [i_nodeIndex] ;
        
            l_nodeChildrenCount [i_nodeIndex] = 8 ;
        
            int i_childrenIndexOffset = i_nodeIndex * 8 ;

            // Create for this node, 8 new children nodes

            // Allocate spare nodes, to children nodes.
            // Is assumed, there is enough spare nodes
            for ( int i = 0; i < 8; i ++ )
            {
                l_nodeChildrenNodesIndex [i_childrenIndexOffset + i] = l_nodeSpares [i_nodeSpareLastIndex] ;
                i_nodeSpareLastIndex -- ;
            }


            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset], f_newBaseLength, V3_center + new Vector3(-f_quarter, f_quarter, -f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 1], f_newBaseLength, V3_center + new Vector3(f_quarter, f_quarter, -f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 2], f_newBaseLength, V3_center + new Vector3(-f_quarter, f_quarter, f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 3], f_newBaseLength, V3_center + new Vector3(f_quarter, f_quarter, f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 4], f_newBaseLength, V3_center + new Vector3(-f_quarter, -f_quarter, -f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 5], f_newBaseLength, V3_center + new Vector3(f_quarter, -f_quarter, -f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 6], f_newBaseLength, V3_center + new Vector3(-f_quarter, -f_quarter, f_quarter) ) ;
            _CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 7], f_newBaseLength, V3_center + new Vector3(f_quarter, -f_quarter, f_quarter) ) ;

	    }
        

	    /// <summary>
	    /// Merge all children into this node - the opposite of Split.
	    /// Note: We only have to check one level down since a merge will never happen if the children already have children,
	    /// since THAT won't happen unless there are already too many objects to merge.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    private void _MergeNodes ( int i_nodeIndex ) 
        {
        
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeUnusedInstancesIndexOffset = i_nodeIndex * numInstancesAllowed ;
        
		    // Note: We know children != null or we wouldn't be merging
	        for (int i = 0; i < 8; i++) 
            {
            
                int i_childNodeIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;
            
                if ( i_childNodeIndex >= 0 )
                {
                
                    int i_childNodeInstanceCount = l_nodeInstancesCount [i_childNodeIndex] ;
                
                    if ( i_childNodeInstanceCount > 0 ) 
                    {


                        int i_childModeInstancesIndexOffset = i_childNodeIndex * numInstancesAllowed ;

                        for (int i_unusedInstance = 0; i_unusedInstance < numInstancesAllowed; i_unusedInstance++) 
                        {

                            int i_unusedInstanceIndexOffset = i_nodeUnusedInstancesIndexOffset + i_unusedInstance ;
                    
                            if ( l_nodeInstancesIndex [i_unusedInstanceIndexOffset] == -1 )
                            {
                              
                                // Iterate through number of children instances.
			                    for (int j = numInstancesAllowed - 1; j >= 0; j--) 
                                {

                                    // Store old instance index
                                    int i_childInstanceIndex = l_nodeInstancesIndex [i_childModeInstancesIndexOffset + j] ;
                                
                                    // If node instance exists (none negative), assign to node
                                    if ( i_childInstanceIndex >= 0 )
                                    {
                                        // Reassign instance index, to next available spare index.                        
                                        l_nodeInstancesIndex [i_unusedInstanceIndexOffset] = i_childInstanceIndex ;
                                        l_nodeInstancesCount [i_nodeIndex] ++ ;
                
                                        l_nodeInstancesIndex [i_childModeInstancesIndexOffset + j] = -1 ;
                                        l_nodeInstancesCount [i_childNodeIndex] -- ;

                                        i_childNodeInstanceCount -- ;
                                    }


                                } // for

                            }

                        } // for

                    }

                }

            } // for

				
            // Reset children
            // Remove the child nodes (and the objects in them - they've been added elsewhere now)
            for (int i = 0; i < 8; i++) 
            {

                int i_childNodeIndexOffset = i_nodeChildrenIndexOffset + i ;
                int i_childNodeIndex = l_nodeChildrenNodesIndex [i_childNodeIndexOffset] ;

                if ( i_childNodeIndex >= 0 )
                {
                    // Iterate though node children.
                    if ( l_nodeChildrenCount [i_childNodeIndex] > 0 )
                    {

                        // Reset node children node index reference.
                        for (int j = 0; j < 8; j++) 
                        {
                            l_nodeChildrenNodesIndex [i_childNodeIndex + j] = -1 ; // Reset child
                        }
            
                        l_nodeInstancesCount [i_childNodeIndex] = 0 ;

                        // Put back node instances to spare instance.
                        for (int j = 0; j < numInstancesAllowed; j++) 
                        {
                            int i_instanceIndex = l_nodeInstancesIndex [i_childNodeIndex] ;
                            _PutBackSpareInstance ( i_instanceIndex + j, i_nodeIndex ) ; 
                        }

                    }

                    // Pu back child nodes to spares
                    i_nodeSpareLastIndex ++ ;
                    l_nodeSpares [i_nodeSpareLastIndex] = i_childNodeIndex ;
                
                    l_nodeChildrenNodesIndex [i_childNodeIndexOffset] = -1 ; // Reset child
                }
            }

            l_nodeChildrenCount [i_nodeIndex] = 0 ;

	    }

        /// <summary>
        /// Allows future reuse of spare instance, by putting it into the end of spares store.
        /// </summary>
        /// <param name="i_instanceIndex">Instance index, to pu back into spares of instances.</param>
        /// <param name="i_nodeIntstanceIndex">Node instance index holder, to be reset.</param>
        private void _PutBackSpareInstance ( int i_instanceIndex, int i_nodeIntstanceIndex )
        {

            if ( i_instanceIndex < 0 ) return ; // This instance index has not been used.
        
            i_instancesSpareLastIndex ++ ; // Put back to spare
            l_instancesSpare [i_instancesSpareLastIndex] = i_instanceIndex ;
        
            // Is assumed, that size of spares store, is appropriate.
            l_nodeInstancesIndex [i_nodeIntstanceIndex] = -1 ; // Reset instance index.
                
        }


	    /// <summary>
	    /// Checks if outerBounds encapsulates innerBounds.
	    /// </summary>
	    /// <param name="outerBounds">Outer bounds.</param>
	    /// <param name="innerBounds">Inner bounds.</param>
	    /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
	    static private bool _Encapsulates ( Bounds outerBounds, Bounds innerBounds ) 
        {
		    return outerBounds.Contains ( innerBounds.min ) && outerBounds.Contains ( innerBounds.max );
	    }

        
	    /// <summary>
	    /// Find which child node this object would be most likely to fit in.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="objBounds">The external instance bounds.</param>
	    /// <returns>One of the eight child octants.</returns>
	    private int _BestFitChild ( int i_nodeIndex, Bounds objBounds ) 
        {
            Vector3 V3_center = l_nodeCenters [i_nodeIndex] ;
		    return ( objBounds.center.x <= V3_center.x ? 0 : 1) + (objBounds.center.y >= V3_center.y ? 0 : 4) + (objBounds.center.z <= V3_center.z ? 0 : 2);
	    }

        
	    /// <summary>
	    /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <returns>True there are less or the same abount of objects in this and its children than numObjectsAllowed.</returns>
	    private bool _ShouldMerge ( int i_nodeIndex ) 
        {

		    int i_totalInstancesCount       = l_nodeInstancesCount [ i_nodeIndex ] ;
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

            int i_childrenCount           = l_nodeChildrenCount [i_nodeIndex] ;

		    // Has children?
		    if ( i_childrenCount > 0 ) 
            {
                for ( int i = 0; i < 8; i ++ )
                {

                    int i_childNodeIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;
                
                    if ( i_childNodeIndex >= 0 )
                    {
                        int i_nodefChildChildrenCount = l_nodeChildrenCount [i_childNodeIndex] ;

                        if ( i_nodefChildChildrenCount > 0 ) 
                        {
					        // If any of the *children* have children, there are definitely too many to merge,
					        // or the child would have been merged already
					        return false;
				        }

				        i_totalInstancesCount += l_nodeInstancesCount [i_childNodeIndex] ;
                    
                        i_childrenCount -- ;

                        if ( i_childrenCount == 0 ) break ;

                    }

                }
            
		    }

		    return i_totalInstancesCount <= numInstancesAllowed ;

	    }
    
	    /// <summary>
	    /// Checks if this node or anything below it has something in it.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <returns>True if this node or any of its children, grandchildren etc have something in them</returns>
	    internal bool _HasAnyInstances ( int i_nodeIndex ) 
        {
            if ( i_nodeIndex == -1 ) return false ;

		    if ( l_nodeInstancesCount [ i_nodeIndex ] > 0 ) return true ;

            // Has children?
		    if ( l_nodeChildrenCount [i_nodeIndex] > 0 ) 
            {
                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

			    for (int i = 0; i < 8; i++) 
                {
                    // Has child any instances
				    if ( _HasAnyInstances ( l_nodeChildrenNodesIndex [ i_nodeChildrenIndexOffset + i ] ) ) return true ;
			    }
		    }

		    return false;
	    }

        /// <summary>
        /// Assign instance to node.
        /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
        /// <param name="i_instanceID">External instance index.</param>
        /// <param name="instanceBounds">Boundary of external instance index.</param>
        private void _AssingInstance2Node ( int i_nodeIndex, int i_instanceID, Bounds instanceBounds )
        {
            int i_nodeInstanceIndexOffset = i_nodeIndex * numInstancesAllowed ;

            // Reuse spare store
            int i_spareInstanceIndex = l_instancesSpare [i_instancesSpareLastIndex] ;     
                
            // Find next spare instance allocation for this node.
            for (int i = 0; i < numInstancesAllowed; i++) 
            {

                int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                // Is spare.
                if ( l_nodeInstancesIndex [i_instanceIndexOffset] == -1 )
                {
                    // Assign instance index.
                    l_nodeInstancesIndex [i_instanceIndexOffset] = i_spareInstanceIndex ;

                    break ;
                }
            }

            l_nodeInstancesCount [i_nodeIndex] ++ ;

            l_instancesBounds [i_spareInstanceIndex] = instanceBounds ;
            l_instancesID [i_spareInstanceIndex]     = i_instanceID ;
        }


        /// <summary>
        /// Add required new spare instances.
        /// </summary>
        private void _AddInstanceSpares ( )
        {

            i_instancesSpareLastIndex -- ;

        
            int i_initialSparesCount = l_instancesID.Count ;
        
            // Add new spares, from the end of storage.
            for ( int i = 0; i < numOfSpareInstances2Add; i ++ )
            {        
                // Need to expand spare store.
                l_instancesSpare.Add ( -1 ) ;
                l_instancesID.Add ( -1 ) ;
                l_instancesBounds.Add ( new Bounds () ) ;
            }

            // Populate indexes references, with new spares.
            for ( int i = 0; i < numOfSpareInstances2Add; i ++ )
            {
                i_instancesSpareLastIndex ++ ;

                // Add new spares.
                // Add spares in reversed order, from higher index, to lower index.
                l_instancesSpare [i_instancesSpareLastIndex] = i_initialSparesCount + numOfSpareInstances2Add - i - 1 ;                     
            
            }
                
        }

    }
    
}