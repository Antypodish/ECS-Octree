using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

namespace ECS.Highlight
{

    public class SetBarrier : BarrierSystem {} ;

    [UpdateAfter (typeof (ResetSystem) )]
    public class SetSystem : JobComponentSystem
    {
        
        [Inject] private SetBarrier barrier ;
        EntityManager entityManager ;
        ComponentGroup group ;


        protected override void OnCreateManager ( )
        {

            group = GetComponentGroup 
            (            
                typeof ( MeshInstanceRenderer ),
                typeof ( SetHighlightTag )
            ) ;
            
            entityManager = World.Active.GetOrCreateManager <EntityManager>() ;

            SwitchMethods._Initialize ( ) ;
        }

        
        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            EntityArray a_entities = group.GetEntityArray () ;

            for (int i = 0; i < a_entities.Length; ++i )
            {
                Entity entity = a_entities [i] ;

                // Renderer
                Common.previousMeshInstanceRenderer = entityManager.GetSharedComponentData <MeshInstanceRenderer> ( entity ) ;

                // assigne new renderrer
                Unity.Rendering.MeshInstanceRenderer renderer = Bootstrap.highlightRenderer ;
                                         
                ecb.SetSharedComponent <MeshInstanceRenderer> ( entity, renderer ) ; // replace renderer with material and mesh

                ecb.RemoveComponent <SetHighlightTag> ( entity ) ; 


            }


            /*
            JobHandle job = new Job
            {
                ecb = barrier.CreateCommandBuffer (),
                entityManager = entityManager,
                a_entities = group.GetEntityArray (),
                
            }.Schedule (inputDeps) ;

            return job ;
            */

            return inputDeps ;

        }

        /*
        /// <summary>
        /// Execute Jobs
        /// </summary>
        // [BurstCompile]
        struct Job : IJob
        {
            
            [ReadOnly] public EntityCommandBuffer ecb ;
            [ReadOnly] public EntityArray a_entities ;
            [ReadOnly] public EntityManager entityManager ;
            

            public void Execute ()
            {

                for (int i = 0; i < a_entities.Length; ++i )
                {
                    Entity blockEntity = a_entities [i] ;
                      
                    // renderer
                    BlockHighlightCommon.previousMeshInstanceRenderer = entityManager.GetSharedComponentData <MeshInstanceRenderer> ( blockEntity ) ;

                    // assigne new renderrer
                    Unity.Rendering.MeshInstanceRenderer renderer = Bootstrap.highlightRenderer ;
                                         
                    ecb.SetSharedComponent <MeshInstanceRenderer> ( blockEntity, renderer ) ; // replace renderer with material and mesh

                    ecb.RemoveComponent <BlockSetHighlightTag> ( blockEntity ) ; 

                }
                               
            }           
            
        } // job
        */

    }

}
