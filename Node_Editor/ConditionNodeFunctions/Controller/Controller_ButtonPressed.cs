using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class Controller_ButtonPressed : ConditionNodeFunctions
{

    public KeyCode Key;

#if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Key : ");
        Key = (KeyCode)EditorGUILayout.EnumPopup(Key);

        GUILayout.EndHorizontal();
    }
#endif

    public override bool DoCondition(GameObject gameObject)
    {
        return Input.GetKey(Key);
    }
}
