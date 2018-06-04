using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class EndNode : Node
{

    public bool Restart = true;

    public static EndNode Create(Rect NodeREct)
    {
        EndNode node = ScriptableObject.CreateInstance<EndNode>();

        node.name = "End";
        node.rect = NodeREct;

        NodeLinker.Create(node, "Link From", (int)Node.LinkType.From);

        node.Init();

        return node;
    }

#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(new GUIContent("End", "End"));

        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.From])
                Linkers[(int)Node.LinkType.From].SetRect(new Rect(
                    rect.x + rect.width / 2,
                    rect.y - 16,
                    16, 
                    16
                    ));
        }

        GUILayout.EndHorizontal();

        Restart = EditorGUILayout.Toggle("Restart : ", Restart);

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
