using Unity.Entities ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{

    internal class GetCollidingBoundsInstances_Common
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_collisionChecksEntities"></param>
        /// <param name="a_isCollidingData"></param>
        /// <param name="collisionInstancesBufferElement"></param>
        /// <param name="canDebugAllChecks">Debug Log all checks, or only one (first one)</param>
        static public void _DebugBounds ( EntityCommandBuffer ecb, EntityArray a_collisionChecksEntities, ComponentDataFromEntity <IsCollidingData> a_isCollidingData, BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement, bool canDebugAllChecks )
        {

            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.


            // Debug all, or only one check
            int i_debugCollisionChecksCount = canDebugAllChecks ? a_collisionChecksEntities.Length : 1 ;

            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < i_debugCollisionChecksCount; i_collisionChecksIndex ++ )
            {
                  
                Entity octreeRayEntity = a_collisionChecksEntities [i_collisionChecksIndex] ;
              
                // Last known instances collisions count.
                IsCollidingData isCollidingData = a_isCollidingData [octreeRayEntity] ;

                if ( isCollidingData.i_collisionsCount > 0 )
                {
                    
                    // Debug.Log ( "Octree: Last known instances collisions count #" + isCollidingData.i_collisionsCount ) ;

                    // Stores reference to detected colliding instance.
                    DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer = collisionInstancesBufferElement [octreeRayEntity] ;    

                    
                    string s_collidingIDs = "" ;

                    CollisionInstancesBufferElement collisionInstancesBuffer ;

                    for ( int i = 0; i < isCollidingData.i_collisionsCount; i ++ )
                    {
                        collisionInstancesBuffer = a_collisionInstancesBuffer [i] ;
                        s_collidingIDs += collisionInstancesBuffer.i_ID + ", " ;
                    }

                    
                    // Test highlight
                    // Highlight.SwitchMethods._Switch ( ecb, closestInstanceEntity ) ;

                    Debug.Log ( "Is colliding with #" + isCollidingData.i_collisionsCount + " instances of IDs: " + s_collidingIDs ) ;
                    
                }
                
            } // for
                        
        }


        /// <summary>
	    /// Returns an collection of instances, that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check. Passing by ref as it improves performance with structs.</param>
	    /// <param name="l_resultInstanceIDs">List result.</param>
        static public bool _GetNodeColliding ( RootNodeData octreeRootNodeData, int i_nodeIndex, Bounds checkBounds, ref DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer, ref IsCollidingData isCollidingData, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 	    
        {

            // float f_distance ;
            CollisionInstancesBufferElement collisionInstancesBuffer = new CollisionInstancesBufferElement () ;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

            // Are the input bounds at least partially in this node?
		    if ( !nodeBuffer.bounds.Intersects ( checkBounds ) ) 
            {
			    return isCollidingData.i_collisionsCount > 0 ? true : false ; 
		    }
        

            if ( nodeBuffer.i_instancesCount >= 0 ) 
            {            

                int i_nodeInstancesIndexOffset = i_nodeIndex * octreeRootNodeData.i_instancesAllowedCount ;

		        // Check against any objects in this node
                for (int i = 0; i < octreeRootNodeData.i_instancesAllowedCount; i++) 
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
				            // l_resultInstanceIDs.Add ( instanceBuffer.i_ID ) ;

                            //if ( f_distance < isCollidingData.f_nearestDistance )
                            //{
                            //    isCollidingData.f_nearestDistance = f_distance ;
                            //    isCollidingData.i_nearestInstanceCollisionIndex = isCollidingData.i_collisionsCount ;
                            //}

                            

                            // Is expected, that the required length is no greater than current length + 1
                            // And is not negative.
                            int i_collisionsCount = isCollidingData.i_collisionsCount ;
                            collisionInstancesBuffer.i_ID = instanceBuffer.i_ID ;      
                            collisionInstancesBuffer.i_version = instanceBuffer.i_entityVersion ; // Optional, used when Id is used as entity index   
                                                        
                            // Assign colliding instance ID to buffer.
                            if ( a_collisionInstancesBuffer.Length <= i_collisionsCount ) 
                            {
                                // Expand buffer if needed
                                a_collisionInstancesBuffer.Add ( collisionInstancesBuffer ) ;
                            }
                            else
                            {
                                a_collisionInstancesBuffer [i_collisionsCount] = collisionInstancesBuffer ;
                            }
                                                        
                            isCollidingData.i_collisionsCount ++ ;

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
                        _GetNodeColliding ( octreeRootNodeData, i_nodeChildIndex, checkBounds, ref a_collisionInstancesBuffer, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) ;
                    }
			    }
		    }
            
            return isCollidingData.i_collisionsCount > 0 ? true : false ; 

	    }

    }

}
