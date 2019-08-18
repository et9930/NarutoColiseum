using Systems.SystemGroups;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class RecordLogicFrameSystem : JobComponentSystem
    {
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
            public void Execute(ref LogicFrameIndex logicFrameIndex)
            {
                logicFrameIndex.index++;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new RecordLogicFrameSystemJob();
            return job.Schedule(this, inputDependencies);
        }
    }
}