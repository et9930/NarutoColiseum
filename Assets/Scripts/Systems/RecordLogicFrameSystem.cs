using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class RecordLogicFrameSystem : JobComponentSystem
    {
        private const float LogicFrameRate = 0.05f;
        private float m_LogicFrameRateTime;

        protected override void OnCreate()
        {
            var barrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            var commandBuffer = barrier.CreateCommandBuffer();
            var rendererFrameEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<LogicFrameIndex>(rendererFrameEntity);
        }

        [BurstCompile]
        private struct RecordLogicFrameSystemJob : IJobForEach<LogicFrameIndex>
        {
            [ReadOnly] public bool RecordLogicFrame;

            public void Execute(ref LogicFrameIndex logicFrameIndex)
            {
                if (RecordLogicFrame)
                {
                    logicFrameIndex.index++;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var recordLogicFrame = false;
            m_LogicFrameRateTime += Time.deltaTime;
            if (m_LogicFrameRateTime >= LogicFrameRate)
            {
                recordLogicFrame = true;
                m_LogicFrameRateTime -= LogicFrameRate;
            }

            var job = new RecordLogicFrameSystemJob
            {
                RecordLogicFrame = recordLogicFrame
            };

            return job.Schedule(this, inputDependencies);
        }
    }
}