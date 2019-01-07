using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Burst ;

namespace ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class OctreeIsRayCollidingSystem : JobComponentSystem
    {

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Ray Colliding System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag), 
                typeof (IsRayCollidingTag), 
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
  


            if ( _IsNodeColliding ( rootNodeData, rootNodeData.i_rootNodeIndex, ray, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) )
            {
                Debug.Log ( "Is colliding." ) ;
            }

            return base.OnUpdate ( inputDeps );

        }


        /// <summary>
	    /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkRay">Ray to check.</param>
	    /// <param name="f_maxDistance">Distance to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    private bool _IsNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Ray checkRay, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer, float f_maxDistance = float.PositiveInfinity ) 
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
                        if ( _IsNodeColliding ( rootNodeData, i_nodeChildIndex, checkRay, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer, f_maxDistance ) )
                        {
					        return true ;
				        }
                    }
			    }
		    }

		    return false;
	    }
    }
}
