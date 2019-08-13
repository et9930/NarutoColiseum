using System;
using Unity.Entities;

namespace Components
{
    [Serializable]
    public struct RendererFrameIndex : ISystemStateComponentData
    {
        public int index;
    }

#if UNITY_EDITOR
    public class RendererFrameIndexComponent : ComponentDataProxy<RendererFrameIndex>
    {
    }
#endif
}
