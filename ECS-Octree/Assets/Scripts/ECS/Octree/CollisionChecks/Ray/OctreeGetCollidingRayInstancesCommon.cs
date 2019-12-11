using Unity.Entities ;
using UnityEngine;


namespace Antypodish.ECS.Octree
{


    internal class GetCollidingRayInstances_Common
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_collisionChecksEntities"></param>
        /// <param name="a_rayData"></param>
        /// <param name="a_rayMaxDistanceData"></param>
        /// <param name="a_isCollidingData"></param>
        /// <param name="collisionInstancesBufferElement"></param>
        /// <param name="canDebugAllChecks">Debug Log all checks, or only one (first one)</param>
        /// <param name="canDebugAllrays">Draw all, or only single ray (first one).</param>
        static public void _DebugRays ( ref EntityCommandBuffer ecb, EntityArray a_collisionChecksEntities, ComponentDataFromEntity <RayData> a_rayData, ComponentDataFromEntity <RayMaxDistanceData> a_rayMaxDistanceData, ComponentDataFromEntity <IsCollidingData> a_isCollidingData, BufferFromEntity <CollisionInstancesBufferElement> collisionInstancesBufferElement, ComponentDataFromEntity <RayEntityPair4CollisionData> a_rayEntityPair4CollisionData, bool canDebugAllChecks, bool canDebugAllrays )
        {

            // Debug
            // ! Ensure test this only with single, or at most few ray entiities.


            // if ( !canDebugAllrays ) 

            // Debug all, or only one check
            int i_debugCollisionChecksCount = canDebugAllChecks ? a_collisionChecksEntities.Length : 1 ;

            for ( int i_collisionChecksIndex = 0; i_collisionChecksIndex < i_debugCollisionChecksCount; i_collisionChecksIndex ++ )
            {
                  
                Entity octreeRayEntity = a_collisionChecksEntities [i_collisionChecksIndex] ;
                Entity octreeRayEntity2 ;

                if ( !a_rayData.Exists ( octreeRayEntity ) )
                {
                    RayEntityPair4CollisionData rayEntityPair4CollisionData =  a_rayEntityPair4CollisionData [octreeRayEntity] ;
                    octreeRayEntity2 = rayEntityPair4CollisionData.ray2CheckEntity ;

                }
                else
                {
                    octreeRayEntity2 = octreeRayEntity ;
                }

                // Draw all available rays, or signle ray
                if ( canDebugAllrays ) 
                {
                    RayData rayData = a_rayData [octreeRayEntity2] ;
                    RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [octreeRayEntity2] ;

                    Debug.DrawLine ( rayData.ray.origin, rayData.ray.origin + rayData.ray.direction * rayMaxDistanceData.f, Color.red )  ;
                }
                else if ( i_collisionChecksIndex == 0 ) 
                {                    
                    RayData rayData = a_rayData [octreeRayEntity2] ;
                    RayMaxDistanceData rayMaxDistanceData = a_rayMaxDistanceData [octreeRayEntity2] ;

                    Debug.DrawLine ( rayData.ray.origin, rayData.ray.origin + rayData.ray.direction * rayMaxDistanceData.f, Color.red )  ;
                }
                //}


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

                    CollisionInstancesBufferElement closestCollisionInstance = a_collisionInstancesBuffer [isCollidingData.i_nearestInstanceCollisionIndex] ;
                    Entity closestInstanceEntity = new Entity () 
                    { 
                        Index = closestCollisionInstance.i_ID, 
                        Version = closestCollisionInstance.i_version 
                    } ;
                    
                    // Test highlight
                    Highlight.SwitchMethods._Switch ( ref ecb, closestInstanceEntity ) ;

                    Debug.Log ( "Is colliding with #" + isCollidingData.i_collisionsCount + " instances of IDs: " + s_collidingIDs + "; Nearest collided instance is at " + isCollidingData.f_nearestDistance + "m, with ID #" + a_collisionInstancesBuffer [isCollidingData.i_nearestInstanceCollisionIndex].i_ID ) ;
                    
                }
                
            } // for

        }

        /// <summary>
	    /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check. Passing by ref as it improves performance with structs.</param>
	    /// <param name="l_resultInstanceIDs">List result.</param>
	    /// <param name="i_nearestIndex">Nerest collision index from the lits.</param>
	    /// <param name="f_nearestDistance">Nerest collision distance.</param>
	    /// <param name="maxDistance">Distance to check.</param>
	    /// <returns>Instances index, that intersect with the specified ray.</returns>
	    static public bool _GetNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Ray checkRay, ref DynamicBuffer <CollisionInstancesBufferElement> a_collisionInstancesBuffer, ref IsCollidingData isCollidingData, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer, float f_maxDistance = float.PositiveInfinity ) 
        {
		
            float f_distance ;
            
            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;
            
		    if ( !nodeBuffer.bounds.IntersectRay ( checkRay, out f_distance ) || f_distance > f_maxDistance ) 
            {
			    return isCollidingData.i_collisionsCount > 0 ? true : false ; 
		    }

            if ( nodeBuffer.i_instancesCount > 0 ) 
            {            
                int i_nodeInstancesIndexOffset = i_nodeIndex * rootNodeData.i_instancesAllowedCount ;

                CollisionInstancesBufferElement collisionInstancesBuffer = new CollisionInstancesBufferElement () ;

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

                            if ( f_distance < isCollidingData.f_nearestDistance )
                            {
                                isCollidingData.f_nearestDistance = f_distance ;
                                isCollidingData.i_nearestInstanceCollisionIndex = isCollidingData.i_collisionsCount ;
                            }


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
                        _GetNodeColliding ( rootNodeData, i_nodeChildIndex, checkRay, ref a_collisionInstancesBuffer, ref isCollidingData, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) ;
                    }
			    }
		    }

            return isCollidingData.i_collisionsCount > 0 ? true : false ; 

	    }

    }
}
