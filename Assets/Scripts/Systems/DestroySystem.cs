using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Systems
{
    public class DestroySystem : JobComponentSystem
    {
        private EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        private struct DestroyEntity : IJobForEachWithEntity<Destroy>
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int jobIndex, [ReadOnly] ref Destroy destroy)
            {
                
                CommandBuffer.DestroyEntity(jobIndex, entity);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new DestroyEntity
            {
                CommandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);

            m_Barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
