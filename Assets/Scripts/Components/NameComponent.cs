using System;
using Helper;
using Unity.Entities;

namespace Components
{
    [Serializable]
    public struct Name : IComponentData
    {
        public Words name;
    }

#if UNITY_EDITOR
    public class NameComponent : ComponentDataProxy<Name>
    {
    }
#endif
}