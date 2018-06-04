using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class ActionNodeFunctions : ScriptableObject
{
#if UNITY_EDITOR
    public virtual void DrawGUI()
    {
        
    }
#endif

    public virtual void DoAction(GameObject gameObject)
    {
    }
}

