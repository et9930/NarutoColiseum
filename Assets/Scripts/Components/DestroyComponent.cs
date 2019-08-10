using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public struct Destroy : IComponentData
    {
    }

#if UNITY_EDITOR
    public class DestroyComponent : ComponentDataProxy<Destroy>
    {
    }
#endif
}