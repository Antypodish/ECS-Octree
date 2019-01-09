using System.Collections.Generic;
using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using UnityEngine;
using Unity.Burst ;


namespace ECS.Octree
{

    internal class IsRayColliding_Common
    {

        /// <summary>
	    /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check.</param>
	    /// <param name="f_maxDistance">Distance to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    static public bool _IsNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Ray checkRay, ref IsCollidingData isCollidingData, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer, float f_maxDistance = float.PositiveInfinity ) 
        {
		    // Is the input ray at least partially in this node?
		
            float f_distance ;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    if ( !nodeBuffer.bounds.IntersectRay ( checkRay, out f_distance ) || f_distance > f_maxDistance ) 
            {
			    return false ;
		    }
        
            if ( nodeBuffer.i_instancesCount >= 0 ) 
            {            

                int i_nodeInstancesIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;

		        // Check against any objects in this node
                for (int i = 0; i < rootNodeData.i_instancesAllowedCount; i++) 
                {
            
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;

                    // Get index of instance
                    int i_instanceIndex = nodeInstancesIndexBuffer.i ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_instanceIndex] ;

			            if ( instanceBuffer.bounds.IntersectRay ( checkRay, out f_distance) && f_distance <= f_maxDistance ) 
                        {
                            isCollidingData.i_collisionsCount = 1 ; // Is colliding
				            return true;
			            }
                    }
            
		        }
            }

            // Check children for collisions
            // Check if having children
		    if ( nodeBuffer.i_childrenCount > 0 ) 
            {

                int i_nodeChildrenIndexOffset = i_nodeIndex * 8 ;

                // We checked that is having children.
			    for (int i = 0; i < 8; i++) 
                {    
                    
                    NodeChildrenBufferElement nodeChildrenBuffer = a_nodeChildrenBuffer [i_nodeChildrenIndexOffset + i] ;
                    int i_nodeChildIndex = nodeChildrenBuffer.i_nodesIndex ;
                    
                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
                        if ( _IsNodeColliding ( rootNodeData, i_nodeChildIndex, checkRay, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) )
                        {
                            isCollidingData.i_collisionsCount = 1 ; // Is colliding
					        return true ;
				        }
                    }
			    }
		    }

		    return false;
	    }

    }

}
