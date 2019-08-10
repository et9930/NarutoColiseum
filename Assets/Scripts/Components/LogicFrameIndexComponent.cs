using System;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    [Serializable]
    public struct LogicFrameIndex : IComponentData
    {
        public int Index;
    }

#if UNITY_EDITOR
    public class LogicFrameIndexComponent : ComponentDataProxy<LogicFrameIndex>
    {
    }
#endif
}