using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class EnemyInFrontFireArcAtLessThanDist : ConditionNodeFunctions 
{

    public int Distance;

    #if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Distance :");
        Distance = EditorGUILayout.IntField("", Distance);

        GUILayout.EndHorizontal();
    }
#endif

    public override bool DoCondition(GameObject gameObject)
    {
        Debug.Log("[AI] Is enemy in front fire arc and less than " + Distance + " CM");

        return false;
    }
}
