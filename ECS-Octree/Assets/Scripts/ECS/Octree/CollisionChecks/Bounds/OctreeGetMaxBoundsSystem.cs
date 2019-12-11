using Unity.Mathematics ;
using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    

    [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]    
    class GetMaxBoundsSystem : JobComponentSystem
    {
        
        ComponentGroup group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Max Bounds Colliding Instances System" ) ;
            
            group = GetComponentGroup (                 
                typeof (IsActiveTag), 
                typeof (GetMaxBoundsTag), 
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

             
            NativeArray <Entity> na_entities                       = group.GetEntityArray () ;
            Entity rootNodeEntity                                  = na_entities [0] ;
            na_entities.Dispose () ;
            
            ComponentDataArray <RootNodeData> a_rootNodeData       = group.GetComponentDataArray <RootNodeData> ( ) ;
            RootNodeData rootNodeData                              = a_rootNodeData [0] ;
            
            BufferFromEntity <NodeBufferElement> nodeBufferElement = GetBufferFromEntity <NodeBufferElement> () ;
            DynamicBuffer <NodeBufferElement> a_nodesBuffer        = nodeBufferElement [rootNodeEntity] ;


            Bounds maxBouds                                        = _GetOctreeMaxBounds ( rootNodeData, a_nodesBuffer ) ;


            return inputDeps ;

        }

        /// <summary>
        /// Get total octree bounds (boundary box).
        /// </summary>
        /// <returns></returns>
	    public Bounds _GetOctreeMaxBounds ( RootNodeData rootNodeData, DynamicBuffer <NodeBufferElement> a_nodesBuffer )
	    {
            NodeBufferElement nodeBuffer = a_nodesBuffer [rootNodeData.i_rootNodeIndex] ;
		    // return _GetNodeBounds ( i_rootNodeIndex ) ;
            return nodeBuffer.bounds ;
	    }

    }

}




        