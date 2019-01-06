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
    // [UpdateAfter ( typeof ( AddNewOctree ) ) ]    
    class AddInstanceSystem : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Add New Octree Instance System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                //typeof (EntityArray),
                typeof (AddInstanceTag), 
                typeof (RootNodeData) 
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
         
            Debug.Log ( "Add New Octree Instance" ) ;

            EntityArray a_entities                                                                        = group.GetEntityArray () ;
            Entity rootNodeEntity                                                                         = a_entities [0] ;
            

            ComponentDataArray <RootNodeData> a_rootNodeData                                              = group.GetComponentDataArray <RootNodeData> ( ) ;
            RootNodeData rootNodeData                                                                     = a_rootNodeData [0] ;



            BufferFromEntity <NodeSparesBufferElement> nodeSparesBufferElement                            = GetBufferFromEntity <NodeSparesBufferElement> () ;
            DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer                                    = nodeSparesBufferElement [rootNodeEntity] ;

            BufferFromEntity <NodeBufferElement> nodeBufferElement                                        = GetBufferFromEntity <NodeBufferElement> () ;
            DynamicBuffer <NodeBufferElement> a_nodesBuffer                                               = nodeBufferElement [rootNodeEntity] ;

            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement            = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;
            DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer                    = nodeInstancesIndexBufferElement [rootNodeEntity] ;   

            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                        = GetBufferFromEntity <NodeChildrenBufferElement> () ;
            DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                                = nodeChildrenBufferElement [rootNodeEntity] ;    

            BufferFromEntity <InstanceBufferElement> instanceBufferElement                                  = GetBufferFromEntity <InstanceBufferElement> () ;
            DynamicBuffer <InstanceBufferElement> a_instanceBuffer                                          = instanceBufferElement [rootNodeEntity] ;   

            BufferFromEntity <InstancesSpareIndexBufferElement> instancesSpareIndexBufferElement            = GetBufferFromEntity <InstancesSpareIndexBufferElement> () ;
            DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer                    = instancesSpareIndexBufferElement [rootNodeEntity] ;    



            // int i_instanceID = 10 ;

            for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
            {  

                int x = i_instanceID % 10 ;
                int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;
                Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

                Bounds bound = new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ;
                _OctreeAddInstance ( 
                    ref rootNodeData,
                    i_instanceID, 
                    bound,
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer,
                    ref a_instanceBuffer, 
                    ref a_instancesSpareIndexBuffer 
                ) ;

            }

            a_rootNodeData [0] = rootNodeData ;
            
            EntityManager.RemoveComponent <AddInstanceTag> ( rootNodeEntity ) ; // Instance added.
            

            return base.OnUpdate ( inputDeps );
        }
        


        /// <summary>
	    /// Add an Instance.
	    /// </summary>
	    /// <param name="i_instanceID">External instance to add.</param>
	    /// <param name="instanceBounds">External instance 3D bounding box around the instance.</param>
	    public void _OctreeAddInstance ( ref RootNodeData rootNodeData, int i_instanceID, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

		    // Add object or expand the octree until it can be added
		    int i_count = 0; // Safety check against infinite/excessive growth

		    while ( 
                !_AddNodeInstance ( ref rootNodeData, 
                    i_instanceID, 
                    instanceBounds, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer,
                    ref a_instanceBuffer, 
                    ref a_instancesSpareIndexBuffer 
                ) 
            ) 
            {

                NodeBufferElement nodeBufferElement = a_nodesBuffer [rootNodeData.i_rootNodeIndex] ;

			    _GrowOctree ( ref rootNodeData, 
                    (float3) instanceBounds.center - nodeBufferElement.f3_center,
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer, 
                    ref a_instanceBuffer,
                    ref a_instancesSpareIndexBuffer
                ) ;

			    if ( ++i_count > 20 ) 
                {
				    Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (i_count - 1) + ") attempts at growing the octree.");
				    return;
			    }
		    }

		    rootNodeData.i_totalInstancesCountInTree ++ ;

	    }


        /// <summary>
	    /// Add an object.
	    /// </summary>
        /// <param name="i_rootNodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance.</param>
	    /// <returns>True if the object fits entirely within this node.</returns>
	    private bool _AddNodeInstance ( ref RootNodeData rootNodeData, int i_instanceID, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

            NodeBufferElement nodeBufferElement = a_nodesBuffer [rootNodeData.i_rootNodeIndex] ;

		    if ( !CommonMethods._Encapsulates ( nodeBufferElement.bounds, instanceBounds ) ) return false ; // Early exit

            _NodeInstanceSubAdd ( 
                ref rootNodeData, 
                rootNodeData.i_rootNodeIndex, 
                i_instanceID, instanceBounds, 
                ref a_nodesBuffer, 
                ref a_nodeSparesBuffer, 
                ref a_nodeChildrenBuffer, 
                ref a_nodeInstancesIndexBuffer, 
                ref a_instanceBuffer, 
                ref a_instancesSpareIndexBuffer 
            ) ;
		    // _NodeInstanceSubAdd ( rootNodeData, i_nodeIndex, i_instanceID, instanceBounds ) ;

		    return true;
	    }


        /// <summary>
	    /// Grow the octree to fit in all objects.
	    /// </summary>
	    /// <param name="f3_direction">Direction to grow.</param>
	    private void _GrowOctree ( ref RootNodeData rootNodeData, float3 f3_direction, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

		    int xDirection = f3_direction.x >= 0 ? 1 : -1 ;
		    int yDirection = f3_direction.y >= 0 ? 1 : -1 ;
		    int zDirection = f3_direction.z >= 0 ? 1 : -1 ;

            

            int i_oldRootNodeIndex              = rootNodeData.i_rootNodeIndex ;            
            NodeBufferElement nodeBufferElement = a_nodesBuffer [i_oldRootNodeIndex] ;
            float f_baseLength                  = nodeBufferElement.f_baseLength ;
		    float f_half                        = f_baseLength / 2 ;
		    float f_newBaseLength               = f_baseLength * 2 ;

		    float3 f3_newCenter                = nodeBufferElement.f3_center + new float3 ( xDirection * f_half, yDirection * f_half, zDirection * f_half ) ;

		    // Create a new, bigger octree root node
                
		    if ( !CommonMethods._HasAnyInstances ( i_oldRootNodeIndex, a_nodesBuffer, a_nodeChildrenBuffer ) )
		    {                
                CommonMethods._CreateNewNode ( 
                    ref rootNodeData, 
                    rootNodeData.i_rootNodeIndex, 
                    f_newBaseLength, 
                    f3_newCenter, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer 
                ) ;
                //_CreateNewNode ( ref rootNodeData, rootNodeData.i_rootNodeIndex, f_newBaseLength, V3_newCenter ) ;
            }
            else
            {
            
                NodeSparesBufferElement nodeSparesBuffer = a_nodeSparesBuffer [rootNodeData.i_nodeSpareLastIndex] ;
                rootNodeData.i_rootNodeIndex = nodeSparesBuffer.i ;
                // rootNodeData.i_rootNodeIndex = l_nodeSpares [rootNodeData.i_nodeSpareLastIndex] ;
            
                rootNodeData.i_nodeSpareLastIndex -- ;   
            
                CommonMethods._CreateNewNode ( 
                    ref rootNodeData, 
                    rootNodeData.i_rootNodeIndex, 
                    f_newBaseLength, 
                    f3_newCenter, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer 
                ) ;
                // _CreateNewNode ( i_rootNodeIndex, f_newBaseLength, f3_newCenter ) ;


			    // Create 7 new octree children to go with the old root as children of the new root
			    int i_rootPos = _GetRootPosIndex ( xDirection, yDirection, zDirection ) ;
			
                                
                NodeBufferElement nodeBuffer = a_nodesBuffer [rootNodeData.i_rootNodeIndex] ;
                nodeBuffer.i_childrenCount = 8 ;
                a_nodesBuffer [rootNodeData.i_rootNodeIndex] = nodeBuffer ; // Set back.

                // l_nodeChildrenCount [rootNodeData.i_rootNodeIndex] = 8 ;
                int i_newRootNodeChildrenIndexOffset = rootNodeData.i_rootNodeIndex * 8 ;

			    for (int i = 0; i < 8; i++)
			    {

                    int i_childIndexOffset = i_newRootNodeChildrenIndexOffset + i ;

                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_childIndexOffset] ;

				    if ( i == i_rootPos )
				    {
                        // Assign old root node as a child.
                        nodeChildrenBuffer.i_nodesIndex      = i_oldRootNodeIndex ;
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_oldRootNodeIndex] ;
                        nodeChildrenBuffer.bounds            = instanceBuffer.bounds ;
                        // l_nodeChildrenNodesIndex [i_childIndexOffset]   = i_oldRootNodeIndex ;    
                        // l_nodeChildrenBounds [i_childIndexOffset]       = l_instancesBounds [i_oldRootNodeIndex] ;
					
				    }
				    else
				    {
                        // Assign rest 7 children
					    xDirection                      = i % 2 == 0 ? -1 : 1;
					    yDirection                      = i > 3 ? -1 : 1;
					    zDirection                      = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;

                        InstancesSpareIndexBufferElement instancesSpareIndexBuffer ;
                        instancesSpareIndexBuffer               = a_instancesSpareIndexBuffer [rootNodeData.i_nodeSpareLastIndex] ;
                        int i_newNodeIndex                      = instancesSpareIndexBuffer.i ; // Expected output 0 at initialization                        
                        rootNodeData.i_nodeSpareLastIndex -- ;  

                        float3 f3_childVector                   = f3_newCenter + new float3 ( xDirection * f_half, yDirection * f_half, zDirection * f_half ) ;
                    
                        CommonMethods._CreateNewNode ( 
                            ref rootNodeData, 
                            i_newNodeIndex, 
                            f_newBaseLength, 
                            f3_childVector, 
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            ref a_nodeChildrenBuffer, 
                            ref a_nodeInstancesIndexBuffer 
                        ) ;
                        
                        // _CreateNewNode ( 
                        //    i_newNodeIndex,
                        //    f_newBaseLength,                                                   
                        //    f3_childVector
                        //) ; 
                    
                        nodeChildrenBuffer.i_nodesIndex                 = i_newNodeIndex ; 
                        // l_nodeChildrenNodesIndex [i_childIndexOffset]       = i_newNodeIndex ; 

                        Bounds bounds = new Bounds ( ) { 
                            center      = f3_childVector,
                            size        = Vector3.one * f_newBaseLength
                        } ;

                        nodeChildrenBuffer.bounds                       = bounds ;
					    // l_nodeChildrenBounds [i_childIndexOffset]           = bounds ;

				    }

                    a_nodeChildrenBuffer [i_childIndexOffset] = nodeChildrenBuffer ; // Set back.

			    } // for

		    }
	    }


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
	    /// Private counterpart to the public Add method.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceIndex">External instance ID to add.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance to add.</param>
	    private void _NodeInstanceSubAdd ( ref RootNodeData rootNodeData, int i_nodeIndex, int i_instanceID, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

            
		    // We know it fits at this level if we've got this far
		    // Just add if few objects are here, or children would be below min size
            int i_instancesCount = nodeBuffer.i_instancesCount ;

            if ( i_instancesCount < rootNodeData.i_instancesAllowedCount || ( nodeBuffer.f_baseLength / 2) < rootNodeData.f_minSize)         
            {
            
                _AssingInstance2Node ( rootNodeData, i_nodeIndex, i_instanceID, instanceBounds, ref a_nodesBuffer, ref a_instanceBuffer, ref a_nodeInstancesIndexBuffer, a_instancesSpareIndexBuffer ) ;
                // _AssingInstance2Node ( i_nodeIndex, i_instanceID, instanceBounds ) ;
            
                if ( rootNodeData.i_instancesSpareLastIndex == 0 )
                {
                    // Add some spares if needed.
                    CommonMethods._AddInstanceSpares ( ref rootNodeData, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer ) ;              
                }
                else
                {
                    rootNodeData.i_instancesSpareLastIndex -- ;
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

                nodeBuffer                  = a_nodesBuffer [i_nodeIndex] ;
                int i_childrenCount         = nodeBuffer.i_childrenCount ;            
                int i_childrenIndexOffset   = i_nodeIndex * 8 ;

                NodeChildrenBufferElement nodeChildrenBuffer ;

			    if ( i_childrenCount == 0) 
                {
                    // Split Octree node, into 8 new smaller nodes as children nodex.
				    _Split ( ref rootNodeData, i_nodeIndex, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer );
                
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeIndex] ;

                    int i_nodeInstanceIndexOffset = nodeInstancesIndexBuffer.i ;

				    // Now that we have the new children, see if this node's existing objects would fit there
				    for (int i = i_instancesCount - 1; i >= 0; i--) 
                    {
                        int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                        if ( i_instanceIndexOffset >= 0 )
                        {
                            InstanceBufferElement existingInstanceBuffer = a_instanceBuffer [i_instanceIndexOffset] ;
                            //Bounds existingInstanceBounds = existingInstanceBuffer.bounds ;
                            //int i_existingInsanceID = existingInstanceBuffer.i_ID ;
                        					    
					        // Find which child the object is closest to based on, where the
					        // object's center is located in relation to the octree's center.
					        i_bestFitChildLocalIndex = CommonMethods._BestFitChild ( i_nodeIndex, existingInstanceBuffer.bounds, a_nodesBuffer ) ;

                            i_bestChildIndex = i_childrenIndexOffset + i_bestFitChildLocalIndex ;
                            nodeChildrenBuffer = a_nodeChildrenBuffer [i_bestChildIndex] ;
                            
                            // Bounds childBounds = l_nodeChildrenBounds [i_bestChildIndex] ;

					        // Does it fit?
					        if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, existingInstanceBuffer.bounds ) ) 
                            {                            
                                _NodeInstanceSubAdd ( 
                                    ref rootNodeData, 
                                    nodeChildrenBuffer.i_nodesIndex, 
                                    existingInstanceBuffer.i_ID, existingInstanceBuffer.bounds, 
                                    ref a_nodesBuffer, 
                                    ref a_nodeSparesBuffer, 
                                    ref a_nodeChildrenBuffer, 
                                    ref a_nodeInstancesIndexBuffer, 
                                    ref a_instanceBuffer, 
                                    ref a_instancesSpareIndexBuffer 
                                ) ; // Go a level deeper
						        // _NodeInstanceSubAdd ( nodeChildrenBuffer.i_nodesIndex, existingInstanceBuffer.i_ID, existingInstanceBuffer.bounds ) ; // Go a level deeper				
                            
                                // Remove from here
                                CommonMethods._PutBackSpareInstance ( ref rootNodeData, i_instanceIndexOffset, i_nodeIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;

                                nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
                                nodeBuffer.i_instancesCount -- ;
                                a_nodesBuffer [i_nodeIndex] = nodeBuffer ;
					        }
                        }
                    
				    }
			    }


			    // Now handle the new object we're adding now.
			    i_bestFitChildLocalIndex    = CommonMethods._BestFitChild ( i_nodeIndex, instanceBounds, a_nodesBuffer ) ;
                i_bestChildIndex            = i_childrenIndexOffset + i_bestFitChildLocalIndex ;

                nodeChildrenBuffer          = a_nodeChildrenBuffer [i_bestChildIndex] ;

			    if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, instanceBounds ) ) 
                {                 
                    _NodeInstanceSubAdd ( 
                        ref rootNodeData, 
                        nodeChildrenBuffer.i_nodesIndex, 
                        i_instanceID, instanceBounds, 
                        ref a_nodesBuffer, 
                        ref a_nodeSparesBuffer, 
                        ref a_nodeChildrenBuffer, 
                        ref a_nodeInstancesIndexBuffer, 
                        ref a_instanceBuffer, 
                        ref a_instancesSpareIndexBuffer 
                    ) ;
				    // _NodeInstanceSubAdd ( nodeChildrenBuffer.i_nodesIndex, i_instanceID, instanceBounds );
			    }
			    else 
                {
                
                    _AssingInstance2Node ( rootNodeData, i_nodeIndex, i_instanceID, instanceBounds, ref a_nodesBuffer, ref a_instanceBuffer, ref a_nodeInstancesIndexBuffer, a_instancesSpareIndexBuffer ) ;
                    // _AssingInstance2Node ( i_nodeIndex, i_instanceID, instanceBounds ) ;

                    if ( rootNodeData.i_instancesSpareLastIndex == 0 )
                    {
                        // Add some spares if needed.
                        CommonMethods._AddInstanceSpares ( ref rootNodeData, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer ) ;                
                    }
                    else
                    {
                        rootNodeData.i_instancesSpareLastIndex -- ;
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
	    private void _Split ( ref RootNodeData rootNodeData, int i_nodeIndex, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer ) 
        {

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

            float f_nodeBaseLength       = nodeBuffer.f_baseLength ;
		    float f_quarter              = f_nodeBaseLength / 4f ;
		    float f_newBaseLength        = f_nodeBaseLength / 2 ;
            float3 f3_center             = nodeBuffer.f3_center ;
        
            nodeBuffer.i_childrenCount   = 8 ;
            a_nodesBuffer [i_nodeIndex]  = nodeBuffer ;

            int i_childrenIndexOffset    = i_nodeIndex * 8 ;

            // Create for this node, 8 new children nodes

            
            NodeChildrenBufferElement nodeChildrenBuffer ;

            // Allocate spare nodes, to children nodes.
            // Is assumed, there is enough spare nodes
            for ( int i = 0; i < 8; i ++ )
            {
                NodeSparesBufferElement nodeSparesBuffer                = a_nodeSparesBuffer [rootNodeData.i_nodeSpareLastIndex] ;
                nodeChildrenBuffer                                      = a_nodeChildrenBuffer [i_childrenIndexOffset + i] ;
                nodeChildrenBuffer.i_nodesIndex                         = nodeSparesBuffer.i ;
                // l_nodeChildrenNodesIndex [i_childrenIndexOffset + i] = nodeSparesBuffer.i ;
                a_nodeChildrenBuffer [i_childrenIndexOffset + i]        = nodeChildrenBuffer ; // Set back
                rootNodeData.i_nodeSpareLastIndex -- ;
            }

            float3 f3_childCenterQuater ;
            

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset] ;        
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 1] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 2] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 3] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 4] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, -f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 5] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, -f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 6] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, -f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 7] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, -f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNodeData, nodeChildrenBuffer.i_nodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            /*
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset], f_newBaseLength, f3_center + new Vector3(-f_quarter, f_quarter, -f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 1], f_newBaseLength, f3_center + new Vector3(f_quarter, f_quarter, -f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 2], f_newBaseLength, f3_center + new Vector3(-f_quarter, f_quarter, f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 3], f_newBaseLength, f3_center + new Vector3(f_quarter, f_quarter, f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 4], f_newBaseLength, f3_center + new Vector3(-f_quarter, -f_quarter, -f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 5], f_newBaseLength, f3_center + new Vector3(f_quarter, -f_quarter, -f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 6], f_newBaseLength, f3_center + new Vector3(-f_quarter, -f_quarter, f_quarter) ) ;
            CommonMethods._CreateNewNode ( l_nodeChildrenNodesIndex [i_childrenIndexOffset + 7], f_newBaseLength, f3_center + new Vector3(f_quarter, -f_quarter, f_quarter) ) ;
            */

	    }
        

        /// <summary>
        /// Assign instance to node.
        /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
        /// <param name="i_instanceID">External instance index.</param>
        /// <param name="instanceBounds">Boundary of external instance index.</param>
        private void _AssingInstance2Node ( RootNodeData rootNodeData, int i_nodeIndex, int i_instanceID, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer )
        {
            int i_nodeInstanceIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;

            // Reuse spare store
            InstancesSpareIndexBufferElement instancesSpareIndexBuffer = a_instancesSpareIndexBuffer [rootNodeData.i_instancesSpareLastIndex] ;
            // int i_spareInstanceIndex = l_instancesSpare [rootData.i_instancesSpareLastIndex] ;     

            NodeInstancesIndexBufferElement nodeInstancesIndexBuffer ;

            // Find next spare instance allocation for this node.
            for (int i = 0; i < rootNodeData.i_instancesAllowedCount; i++) 
            {

                int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_instanceIndexOffset] ;

                // Is spare.
                if ( nodeInstancesIndexBuffer.i == -1 )
                {
                    // Assign instance index.
                    nodeInstancesIndexBuffer.i = instancesSpareIndexBuffer.i ;
                    a_nodeInstancesIndexBuffer [i_instanceIndexOffset] = nodeInstancesIndexBuffer ; // Set back.
                    break ;
                }
            }
            
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            nodeBuffer.i_instancesCount ++ ;
            a_nodesBuffer [i_nodeIndex] = nodeBuffer ; // Set back.
            //l_nodeInstancesCount [i_nodeIndex] ++ ;

            InstanceBufferElement instanceBuffer = new InstanceBufferElement () 
            { 
                i_ID = i_instanceID, 
                bounds = instanceBounds 
            } ;
            
            a_instanceBuffer [instancesSpareIndexBuffer.i] = instanceBuffer ;
            // l_instancesBounds [instancesSpareIndexBuffer.i] = instanceBounds ;
            // l_instancesID [instancesSpareIndexBuffer.i]     = i_instanceID ;
        }
        

        /// <summary>
	    /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <returns>True there are less or the same abount of objects in this and its children than numObjectsAllowed.</returns>
	    private bool _ShouldMerge ( RootNodeData rootNodeData, int i_nodeIndex, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer ) 
        {
            
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    int i_totalInstancesCount       = nodeBuffer.i_instancesCount ;
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

            int i_childrenCount           = nodeBuffer.i_childrenCount ;

		    // Has children?
		    if ( i_childrenCount > 0 ) 
            {
                for ( int i = 0; i < 8; i ++ )
                {
                    NodeChildrenBufferElement nodeChildBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_childNodeIndex = nodeChildBuffer.i_nodesIndex ;
                
                    if ( i_childNodeIndex >= 0 ) // validate
                    {
                        nodeBuffer = a_nodesBuffer [i_childNodeIndex] ;
                        int i_nodefChildChildrenCount = nodeBuffer.i_childrenCount ;

                        if ( i_nodefChildChildrenCount > 0 ) 
                        {
					        // If any of the *children* have children, there are definitely too many to merge,
					        // or the child would have been merged already
					        return false;
				        }

				        i_totalInstancesCount += nodeBuffer.i_instancesCount ;
                    
                        i_childrenCount -- ;

                        if ( i_childrenCount == 0 ) break ;

                    }

                }
            
		    }

		    return i_totalInstancesCount <= rootNodeData.i_instancesAllowedCount ;

	    }


    }
}
