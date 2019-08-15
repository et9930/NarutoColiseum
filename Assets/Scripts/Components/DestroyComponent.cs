using System;
using Unity.Entities;

namespace Components
{
    [Serializable]
    public struct Destroy : IComponentData
    {
    }

#if UNITY_EDITOR
    public class DestroyComponent : ComponentDataProxy<Destroy>
    {
    }
#endif
}