using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class DebugOutputToggle : EditorWindow
{
    private const string debugOutputMenu = "Tool/Debug Output";
    private static bool debugOutputOn = true;
    private const string _DEBUG_OUTPUT = "DEBUG_OUTPUT";
    private static ScriptingDefineSymbols _scriptingDefineSymbols = new ScriptingDefineSymbols();


    [MenuItem(debugOutputMenu, false)]
    public static void DebugOutputFalse()
    {
        debugOutputOn = !debugOutputOn;
        if (debugOutputOn)
            SetDebugOutDefine();
        else
            ClearDebugOutDefine();
        Menu.SetChecked(debugOutputMenu, debugOutputOn);
    }

    [MenuItem(debugOutputMenu, true)]
    public static bool DebugOutput()
    {
//        Menu.SetChecked(debugOutputMenu, debugOutputOn);
//        if (!debugOutputOn)
//            SetDebugOutDefine();
//        else
//            ClearDebugOutDefine();
//        //        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "DEBUG_OUTPUT");
        return true;
    }

    private static void ClearDebugOutDefine()
    {
        Debug.Log("Clear " + _DEBUG_OUTPUT);
        _scriptingDefineSymbols.RemoveDefineSymbol(_DEBUG_OUTPUT);
    }

    private static void SetDebugOutDefine()
    {
        Debug.Log("Set " + _DEBUG_OUTPUT);
        _scriptingDefineSymbols.AddDefineSymbol(_DEBUG_OUTPUT);
    }

    private void OnEnable()
    {
        debugOutputOn = !_scriptingDefineSymbols.buildTargetToDefSymbol.Values
            .All<string>(defs => defs.Contains(_DEBUG_OUTPUT));
        Menu.SetChecked(debugOutputMenu, debugOutputOn);
    }
}
    