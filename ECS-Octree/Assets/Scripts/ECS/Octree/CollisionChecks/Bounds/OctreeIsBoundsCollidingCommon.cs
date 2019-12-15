using Unity.Collections ;
using Unity.Entities ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{


    internal class IsBoundsColliding_Common
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="na_collisionChecksEntities"></param>
        /// <param name="a_isCollidingData"></param>
        /// <param name="canDebugAllChecks">Debug Log all checks, or only one (first one)</param>
        static public void _DebugBounds ( [ReadOnly] ref NativeArray <Entity> na_collisionChecksEntities, [ReadOnly] ref ComponentDataFromEntity <IsCollidingData> a_isCollidingData, bool canDebugAllChecks )
        {

            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.

            
            // Debug all, or only one check
            int i_debugCollisionChecksCount = canDebugAllChecks ? na_collisionChecksEntities.Length : 1 ;
            
            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < i_debugCollisionChecksCount; i_collisionChecksIndex ++ )
            // for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < a_collisionChecksEntities.Length; i_collisionChecksIndex ++ )
            {                  
                Entity octreeEntity = na_collisionChecksEntities [i_collisionChecksIndex] ;
                IsCollidingData isCollidingData = a_isCollidingData [octreeEntity] ;

                if ( isCollidingData.i_collisionsCount > 0 ) Debug.Log ( "Is colliding." ) ;                
            }
                        
        }


        /// <summary>
	    /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    static public bool _IsNodeColliding ( [ReadOnly] ref RootNodeData rootNode, int i_nodeIndex, Bounds checkBounds, ref IsCollidingData isCollidingData, [ReadOnly] ref DynamicBuffer <NodeBufferElement> a_nodesBuffer, [ReadOnly] ref DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, [ReadOnly] ref DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, [ReadOnly] ref DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 
        {

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    // Are the input bounds at least partially in this node?
		    if ( !nodeBuffer.bounds.Intersects ( checkBounds ) ) 
            {
			    return false;
		    }


            if ( nodeBuffer.i_instancesCount >= 0 ) 
            {     
                
                int i_nodeInstancesIndexOffset = i_nodeIndex * rootNode.i_instancesAllowedCount ;

		        // Check against any objects in this node
                for (int i = 0; i < rootNode.i_instancesAllowedCount; i++) 
                {
                    
                    NodeInstancesIndexBufferElement nodeInstancesIndexBuffer = a_nodeInstancesIndexBuffer [i_nodeInstancesIndexOffset + i] ;

                    // Get index of instance
                    int i_instanceIndex = nodeInstancesIndexBuffer.i ;
                
                    // Check if instance exists, and if has intersecting bounds.
                    if ( i_instanceIndex >= 0 )
                    {
                                            
                        InstanceBufferElement instanceBuffer = a_instanceBuffer [i_instanceIndex] ;

                        // Check if instance exists, and if has intersecting bounds.
			            if ( instanceBuffer.bounds.Intersects (checkBounds) ) 
                        {
                            isCollidingData.i_collisionsCount = 1 ;
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
                    int i_nodeChildIndex = nodeChildrenBuffer.i_group8NodesIndex ;

                    // Check if node exists
                    if ( i_nodeChildIndex >= 0 )
                    {
				        if ( _IsNodeColliding ( ref rootNode, i_nodeChildIndex, checkBounds, ref isCollidingData, ref a_nodesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer, ref a_instanceBuffer ) ) 
                        {
                            isCollidingData.i_collisionsCount = 1 ;
					        return true ;
				        }

                    }
			    }
		    }

		    return false ;

	    }

    }

}
