using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Xml.Serialization;

[System.Serializable]
public class NodeLinker : ScriptableObject 
{
    [XmlIgnore]
    public Node body;
    public Rect rect = new Rect();
    [XmlIgnore]
    public NodeLinker connection;
    public int connectionID = -1;
    public string type;

    public static NodeLinker Create(Node NodeBody, string InputName, int link)
    {
        NodeLinker linker = NodeLinker.CreateInstance(typeof(NodeLinker)) as NodeLinker;
        linker.body = NodeBody;
        linker.name = InputName;
        NodeBody.Linkers[link] = linker;

//#if UNITY_EDITOR
//        if (!String.IsNullOrEmpty(AssetDatabase.GetAssetPath(Node_Editor.editor.nodeCanvas)))
//        {
//            AssetDatabase.AddObjectToAsset(linker, NodeBody);
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//        }
//#endif

        return linker;
    }

    public void DisplayLayout()
    {
        DisplayLayout(new GUIContent(name));
    }

    public void DisplayLayout(GUIContent content)
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle(UnityEditor.EditorStyles.label);
        GUILayout.Label(content, style);
        if (Event.current.type == EventType.Repaint)
        {
            SetRect(GUILayoutUtility.GetLastRect());
        }
#endif
    }

    public void SetRect(Rect labelRect)
    {
        rect = labelRect;
    }

    public Rect GetKnob()
    {
#if UNITY_EDITOR
        int knobSize = Node_Editor.editor.knobSize;
#else
        int knobSize = 16;
#endif
        return new Rect(
            rect.x - knobSize / 2,
            rect.y + (rect.height - knobSize) / 2,
            knobSize, knobSize);
    }
}
