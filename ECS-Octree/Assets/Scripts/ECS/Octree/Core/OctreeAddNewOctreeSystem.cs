using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;
using UnityEngine ;


namespace Antypodish.ECS.Octree
{

    public class AddNewOctreeSystem : JobComponentSystem
    {

        EndInitializationEntityCommandBufferSystem eiecb ;

        ComponentGroup group ;

        protected override void OnCreate ( )
        {

            Debug.Log ( "Start Add New Octree System" ) ;
            Debug.LogWarning ( "TODO: Replace instance with entity?" ) ;
            Debug.LogWarning ( "TODO: incomplete Get max bounds?" ) ;

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            
            group = GetComponentGroup 
            ( 
                typeof (AddNewOctreeData)    
            ) ;

        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            EntityArray a_newOctreeEntities = group.GetEntityArray () ;
            
            for ( int i = 0; i < a_newOctreeEntities.Length; i ++)
            {
                Entity e = a_newOctreeEntities [i] ;

                Debug.Log ( "#" + i + "; e #" + e.Index )  ;
            }


            int i_groupLength = group.CalculateLength () ;

            var initialiseOctreeJob = new InitialiseOctreeJob 
            {
                          
                a_newOctreeEntities                 = group.GetEntityArray (),

                a_addNewOctreeData                  = GetComponentDataFromEntity <AddNewOctreeData> (),
                a_rootNodeData                      = GetComponentDataFromEntity <RootNodeData> (),

                nodeSparesBufferElement             = GetBufferFromEntity <NodeSparesBufferElement> (),
                nodeBufferElement                   = GetBufferFromEntity <NodeBufferElement> (),
                nodeInstancesIndexBufferElement     = GetBufferFromEntity <NodeInstancesIndexBufferElement> (),
                nodeChildrenBufferElement           = GetBufferFromEntity <NodeChildrenBufferElement> (),
                instanceBufferElement               = GetBufferFromEntity <InstanceBufferElement> (),
                instancesSpareIndexBufferElement    = GetBufferFromEntity <InstancesSpareIndexBufferElement> ()

            }.Schedule ( i_groupLength, 8, inputDeps ) ;

            var finalizeInitialisationOctreeJob = new FinalizeInitialisationOctreeJob 
            {
                
                ecb                                 = eiecb.CreateCommandBuffer ().ToConcurrent (),                
                a_newOctreeEntities                 = group.GetEntityArray ()

            }.Schedule ( i_groupLength, 8, initialiseOctreeJob ) ;


            return finalizeInitialisationOctreeJob ;

        }

        [BurstCompile]
        [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct InitialiseOctreeJob : IJobParallelFor 
        {
            
            /// [ReadOnly] public EntityCommandBuffer.Concurrent ecb ;
            [ReadOnly] public EntityArray a_newOctreeEntities ;

            [ReadOnly] public ComponentDataFromEntity <AddNewOctreeData> a_addNewOctreeData ;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity <RootNodeData> a_rootNodeData ;
            
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeSparesBufferElement> nodeSparesBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeBufferElement> nodeBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeInstancesIndexBufferElement> nodeInstancesIndexBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <NodeChildrenBufferElement> nodeChildrenBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <InstanceBufferElement> instanceBufferElement ;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity <InstancesSpareIndexBufferElement> instancesSpareIndexBufferElement ;


            public void Execute ( int i_arrayIndex )
            {

                Entity octreeRootNodeEntity = a_newOctreeEntities [i_arrayIndex] ;


                AddNewOctreeData addNewOctreeData                                                   = a_addNewOctreeData [octreeRootNodeEntity] ;

                RootNodeData rootNodeData                                                           = a_rootNodeData [octreeRootNodeEntity] ;
                rootNodeData.i_nodeSpareLastIndex -- ;               

            
                DynamicBuffer <NodeSparesBufferElement> a_nodeSparesBuffer                          = nodeSparesBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeBufferElement> a_nodesBuffer                                     = nodeBufferElement [octreeRootNodeEntity] ;
                DynamicBuffer <NodeInstancesIndexBufferElement> a_nodeInstancesIndexBuffer          = nodeInstancesIndexBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <NodeChildrenBufferElement> a_nodeChildrenBuffer                      = nodeChildrenBufferElement [octreeRootNodeEntity] ;    

                CommonMethods._CreateNewNode ( ref rootNodeData, rootNodeData.i_rootNodeIndex, addNewOctreeData.f_initialSize, addNewOctreeData.f3_initialPosition, ref a_nodesBuffer, ref a_nodeSparesBuffer, ref a_nodeChildrenBuffer, ref a_nodeInstancesIndexBuffer ) ;
            
                rootNodeData.i_nodeSpareLastIndex -- ;                 
            
                rootNodeData.i_instancesSpareLastIndex                                              = 0 ;

            
                DynamicBuffer <InstanceBufferElement> a_instanceBuffer                              = instanceBufferElement [octreeRootNodeEntity] ;   
                DynamicBuffer <InstancesSpareIndexBufferElement> a_instancesSpareIndexBuffer        = instancesSpareIndexBufferElement [octreeRootNodeEntity] ;    

                // Add some spares if needed.
                //int i_requiredNumberOfInstances = CommonMethods.numOfSpareInstances2Add ;
                //int i_requiredNumberOfInstances = Octree.Examples.ExampleSelector.i_generateInstanceInOctreeCount ;
                int i_requiredNumberOfSpareInstances = 100 ;
                CommonMethods._AddInstanceSpares ( ref rootNodeData, ref a_instanceBuffer, ref a_instancesSpareIndexBuffer, i_requiredNumberOfSpareInstances ) ;   
            
                    

                a_rootNodeData [octreeRootNodeEntity] = rootNodeData ; // Set back
                    
            }
        }


        [RequireComponentTag ( typeof (AddNewOctreeData) ) ]
        struct FinalizeInitialisationOctreeJob : IJobParallelFor
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent ecb ;
            [ReadOnly] public EntityArray a_newOctreeEntities ;


            public void Execute ( int i_arrayIndex )
            {
                
                Entity octreeRootNodeEntity = a_newOctreeEntities [i_arrayIndex] ;

                ecb.AddComponent ( i_arrayIndex, octreeRootNodeEntity, new IsActiveTag () ) ; // Octree initialized
                ecb.RemoveComponent <AddNewOctreeData> ( i_arrayIndex, octreeRootNodeEntity ) ; // Octree initialized

            }

        }


