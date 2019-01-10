using Unity.Mathematics ;
using Unity.Entities ;
using UnityEngine ;


namespace ECS.Octree
{


    // ******** Octree Initialization Data ************ //
    
    /// <summary>
    /// Request to create new octree structure.
    /// This tag is Immediately deleted from the entity, when job is executed and new octree structure is assaigned to this entity.
    /// </summary>
    public struct AddNewOctreeData : IComponentData 
    {
        public float f_initialSize ;
        public float3 f3_initialPosition ;                        
    }
    


    // ******** Instance Buffer ************ //
    

    /// <summary>
    /// Contains a list of instances to add, with its properties.
    /// Instance must be unique, and must not exists already in a octree.
    /// </summary>
    public struct AddInstanceBufferElement : IBufferElementData 
    {
        public int i_instanceID ;
        /// <summary>
        /// Optional, can be used by entity.
        /// </summary>
        public int i_version ;
        public Bounds instanceBounds ;
    }

    /// <summary>
    /// Contains a list of instances to remove, addressed by instance ID.
    /// </summary>
    public struct RemoveInstanceBufferElement : IBufferElementData 
    {
        public int i_instanceID ;
        /// <summary>
        /// Optional, can be used by entity.
        /// </summary>
        // public int i_version ; // not required for removal. Is not checked.
    }



    // ******** Collision check Tags / Data / Buffers ************ //
    
    // Use thse, to select octree collision checks. 
    
    /// <summary>
    /// Paired octree entity, required for collision checks.
    /// Can be used for example in many raycast/bounds to target octree.
    /// </summary>
    public struct OctreeEntityPair4CollisionData : IComponentData
    {  
        public Entity octree2CheckEntity ;
    }

    /// <summary>
    /// Paired ray entity, required for collision checks.
    /// Can be used for example in many octrees to target ray.
    /// </summary>
    public struct RayEntityPair4CollisionData : IComponentData
    {  
        public Entity ray2CheckEntity ;
    }
    
    /// <summary>
    /// Paired bounds entity, required for collision checks.
    /// Can be used for example in many octrees to target bound.
    /// </summary>
    public struct BoundsEntityPair4CollisionData : IComponentData
    {  
        public Entity bounds2CheckEntity ;
    }

    


    /// <summary>
    /// 0 means it is not colliding.
    /// >= 1 means is colliding, and possible information, about number of collisions, if applicable.
    /// </summary>
    public struct IsCollidingData : IComponentData
    {
        /// <summary>
        /// 0 means it is not colliding.
        /// >= 1 means is colliding, and possible information, about number of collisions, if applicable.
        /// </summary>
        public int i_collisionsCount ;
        /// <summary>
        /// If applicable.
        /// Index to nearest collision instances in buffer array.
        /// </summary>
        public int i_nearestInstanceCollisionIndex ;
        /// <summary>
        /// If applicable.
        /// </summary>
        public float f_nearestDistance ;
    }

    /// <summary>
    /// Collection of colliding instances.
    /// Use with conjunction of IsCollidingData, storing number of currently used buffer elements.
    /// This is to prevent clearing and allocting buffer, every time is accessed.
    /// Its size grows as required, and stay of that size, as long entity holding this buffer exists.
    /// </summary>
    public struct CollisionInstancesBufferElement : IBufferElementData
    {
        public int i_ID ;

        /// <summary>
        /// Optional, can be used by entity.
        /// </summary>
        public int i_version ;
    }

    /// <summary>
    /// Returns true, if bounds overlap, otherwise false.
    /// Also returns list of overlapping bounds instances.
    /// </summary>
    public struct GetCollidingBoundsInstancesTag : IComponentData {}    
    /// <summary>
    /// Returns true, if ray intersects, otherwise false.
    /// Also returns list of overlapping bounds instances.
    /// And closes instance distance and instance's ID.
    /// </summary>
    public struct GetCollidingRayInstancesTag : IComponentData {}
    /// <summary>
    /// Returns true, if bounds overlap, otherwise false.
    /// </summary>
    public struct IsBoundsCollidingTag : IComponentData {}
    /// <summary>
    /// Returns true, if ray intersects, otherwise false.
    /// </summary>
    public struct IsRayCollidingTag : IComponentData {}

    public struct GetMaxBoundsTag : IComponentData {}



    // ******** Common components ************ //


    /// <summary>
    /// Core data of octree structure.
    /// </summary>
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
                
        /// <summary>
        /// Number of instances allowed per octree node.
        /// </summary>
        public int i_instancesAllowedCount ;
                
    }


    public struct NodeBufferElement : IBufferElementData 
    {
        public float f_baseLength ;
        public float f_adjLength ;
        public float f_minSize ;
        public float3 f3_center ;
            
        public Bounds bounds ;
                  
        public int i_childrenCount ;
        
        /// <summary>
        /// Count of instances per node.
        /// Each node instance stores index to appropriate instance collection.
        /// Same idex refers to the instance Boundry
        /// </summary>  
        public int i_instancesCount ;
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

        /// <summary>
        /// Group of 8 children per node
        /// </summary>
        public Bounds bounds ;
    
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
        /// If used as entity index, use also i_version.
        /// </summary>
        public int i_ID ;
        
        /// <summary>
        /// Optional, can be used by entity.
        /// </summary>
        public int i_entityVersion ;

// Entity?
    }

    /*
    /// <summary>
    /// Result of GetCollision, which outputs number instances, that boundery, or raycas has interact with.
    /// Should be read only. Is reset, every time GeCollision is executed.
    /// </summary>   
    public struct GetCollidingWithInstenceBufferElement : IBufferElementData 
    {     
        public int i ;
    }
    */

}