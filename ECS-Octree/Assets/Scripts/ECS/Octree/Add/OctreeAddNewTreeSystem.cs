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


    // [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class AddNewOctree : JobComponentSystem
    {

        const int i_collisionRaycastDistance = 1000 ;

        EntityArchetype octreeArchetype ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );

            Debug.Log ( "Start Add New Octree System" ) ;
            Debug.Log ( "TODO: Replace instance with entity?" ) ;

            octreeArchetype = EntityManager.CreateArchetype ( 
                typeof (AddInstanceTag), // once added, tag will be deleted.
                typeof (RemoveInstanceTag), // once removed, tag will be deleted.

//                typeof (IsBoundsCollidingTag), // Check boundary collision with octree instances.
//                typeof (IsRayCollidingTag), // Check ray collision with octree instances.
//                typeof (GetCollidingBoundsInstancesTag), // Check bounds collision with octree and return colliding instances.
                typeof (GetCollidingRayInstancesTag), // Check bounds collision with octree and return colliding instances.
                
                
                

                typeof (RootNodeData),
                typeof (NodeBufferElement),
                typeof (NodeChildrenBufferElement),
                typeof (NodeInstancesIndexBufferElement),
                typeof (NodeSparesBufferElement),
                
                typeof (InstanceBufferElement),
                typeof (InstancesSpareIndexBufferElement),

// TODO: replace instance ID with entity?
                typeof (GetCollidingWithInstenceBufferElement)                
            ) ;

            Entity entity = EntityManager.CreateEntity ( octreeArchetype ) ;
            
            _Initialize ( entity, 8, float3.zero, 1, 1, 1 ) ;

             
        }


        /// <summary>
	    /// Constructor for the bounds octree.
        /// For minimum size of 1 initial size could be for example 1, 2, 4, 8, 16 ect
        /// For minimum size of 3 initial size could be for example 3, 9, 27, 81 etc
	    /// </summary>
	    /// <param name="f_initialSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
	    /// <param name="f3_initialPosition">Position of the centre of the initial node.</param>
	    /// <param name="f_minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
	    /// <param name="f_looseness">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
        public void _Initialize ( Entity rootNodeEntity, float f_initialSize, float3 f3_initialPosition, float f_minNodeSize, float f_looseness, int i_instancesAllowedCount )
        {

            if ( f_minNodeSize > f_initialSize ) 
            {
			    Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + f_initialSize + " Adjusted to: " + f_minNodeSize );
			    f_minNodeSize = f_initialSize;
		    }
            
            RootNodeData rootNodeData = new RootNodeData ()
            {
                i_rootNodeIndex             = 0,

                f_initialSize               = f_initialSize,
                f_minSize                   = f_minNodeSize,                                
                f_looseness                 = math.clamp ( f_looseness, 1.0f, 2.0f ),

                i_totalInstancesCountInTree = 0, 

                i_instancesSpareLastIndex   = 0,
                i_nodeSpareLastIndex        = 0,

                i_instancesAllowedCount     = i_instancesAllowedCount
            } ;

            
            EntityManager.SetComponentData ( rootNodeEntity, rootNodeData ) ;

            /*
            i_rootNodeIndex                = 0 ;

            this.f_initialSize             = f_initialSize ;
            this.f_minSize                 = f_minNodeSize ;
            this.f_looseness               = f_looseness ;

            
             * // buffers
            l_nodeBaseLength               = new List <float> () ;
            l_nodeAdjLength                = new List <float> () ;
            // l_nodeMinSize               = new List <float> () ;
            // l_nodeLooseness             = new List <float> () ;
            l_nodeCenters                  = new List <Vector3> () ;
            l_nodeBounds                   = new List <Bounds> () ;
            // Size should be always 8 times size of the node elements.
            l_childrenBounds               = new List <Bounds> () ;

            l_nodeChildrenNodesIndex       = new List <int> () ;
            l_nodeChildrenCount            = new List <int> () ;
            l_nodeSpares                   = new List <int> () ;
            l_nodeInstancesCount           = new List <int> () ;

            l_nodeInstancesIndex           = new List <int> () ;
            l_instancesBounds              = new List <Bounds> () ;
            l_instancesID                  = new List <int> () ;
            l_instancesSpare               = new List <int> () ;
                

            l_collidingWith                = new List <int> () ;
            */
// (constant)            i_collisionRaycastDistance     = 1000 ; // default distance

 //           i_totalInstancesCountInTree    = 0 ;
 //           f_looseness                  = Mathf.Clamp ( f_looseness, 1.0f, 2.0f ) ;
 //           i_rootNodeIndex              = 0 ;  
            rootNodeData.i_nodeSpareLastIndex -- ;               

            
            BufferFromEntity <NodeSparesBufferElement> nodeSparesBufferElement                            = GetBufferFromEntity <NodeSparesBufferElement> () ;
            DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer                                    = nodeSparesBufferElement [rootNodeEntity] ;

            BufferFromEntity <NodeBufferElement> nodeBufferElement                                        = GetBufferFromEntity <NodeBufferElement> () ;
            DynamicBuffer <NodeBufferElement> a_nodesBuffer                                               = nodeBufferElement [rootNodeEntity] ;

            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement            = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;
            DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer                    = nodeInstancesIndexBufferElement [rootNodeEntity] ;   

            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                        = GetBufferFromEntity <NodeChildrenBufferElement> () ;
            DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                                = nodeChildrenBufferElement [rootNodeEntity] ;    

            CommonMethods._CreateNewNode ( ref rootNodeData, rootNodeData.i_rootNodeIndex, f_initialSize, f3_initialPosition, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;
            // _CreateNewNode ( entity, rootData.i_rootNodeIndex, rootData.f_initialSize, f3_initialPosition ) ;

            rootNodeData.i_nodeSpareLastIndex -- ;                 
            // EntityManager.SetComponentData ( rootNodeEntity, rootNodeData ) ;

            rootNodeData.i_instancesSpareLastIndex    = 0 ;



            
            BufferFromEntity <InstanceBufferElement> instanceBufferElement                                  = GetBufferFromEntity <InstanceBufferElement> () ;
            DynamicBuffer <InstanceBufferElement> a_instanceBuffer                                          = instanceBufferElement [rootNodeEntity] ;   

            BufferFromEntity <InstancesSpareIndexBufferElement> instancesSpareIndexBufferElement            = GetBufferFromEntity <InstancesSpareIndexBufferElement> () ;
            DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer                    = instancesSpareIndexBufferElement [rootNodeEntity] ;    

            // Add some spares if needed.
            CommonMethods._AddInstanceSpares ( ref rootNodeData, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer ) ;   
            
            EntityManager.SetComponentData ( rootNodeEntity, rootNodeData ) ;
        }


        
        
    }
}

