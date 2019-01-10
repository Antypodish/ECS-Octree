using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using UnityEngine ;


namespace ECS.Blocks
{    

    public class RemoveBlockBarrier : BarrierSystem {} ;


    public class RemoveBlockSystem : JobComponentSystem
    {
        
        [Inject] private RemoveBlockBarrier barrier ;

        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            
            group = GetComponentGroup 
            (
                typeof ( RemoveBlockTag )
            ) ;

            // entityManager = World.Active.GetOrCreateManager <EntityManager>() ;
            
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
        {
            
            [ReadOnly] public EntityCommandBuffer ecb ;
            [ReadOnly] public EntityArray a_entities;
            

            public void Execute ()
            {
                
                for (int i = 0; i < a_entities.Length; ++i)
                {
                    Entity blockEntity = a_entities [i] ;

                    ecb.DestroyEntity ( blockEntity ) ;
                    // _RemoveBlock ( blockEntity );                
                }
            
                               
            }           
            
        } // job
        
    }
}