        /// <summary>
	    /// Constructor for the bounds octree.
        /// For minimum size of 1 initial size could be for example 1, 2, 4, 8, 16 ect.
        /// For minimum size of 3 initial size could be for example 3, 9, 27, 81 etc.
        /// Also:
        /// Size 2 per node, with up to 16 instances per node (1x1x1 size each);
        /// Size 4 per node, with up to 64 instances per node (1x1x1 size each); Less memory usage, but more cpu demanding (for searching and removing elements).
	    /// </summary>
	    /// <param name="f_initialSize">Size of the sides of the initial node, in metres. The octree will never shrink smaller than this.</param>
	    /// <param name="f3_initialPosition">Position of the centre of the initial node.</param>
	    /// <param name="f_minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this (metres).</param>
	    /// <param name="f_looseness">Clamped between 1 and 2. Values > 1 let nodes overlap.</param>
        static public void _CreateNewOctree ( EntityCommandBuffer ecb, Entity newOctreeEntity, float f_initialSize, float3 f3_initialPosition, float f_minNodeSize, float f_looseness )
        {
                        
            Debug.Log ( "Create new octree #" + newOctreeEntity.Index ) ;
            
            ecb.AddComponent ( newOctreeEntity, new AddNewOctreeData ()
            {
                f3_initialPosition = f3_initialPosition,
                f_initialSize = f_initialSize

            } ) ; // This tag is removed, after octree is created.



            if ( f_minNodeSize > f_initialSize ) 
            {
			    Debug.LogWarning ( "Minimum node size must be at least as big as the initial world size. Was: " + f_initialSize + " Adjusted to: " + f_minNodeSize ) ;
			    f_minNodeSize = f_initialSize;
		    }

            // Minimum size to power 3
            // For example, size of 2, gives 8 instances per node
            int i_instancesAllowedInNodeCount = (int) math.round ( f_minNodeSize * f_minNodeSize * f_minNodeSize ) ;

            RootNodeData rootNodeData = new RootNodeData ()
            {
                i_rootNodeIndex             = 0,

                f_initialSize               = f_initialSize,
                f_minSize                   = f_minNodeSize,                                
                f_looseness                 = math.clamp ( f_looseness, 1.0f, 2.0f ),

                i_totalInstancesCountInTree = 0, 

                i_instancesSpareLastIndex   = 0,
                i_nodeSpareLastIndex        = 0,

                i_instancesAllowedCount     = i_instancesAllowedInNodeCount
            } ;


            // ***** Core Components ***** //

            
            ecb.AddComponent ( newOctreeEntity, rootNodeData ) ; // Core of octree structure.

            // Add buffer arrays
            ecb.AddBuffer <NodeBufferElement> ( newOctreeEntity ) ;
            ecb.AddBuffer <NodeChildrenBufferElement> ( newOctreeEntity ) ;
            ecb.AddBuffer <NodeInstancesIndexBufferElement> ( newOctreeEntity ) ;
            ecb.AddBuffer <NodeSparesBufferElement> ( newOctreeEntity ) ;

            ecb.AddBuffer <InstanceBufferElement> ( newOctreeEntity ) ;
            ecb.AddBuffer <InstancesSpareIndexBufferElement> ( newOctreeEntity ) ; 


            
            // ***** Instance Optional Components ***** //

            // ecb.AddBuffer <AddInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.
            // ecb.AddComponent ( newOctreeEntity, new RemoveInstanceBufferElement () ) ; // Once system executed and instances were removed, tag will be deleted.


        }

    }
}

