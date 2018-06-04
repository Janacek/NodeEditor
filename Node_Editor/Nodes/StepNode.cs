using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class StepNode : Node
{

#if UNITY_EDITOR
    public override void DrawNode()
    {
    }
#endif

    public override bool Calculate()
    {
        return true;
    }

}
