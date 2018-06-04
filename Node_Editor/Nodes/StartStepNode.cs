using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Xml.Serialization;

public class StartStepNode : StepNode
{

    public bool Reduce;
    public string ContentName = "";

    [XmlIgnore]
    public NodeLinker LinkSave = null;
    public int LinkSaveId = -1;

    public static StartStepNode Create(Rect NodeRect)
    {
        StartStepNode node = ScriptableObject.CreateInstance<StartStepNode>();

        node.name = "Start Step";
        node.rect = NodeRect;

        NodeLinker.Create(node, "Link To", (int)Node.LinkType.To);
        NodeLinker.Create(node, "Link From", (int)Node.LinkType.From);


        node.Init();

        if (node.LinkSaveId >= 0)
        {
            foreach (Node n in Node_Editor.editor.nodeCanvas.nodes)
            {
                if (n.ID == node.LinkSaveId)
                {
                    node.LinkSave = n.Linkers[(int)Node.LinkType.From].connection;
                }
            }
        }
        return node;
    }

    Node findEndStepNode(Node node)
    {
        if (node.name == "End")
            return null;

        if (node.name == "End Step")
            return node;

        if (node.Linkers[(int)Node.LinkType.To] && node.Linkers[(int)Node.LinkType.To].connection)
        {
            return findEndStepNode(node.Linkers[(int)Node.LinkType.To].connection.body);
        }

        if (node.Linkers[(int)Node.LinkType.ToOK] && node.Linkers[(int)Node.LinkType.ToOK].connection)
        {
            return findEndStepNode(node.Linkers[(int)Node.LinkType.ToOK].connection.body);
        }

        return null;
    }

    void displayNodesRec(Node node, bool display)
    {
        if (node.name == "End Step")
        {
            node.Displayed = display;
            return;
        }

        if (node.Linkers[(int)Node.LinkType.To] && node.Linkers[(int)Node.LinkType.To].connection)
        {
            displayNodesRec(node.Linkers[(int)Node.LinkType.To].connection.body, display);
        }

        if (node.Linkers[(int)Node.LinkType.ToOK] && node.Linkers[(int)Node.LinkType.ToOK].connection)
        {
            displayNodesRec(node.Linkers[(int)Node.LinkType.ToOK].connection.body, display);
        }

        if (node.Linkers[(int)Node.LinkType.ToOK] && node.Linkers[(int)Node.LinkType.ToKO].connection)
        {
            displayNodesRec(node.Linkers[(int)Node.LinkType.ToKO].connection.body, display);
        }

        if (node.name == "Start Step")
            return;
        node.Displayed = display;

    }

    void displayNodes(bool display)
    {
        displayNodesRec(this.Linkers[(int)Node.LinkType.To].connection.body, display);
    }

    bool connectedToEndStepNode()
    {
        Node n = findEndStepNode(this);

        if (n && n.Linkers[(int)Node.LinkType.To] && n.Linkers[(int)Node.LinkType.To].connection)
        {
            //endStepNode = n;
            return true;
        }
        else
        {
            //endStepNode = null;
            return false;
        }
    }

    public override void InitCustom()
    {
        base.InitCustom();

        if (LinkSave == null && LinkSaveId >= 0)
        {
            foreach (Node n in Node_Editor.editor.nodeCanvas.nodes)
            {
                if (n.ID == LinkSaveId)
                {
                    LinkSave = n.Linkers[(int)Node.LinkType.From];
                }
            }
        }
    }

#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        GUILayoutOption op = GUILayout.MinWidth(300);

        bool tmp = Reduce;

        if (connectedToEndStepNode() || Reduce)
            Reduce = (bool)EditorGUILayout.Toggle("Reduce : ", Reduce);


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

        ContentName = EditorGUILayout.TextField("", ContentName, op);


        if (GUI.changed)
        {
            Node_Editor.editor.RecalculateFrom(this);

            if (Reduce && tmp != Reduce)
            {
                displayNodes(false);

                LinkSave = Linkers[(int)Node.LinkType.To].connection;
                LinkSaveId = Linkers[(int)Node.LinkType.To].connection.body.ID;

                Linkers[(int)Node.LinkType.To].connectionID = findEndStepNode(this).Linkers[(int)Node.LinkType.To].connection.body.ID;
                Linkers[(int)Node.LinkType.To].connection = findEndStepNode(this).Linkers[(int)Node.LinkType.To].connection;
            }

            if (!Reduce && tmp != Reduce)
            {
                //if (LinkSave == null && LinkSaveId >= 0)
                //{
                //    foreach (Node n in Node_Editor.editor.nodeCanvas.nodes)
                //    {
                //        if (n.ID == LinkSaveId)
                //        {
                //            LinkSave = n.Linkers[(int)Node.LinkType.From].connection;
                //        }
                //    }
                //}
                Linkers[(int)Node.LinkType.To].connection = LinkSave;

                LinkSave = null;
                LinkSaveId = -1;

                displayNodes(true);
            }
        }
    }
#endif

    public override bool Calculate()
    {
        return true;
    }

}
