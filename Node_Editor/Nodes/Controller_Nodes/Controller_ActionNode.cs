using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Controller_ActionNode : ActionNode
{
    public enum Controller_Actions
    {
        Move,
    }

    public Controller_Actions Action;
    private Controller_Actions oldAction;

    public List<string> Controller_ActionsDico = new List<string>();

    public static Controller_ActionNode Create(Rect NodeRect)
    {
        Controller_ActionNode node = ScriptableObject.CreateInstance<Controller_ActionNode>();

        node.name = "Action";
        node.rect = NodeRect;

        node.Controller_ActionsDico.Add("Controller_Move");

        var type = System.Type.GetType(node.Controller_ActionsDico[(int)node.Action]);
        node.oldAction = node.Action;
        node.ActionFunction = (ActionNodeFunctions)System.Activator.CreateInstance(type);

        NodeLinker.Create(node, "Link To", (int)Node.LinkType.To);
        NodeLinker.Create(node, "Link From", (int)Node.LinkType.From);

        node.Init();

        return node;
    }

#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        GUILayoutOption op = GUILayout.MinWidth(300);

        Action = (Controller_Actions)EditorGUILayout.EnumPopup("Action : ", Action, op);

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

        ActionFunction.DrawGUI();


        if (GUI.changed)
        {
            Node_Editor.editor.RecalculateFrom(this);

            if (oldAction != Action)
            {
                var type = System.Type.GetType(Controller_ActionsDico[(int)Action]);
                ActionFunction = (ActionNodeFunctions)System.Activator.CreateInstance(type);
                oldAction = Action;
            }
        }
    }
#endif

    public override bool Calculate()
    {
        return true;
    }


}
