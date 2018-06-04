using UnityEngine;
#if UNITY_ENGINE
using UnityEditor;
#endif
using System.Collections;

public class ActionMove : ActionNodeFunctions
{

#if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();

        GUILayout.BeginHorizontal();

        // Display things

        GUILayout.EndHorizontal();
    }
#endif

    public override void DoAction(GameObject gameObject)
    {
    }
}
