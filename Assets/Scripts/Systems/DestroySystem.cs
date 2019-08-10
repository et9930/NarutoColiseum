using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Experimental.PlayerLoop;

namespace Systems
{
    public class DestroySystem : JobComponentSystem
    {
        EntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        [BurstCompile]
        struct DestroyEntity : IJobForEachWithEntity<Destroy>
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
            var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
            var job = new DestroyEntity
            {
                CommandBuffer = commandBuffer,
            }.Schedule(this, inputDeps);

            m_Barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}
