using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class RootNode : Node {

    public static RootNode Create(Rect NodeRect)
    {
        RootNode node = ScriptableObject.CreateInstance<RootNode>();

        node.name = "Root";
        node.rect = NodeRect;

        node.Init();

        NodeLinker.Create(node, "Link To", (int)Node.LinkType.To);

        return node;
    }

#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(new GUIContent("Root", "Root"));

        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.To])
                Linkers[(int)Node.LinkType.To].SetRect(new Rect(
                    rect.x + rect.width / 2,
                    rect.y + rect.height,
                    16,
                    16));
        }

        GUILayout.EndHorizontal();

        if (GUI.changed)
        {
            Node_Editor.editor.RecalculateFrom(this);
        }
    }

#endif

    public override bool Calculate()
    {
        return true;
    }
}
