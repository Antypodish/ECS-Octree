using Unity.Collections ;
using Unity.Mathematics ;
using Unity.Transforms ;
using Unity.Rendering ;
using Unity.Entities ;
using Unity.Jobs ;


namespace Antypodish.ECS.Blocks
{    
        
    public class AddBlockSystem : JobComponentSystem
    {

        EndInitializationEntityCommandBufferSystem eiecb ;

        // ComponentGroup group ;

        // static EntityManager entityManager ;

        protected override void OnCreate ( )
        {         
            
            // Cache the EndInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;

            /*
            group = GetComponentGroup 
            (            
                typeof ( AddBlockData )
            ) ;
            */
            
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            JobHandle jobHandle = new Job
            {
                ecb             = eiecb.CreateCommandBuffer ().ToConcurrent (),
                renderMeshTypes = Bootstrap.renderMeshTypes
                // a_entities = group.GetEntityArray (),
                // a_addBlockData = group.GetComponentDataArray <AddBlockData> (),
                
            }.Schedule ( this, inputDeps ) ;

            eiecb.AddJobHandleForProducer ( jobHandle ) ;

            return jobHandle ;

        }


        /// <summary>
        /// Execute Jobs
        /// </summary>
        [RequireComponentTag ( typeof ( AddBlockTag ) ) ]
        // [BurstCompile]
        struct Job : IJobForEachWithEntity <MeshTypeData>
        {

            [ReadOnly] 
            public EntityCommandBuffer.Concurrent ecb ;
            // [ReadOnly] public EntityArray a_entities ;
            // [ReadOnly] public ComponentDataArray <AddBlockData> a_addBlockData ;
            [ReadOnly] 
            public Bootstrap.RenderMeshTypes renderMeshTypes ;

            public void Execute ( Entity blockEntity, int jobIndex, [ReadOnly] ref MeshTypeData meshType )
            {

                // AddBlockData addBlockData = a_addBlockData [i] ;

                ecb.AddComponent ( jobIndex, blockEntity, new Translation { Value = addBlockData.f3_position } ) ;
                ecb.AddComponent ( jobIndex, blockEntity, new Rotation { Value = quaternion.identity} ) ;
                ecb.AddComponent ( jobIndex, blockEntity, new NonUniformScale { Value = addBlockData.f3_scale } ) ;
                // ...
                // ecb.AddComponent ( jobIndex, blockEntity, new Highlight.MeshType { type = Highlight.Common.MeshType.Default } ) ;

                    
                RenderMesh renderer = Bootstrap._SelectRenderMesh ( meshType.type, ref renderMeshTypes ) ;
                ecb.AddSharedComponent ( jobIndex, blockEntity, renderer ) ;
                
                
                ecb.RemoveComponent <AddBlockTag> ( jobIndex, blockEntity ) ; // Block added. Remove tag
                // ecb.RemoveComponent <AddBlockData> ( jobIndex, blockEntity ) ; // Block added. Remove tag

                    // _AddBlock ( blockEntity );
                                               
            }           
            
        } // job
                        
    }
}
