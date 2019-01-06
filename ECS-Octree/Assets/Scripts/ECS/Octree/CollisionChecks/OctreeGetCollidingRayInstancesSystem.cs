using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Burst ;

using System.Collections.Generic ; // temporary for list ( to remove )

namespace ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class OctreeGetCollidingRayInstancesSystem : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Bounds Colliding System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (GetCollidingRayInstancesTag), 
                typeof (RootNodeData) 
            ) ;

        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            // Debug.LogWarning ( "Col" ) ;
            /*
            Ray ray = new Ray () 
            { 
                origin = new float3 ( 7.7f, 0.5f, 0 ), 
                direction = new float3 ( 0, 0, 1 ) // Total size of boundry 
            } ;
            */

            Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ;
                     
            // Return collided instances ID
            // Ensure collection is clear.
            List <int> l_resultInstanceIDs = new List<int> () ;  


            float f_maxDistance = 100 ;

            Debug.DrawLine ( ray.origin, ray.origin + ray.direction * f_maxDistance, Color.red )  ;
             
            EntityArray a_entities                                                                        = group.GetEntityArray () ;
            Entity rootNodeEntity                                                                         = a_entities [0] ;
            
            ComponentDataArray <RootNodeData> a_rootNodeData                                              = group.GetComponentDataArray <RootNodeData> ( ) ;
            RootNodeData rootNodeData                                                                     = a_rootNodeData [0] ;
            
            BufferFromEntity <NodeBufferElement> nodeBufferElement                                        = GetBufferFromEntity <NodeBufferElement> () ;
            DynamicBuffer <NodeBufferElement> a_nodesBuffer                                               = nodeBufferElement [rootNodeEntity] ;

            BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement            = GetBufferFromEntity <NodeInstancesIndexBufferElement> () ;
            DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer                    = nodeInstancesIndexBufferElement [rootNodeEntity] ;   

            BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement                        = GetBufferFromEntity <NodeChildrenBufferElement> () ;
            DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                                = nodeChildrenBufferElement [rootNodeEntity] ;    

            BufferFromEntity <InstanceBufferElement> instanceBufferElement                                = GetBufferFromEntity <InstanceBufferElement> () ;
            DynamicBuffer <InstanceBufferElement> a_instanceBuffer                                        = instanceBufferElement [rootNodeEntity] ;   
  

            int i_nearestIndex = 0 ;
            float f_nearestDistance = float.PositiveInfinity ;

            if ( _GetNodeColliding ( rootNodeData, rootNodeData.i_rootNodeIndex, ray, ref l_resultInstanceIDs, ref i_nearestIndex, ref f_nearestDistance, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) )
            {
                string s_collidingIDs = "" ;

                for ( int i = 0; i < l_resultInstanceIDs.Count; i ++ )
                {
                    s_collidingIDs += l_resultInstanceIDs [i] + ", " ;
                }

                Debug.Log ( "Is colliding with #" + l_resultInstanceIDs.Count + " instances of IDs: " + s_collidingIDs + "; Nearest collided instance is at " + f_nearestDistance + "m, with ID #" + i_nearestIndex ) ;

            }

            return base.OnUpdate ( inputDeps );

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
	    private bool _GetNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Ray checkRay, ref List <int> l_resultInstanceIDs, ref int i_nearestIndex, ref float f_nearestDistance, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer, float f_maxDistance = float.PositiveInfinity ) 
        {
		
            float f_distance;

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    if ( !nodeBuffer.bounds.IntersectRay ( checkRay, out f_distance ) || f_distance > f_maxDistance ) 
            {
			    return l_resultInstanceIDs.Count > 0 ? true : false ; 
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

                            if ( f_distance < f_nearestDistance )
                            {
                                f_nearestDistance = f_distance ;
                                i_nearestIndex = instanceBuffer.i_ID ;
                            }

				            l_resultInstanceIDs.Add ( instanceBuffer.i_ID ) ;

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
                        _GetNodeColliding ( rootNodeData, i_nodeChildIndex, checkRay, ref l_resultInstanceIDs, ref i_nearestIndex, ref f_nearestDistance, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) ;
                    }
			    }
		    }

            return l_resultInstanceIDs.Count > 0 ? true : false ; 

	    }


    }

}