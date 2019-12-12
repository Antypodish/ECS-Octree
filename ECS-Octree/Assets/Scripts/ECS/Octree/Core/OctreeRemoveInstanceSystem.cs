using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
        
    [UpdateAfter ( typeof ( Octree.AddNewOctreeSystem ) ) ]
    [UpdateAfter ( typeof ( Octree.AddInstanceTag ) ) ]
    class RemoveInstanceSystem : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Remove Octree Instance System" ) ;
            
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;

            group = GetEntityQuery 
            ( 
                typeof ( IsActiveTag ), 
                typeof ( RemoveInstanceBufferElement ),
                typeof ( RemoveInstanceTag ),
                typeof ( RootNodeData ) 
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            
            Debug.Log ( "Remove Octree Instance." ) ;

            // int i_groupLength = group.CalculateLength () ;

            JobHandle removeInstanceJobHandle = new RemoveInstanceJob 
            {            
                // a_octreeEntities                    = group.GetEntityArray (),

                // Contains a list of instances to add, with its properties.
                // removeInstanceBufferElement         = GetBufferFromEntity <RemoveInstanceBufferElement> (),

                a_rootNodeData                      = GetComponentDataFromEntity <RootNodeData> (),

                nodeSparesBufferElement             = GetBufferFromEntity <NodeSparesBufferElement> (),
                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> (),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> (),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> (),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> (),
                instancesSpareIndexBufferElement    = GetBufferFromEntity <InstancesSpareIndexBufferElement> ()

            }.Schedule ( group, inputDeps ) ;


            
            JobHandle completeRemoveInstanceJobHandle = new CompleteRemoveInstanceJob 
            {
                
                ecb                                 = eiecb.CreateCommandBuffer ().ToConcurrent (),         
                // a_octreeEntities                 = group.GetEntityArray ()

            }.Schedule ( group, removeInstanceJobHandle ) ;
            
            eiecb.AddJobHandleForProducer ( completeRemoveInstanceJobHandle ) ;

            return completeRemoveInstanceJobHandle ;

        }

        [BurstCompile]
        [RequireComponentTag ( typeof ( RemoveInstanceTag ) ) ]
        struct RemoveInstanceJob : IJobForEachWithEntity_EB <RemoveInstanceBufferElement>
        // struct RemoveInstanceJob : IJobParallelFor 
        {

            // [ReadOnly] public EntityArray a_octreeEntities ;

            // Contains a list of instances to add, with its properties.
            // [NativeDisableParallelForRestriction]            
            //public BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement ;
            
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <RootNodeData> a_rootNodeData ;
            
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeSparesBufferElement> nodeSparesBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeBufferElement> nodeBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <InstancesSpareIndexBufferElement> instancesSpareIndexBufferElement ;

            
            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBuffer )
            // public void Execute ( int i_arrayIndex )
            {
                
                // Entity octreeRootNodeEntity = a_octreeEntities [i_arrayIndex] ;

                // DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement           = removeInstanceBufferElement [octreeRootNodeEntity] ;    
                            
                RootNodeData rootNode = a_rootNodeData [octreeRootNodeEntity] ;

                DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer                          = nodeSparesBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                                     = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer          = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                      = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                              = instanceBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer        = instancesSpareIndexBufferElement [octreeRootNodeEntity] ;



                // Iterate through number of instances to add, from the buffer
                for ( int i = 0; i < a_removeInstanceBuffer.Length; i ++ )
                {
                    
                    // RootNodeData rootNodeData = a_rootNodeData [octreeRootNodeEntity] ;

                    RemoveInstanceBufferElement removeInstanceBuffer = a_removeInstanceBuffer [i] ;

                    bool removed = _NodeRemoveInstance ( 
                        ref rootNode, 
                        rootNode.i_rootNodeIndex, 
                        removeInstanceBuffer.i_instanceID, 
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
			            rootNode.i_totalInstancesCountInTree -- ;
                
                        // Shrink if possible.
                        rootNode.i_rootNodeIndex = _ShrinkIfPossible ( 
                            rootNode, 
                            rootNode.i_rootNodeIndex, 
                            rootNode.f_initialSize, 
                            a_nodesBuffer, 
                            ref a_nodeChildrenBuffer,
                            a_nodeInstancesIndexBuffer,
                            a_instanceBuffer
                        ) ;
		            }
                    
                    a_rootNodeData [octreeRootNodeEntity] = rootNode ;

                } // for

                a_removeInstanceBuffer.ResizeUninitialized ( 0 ) ; // Clear buffer, after removing is complete.

            }

        } // Job


        [RequireComponentTag ( typeof ( RemoveInstanceTag ) ) ]
        struct CompleteRemoveInstanceJob : IJobForEachWithEntity_EB <RemoveInstanceBufferElement>
        // struct CompleteRemoveInstanceJob : IJobParallelFor 
        {

            public EntityCommandBuffer.Concurrent ecb ;
            // [ReadOnly] public EntityArray a_octreeEntities ;
                        
            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, [ReadOnly] DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBuffer )
            // public void Execute ( int i_arrayIndex )
            {
                
                // Entity octreeRootNodeEntity = a_octreeEntities [jobIndex] ;

                // Remove component, as instances has been already removed.
                ecb.RemoveComponent <RemoveInstanceTag> ( jobIndex, octreeRootNodeEntity ) ;                
            }

        } // Job


        /*
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
        */
               

        /// <summary>
	    /// Remove an instace. Makes the assumption that the instance only exists once in the tree.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>
	    /// <returns>True if the object was removed successfully.</returns>
	    static private bool _NodeRemoveInstance ( ref RootNodeData rootNode, int i_nodeIndex, int i_instanceID, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

		    bool removed = false;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
                
            int i_nodeInstancesIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;

            if ( nodeBuffer.i_instancesCount > 0 )
            {

                // Try remove instance from this node
                for (int i = 0; i < rootNode.i_instancesAllowedCount; i++) 
                {

                    int i_nodeInstanceIndex = i_nodeInstancesIndexOffset + i ;

                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;
                    int i_existingInstanceIndex = nodeInstancesIndexBuffer.i ;

                    // If instance exists
                    if ( i_existingInstanceIndex >= 0 )
                    {
                 
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_existingInstanceIndex] ;
                        if ( instanceBuffer.i_ID == i_instanceID ) 			        
                        {   
                            removed = true ;
                            
                            // Remove from here
                            CommonMethods._PutBackSpareInstance ( ref rootNode, i_existingInstanceIndex, i_nodeInstanceIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
                            
/*
                            // Debugging
    GameObject go = GameObject.Find ( "Instance " + i_instanceID.ToString () ) ;

    if ( go != null ) 
    {
        Debug.Log ( "Instance: Hide game object #" + i_instanceID.ToString () ) ;
        go.SetActive ( false ) ;
        // go.transform.localScale = instanceBounds.size ;
    }

       Debug.LogWarning ( "Node: Remove #" + i_nodeIndex ) ;
    GameObject.Destroy ( GameObject.Find ( "Node " + i_nodeIndex.ToString () ) ) ;
*/
                            nodeBuffer.i_instancesCount -- ;
                            instanceBuffer.i_ID = -1 ; // Reset
                            a_instanceBuffer [i_existingInstanceIndex] = instanceBuffer ; // Set back
                    
                            
 
				            break;
			            }
                
                    }           

		        } // for

                a_nodesBuffer [i_nodeIndex] = nodeBuffer ; // Set back

            }

            
            int i_nodeChildrenCount = nodeBuffer.i_childrenCount ;
            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

            // Try remove instance from this node children, if node don't have this instance
		    if ( !removed && i_nodeChildrenCount > 0 ) 
            {
			    for (int i = 0; i < 8; i++) 
                {
                    // Get children index of this node
                    
                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_childNodeIndex = nodeChildrenBuffer.i_nodesIndex ;

                    // Ignore negative index
                    if ( i_childNodeIndex >= 0 )
                    {
                        removed = _NodeRemoveInstance ( 
                            ref rootNode, 
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
			    if ( _ShouldMerge ( ref rootNode, i_nodeIndex, a_nodesBuffer, ref a_nodeChildrenBuffer ) ) 
                {
				    _MergeNodes ( 
                        ref rootNode, 
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
	    static private bool _ShouldMerge ( ref RootNodeData rootNode, int i_nodeIndex, DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer ) 
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

		    return i_totalInstancesCount <= rootNode.i_instancesAllowedCount ;

	    }


        /// <summary>
	    /// Merge all children into this node - the opposite of Split.
	    /// Note: We only have to check one level down since a merge will never happen if the children already have children,
	    /// since THAT won't happen unless there are already too many objects to merge.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    static private void _MergeNodes ( ref RootNodeData rootNode, int i_nodeIndex, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

            NodeBufferElement nodeBuffer ;
            NodeChildrenBufferElement nodeChildrenBuffer ;
            NodeInstancesIndexBufferElement nodeInstancesIndexBuffer ;

            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeUnusedInstancesIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;
        
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


                        int i_childModeInstancesIndexOffset = i_childNodeIndex * rootNode.i_instancesAllowedCount ;
                        int i_instancesAllowedCount = rootNode.i_instancesAllowedCount - 1 ;

                        for (int i_unusedInstance = 0; i_unusedInstance < rootNode.i_instancesAllowedCount; i_unusedInstance++) 
                        {

                            int i_unusedInstanceIndexOffset = i_nodeUnusedInstancesIndexOffset + i_unusedInstance ;
                    
                            
                            nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_unusedInstanceIndexOffset] ;

                            if ( nodeInstancesIndexBuffer.i == -1 )                            
                            {
                              
                                // Iterate through number of children instances.
                                while ( i_instancesAllowedCount >= 0 ) 
			                    //for (int j = rootNodeData.i_instancesAllowedCount - 1; j >= 0; j--) 
                                {

                                    int i_nodeIstancesIndexOffset = i_childModeInstancesIndexOffset + i_instancesAllowedCount ;

                                    // Store old instance index
                                    nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeIstancesIndexOffset] ;
                                    int i_childInstanceIndex = nodeInstancesIndexBuffer.i ;
                                    
                                
                                    // If node instance exists (none negative), assign to node
                                    if ( i_childInstanceIndex >= 0 )
                                    {
                                        // Reassign instance index, to next available spare index.                        
                                        nodeInstancesIndexBuffer.i                                       = i_childInstanceIndex ;
                                        a_nodeInstancesIndexBuffer [i_unusedInstanceIndexOffset]         = nodeInstancesIndexBuffer ; // Set back
                                        
                                        nodeBuffer                                                       = a_nodesBuffer [i_nodeIndex] ;
                                        nodeBuffer.i_instancesCount ++ ;
                                        a_nodesBuffer [i_nodeIndex]                                      = nodeBuffer ; // Set back
                                        
                
                                        nodeInstancesIndexBuffer.i                                       = -1 ; // Reset
                                        a_nodeInstancesIndexBuffer [i_nodeIstancesIndexOffset]           = nodeInstancesIndexBuffer ; // Set back
                                        
                                        nodeBuffer                                                       = a_nodesBuffer [i_childNodeIndex] ;
                                        nodeBuffer.i_instancesCount -- ;
                                        a_nodesBuffer [i_childNodeIndex]                                 = nodeBuffer ; // Set back
                                        
                                        i_childNodeInstanceCount -- ;
                                                         
                                        i_instancesAllowedCount -- ;

                                        break ;
                                    }
                                    else
                                    {
                                        i_instancesAllowedCount -- ;
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
                            // Reset child
                            nodeChildrenBuffer.i_nodesIndex = -1 ; // Bounds are ignored
                            a_nodeChildrenBuffer [i_childNodeIndex + j] = nodeChildrenBuffer ; // Set back                            
                        }
            
                        nodeBuffer.i_instancesCount = 0 ; // Reset
                        a_nodesBuffer [i_childNodeIndex] = nodeBuffer ; // Set back
                        
                        // Put back node instances to spare instance.
                        for (int j = 0; j < rootNode.i_instancesAllowedCount; j++) 
                        {
                            
                            nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_childNodeIndex] ; // Set back
                            int i_instanceIndex = nodeInstancesIndexBuffer.i ;
                            
                            // Remove from here
                            CommonMethods._PutBackSpareInstance ( ref rootNode, i_instanceIndex + j, i_nodeIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
                            
                        }

                    }

                    // Pu back child nodes to spares
                    rootNode.i_nodeSpareLastIndex ++ ;
                    a_nodeSparesBuffer [rootNode.i_nodeSpareLastIndex]  = new NodeSparesBufferElement () { i = i_childNodeIndex } ;
             
                    // Reset child
                    nodeChildrenBuffer.i_nodesIndex                         = -1 ; // Bounds are ignored
                    a_nodeChildrenBuffer [i_childNodeIndexOffset]           = nodeChildrenBuffer ; // Set back
                }
            }

            
            nodeBuffer                   = a_nodesBuffer [i_nodeIndex] ;
            nodeBuffer.i_childrenCount   = 0 ;
            a_nodesBuffer [i_nodeIndex]  = nodeBuffer ; // Set back

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
	    static private int _ShrinkIfPossible ( RootNodeData rootNode, int i_nodeIndex, float minLength, DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 
        {

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            
                
            NodeChildrenBufferElement nodeChildrenBuffer ;


		    if ( nodeBuffer.f_baseLength < ( 2 * minLength ) ) 
            {
			    return i_nodeIndex ;
		    }

		    if ( nodeBuffer.i_instancesCount == 0 && nodeBuffer.i_childrenCount == 0 ) 
            {
			    return i_nodeIndex ;
		    }

            int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;
            int i_nodeInstancesIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;

		
            // -1 to 7, where -1 is no result found
		    int i_bestFit = -1;

            // Check objects in root
		    for (int i = 0; i < rootNode.i_instancesAllowedCount; i++) 
            {

                if ( nodeBuffer.i_instancesCount == 0 )  break ;

                NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;


                if ( nodeInstancesIndexBuffer.i >= 0 )
                {
                    InstanceBufferElement instanceBuffer = a_instanceBuffer [nodeInstancesIndexBuffer.i] ;

                    int newBestFit = CommonMethods._BestFitChild ( i_nodeIndex, instanceBuffer.bounds, a_nodesBuffer ) ;
			
                    if (i == 0 || newBestFit == i_bestFit) 
                    {
                        
                        nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + newBestFit] ;

				        // In same octant as the other(s). Does it fit completely inside that octant?
                        if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, instanceBuffer.bounds ) ) 
                        {
					        if ( i_bestFit < 0 ) 
                            {
						        i_bestFit = newBestFit ;
					        }
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

			    // We don't have any children, so just shrink this node to the new size
			    // We already know that everything will still fit in it
			    CommonMethods._SetValues ( rootNode, i_nodeIndex, nodeBuffer.f_baseLength / 2, childBounds.center, ref a_nodesBuffer, ref a_nodeChildrenBuffer ) ;

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

	    }


    }
}
