using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class OrdnancesNeedReloading : ConditionNodeFunctions
{

#if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();
    }
#endif

    public override bool DoCondition(GameObject gameObject)
    {
        Debug.Log("[AI] Ordnances need reload ?");

        return false;
    }
}
