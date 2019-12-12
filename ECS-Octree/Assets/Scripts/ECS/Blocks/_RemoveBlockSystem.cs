/*
using Unity.Collections ;
using Unity.Entities ;
using Unity.Burst ;
using Unity.Jobs ;


namespace Antypodish.ECS.Blocks
{    

    public class RemoveBlockSystem : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;

        protected override void OnCreate ( )
        {
            // Cache the EndInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
        }


        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {   

            JobHandle jobHandle = new Job
            {
                ecb = eiecb.CreateCommandBuffer ().ToConcurrent ()

            }.Schedule ( this, inputDeps ) ;

            eiecb.AddJobHandleForProducer ( jobHandle ) ;

            return jobHandle ;
            
                        
        }


        /// <summary>
        /// Execute Jobs
        /// </summary>
        [BurstCompile]
        struct Job : IJobForEachWithEntity <RemoveBlockTag>
        {
            
            public EntityCommandBuffer.Concurrent ecb ;
            
            public void Execute ( Entity blockEntity, int jobIndex, [ReadOnly] ref RemoveBlockTag removeBlockTag )
            {
                ecb.DestroyEntity ( jobIndex, blockEntity ) ;
            }           
            
        } // job
        
    }
}
*/
