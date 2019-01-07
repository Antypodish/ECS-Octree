using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Burst ;

namespace ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class OctreeIsBoundsCollidingSystem : JobComponentSystem
    {
        
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Bounds Colliding System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag), 
                typeof (IsBoundsCollidingTag), 
                typeof (RootNodeData) 
            ) ;

        }

        
        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            // Debug.LogWarning ( "Col" ) ;
            Bounds checkBounds = new Bounds () 
            { 
                center = new float3 ( 10, 2, 10 ), 
                size = new float3 ( 1, 1, 1 ) * 5 // Total size of boundry 
            } ;

             
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
  


            if ( _IsNodeColliding ( rootNodeData, rootNodeData.i_rootNodeIndex, checkBounds, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) )
            {
                Debug.Log ( "Is colliding." ) ;
            }

            return base.OnUpdate ( inputDeps );

        }



        /// <summary>
	    /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check.</param>
	    /// <returns>True if there was a collision.</returns>
	    private bool _IsNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Bounds checkBounds, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 
        {


            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

		    // Are the input bounds at least partially in this node?
		    if ( !nodeBuffer.bounds.Intersects ( checkBounds ) ) 
            {
			    return false;
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

                        // Check if instance exists, and if has intersecting bounds.
			            if ( instanceBuffer.bounds.Intersects (checkBounds) ) 
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
				        if ( _IsNodeColliding ( rootNodeData, i_nodeChildIndex, checkBounds, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) ) 
                        {
					        return true ;
				        }

                    }
			    }
		    }

		    return false ;
	    }


    }

}
