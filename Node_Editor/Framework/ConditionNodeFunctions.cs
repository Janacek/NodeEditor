using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class ConditionNodeFunctions : ScriptableObject
{
#if UNITY_EDITOR
    public virtual void DrawGUI()
    {

    }
#endif

    public virtual bool DoCondition(GameObject gameObject)
    {
        return true;
    }
}
