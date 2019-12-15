using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    
    [UpdateAfter ( typeof ( AddNewOctreeSystem ) ) ]    
    class AddInstanceSystem : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Add New Octree Instance System" ) ;
            
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;

            group = GetEntityQuery 
            ( 
                typeof ( IsActiveTag ), 
                typeof ( AddInstanceBufferElement ),
                typeof ( AddInstanceTag ),
                typeof ( RootNodeData ) 
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
         
            Debug.Log ( "Add New Octree Instance" ) ;


            // int i_groupLength = group.CalculateLength () ;

            JobHandle addInstanceJobHandle = new AddInstanceJob 
            {            
                // a_octreeEntities                 = group.GetEntityArray (),

                // Contains a list of instances to add, with its properties.
                // addInstanceBufferElement            = GetBufferFromEntity <AddInstanceBufferElement> (),

                a_rootNodeData                      = GetComponentDataFromEntity <RootNodeData> (),

                nodeSparesBufferElement             = GetBufferFromEntity <NodeSparesBufferElement> (),
                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> (),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> (),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> (),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> (),
                instancesSpareIndexBufferElement    = GetBufferFromEntity <InstancesSpareIndexBufferElement> ()

            }.Schedule ( group, inputDeps ) ;


            
            JobHandle completeAddInstanceJobHandle = new CompleteAddInstanceJob 
            {
                
                ecb                                 = eiecb.CreateCommandBuffer ().ToConcurrent (),                
                // a_octreeEntities                    = group.GetEntityArray ()

            }.Schedule ( group, addInstanceJobHandle ) ;
            
            eiecb.AddJobHandleForProducer ( completeAddInstanceJobHandle ) ;

            return completeAddInstanceJobHandle ;
        }
        

        [BurstCompile]
        [RequireComponentTag ( typeof ( AddInstanceTag ) ) ]
        struct AddInstanceJob : IJobForEachWithEntity_EB <AddInstanceBufferElement>
        // struct AddInstanceJob : IJobParallelFor 
        {

            // [ReadOnly] public EntityArray a_octreeEntities ;

            // Contains a list of instances to add, with its properties.
            // [NativeDisableParallelForRestriction]            
            // public BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement ;

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


            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, DynamicBuffer <AddInstanceBufferElement> a_addInstanceBuffer )
            // public void Execute ( int i_arrayIndex )
            {
                
                // Entity octreeRootNodeEntity = a_octreeEntities [i_arrayIndex] ;

                // DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement          = addInstanceBufferElement [octreeRootNodeEntity] ;    
                            
                // RootNodeData rootNodeData                                                 = a_rootNodeData [octreeRootNodeEntity] ;

                DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer                   = nodeSparesBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                              = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer   = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer               = nodeChildrenBufferElement [octreeRootNodeEntity] ;    
                
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                       = instanceBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer = instancesSpareIndexBufferElement [octreeRootNodeEntity] ;    


        /// <summary>
	    /// Add an Instance.
	    /// </summary>
	    /// <param name="i_instanceID">External instance to add.</param>
	    /// <param name="instanceBounds">External instance 3D bounding box around the instance.</param>
	    //public void _OctreeAddInstance ( ref RootNodeData rootNodeData, int i_instanceID, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        //{

                // Iterate through number of instances to add, from the buffer
                for ( int i = 0; i < a_addInstanceBuffer.Length; i ++ )
                {


                    RootNodeData rootNodeData = a_rootNodeData [octreeRootNodeEntity] ;

                    AddInstanceBufferElement addInstanceBuffer = a_addInstanceBuffer [i] ;

// Debug.LogWarning ( "Add instance 2 Node " + i + " / " + a_addInstanceBuffer.Length + "; Instance ID: " + addInstanceBuffer.i_instanceID ) ;

		            // Add object or expand the octree until it can be added
		            int i_count = 0; // Safety check against infinite/excessive growth
                    bool isInstanceAdded = false ;

		            while ( 
                        !_AddNodeInstance ( ref rootNodeData, 
                            addInstanceBuffer.i_instanceID,                             
                            addInstanceBuffer.i_version, 
                            addInstanceBuffer.instanceBounds, 
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            ref a_nodeChildrenBuffer, 
                            ref a_nodeInstancesIndexBuffer,
                            ref a_instanceBuffer, 
                            ref a_instancesSpareIndexBuffer,
                            out isInstanceAdded
                        ) 
                    ) 
                    {

                        NodeBufferElement nodeBufferElement = a_nodesBuffer [rootNodeData.i_rootNodeIndex] ;
// Debug.LogWarning ( "Execute -> Grow " + rootNodeData.i_rootNodeIndex ) ;
			            _GrowOctree ( ref rootNodeData, 
                            (float3) addInstanceBuffer.instanceBounds.center - nodeBufferElement.f3_center,
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            ref a_nodeChildrenBuffer, 
                            ref a_nodeInstancesIndexBuffer, 
                            ref a_instanceBuffer,
                            ref a_instancesSpareIndexBuffer
                        ) ;

			            if ( ++i_count > 20 ) 
                        {
				            // Debug.LogError("Aborted Add operation as it seemed to be going on forever (" + (i_count - 1) + ") attempts at growing the octree.");
				            return;
			            }
		            }

		            rootNodeData.i_totalInstancesCountInTree ++ ;

                    a_rootNodeData [octreeRootNodeEntity] = rootNodeData ;

                } // for
                
                a_addInstanceBuffer.ResizeUninitialized ( 0 ) ; // Clear buffer after instances are added.

	        }

        } // Job


        [RequireComponentTag ( typeof ( AddInstanceTag ) ) ]
        struct CompleteAddInstanceJob : IJobForEachWithEntity_EB <AddInstanceBufferElement>
        // struct CompleteAddInstanceJob : IJobParallelFor 
        {

            public EntityCommandBuffer.Concurrent ecb ;
            // [ReadOnly] public EntityArray a_octreeEntities ;

            public void Execute ( Entity octreeRootNodeEntity, int jobIndex, [ReadOnly] DynamicBuffer <AddInstanceBufferElement> a_addInstanceBuffer )
            {
                
                // Entity octreeRootNodeEntity = a_octreeEntities [i_arrayIndex] ;
                
                // Remove component, as instances has been already added.
                ecb.RemoveComponent <AddInstanceTag> ( jobIndex, octreeRootNodeEntity ) ;

            }

        } // Job

        /// <summary>
	    /// Add an object.
	    /// </summary>
        /// <param name="i_rootNodeIndex">Internal octree node index.</param>
	    /// <param name="i_instanceID">External instance index ID to remove. Is assumed, only one unique instance ID exists in the tree.</param>        
        /// <param name="i_entityVersion">Optional, used when Id is used as entity index.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance.</param>
	    /// <returns>True if the object fits entirely within this node.</returns>
	    static private bool _AddNodeInstance ( ref RootNodeData rootNode, int i_instanceID, int i_entityVersion, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer, out bool isInstanceAdded ) 
        {

            isInstanceAdded = false ;

            NodeBufferElement nodeBufferElement = a_nodesBuffer [rootNode.i_rootNodeIndex] ;

		    if ( !CommonMethods._Encapsulates ( nodeBufferElement.bounds, instanceBounds ) ) return false ; // Early exit

            int i_requiredNumberOfInstances = a_nodeInstancesIndexBuffer.Length ;  // l_nodeBounds.Count ;

            isInstanceAdded = _NodeInstanceSubAdd ( 
                ref rootNode, 
                rootNode.i_rootNodeIndex, 
                i_instanceID, 
                i_entityVersion,
                instanceBounds, 
                ref a_nodesBuffer, 
                ref a_nodeSparesBuffer, 
                ref a_nodeChildrenBuffer, 
                ref a_nodeInstancesIndexBuffer, 
                ref a_instanceBuffer, 
                ref a_instancesSpareIndexBuffer, 
                i_requiredNumberOfInstances 
            ) ;

		    return true;
	    }


        /// <summary>
	    /// Grow the octree to fit in all objects.
	    /// </summary>
	    /// <param name="f3_direction">Direction to grow.</param>
	    static private void _GrowOctree ( ref RootNodeData rootNode, float3 f3_direction, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer ) 
        {

		    int xDirection = f3_direction.x >= 0 ? 1 : -1 ;
		    int yDirection = f3_direction.y >= 0 ? 1 : -1 ;
		    int zDirection = f3_direction.z >= 0 ? 1 : -1 ;

            

            int i_oldRootNodeIndex              = rootNode.i_rootNodeIndex ;            
            NodeBufferElement nodeBufferElement = a_nodesBuffer [i_oldRootNodeIndex] ;
            float f_baseLength                  = nodeBufferElement.f_baseLength ;
		    float f_half                        = f_baseLength / 2 ;
		    float f_newBaseLength               = f_baseLength * 2 ;

		    float3 f3_newCenter                = nodeBufferElement.f3_center + new float3 ( xDirection * f_half, yDirection * f_half, zDirection * f_half ) ;
// Debug.LogWarning ( "Grow: " + f3_newCenter + "; new base length: " + f_newBaseLength + "; halve: " + f_half + "; pos: " + f3_newCenter ) ;
		    // Create a new, bigger octree root node
                
		    if ( !CommonMethods._HasAnyInstances ( i_oldRootNodeIndex, a_nodesBuffer, a_nodeChildrenBuffer ) )
		    {                

                CommonMethods._CreateNewNode ( 
                    ref rootNode, 
                    rootNode.i_rootNodeIndex, 
                    f_newBaseLength, 
                    f3_newCenter, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer 
                ) ;

            }
            else
            {
            
                NodeSparesBufferElement nodeSparesBuffer = a_nodeSparesBuffer [rootNode.i_nodeSpareLastIndex] ;
                rootNode.i_rootNodeIndex = nodeSparesBuffer.i ;
            
                rootNode.i_nodeSpareLastIndex -- ;   
            
                CommonMethods._CreateNewNode ( 
                    ref rootNode, 
                    rootNode.i_rootNodeIndex, 
                    f_newBaseLength, 
                    f3_newCenter, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer 
                ) ;


			    // Create 7 new octree children to go with the old root as children of the new root
			    int i_rootPos = _GetRootPosIndex ( xDirection, yDirection, zDirection ) ;
			
                                
                NodeBufferElement nodeBuffer = a_nodesBuffer [rootNode.i_rootNodeIndex] ;
                nodeBuffer.i_childrenCount = 8 ;
                a_nodesBuffer [rootNode.i_rootNodeIndex] = nodeBuffer ; // Set back.

                int i_newRootNodeChildrenIndexOffset = rootNode.i_rootNodeIndex * 8 ;

			    for (int i = 0; i < 8; i++)
			    {

                    int i_childIndexOffset = i_newRootNodeChildrenIndexOffset + i ;

                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_childIndexOffset] ;

				    if ( i == i_rootPos )
				    {
                        // Assign old root node as a child.
                        nodeChildrenBuffer.i_group8NodesIndex      = i_oldRootNodeIndex ;
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_oldRootNodeIndex] ;
                        nodeChildrenBuffer.bounds            = instanceBuffer.bounds ;					
				    }
				    else
				    {
                        // Assign rest 7 children
					    xDirection                      = i % 2 == 0 ? -1 : 1;
					    yDirection                      = i > 3 ? -1 : 1;
					    zDirection                      = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;

                        nodeSparesBuffer                        = a_nodeSparesBuffer [rootNode.i_nodeSpareLastIndex] ;
                        int i_newNodeIndex                      = nodeSparesBuffer.i ; // Expected output 0 at initialization      
                        rootNode.i_nodeSpareLastIndex -- ;  

                        float3 f3_childVector                   = f3_newCenter + new float3 ( xDirection * f_half, yDirection * f_half, zDirection * f_half ) ;
                    
                        CommonMethods._CreateNewNode ( 
                            ref rootNode, 
                            i_newNodeIndex, 
                            f_newBaseLength, 
                            f3_childVector, 
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            ref a_nodeChildrenBuffer, 
                            ref a_nodeInstancesIndexBuffer 
                        ) ;
                        
                    
                        nodeChildrenBuffer.i_group8NodesIndex                 = i_newNodeIndex ; 

                        Bounds bounds = new Bounds ( ) { 
                            center      = f3_childVector,
                            size        = Vector3.one * f_newBaseLength
                        } ;

                        nodeChildrenBuffer.bounds                       = bounds ;

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
        /// <param name="i_instanceID">External instance index, ot unique entity index.</param>
        /// <param name="i_entityVersion">Optional, used when Id is used as entity index.</param>
	    /// <param name="instanceBounds">External 3D bounding box around the instance to add.</param>
	    static private bool _NodeInstanceSubAdd ( ref RootNodeData rootNode, int i_nodeIndex, int i_instanceID, int i_entityVersion, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer, int i_requiredNumberOfInstances ) 
        {

// Debug.Log ( "SubAdd node index: " + i_nodeIndex + "; instance ID: " + i_instanceID + "; inst bounds: " + instanceBounds ) ;
            bool isInstanceAdded = false ;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
                        
		    // We know it fits at this level if we've got this far
		    // Just add if few objects are here, or children would be below min size
            int i_instancesCount = nodeBuffer.i_instancesCount ;

            if ( nodeBuffer.i_childrenCount == 0 && i_instancesCount < rootNode.i_instancesAllowedCount || ( nodeBuffer.f_baseLength / 2 ) < rootNode.f_minSize )             
            {
            
// Debug.Log ( "Instance allowed?" ) ;

                isInstanceAdded = _AssingInstance2Node ( ref rootNode, i_nodeIndex, i_instanceID, i_entityVersion, instanceBounds, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_instanceBuffer, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
  
// Debug.Log ( "Is Instance Addded A: " + isInstanceAdded ) ;                 
                // a_nodesBuffer
                if ( rootNode.i_instancesSpareLastIndex == 0 || i_requiredNumberOfInstances > a_instanceBuffer.Length )
                {
                    // Add some spares if needed.
                    CommonMethods._AddInstanceSpares ( ref rootNode, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer, i_requiredNumberOfInstances ) ;              
                }
                else
                {
                    rootNode.i_instancesSpareLastIndex -- ;
                }


                /*
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
    */

		    }
		    else 
            {
			    // Fits at this level, but we can go deeper. Would it fit there?

// Debug.Log ( "Instance fits at other level?" ) ;
			    // Create 8 children.
			    int i_bestFitChildLocalIndex ;
                int i_bestChildIndex ;

                nodeBuffer                  = a_nodesBuffer [i_nodeIndex] ;
                int i_childrenCount         = nodeBuffer.i_childrenCount ;            
                int i_childrenIndexOffset   = i_nodeIndex * 8 ;

                NodeChildrenBufferElement nodeChildrenBuffer ;

			    if ( i_childrenCount == 0 ) 
                {
                    // Split Octree node, into 8 new smaller nodes as children nodex.
				    _Split ( ref rootNode, i_nodeIndex, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer );
                
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeIndex] ;

                    int i_nodeInstanceIndexOffset = nodeInstancesIndexBuffer.i ;

				    // Now that we have the new children, see if this node's existing objects would fit there
				    for (int i = i_instancesCount - 1; i >= 0; i--) 
                    {

                        int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                        if ( i_instanceIndexOffset >= 0 )
                        {

                            InstanceBufferElement existingInstanceBuffer = a_instanceBuffer [i_instanceIndexOffset] ;
                        		
                            if ( existingInstanceBuffer.i_ID >= 0 )
                            {

					            // Find which child the object is closest to based on, where the
					            // object's center is located in relation to the octree's center.
					            i_bestFitChildLocalIndex = CommonMethods._BestFitChild ( i_nodeIndex, existingInstanceBuffer.bounds, a_nodesBuffer ) ;

                                i_bestChildIndex = i_childrenIndexOffset + i_bestFitChildLocalIndex ;
                                nodeChildrenBuffer = a_nodeChildrenBuffer [i_bestChildIndex] ;
                            

					            // Does it fit?
					            if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, existingInstanceBuffer.bounds ) ) 
                                {             
   
// Debug.Log ( "Encapsulates split instance " + i_instanceID + "; " + instanceBounds + " in child node " + nodeChildrenBuffer.bounds ) ;                                 
// Debug.Log ( "Best Chuld index " + i_bestChildIndex + " node child index: " + nodeChildrenBuffer.i_group8NodesIndex + "; for instance ID: " + existingInstanceBuffer.i_ID ) ;
                                    isInstanceAdded =_NodeInstanceSubAdd ( 
                                        ref rootNode, 
                                        nodeChildrenBuffer.i_group8NodesIndex, 
                                        existingInstanceBuffer.i_ID, 
                                        existingInstanceBuffer.i_entityVersion,
                                        existingInstanceBuffer.bounds, 
                                        ref a_nodesBuffer, 
                                        ref a_nodeSparesBuffer, 
                                        ref a_nodeChildrenBuffer, 
                                        ref a_nodeInstancesIndexBuffer, 
                                        ref a_instanceBuffer, 
                                        ref a_instancesSpareIndexBuffer, 
                                        i_requiredNumberOfInstances
                                    ) ; // Go a level deeper
						        		
                            
                                    // Remove from here
                                    CommonMethods._PutBackSpareInstance ( ref rootNode, i_instanceIndexOffset, i_nodeIndex, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;

                                    nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
                                    nodeBuffer.i_instancesCount -- ;
                                    a_nodesBuffer [i_nodeIndex] = nodeBuffer ;
					            }
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
                                        
// Debug.Log ( "Encapsulates A instance " + i_instanceID + "; " + instanceBounds + " in node " + nodeChildrenBuffer.bounds + "; " + nodeChildrenBuffer.bounds.extents.ToString ("F7") ) ;
                    isInstanceAdded = _NodeInstanceSubAdd 
                    ( 
                        ref rootNode, 
                        nodeChildrenBuffer.i_group8NodesIndex, 
                        i_instanceID, 
                        i_entityVersion,
                        instanceBounds, 
                        ref a_nodesBuffer, 
                        ref a_nodeSparesBuffer, 
                        ref a_nodeChildrenBuffer, 
                        ref a_nodeInstancesIndexBuffer, 
                        ref a_instanceBuffer, 
                        ref a_instancesSpareIndexBuffer, 
                        i_requiredNumberOfInstances 
                    ) ;
			    }
			    else 
                {
                
// Debug.Log ( "Does not encapsulates instance " + instanceBounds + " in node " + nodeChildrenBuffer.bounds ) ;
                    isInstanceAdded = _AssingInstance2Node ( ref rootNode, i_nodeIndex, i_instanceID, i_entityVersion, instanceBounds, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_instanceBuffer, ref a_nodeInstancesIndexBuffer, ref a_instancesSpareIndexBuffer ) ;
                    
// Debug.Log ( "Is Instance Addded B: " + isInstanceAdded ) ;    
                    if ( rootNode.i_instancesSpareLastIndex == 0 || i_requiredNumberOfInstances > a_instanceBuffer.Length )
                    {
                        // Add some spares if needed.
                        CommonMethods._AddInstanceSpares ( ref rootNode, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer, i_requiredNumberOfInstances ) ;                
                    }
                    else
                    {
                        rootNode.i_instancesSpareLastIndex -- ;
                    }
/*            
// Debugging
Debug.Log ( "Instance: New game object #" + i_instanceID.ToString () ) ;

GameObject newGameObject = GameObject.Instantiate ( GameObject.Find ( "TempInstance" ), instanceBounds.center, Quaternion.identity ) ;
newGameObject.transform.localScale = instanceBounds.size ;
newGameObject.name = i_instanceID.ToString () ;
*/

			    }

		    }

            return isInstanceAdded ;

	    }


        /// <summary>
	    /// Splits the octree into eight children.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    static private void _Split ( ref RootNodeData rootNode, int i_nodeIndex, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer ) 
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
                NodeSparesBufferElement nodeSparesBuffer                = a_nodeSparesBuffer [rootNode.i_nodeSpareLastIndex] ;
                nodeChildrenBuffer                                      = a_nodeChildrenBuffer [i_childrenIndexOffset + i] ;
                nodeChildrenBuffer.i_group8NodesIndex                         = nodeSparesBuffer.i ;
                a_nodeChildrenBuffer [i_childrenIndexOffset + i]        = nodeChildrenBuffer ; // Set back
                rootNode.i_nodeSpareLastIndex -- ;
            }

            float3 f3_childCenterQuater ;
            

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset] ;        
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 1] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 2] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 3] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 4] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, -f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 5] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, -f_quarter, -f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 6] ;
            f3_childCenterQuater = f3_center + new float3 (-f_quarter, -f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;

            nodeChildrenBuffer   = a_nodeChildrenBuffer [i_childrenIndexOffset + 7] ;
            f3_childCenterQuater = f3_center + new float3 (f_quarter, -f_quarter, f_quarter) ;
            CommonMethods._CreateNewNode ( ref rootNode, nodeChildrenBuffer.i_group8NodesIndex, f_newBaseLength, f3_childCenterQuater, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;
            
	    }
        

        /// <summary>
        /// Assign instance to node.
        /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
        /// <param name="i_instanceID">External instance index, ot unique entity index.</param>
        /// <param name="i_entityVersion">Optional, used when Id is used as entity index.</param>
        /// // Optional, used when Id is used as entity index
        /// <param name="instanceBounds">Boundary of external instance index.</param>
        static private bool _AssingInstance2Node ( ref RootNodeData rootNode, int i_nodeIndex, int i_instanceID, int i_entityVersion, Bounds instanceBounds, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer )
        {
            int i_nodeInstanceIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;

//             Debug.Log ( "Assign instance 2 Node; node offset " + i_nodeInstanceIndexOffset + " Instance ID: " + i_instanceID ) ;

            // Reuse spare store
            InstancesSpareIndexBufferElement instancesSpareIndexBuffer = a_instancesSpareIndexBuffer [rootNode.i_instancesSpareLastIndex] ;   

            NodeInstancesIndexBufferElement nodeInstancesIndexBuffer ;
            
            bool isInstanceAdded = false ;

            // Find next spare instance allocation for this node.
            // Find next spare instance allocation for this node.
            for ( int i = 0; i < rootNode.i_instancesAllowedCount; i++ ) 
            {
                
                int i_instanceIndexOffset = i_nodeInstanceIndexOffset + i ;

                nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_instanceIndexOffset] ;

NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeIndex] ; // Only test. Remove.
// Debug.Log ( "#" + i + " offset: " + i_instanceIndexOffset + " node index -1? " + nodeInstancesIndexBuffer.i + " <= " + instancesSpareIndexBuffer.i + "; bounds: " + nodeChildrenBuffer.bounds ) ;
                // Is spare.
                if ( nodeInstancesIndexBuffer.i == -1 )
                {
                    // Assign instance index.
                    nodeInstancesIndexBuffer.i = instancesSpareIndexBuffer.i ;
                    a_nodeInstancesIndexBuffer [i_instanceIndexOffset] = nodeInstancesIndexBuffer ; // Set back.

                    isInstanceAdded = true ;

                    break ;
                }

            } // for

