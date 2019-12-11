using Unity.Collections ;
using Unity.Rendering ;
using Unity.Entities ;
using Unity.Jobs ;


namespace Antypodish.ECS.Highlight
{

    public class ResetBarrier : BarrierSystem {} ;


    public class ResetSystem : JobComponentSystem
    {

        [Inject] private ResetBarrier barrier ;

        ComponentGroup group ;
        
        protected override void OnCreateManager ( )
        {

            group = GetComponentGroup 
            (    
                typeof ( MeshInstanceRenderer ),
                typeof ( ResetHighlightTag )
            ) ;
            
            base.OnCreateManager ( );
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {

            JobHandle job = new Job
            {
                ecb = barrier.CreateCommandBuffer (),
                a_entities = group.GetEntityArray (),

            }.Schedule ( inputDeps ) ;

            return job ;

        }

        /// <summary>
        /// Execute Jobs
        /// </summary>
        // [BurstCompile]
        struct Job : IJob
        // struct CollisionJob : IJobParallelFor // example of job parallel for
        {
            
            [ReadOnly] public EntityCommandBuffer ecb ;
            [ReadOnly] public EntityArray a_entities;
            

            public void Execute ()
            {

                for (int i = 0; i < a_entities.Length; ++i )
                {

                    Entity highlight = a_entities [i] ;
                     
                    // renderer
                    Unity.Rendering.MeshInstanceRenderer renderer = Common.previousMeshInstanceRenderer ; // Bootstrap.playerRenderer ;
                     
                    ecb.SetSharedComponent <MeshInstanceRenderer> ( highlight, renderer ) ; // replace renderer with material and mesh

                    ecb.RemoveComponent <ResetHighlightTag> ( highlight ) ; 
                    
                }
                               
            }           
            
        } // job

    }

}
