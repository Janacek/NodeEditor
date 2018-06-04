using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ActionNode : Node 
{
    public ActionNodeFunctions ActionFunction;


#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
    }
#endif

    public override bool Calculate()
    {
        return true;
    }

}
