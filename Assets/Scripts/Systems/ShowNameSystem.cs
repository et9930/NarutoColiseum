﻿using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Systems
{
    [UpdateBefore(typeof(DestroySystem))]
    public class ShowNameSystem : JobComponentSystem
    {
        EntityCommandBufferSystem m_Barrier;

        
        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        [BurstCompile]
        struct ShowNameSystemJob : IJobForEachWithEntity<Name>
        {
            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;


            public void Execute(Entity entity, int index, [ReadOnly] ref Name name)
            {
                Debug.Log(name.name.ToString());
                //CommandBuffer.AddComponent<Destroy>(index, entity);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ShowNameSystemJob
            {
                CommandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDependencies);
            m_Barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}