// Debug.Log ( "Node index: " + i_nodeIndex + " Is instance ID " + i_instanceID + " added: " + isInstanceAdded ) ; 

            if ( !isInstanceAdded )
            {
                    

                // Node has 8 children and an more instances, than capacity.
                // Try to move instance to child, with instance space.
                // if ( i_instancesCount < rootNodeData.i_instancesAllowedCount && i_childrenCount > 0 )
                    
                // Iterate though node's children
                for ( int i_childIndex = 0; i_childIndex < 8; i_childIndex ++ )
                {
                    int i_childIndexOffset = i_nodeIndex * 8 + i_childIndex ;

                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_childIndexOffset] ;
   
// Debug.Log ( "Is instance child " + i_childIndex + " / " + 8 + "; " + i_instanceID + "; child buffer: " + nodeChildrenBuffer.bounds ) ; 
                    
                    // Check if instance bounds fit in child
                    if ( CommonMethods._Encapsulates ( nodeChildrenBuffer.bounds, instanceBounds ) ) 
                    {
                        
// Debug.Log ( "Encapsulates B instance ID: " + i_instanceID + "; bound: " + instanceBounds + " in node " + nodeChildrenBuffer.bounds ) ;

                        //NodeBufferElement childNode = a_nodesBuffer [nodeChildrenBuffer.i_nodesIndex] ;
                            
                        // Debug.LogWarning ( "ChNode: " + childNode.i_instancesCount + "; " + childNode.bounds ) ;


                        int i_requiredNumberOfInstances = a_nodeInstancesIndexBuffer.Length ;  // l_nodeBounds.Count ;

                        isInstanceAdded = _NodeInstanceSubAdd ( 
                            ref rootNode, 
                            nodeChildrenBuffer.i_group8NodesIndex, 
                            i_instanceID, 
                            i_entityVersion,
                            instanceBounds, 
                            ref a_nodesBuffer, 
                            ref a_nodeSparesBuffer, 
                            ref a_nodeChildrenBuffer, 
                            ref a_nodeInstancesIndexBuffer, 
                            ref a_instanceBuffer, 
                            ref a_instancesSpareIndexBuffer, 
                            i_requiredNumberOfInstances 
                        ) ;

                        if ( isInstanceAdded ) return true ; // Added instance

                    }
                }


                NodeBufferElement rootNodeBufferElement = a_nodesBuffer [rootNode.i_rootNodeIndex] ;
   
