using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ActionDoNothing : ActionNodeFunctions
{

    #if UNITY_EDITOR
    public override void DrawGUI()
    {
        base.DrawGUI();
    }
#endif

    public override void DoAction(GameObject gameObject)
    {
    }
}
