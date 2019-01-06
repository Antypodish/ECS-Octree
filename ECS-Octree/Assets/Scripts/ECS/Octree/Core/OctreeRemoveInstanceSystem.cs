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

    [UpdateAfter ( typeof ( AddInstanceSystem ) ) ]    
    class OctreeRemoveInstanceSystem : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Add New Octree Instance System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                //typeof (EntityArray),
                typeof (RemoveInstanceTag), 
                typeof (RootNodeData) 
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            Debug.Log ( "Remove Octree Instance." ) ;


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



            for ( int i_instanceID = 0; i_instanceID < 50; i_instanceID ++ )
            {            
                int x = i_instanceID % 10 ;
                int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;

                _OctreeRemoveInstance ( 
                    ref rootNodeData, 
                    i_instanceID, 
                    a_nodesBuffer, 
                    ref a_nodeSparesBuffer,
                    ref a_nodeChildrenBuffer,                    
                    a_nodeInstancesIndexBuffer,
                    ref a_instanceBuffer,
                    ref a_instancesSpareIndexBuffer
                ) ;

            }

            
            a_rootNodeData [0] = rootNodeData ;
            
            EntityManager.RemoveComponent <RemoveInstanceTag> ( rootNodeEntity ) ; // Instance added.

            return base.OnUpdate ( inputDeps );

        }

        
        /// <summary>
	    /// Remove an instance. Makes the assumption that the instance only exists once in the tree.
	    /// </summary>
	    /// <param name="i_instanceID">External instance to remove.</param>
	    /// <returns>True if the object was removed successfully.</returns>
	    public bool _OctreeRemoveInstance ( ref RootNodeData rootNodeData, int i_instanceID, DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {
		
            bool removed = _NodeRemoveInstance ( 
                ref rootNodeData, 
                rootNodeData.i_rootNodeIndex, 
                i_instanceID, 
                ref a_nodesBuffer, 
                ref a_nodeSparesBuffer, 
                a_nodeChildrenBuffer,                    
                a_nodeInstancesIndexBuffer,
                ref a_instanceBuffer,
                ref a_instancesSpareIndexBuffer
            );

		    // See if we can shrink the octree down now that we've removed the item
		    if ( removed ) 
            {            
			    rootNodeData.i_totalInstancesCountInTree -- ;
                
                // Shrink if possible.
                rootNodeData.i_rootNodeIndex = _ShrinkIfPossible ( 
                    rootNodeData, 
                    rootNodeData.i_rootNodeIndex, 
                    rootNodeData.f_initialSize, 
                    a_nodesBuffer, 
                    ref a_nodeChildrenBuffer,
                    a_nodeInstancesIndexBuffer,
                    a_instanceBuffer
                ) ;
		    }

		    return removed ;
	    }

               

        /// <summary>
	    /// Remove an instace. Makes the assumption that the instance only exists once in the tree.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>
	    /// <returns>True if the object was removed successfully.</returns>
	    private bool _NodeRemoveInstance ( ref RootNodeData rootNodeData, int i_nodeIndex, int i_instanceID, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

		    bool removed = false;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            //int i_nodeInstanceCount = l_nodeInstancesCount [i_nodeIndex] ;
                
            int i_nodeInstancesIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;

            if ( nodeBuffer.i_instancesCount > 0 )
            {

                // Try remove instance from this node
                for (int i = 0; i < rootNodeData.i_instancesAllowedCount; i++) 
		        // for (int i = 0; i < l_nodeInstancesCount [i_nodeIndex]; i++) 
                {

                    
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;
                    int i_existingInstanceIndex = nodeInstancesIndexBuffer.i ;
                    // int i_existingInstanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;

                    // If instance exists
                    if ( i_existingInstanceIndex >= 0 )
                    {
                 
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_existingInstanceIndex] ;
                        if ( instanceBuffer.i_ID == i_instanceID ) 			        
                        {   
                            removed = true ;
                            
                            // Remove from here
                            CommonMethods._PutBackSpareInstance ( ref rootNodeData, i_existingInstanceIndex, i_nodeIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
                            //CommonMethods._PutBackSpareInstance ( i_existingInstanceIndex, i_nodeIndex ) ;

                            // Debugging
    GameObject go = GameObject.Find ( "Instance " + i_instanceID.ToString () ) ;

    if ( go != null ) 
    {
        Debug.Log ( "Instance: Hide game object #" + i_instanceID.ToString () ) ;
        go.SetActive ( false ) ;
        // go.transform.localScale = instanceBounds.size ;
    }

                            nodeBuffer.i_instancesCount -- ;
                            // l_nodeInstancesCount [i_nodeIndex] -- ;
                            instanceBuffer.i_ID = -1 ; // Reset
                            a_instanceBuffer [i_existingInstanceIndex] = instanceBuffer ; // Set back
                            // l_instancesID [i_existingInstanceIndex] = -1 ;
                    
                            
    Debug.LogWarning ( "Node: Remove #" + i_nodeIndex ) ;
    GameObject.Destroy ( GameObject.Find ( "Node " + i_nodeIndex.ToString () ) ) ;

				            break;
			            }
                
                    }           

		        } // for

                a_nodesBuffer [i_nodeIndex] = nodeBuffer ; // Set back

            }

            
            // nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            int i_nodeChildrenCount = nodeBuffer.i_childrenCount ;
            // int i_nodeChildrenCount = l_nodeChildrenCount [i_nodeIndex] ;
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

            // Try remove instance from this node children, if node don't have this instance
		    if ( !removed && i_nodeChildrenCount > 0 ) 
            {
			    for (int i = 0; i < 8; i++) 
                {
                    // Get children index of this node
                    
                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_childNodeIndex = nodeChildrenBuffer.i_nodesIndex ;
                    // int i_childNodeIndex = l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i] ;

                    // Ignore negative index
                    if ( i_childNodeIndex >= 0 )
                    {
                        removed = _NodeRemoveInstance ( 
                            ref rootNodeData, 
                            i_childNodeIndex, 
                            i_instanceID, 
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            a_nodeChildrenBuffer,
                            a_nodeInstancesIndexBuffer,
                            ref a_instanceBuffer,
                            ref a_instancesSpareIndexBuffer
                        ) ;
				   
				        if ( removed ) break ;
                    }
			    }
		    }

		    if ( removed && i_nodeChildrenCount > 0 )
            {
			    // Check if we should merge nodes now that we've removed an item
			    if ( _ShouldMerge ( ref rootNodeData, i_nodeIndex, a_nodesBuffer, ref a_nodeChildrenBuffer ) ) 
                {
				    _MergeNodes ( 
                        ref rootNodeData, 
                        i_nodeIndex, 
                        ref a_nodesBuffer, 
                        ref a_nodeSparesBuffer,
                        a_nodeChildrenBuffer,
                        a_nodeInstancesIndexBuffer,
                        ref a_instancesSpareIndexBuffer
                    ) ;
			    }
		    }

		    return removed;
	    }


        /// <summary>
	    /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <returns>True there are less or the same abount of objects in this and its children than numObjectsAllowed.</returns>
	    private bool _ShouldMerge ( ref RootNodeData rootNodeData, int i_nodeIndex, DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer ) 
        {
                        
            NodeBufferElement nodeBuffer    = a_nodesBuffer [i_nodeIndex] ;

		    int i_totalInstancesCount       = nodeBuffer.i_instancesCount ;
            int i_nodeChildrenIndexOffset   = i_nodeIndex * 8 ;

            int i_childrenCount             = nodeBuffer.i_childrenCount ;

		    // Has children?
		    if ( i_childrenCount > 0 ) 
            {
                for ( int i = 0; i < 8; i ++ )
                {

                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_childNodeIndex = nodeChildrenBuffer.i_nodesIndex ;
                
                    if ( i_childNodeIndex >= 0 )
                    {
                        nodeBuffer = a_nodesBuffer [i_childNodeIndex] ;
                        int i_nodefChildChildrenCount = nodeBuffer.i_childrenCount ;

                        if ( i_nodefChildChildrenCount > 0 ) 
                        {
					        // If any of the *children* have children, there are definitely too many to merge,
					        // or the child would have been merged already
					        return false;
				        }

				        i_totalInstancesCount += nodeBuffer.i_instancesCount;
                    
                        i_childrenCount -- ;

                        if ( i_childrenCount == 0 ) break ;

                    }

                }
            
		    }

		    return i_totalInstancesCount <= rootNodeData.i_instancesAllowedCount ;

	    }


        /// <summary>
	    /// Merge all children into this node - the opposite of Split.
	    /// Note: We only have to check one level down since a merge will never happen if the children already have children,
	    /// since THAT won't happen unless there are already too many objects to merge.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    private void _MergeNodes ( ref RootNodeData rootNodeData, int i_nodeIndex, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

            NodeBufferElement nodeBuffer ;
            NodeChildrenBufferElement nodeChildrenBuffer ;
            NodeInstancesIndexBufferElement nodeInstancesIndexBuffer ;

            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeUnusedInstancesIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;
        
		    // Note: We know children != null or we wouldn't be merging
	        for (int i = 0; i < 8; i++) 
            {
            
                nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                int i_childNodeIndex = nodeChildrenBuffer.i_nodesIndex ;
            
                if ( i_childNodeIndex >= 0 )
                {
                
                    nodeBuffer = a_nodesBuffer [i_childNodeIndex] ;
                    int i_childNodeInstanceCount = nodeBuffer.i_instancesCount ;
                
                    if ( i_childNodeInstanceCount > 0 ) 
                    {


                        int i_childModeInstancesIndexOffset = i_childNodeIndex * rootNodeData.i_instancesAllowedCount ;

                        for (int i_unusedInstance = 0; i_unusedInstance < rootNodeData.i_instancesAllowedCount; i_unusedInstance++) 
                        {

                            int i_unusedInstanceIndexOffset = i_nodeUnusedInstancesIndexOffset + i_unusedInstance ;
                    
                            
                            nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_unusedInstanceIndexOffset] ;

                            if ( nodeInstancesIndexBuffer.i == -1 )
                            // if ( l_nodeInstancesIndex [i_unusedInstanceIndexOffset] == -1 )
                            {
                              
                                // Iterate through number of children instances.
			                    for (int j = rootNodeData.i_instancesAllowedCount - 1; j >= 0; j--) 
                                {

                                    // Store old instance index
                                    nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_childModeInstancesIndexOffset + j] ;
                                    int i_childInstanceIndex = nodeInstancesIndexBuffer.i ;
                                    // int i_childInstanceIndex = l_nodeInstancesIndex [i_childModeInstancesIndexOffset + j] ;
                                
                                    // If node instance exists (none negative), assign to node
                                    if ( i_childInstanceIndex >= 0 )
                                    {
                                        // Reassign instance index, to next available spare index.                        
                                        nodeInstancesIndexBuffer.i                                       = i_childInstanceIndex ;
                                        a_nodeInstancesIndexBuffer [i_unusedInstanceIndexOffset]         = nodeInstancesIndexBuffer ; // Set back
                                        //l_nodeInstancesIndex [i_unusedInstanceIndexOffset]             = i_childInstanceIndex ;
                                        nodeBuffer                                                       = a_nodesBuffer [i_nodeIndex] ;
                                        nodeBuffer.i_instancesCount ++ ;
                                        a_nodesBuffer [i_nodeIndex]                                      = nodeBuffer ; // Set back
                                        // l_nodeInstancesCount [i_nodeIndex] ++ ;
                
                                        nodeInstancesIndexBuffer.i                                       = -1 ; // Reset
                                        a_nodeInstancesIndexBuffer [i_childModeInstancesIndexOffset + j] = nodeInstancesIndexBuffer ; // Set back
                                        // l_nodeInstancesIndex [i_childModeInstancesIndexOffset + j]    = -1 ; // Reset
                                        nodeBuffer                                                       = a_nodesBuffer [i_childNodeIndex] ;
                                        nodeBuffer.i_instancesCount -- ;
                                        a_nodesBuffer [i_childNodeIndex]                                 = nodeBuffer ; // Set back
                                        // l_nodeInstancesCount [i_childNodeIndex] -- ;

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
                nodeChildrenBuffer = a_nodeChildrenBuffer [i_childNodeIndexOffset] ;
                int i_childNodeIndex = nodeChildrenBuffer.i_nodesIndex ;

                if ( i_childNodeIndex >= 0 )
                {
                    // Get child node ;
                    nodeBuffer = a_nodesBuffer [i_childNodeIndex] ;

                    // Iterate though node children.
                    if ( nodeBuffer.i_childrenCount > 0 )
                    {

                        // Reset node children node index reference.
                        for (int j = 0; j < 8; j++) 
                        {
                            // int i_childIndexOffset = i_childNodeIndex + j ;
                            
                            // nodeChildrenBuffer = a_nodeChildrenBuffer [i_childIndexOffset] ;
                            // Reset child
                            nodeChildrenBuffer.i_nodesIndex = -1 ; // Bounds are ignored
                            a_nodeChildrenBuffer [i_childNodeIndex + j] = nodeChildrenBuffer ; // Set back
                            //l_nodeChildrenNodesIndex [i_childNodeIndex + j] = -1 ; // Reset child
                        }
            
                        nodeBuffer.i_instancesCount = 0 ; // Reset
                        a_nodesBuffer [i_childNodeIndex] = nodeBuffer ; // Set back
                        // l_nodeInstancesCount [i_childNodeIndex] = 0 ;

                        // Put back node instances to spare instance.
                        for (int j = 0; j < rootNodeData.i_instancesAllowedCount; j++) 
                        {
                            
                            nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_childNodeIndex] ; // Set back
                            int i_instanceIndex = nodeInstancesIndexBuffer.i ;
                            // int i_instanceIndex = l_nodeInstancesIndex [i_childNodeIndex] ;
                            // Remove from here
                            CommonMethods._PutBackSpareInstance ( ref rootNodeData, i_instanceIndex + j, i_nodeIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
                            // _PutBackSpareInstance ( i_instanceIndex + j, i_nodeIndex ) ; 
                        }

                    }

                    // Pu back child nodes to spares
                    rootNodeData.i_nodeSpareLastIndex ++ ;
                    a_nodeSparesBuffer [rootNodeData.i_nodeSpareLastIndex]  = new NodeSparesBufferElement () { i = i_childNodeIndex } ;
                    // l_nodeSpares [rootNodeData.i_nodeSpareLastIndex]     = i_childNodeIndex ;
             
                    // Reset child
                    nodeChildrenBuffer.i_nodesIndex                         = -1 ; // Bounds are ignored
                    a_nodeChildrenBuffer [i_childNodeIndexOffset]           = nodeChildrenBuffer ; // Set back
                    // nodeChildrenBuffer                                   = a_nodeChildrenBuffer [i_childNodeIndexOffset] ;   
                }
            }

            
            nodeBuffer                   = a_nodesBuffer [i_nodeIndex] ;
            nodeBuffer.i_childrenCount   = 0 ;
            a_nodesBuffer [i_nodeIndex]  = nodeBuffer ; // Set back
            // l_nodeChildrenCount [i_nodeIndex] = 0 ;

	    }
        
        
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
	    private int _ShrinkIfPossible ( RootNodeData rootNodeData, int i_nodeIndex, float minLength, DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 
        {
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            // float f_baseLength = nodeBuffer.f_baseLength ;
            
                
            NodeChildrenBufferElement nodeChildrenBuffer ;


		    if ( nodeBuffer.f_baseLength < ( 2 * minLength ) ) 
            {
			    return i_nodeIndex ;
		    }
        
            // int i_nodeChildrenCount = l_nodeChildrenCount [i_nodeIndex] ;
            // int i_nodeInstanceCount = l_nodeInstancesCount [i_nodeIndex] ;

		    if ( nodeBuffer.i_instancesCount == 0 && nodeBuffer.i_childrenCount == 0 ) 
            {
			    return i_nodeIndex ;
		    }

            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeInstancesIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;

		
            // -1 to 7, where -1 is no result found
		    int i_bestFit = -1;

            // Check objects in root
		    for (int i = 0; i < rootNodeData.i_instancesAllowedCount; i++) 
            {

                if ( nodeBuffer.i_instancesCount == 0 )  break ;

                NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;
                // int i_instanceIndex = l_nodeInstancesIndex [i_nodeInstancesIndexOffset + i] ;


                if ( nodeInstancesIndexBuffer.i >= 0 )
                {
                    InstanceBufferElement instanceBuffer = a_instanceBuffer [nodeInstancesIndexBuffer.i] ;
                    // Bounds instanceBounds = l_instancesBounds [nodeInstancesIndexBuffer.i] ;

                    int newBestFit = CommonMethods._BestFitChild ( i_nodeIndex, instanceBuffer.bounds, a_nodesBuffer ) ;
			        // int newBestFit = CommonMethods._BestFitChild ( i_nodeIndex, instanceBounds ) ;
			
                    if (i == 0 || newBestFit == i_bestFit) 
                    {
                        
                        nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + newBestFit] ;

				        // In same octant as the other(s). Does it fit completely inside that octant?
                        if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, instanceBuffer.bounds ) ) 
                        // if ( CommonMethods._Encapsulates ( l_nodeChildrenBounds [i_nodeChildrenIndexOffset + newBestFit], instanceBuffer.bounds ) ) 
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
		    if ( nodeBuffer.i_childrenCount > 0 ) 
            {
			    bool childHadContent = false ;
            
                for (int i = 0; i < 8; i++) 
                {

                    nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;

                    // Has child any instances
                    if ( CommonMethods._HasAnyInstances ( nodeChildrenBuffer.i_nodesIndex, a_nodesBuffer, a_nodeChildrenBuffer ) )
                    //if ( CommonMethods._HasAnyInstances ( l_nodeChildrenNodesIndex [ i_nodeChildrenIndexOffset + i ], a_nodesBuffer, a_nodeChildrenBuffer ) )
				    // if ( _HasAnyInstances ( l_nodeChildrenNodesIndex [ i_nodeChildrenIndexOffset + i ] ) ) 
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
		    else if ( nodeBuffer.i_childrenCount == 0 ) 
            {
                nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i_bestFit] ;
                Bounds childBounds = nodeChildrenBuffer.bounds ;
                // Bounds childBounds = l_nodeChildrenBounds [i_nodeChildrenIndexOffset + i_bestFit] ;

			    // We don't have any children, so just shrink this node to the new size
			    // We already know that everything will still fit in it
			    CommonMethods._SetValues ( rootNodeData, i_nodeIndex, nodeBuffer.f_baseLength / 2, childBounds.center, ref a_nodesBuffer, ref a_nodeChildrenBuffer ) ;

			    return i_nodeIndex ;
		    }

		    // No objects in entire octree
		    if ( i_bestFit == -1 ) 
            {
			    return i_nodeIndex ;
		    }


		    // We have children. Use the appropriate child as the new root node
            nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i_bestFit] ;
            return nodeChildrenBuffer.i_nodesIndex ;
            // return l_nodeChildrenNodesIndex [i_nodeChildrenIndexOffset + i_bestFit] ;
	    }


    }
}
