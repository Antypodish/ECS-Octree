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

    class CommonMethods
    {

        /// <summary>
        /// Number of instances allowed per node
        /// </summary>
        // static public int numInstancesAllowed = 1 ;
                
        public const int numOfSpareInstances2Add = 100 ;

        /// <summary>
	    /// Node Constructor.
	    /// </summary>
        static public void _CreateNewNode ( ref RootNodeData rootNodeData, int i_nodeIndex, float f_baseLength, float3 f3_center, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer )
        // private void _CreateNewNode ( Entity entity, int i_nodeIndex, float f_baseLength, Vector3 V3_center )
        {

            // Expand storage if needed
            if ( rootNodeData.i_nodeSpareLastIndex <= 9 )
            {

                int i_baseAddNodeCount                                 = 100 ; 

                int i_initialSparesCount                               = a_nodeSparesBuffer.Length + i_baseAddNodeCount - 1 ;

                NodeSparesBufferElement nodeSpareBuffer                = new NodeSparesBufferElement () ;

                NodeBufferElement nodeBuffer                           = new NodeBufferElement () ;


                // Populate with default data.
                for ( int i = 0; i < i_baseAddNodeCount; i ++ )
                {

                    // Add spares in reversed order, from higher index, to lower index.
                    nodeSpareBuffer.i                                  = -1 ;
                    a_nodeSparesBuffer.Add ( nodeSpareBuffer ) ;
                    rootNodeData.i_nodeSpareLastIndex ++ ;
                    nodeSpareBuffer.i                                  = i_initialSparesCount - i ;
                    a_nodeSparesBuffer [rootNodeData.i_nodeSpareLastIndex] = nodeSpareBuffer;

                    nodeBuffer.f_baseLength                            = -1 ;
                    nodeBuffer.f_adjLength                             = -1 ;
                    nodeBuffer.f3_center                               = float3.zero ;
                    nodeBuffer.bounds                                  = new Bounds () ;
                    a_nodesBuffer.Add ( nodeBuffer ) ;
                    nodeBuffer.i_childrenCount                         = 0 ;
                    nodeBuffer.i_instancesCount                        = 0 ;

                    // l_nodeBaseLength.Add (-1) ;
                    // l_nodeCenters.Add ( Vector3.zero ) ;
                    // l_nodeAdjLength.Add (-1) ;
                    // l_nodeBounds.Add ( new Bounds () ) ;
                    
                    // l_nodeChildrenCount.Add ( 0 ) ;
                    // l_nodeInstancesCount.Add ( 0 ) ;
                           

                    for ( int j = 0; j < rootNodeData.i_instancesAllowedCount; j ++ )
                    {
                        a_nodeInstancesIndexBuffer.Add ( new NodeInstancesIndexBufferElement () { i = -1 } ) ;                    
                    }
                
             
                    NodeChildrenBufferElement nodeChildrenBuffer = new NodeChildrenBufferElement () ;

                    // Generate 8 new empty children.                    
                    for ( int j = 0; j < 8; j ++ )
                    {
                        nodeChildrenBuffer.i_nodesIndex = -1 ;
                        nodeChildrenBuffer.bounds = new Bounds () ;
                        a_nodeChildrenBuffer.Add ( new NodeChildrenBufferElement () ) ;
                        //l_childrenBounds.Add ( new Bounds () ) ;        
                        //l_nodeChildrenNodesIndex.Add ( -1 ) ;                           
                    }
                }
            }


		    _SetValues ( rootNodeData, i_nodeIndex, f_baseLength, f3_center, ref a_nodesBuffer, ref a_nodeChildrenBuffer ) ;
        
        }


        /// <summary>
	    /// Set values for this node. 
	    /// </summary>
	    /// <param name="f_baseLength">Length of this node, not taking looseness into account.</param>
	    /// <param name="f_minSize">Minimum size of nodes in this octree.</param>
	    /// <param name="f_looseness">Multiplier for baseLengthVal to get the actual size.</param>
	    /// <param name="f3_center">Centre position of this node.</param>
        static public void _SetValues ( RootNodeData rootNodeData, int i_nodeIndex, float f_baseLength, float3 f3_center, ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer ) 
        {

            NodeBufferElement nodeBuffer       = new NodeBufferElement () ;
            
            nodeBuffer.f_baseLength            = f_baseLength ;
		    // l_nodeBaseLength [i_nodeIndex]  = f_baseLength;
            nodeBuffer.f3_center               = f3_center ;
		    // l_nodeCenters [i_nodeIndex]     = f3_center ;
            float f_adjustLength               = rootNodeData.f_looseness * f_baseLength ;
            nodeBuffer.f_adjLength             = f_adjustLength ;
		    //l_nodeAdjLength [i_nodeIndex]    = f_adjustLength ;

		    // Create the bounding box.
		    Vector3 size                       = new Vector3 ( f_adjustLength, f_adjustLength, f_adjustLength ) ;

            nodeBuffer.bounds                  = new Bounds ( f3_center, size ) ;
		    //l_nodeBounds [i_nodeIndex]       = new Bounds ( f3_center, size ) ;
            
            a_nodesBuffer [i_nodeIndex]         = nodeBuffer ;



		    float f_quarter                    = f_baseLength / 4f ;
		    float f_childActualLength          = ( f_baseLength / 2) * rootNodeData.f_looseness ;
            
		    float3 f3_childActualSize          = new float3 ( f_childActualLength, f_childActualLength, f_childActualLength );

            int i_childrenIndexOffset          = i_nodeIndex * 8 ;



            NodeChildrenBufferElement nodeChildrenBuffer ;
            
            // Set 8 children for this node
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (-f_quarter, f_quarter, -f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset]        = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 1] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (f_quarter, f_quarter, -f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 1]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 2] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (-f_quarter, f_quarter, f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 2]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 3] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (f_quarter, f_quarter, f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 3]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 4] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (-f_quarter, -f_quarter, -f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 4]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 5] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (f_quarter, -f_quarter, -f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 5]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 6] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (-f_quarter, -f_quarter, f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 6]    = nodeChildrenBuffer ; // Set back
            nodeChildrenBuffer                                  = a_nodeChildrenBuffer [i_childrenIndexOffset + 7] ; // Get
		    nodeChildrenBuffer.bounds                           = new Bounds ( f3_center + new float3 (f_quarter, -f_quarter, f_quarter), f3_childActualSize ) ;
            a_nodeChildrenBuffer [i_childrenIndexOffset + 7]    = nodeChildrenBuffer ; // Set back

	    }


        /// <summary>
        /// Add required new spare instances.
        /// </summary>
        static public void _AddInstanceSpares ( ref RootNodeData rootNodeData, ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer )
        {

            rootNodeData.i_instancesSpareLastIndex -- ;
                    
            int i_initialSparesCount                                   = a_instanceBuffer.Length ;
            //int i_initialSparesCount                                 = l_instancesID.Count ;
        
            InstancesSpareIndexBufferElement instancesSpareIndexBuffer = new InstancesSpareIndexBufferElement () ;            
            instancesSpareIndexBuffer.i                                = -1 ;

            InstanceBufferElement instanceBuffer                       = new InstanceBufferElement () ;
            instanceBuffer.bounds                                      = new Bounds () ;
            instanceBuffer.i_ID                                        = -1 ;

            // Add new spares, from the end of storage.
            for ( int i = 0; i < numOfSpareInstances2Add; i ++ )
            {        
                // Need to expand spare store.
                a_instancesSpareIndexBuffer.Add ( instancesSpareIndexBuffer ) ;
                // l_instancesSpare.Add ( -1 ) ;
                a_instanceBuffer.Add ( instanceBuffer ) ;
                // l_instancesID.Add ( -1 ) ;
                // l_instancesBounds.Add ( new Bounds () ) ;
            }

            // Populate indexes references, with new spares.
            for ( int i = 0; i < numOfSpareInstances2Add; i ++ )
            {
                rootNodeData.i_instancesSpareLastIndex ++ ;

                // Add new spares.
                // Add spares in reversed order, from higher index, to lower index.                
                instancesSpareIndexBuffer.i                                             = i_initialSparesCount + numOfSpareInstances2Add - i - 1 ;
                a_instancesSpareIndexBuffer [rootNodeData.i_instancesSpareLastIndex]    = instancesSpareIndexBuffer ;
                // a_instancesSpareIndexBuffer [rootNodeData.i_instancesSpareLastIndex] = i_initialSparesCount + numOfSpareInstances2Add - i - 1 ;                     
            
            }
                
        }        

        
        /// <summary>
        /// Allows future reuse of spare instance, by putting it into the end of spares store.
        /// </summary>
        /// <param name="i_instanceIndex">Instance index, to pu back into spares of instances.</param>
        /// <param name="i_nodeIntstanceIndex">Node instance index holder, to be reset.</param>
        static public void _PutBackSpareInstance ( ref RootNodeData rootNodeData, int i_instanceIndex, int i_nodeIntstanceIndex, ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, ref DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer )
        {

            if ( i_instanceIndex < 0 ) return ; // This instance index has not been used.
        
            rootNodeData.i_instancesSpareLastIndex ++ ; // Put back to spare

            InstancesSpareIndexBufferElement instancesSpareIndexBuffer = new InstancesSpareIndexBufferElement () ;
            instancesSpareIndexBuffer.i = i_instanceIndex ;
            a_instancesSpareIndexBuffer [rootNodeData.i_instancesSpareLastIndex] = instancesSpareIndexBuffer ;
            // l_instancesSpare [i_instancesSpareLastIndex] = i_instanceIndex ;
                    
            // Is assumed, that size of spares store, is appropriate.
            NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = new NodeInstancesIndexBufferElement () ;
            nodeInstancesIndexBuffer.i = -1 ; // Reset instance index.
            a_nodeInstancesIndexBuffer [i_nodeIntstanceIndex] = nodeInstancesIndexBuffer ;
            // l_nodeInstancesIndex [i_nodeIntstanceIndex] = -1 ; // Reset instance index.
                
        }

                
        /// <summary>
	    /// Checks if outerBounds encapsulates innerBounds.
	    /// </summary>
	    /// <param name="outerBounds">Outer bounds.</param>
	    /// <param name="innerBounds">Inner bounds.</param>
	    /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
	    static public bool _Encapsulates ( Bounds outerBounds, Bounds innerBounds ) 
        {
		    return outerBounds.Contains ( innerBounds.min ) && outerBounds.Contains ( innerBounds.max );
	    }
                

        /// <summary>
	    /// Find which child node this object would be most likely to fit in.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="objBounds">The external instance bounds.</param>
	    /// <returns>One of the eight child octants.</returns>
	    static public int _BestFitChild ( int i_nodeIndex, Bounds objBounds, DynamicBuffer <NodeBufferElement> a_nodesBuffer ) 
        {
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            // Vector3 V3_center = l_nodeCenters [i_nodeIndex] ;
		    return ( objBounds.center.x <= nodeBuffer.f3_center.x ? 0 : 1) + (objBounds.center.y >= nodeBuffer.f3_center.y ? 0 : 4) + (objBounds.center.z <= nodeBuffer.f3_center.z ? 0 : 2);
	    }
        

        /// <summary>
	    /// Checks if this node or anything below it has something in it.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <returns>True if this node or any of its children, grandchildren etc have something in them</returns>
	    static public bool _HasAnyInstances ( int i_nodeIndex, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer ) 
        {
            if ( i_nodeIndex == -1 ) return false ;

            
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    if ( nodeBuffer.i_instancesCount > 0 ) return true ;

            // Has children?
		    if ( nodeBuffer.i_childrenCount > 0 ) 
            {
                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

			    for (int i = 0; i < 8; i++) 
                {
                    NodeChildrenBufferElement nodeChildBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;

                    // Has child any instances
                    if ( _HasAnyInstances ( nodeChildBuffer.i_nodesIndex, a_nodesBuffer, a_nodeChildrenBuffer ) ) return true ;
				    // if ( _HasAnyInstances ( l_nodeChildrenNodesIndex [ i_nodeChildrenIndexOffset + i ] ) ) return true ;
			    }
		    }

		    return false;
	    }

    }
}
