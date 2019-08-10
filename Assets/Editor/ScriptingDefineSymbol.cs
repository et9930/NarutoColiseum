using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Editor
{
    public class ScriptingDefineSymbols
    {
        public Dictionary<BuildTargetGroup, string> buildTargetToDefSymbol { get; }

        public ScriptingDefineSymbols()
        {
            buildTargetToDefSymbol = Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>()
                .Where(buildTargetGroup => buildTargetGroup > 0)
                .Where(buildTargetGroup => !isBuildTargetObsolete(buildTargetGroup))
                .Distinct().ToDictionary(
                    buildTargetGroup => buildTargetGroup,
                    PlayerSettings.GetScriptingDefineSymbolsForGroup);
        }

        public void AddDefineSymbol(string defineSymbol)
        {
            using (Dictionary<BuildTargetGroup, string>.Enumerator enumerator = this.buildTargetToDefSymbol.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<BuildTargetGroup, string> current = enumerator.Current;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(current.Key, current.Value.Replace(defineSymbol, string.Empty) + "," + defineSymbol);
                }
            }
        }

        public void RemoveDefineSymbol(string defineSymbol)
        {
            using (Dictionary<BuildTargetGroup, string>.Enumerator enumerator = this.buildTargetToDefSymbol.GetEnumerator()
            )
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<BuildTargetGroup, string> current = enumerator.Current;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(current.Key,
                        current.Value.Replace(defineSymbol, string.Empty));
                }
            }
        }

        private bool isBuildTargetObsolete(BuildTargetGroup buildTargetGroup)
        {
            return Attribute.IsDefined(
                buildTargetGroup.GetType().GetField(buildTargetGroup.ToString()),
                typeof(ObsoleteAttribute));
        }
    }
}