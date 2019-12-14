using Unity.Mathematics ;
using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{
    

    // [UpdateAfter ( typeof ( UnityEngine.PlayerLoop.PostLateUpdate ) ) ]    
    class GetMaxBoundsSystem : JobComponentSystem
    {
        
        EntityQuery group ;

        protected override void OnCreate ( )
        {
            
            Debug.Log ( "Start Octree Get Max Bounds Colliding Instances System" ) ;
            
            group = GetEntityQuery 
            (                 
                typeof ( IsActiveTag ), 
                typeof ( GetMaxBoundsTag ), 
                typeof ( RootNodeData ) 
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

            int i_firstEntity = 0 ; 

            NativeArray <Entity> na_entities                       = group.ToEntityArray ( Allocator.TempJob ) ;
            Entity rootNodeEntity                                  = na_entities [i_firstEntity] ;
            na_entities.Dispose () ;
            
            ComponentDataFromEntity <RootNodeData> a_rootNodeData  = GetComponentDataFromEntity <RootNodeData> ( true ) ;
            // ComponentDataArray <RootNodeData> a_rootNodeData       = group.GetComponentDataArray <RootNodeData> ( ) ;
            RootNodeData rootNode                                  = a_rootNodeData [rootNodeEntity] ;
            
            BufferFromEntity <NodeBufferElement> nodeBufferElement = GetBufferFromEntity <NodeBufferElement> ( true ) ;
            DynamicBuffer <NodeBufferElement> a_nodesBuffer        = nodeBufferElement [rootNodeEntity] ;


            Bounds maxBouds                                        = _GetOctreeMaxBounds ( ref rootNode, ref a_nodesBuffer ) ;


            return inputDeps ;

        }

        /// <summary>
        /// Get total octree bounds (boundary box).
        /// </summary>
        /// <returns></returns>
	    public Bounds _GetOctreeMaxBounds ( [ReadOnly] ref RootNodeData rootNode, [ReadOnly] ref DynamicBuffer <NodeBufferElement> a_nodesBuffer )
	    {
            NodeBufferElement nodeBuffer = a_nodesBuffer [rootNode.i_rootNodeIndex] ;
		    // return _GetNodeBounds ( i_rootNodeIndex ) ;
            return nodeBuffer.bounds ;
	    }

    }

}




        