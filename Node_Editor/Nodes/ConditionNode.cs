using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class ConditionNode : Node
{
    public ConditionNodeFunctions ConditionFunction;


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