Debug.LogError ( "Found no child node, to fit instance. Try grow octree; Instance ID " + i_instanceID + " bounds: " + instanceBounds + "; root node bounds: " + rootNodeBufferElement.bounds ) ;
                
                // Check if still fits in the root node.
                if ( CommonMethods._Encapsulates ( rootNodeBufferElement.bounds, instanceBounds ) ) 
                {
Debug.Log ( "Still Fits in root." ) ;
                }
                else
                {

                // Try add node at the root
                
Debug.LogError ( "Grow again." ) ;
                    
                    _GrowOctree ( ref rootNode, 
                        (float3) instanceBounds.center - rootNodeBufferElement.f3_center,
                        ref a_nodesBuffer, 
                        ref a_nodeSparesBuffer, 
                        ref a_nodeChildrenBuffer, 
                        ref a_nodeInstancesIndexBuffer, 
                        ref a_instanceBuffer,
                        ref a_instancesSpareIndexBuffer
                    ) ;
                    

                }

                /*
                _AddNodeInstance ( ref rootNode, 
                    i_instanceID,                             
                    i_entityVersion, 
                    instanceBounds, 
                    ref a_nodesBuffer, 
                    ref a_nodeSparesBuffer, 
                    ref a_nodeChildrenBuffer, 
                    ref a_nodeInstancesIndexBuffer,
                    ref a_instanceBuffer, 
                    ref a_instancesSpareIndexBuffer, 
                    out isInstanceAdded
                ) ;
                */
                
// Debug.LogWarning ( "Is instant added? " + (isInstanceAdded? "y" : "n") ) ;
                return isInstanceAdded ;

            } // if

            
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            nodeBuffer.i_instancesCount ++ ;
            a_nodesBuffer [i_nodeIndex] = nodeBuffer ; // Set back.

            InstanceBufferElement instanceBuffer = new InstanceBufferElement () 
            { 
                i_ID = i_instanceID, 
                i_entityVersion = i_entityVersion, // Optional, used when Id is used as entity index.
                bounds = instanceBounds 
            } ;
            
            a_instanceBuffer [instancesSpareIndexBuffer.i] = instanceBuffer ;

            return true ;
        }
        

        /*
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
        */

    }

}
