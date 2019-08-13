using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components
{
    [Serializable]
    public struct LogicFrameIndex : ISystemStateComponentData
    {
        public int index;
    }

#if UNITY_EDITOR
    public class LogicFrameIndexComponent : ComponentDataProxy<LogicFrameIndex>
    {
    }
#endif
}