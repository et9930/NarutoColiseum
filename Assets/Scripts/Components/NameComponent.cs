using System;
using Unity.Entities;

[Serializable]
public struct Name : IComponentData
{
    public int NameStringIndex;
}

#if UNITY_EDITOR
public class NameComponent : ComponentStringDataProxy<Name> { }
#endif
