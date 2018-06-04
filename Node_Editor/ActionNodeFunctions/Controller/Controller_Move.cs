using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class Controller_Move : ActionNodeFunctions
{

    public Vector3 Translation;

#if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Translation : ");
        Translation = EditorGUILayout.Vector3Field("", Translation);

        GUILayout.EndHorizontal();
    }
#endif

    public override void DoAction(GameObject gameObject)
    {
        gameObject.transform.Translate(Translation * Time.deltaTime);
    }

}
