using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class EnemyInRearFireArc : ConditionNodeFunctions
{
#if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();
    }
#endif

    public override bool DoCondition(GameObject gameObject)
    {
        Debug.Log("[AI] Is enemy in rear fire arc");

        return false;
    }
}
