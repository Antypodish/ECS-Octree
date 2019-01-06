using System;
using System.Collections.Generic;
using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Rendering ;
using Unity.Burst ;

namespace ECS.Octree
{

    // ******** Collision check Tags ************ //
    
    // Use thse, to select octree collision checks. 
    public struct GetCollidingBoundsInstancesTag : IComponentData {}
    public struct GetCollidingRayInstancesTag : IComponentData {}
    public struct IsBoundsCollidingTag : IComponentData {}
    public struct IsRayCollidingTag : IComponentData {}



    // ******** Instance Tags ************ //
    
    public struct AddInstanceTag : IComponentData {}
    public struct RemoveInstanceTag : IComponentData {}




    // ******** Common components ************ //


    public struct RootNodeData : IComponentData 
    {
        
        /// <summary>
        /// The total amount of objects currently in the tree
        /// </summary>
        public int i_totalInstancesCountInTree ;
     
        /// <summary>        
        /// Root node of the octree
        /// Initial node root index 0
        /// </summary>
        public int i_rootNodeIndex ;
    
        /// <summary>
        /// Should be a value between 1 and 2. A multiplier for the base size of a node.
	    /// 1.0 is a "normal" octree, while values > 1 have overlap
        /// </summary>
        public float f_looseness ;
                
        /// <summary>
        /// Size that the octree was on creation
        /// </summary>
	    public float f_initialSize ;
    
	    /// <summary>
        /// Minimum side length that a node can be - essentially an alternative to having a max depth
        /// </summary>
	    public float f_minSize;
        
        
        public int i_nodeSpareLastIndex ;

        public int i_instancesSpareLastIndex ;

        public int i_instancesAllowedCount ;
                
    }


    public struct NodeBufferElement : IBufferElementData 
    {
        public float f_baseLength ;
        // private List <float> l_nodeBaseLength ;
        public float f_adjLength ;
        // private List <float> l_nodeAdjLength ;
        public float f_minSize ;
        // private List <float> l_nodeMinSize ;
        public float3 f3_center ;
        // private List <Vector3> l_nodeCenters ;
            
        public Bounds bounds ;
        // public List <Bounds> l_nodeBounds ;
                  
        public int i_childrenCount ;
        // private List <int> l_nodeChildrenCount ;   
        
        /// <summary>
        /// Count of instances per node.
        /// Each node instance stores index to appropriate instance collection.
        /// Same idex refers to the instance Boundry
        /// </summary>  
        public int i_instancesCount ;
        // private List <int> l_nodeInstancesCount ;
    }
    

    /// <summary>
    /// Group of 8 children per node
    /// </summary>
    public struct NodeChildrenBufferElement : IBufferElementData 
    {
        
        /// <summary>
        /// Group of 8 children per node
        /// Reference to child node by index, if any
        /// </summary>
        public int i_nodesIndex ;
        // private List <int> l_nodeChildrenNodesIndex ;  

        /// <summary>
        /// Group of 8 children per node
        /// </summary>
        public Bounds bounds ;
        // private List <Bounds> l_childrenBounds ;
    
    }
    

    public struct NodeSparesBufferElement : IBufferElementData 
    {
        public int i ;
        // private List <int> l_nodeSpares ;
    }
    

    /// <summary>
    /// Replacement for the object
    /// It references index to desired instances.
    /// Number of elements per node, should not exceed maximum allowed number of instances per node.
    /// If unused, value should be set to -1, as defualt.
    /// Index of instance is pointing at any desired instance list, where first three lists of instance bounds, spares and list of instance spares are mandatory.
    /// Further lists are optional. For example list of enitities, which are accessed with same index as bounds instance.
    /// Index from list should be returned, when testing for collision, new element is added, or removed.
    /// </summary>
    public struct NodeInstancesIndexBufferElement : IBufferElementData 
    {
        /// <summary>
        /// Replacement for the object
        /// It references index to desired instances.
        /// Number of elements per node, should not exceed maximum allowed number of instances per node.
        /// If unused, value should be set to -1, as defualt.
        /// Index of instance is pointing at any desired instance list, where first three lists of instance bounds, spares and list of instance spares are mandatory.
        /// Further lists are optional. For example list of enitities, which are accessed with same index as bounds instance.
        /// Index from list should be returned, when testing for collision, new element is added, or removed.
        /// </summary>
        public int i ;
        // private List <int> l_nodeInstancesIndex ;
    }


    /// <summary>
    /// Mandatory
    /// </summary>
    public struct InstancesSpareIndexBufferElement : IBufferElementData 
    {
        /// <summary>
        /// Mandatory
        /// </summary>
        public int i ;
        // private List <int> l_instancesSpare ;
    }


    public struct InstanceBufferElement : IBufferElementData 
    {
        /// <summary>
        /// Accessed by index from l_nodeInstancesIndex
        /// </summary>
        public Bounds bounds ;
        // private List <Bounds> l_instancesBounds ;

        /// <summary>
        /// Store instance ID's which must be unique.
        /// </summary>
        public int i_ID ;
        // Entity?
        // private List <int> l_instancesID ;

        // You can add more lists with desired properties, wich maching list size of total instance bounds count (list size)
    }


    /// <summary>
    /// Result of GetCollision, which outputs number instances, that boundery, or raycas has interact with.
    /// Should be read only. Is reset, every time GeCollision is executed.
    /// </summary>   
    public struct GetCollidingWithInstenceBufferElement : IBufferElementData 
    {     
        public int i ;
        // public List <int> l_collidingWith ;
    }

}