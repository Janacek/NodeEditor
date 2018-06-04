using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ConditionCondition1 : ConditionNodeFunctions
{

    #if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();
    }
#endif

    public override bool DoCondition(GameObject gameObject)
    {
        if (gameObject.transform.position.x > 0)
            return true;
        return false;
    }

}
