using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class EndStepNode : StepNode
{

    public static EndStepNode Create(Rect NodeRect)
    {
        EndStepNode node = ScriptableObject.CreateInstance<EndStepNode>();

        node.name = "End Step";
        node.rect = NodeRect;

        node.Init();

        NodeLinker.Create(node, "Link To", (int)Node.LinkType.To);
        NodeLinker.Create(node, "Link From", (int)Node.LinkType.From);

        return node;
    }

#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.To])
                Linkers[(int)Node.LinkType.To].SetRect(new Rect(
                    rect.x + rect.width / 2,
                         rect.y + rect.height,
                         16,
                         16));

            if (Linkers[(int)Node.LinkType.From])
                Linkers[(int)Node.LinkType.From].SetRect(new Rect(
                    rect.x + rect.width / 2,
                         rect.y - 16,
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
