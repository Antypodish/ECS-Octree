using Unity.Entities ;
using Unity.Collections ;
using Unity.Transforms ;
using Unity.Mathematics ;
using Unity.Rendering ;
using Unity.Jobs ;

namespace ECS.Blocks
{    
    
    public class AddBlockBarrier : BarrierSystem {} ;


    public class AddBlockSystem : JobComponentSystem
    {

        [Inject] private AddBlockBarrier barrier ;

        ComponentGroup group ;

        // static EntityManager entityManager ;

        protected override void OnCreateManager ( )
        {         

            group = GetComponentGroup 
            (            
                typeof ( AddBlockData )
            ) ;


            /*
            Debug.Log ( "Add block system requires add Job Parallel For" ) ;

            commandsBuffer = addBlockBarrier.CreateCommandBuffer () ;

            entityManager = World.Active.GetOrCreateManager <EntityManager>() ;
                        
            // entityManager = World.Active.GetOrCreateManager <EntityManager>() ;
            MeshInstanceRenderer renderer = Bootstrap.blockPrefab01 ;
            commandsBuffer.CreateEntity () ;
            // commandsBuffer.AddComponent ( new MeshCullingComponent { } ) ;     
            commandsBuffer.AddSharedComponent ( renderer ) ;
            */
            
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            JobHandle job = new Job
            {
                ecb = barrier.CreateCommandBuffer (),
                a_entities = group.GetEntityArray (),
                a_addBlockData = group.GetComponentDataArray <AddBlockData> (),
                
            }.Schedule (inputDeps) ;

            return job ;

        }


        /// <summary>
        /// Execute Jobs
        /// </summary>
        // [BurstCompile]
        struct Job : IJob
        {

            [ReadOnly] public EntityCommandBuffer ecb ;
            [ReadOnly] public EntityArray a_entities ;
            [ReadOnly] public ComponentDataArray <AddBlockData> a_addBlockData ;

            public void Execute ()
            {

                for (int i = 0; i < a_entities.Length; ++i)
                {

                    Entity blockEntity = a_entities [i] ;
                    AddBlockData addBlockData = a_addBlockData [i] ;

                    ecb.AddComponent ( blockEntity, new Position { Value = addBlockData.f3_position } ) ;
                    ecb.AddComponent ( blockEntity, new Rotation { Value = quaternion.identity} ) ;
                    ecb.AddComponent ( blockEntity, new Scale { Value = addBlockData.f3_scale } ) ;

                    
                    MeshInstanceRenderer renderer = Bootstrap.blockPrefab01 ;
                    ecb.AddSharedComponent ( blockEntity, renderer ) ;

                    ecb.RemoveComponent <AddBlockData> ( blockEntity ) ; // Block added. Remove tag

                    // _AddBlock ( blockEntity );
                
                }
                               
            }           
            
        } // job



        private void _AddBlock ( Entity blockEntity )
        {


            // AddBlocktag blockTagsData = blockData.a_blockTags [i] ;
                        
            // float3 position = blockTagsData.f3_position ;
            // float3 scale = blockTagsData.f3_scale ;
            
            // Entity entity = blockData.a_entities [i] ;
            // float4x4 f4x4 = math.mul ( float4x4.identity, new float4x4(scale.x, 0, 0, 0, 0, scale.y, 0, 0, 0, 0, scale.z, 0, 0, 0, 0, 1) ) ; // set default position/rotation/scale matrix
            //float4x4 f4x4 = new float4x4(scale.x, 0, 0, 0, 0, scale.y, 0, 0, 0, 0, scale.z, 0, position.x, position.y, position.z, 1) ; // set default position/rotation/scale matrix
            // commandsBuffer.AddComponent ( entity, new TransformMatrix { Value = f4x4 } ) ;
//            commandsBuffer.AddComponent ( blockEntity, new Position { Value = position } ) ;
//            commandsBuffer.AddComponent ( blockEntity, new Rotation { Value = quaternion.identity} ) ;
//            commandsBuffer.AddComponent ( blockEntity, new Scale { Value  = scale } ) ;
                       
            // renderer
//            MeshInstanceRenderer renderer ;
              
            /*
            //blockTagsData.f4_color
            if ( blockTagsData.f4_color.x == 1 )
            {
                // apply random renderer (color/mesh), from the prefabs

                int i_textureIndex ;
                

                if ( Mathf.RoundToInt ( blockTagsData.f4_color.z ) == 0 || Mathf.RoundToInt ( blockTagsData.f4_color.z ) == 1 )
                {
                    
                    i_textureIndex = blockTagsData.f4_color.z == 1 ? Mathf.RoundToInt ( UnityEngine.Random.Range ( 1, 7) ) : Mathf.RoundToInt ( blockTagsData.f4_color.y ) ;

                    switch ( i_textureIndex )
                    {
                        case 1:
                            renderer = Bootstrap.octreeCenter01 ;
                            break ;
                        case 2:
                            renderer = Bootstrap.octreeCenter02 ;
                            break ;
                        case 3:
                            renderer = Bootstrap.octreeCenter03 ;
                            break ;
                        case 4:
                            renderer = Bootstrap.octreeCenter04 ;
                            break ;
                        case 5:
                            renderer = Bootstrap.octreeCenter05 ;
                            break ;
                        case 6:
                            renderer = Bootstrap.octreeCenter06 ;
                            break ;
                        case 7:
                            renderer = Bootstrap.octreeCenter07 ;
                            break ;
                        default:
                            renderer = Bootstrap.octreeCenter01 ;
                            break ;
                    }

                }
                else
                {
                    renderer = Bootstrap.octreeNode ;
                }
                
            }
            else
            {
                renderer = Bootstrap.playerRenderer ;
            }
            */

//            renderer = Bootstrap.blockPrefab01 ;
//            commandsBuffer.AddSharedComponent ( blockEntity, renderer ) ;

//            commandsBuffer.RemoveComponent <AddBlockData> ( blockEntity ) ; // Block added. Remove tag
                  
        }
                
    }
}
