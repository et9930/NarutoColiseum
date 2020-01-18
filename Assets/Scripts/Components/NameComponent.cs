using System;
using Helper;
using Unity.Entities;

namespace Components
{
    [Serializable]
    public struct Name : IComponentData
    {
        public int strIndex;
    }

#if UNITY_EDITOR
    public class NameComponent : ComponentStringDataProxy<Name>
    {
    }
#endif
}