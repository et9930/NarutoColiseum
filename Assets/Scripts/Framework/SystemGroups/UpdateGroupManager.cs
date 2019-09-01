using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Framework.SystemGroups
{
    public class UpdateGroupManager : MonoBehaviour
    {
        private IEnumerable<ComponentSystemBase> m_FixedUpdateGroupSystems;

        private void Start()
        {
            var fixedUpdateGroup = World.Active.GetOrCreateSystem<FixedUpdateGroup>();
            fixedUpdateGroup.Enabled = false;
            m_FixedUpdateGroupSystems = fixedUpdateGroup.Systems;
        }
        
        private void FixedUpdate()
        {
            foreach (var fixedUpdateGroupSystem in m_FixedUpdateGroupSystems)
            {
                fixedUpdateGroupSystem.Update();
            }
        }
    }
}
