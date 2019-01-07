using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Burst ;

using System.Collections.Generic ; // temporary for list ( to remove )

namespace ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.Experimental.PlayerLoop.PostLateUpdate ) ) ]    
    class OctreeGetCollidingBoundsInstancesSystem : JobComponentSystem
    {
        
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            Debug.Log ( "Start Octree Get Colliding Bounds Instances System" ) ;

            base.OnCreateManager ( );

            group = GetComponentGroup ( 
                typeof (IsActiveTag), 
                typeof (GetCollidingBoundsInstancesTag), 
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

            // Return collided instances ID
            // Ensure collection is clear.
            List <int> l_resultInstanceIDs = new List<int> () ;
            
                         
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
  
            

            if ( _GetNodeColliding ( rootNodeData, rootNodeData.i_rootNodeIndex, checkBounds, ref l_resultInstanceIDs, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) )
            {
                string s_collidingIDs = "" ;

                for ( int i = 0; i < l_resultInstanceIDs.Count; i ++ )
                {
                    s_collidingIDs += l_resultInstanceIDs [i] + ", " ;
                }

                Debug.Log ( "Is colliding with count #" + l_resultInstanceIDs.Count + "; IDs: " + s_collidingIDs ) ;
            }

            return base.OnUpdate ( inputDeps );


        }


        /// <summary>
	    /// Returns an collection of objects, that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
	    /// </summary>
        /// <param name="i_nodeIndex">Internal octree node index.</param>
	    /// <param name="checkBounds">Bounds to check. Passing by ref as it improves performance with structs.</param>
	    /// <param name="l_resultInstanceIDs">List result.</param>
        private bool _GetNodeColliding ( RootNodeData rootNodeData, int i_nodeIndex, Bounds checkBounds, ref List <int> l_resultInstanceIDs, DynamicBuffer <NodeBufferElement> a_nodesBuffer, DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer, DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer, DynamicBuffer <InstanceBufferElement> a_instanceBuffer ) 
	    //private void _GetNodeColliding ( int i_nodeIndex, Bounds checkBounds, ref List <int> l_resultInstanceIDs ) 
        {

            NodeBufferElement nodeBuffer = a_nodesBuffer [i_nodeIndex] ;

            // Are the input bounds at least partially in this node?
		    if ( !nodeBuffer.bounds.Intersects ( checkBounds ) ) 
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

                        // Check if instance exists, and if has intersecting bounds.
			            if ( instanceBuffer.bounds.Intersects (checkBounds) ) 
                        {                            
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
                        _GetNodeColliding ( rootNodeData, i_nodeChildIndex, checkBounds, ref l_resultInstanceIDs, a_nodesBuffer, a_nodeChildrenBuffer, a_nodeInstancesIndexBuffer, a_instanceBuffer ) ;
                    }
			    }
		    }
            
            return l_resultInstanceIDs.Count > 0 ? true : false ; 

	    }


    }

}
