using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ActionDisplayText : ActionNodeFunctions 
{

    public string TextDisplayed;

    #if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Text : ");
        TextDisplayed = EditorGUILayout.TextField("", TextDisplayed);

        GUILayout.EndHorizontal();
    }
#endif

    public override void DoAction(GameObject gameObject)
    {
        Debug.Log(TextDisplayed);

    }

}
