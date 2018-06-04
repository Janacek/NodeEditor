using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Controller_ConditionNode : ConditionNode
{

    public enum Controller_Conditions
    {
        ButtonPressed,
    }

    public Controller_Conditions Condition;
    private Controller_Conditions oldCondition;

    public List<string> Controller_ConditionsDico = new List<string>();

    public static Controller_ConditionNode Create(Rect NodeRect)
    {
        Controller_ConditionNode node = ScriptableObject.CreateInstance<Controller_ConditionNode>();

        node.name = "Condition";
        node.rect = NodeRect;


        node.Controller_ConditionsDico.Add("Controller_ButtonPressed");

        var type = System.Type.GetType(node.Controller_ConditionsDico[(int)node.Condition]);
        node.ConditionFunction = (ConditionNodeFunctions)System.Activator.CreateInstance(type);
        node.oldCondition = node.Condition;


        NodeLinker.Create(node, "Link From", (int)Node.LinkType.From);
        NodeLinker.Create(node, "Link ToKO", (int)Node.LinkType.ToKO);
        NodeLinker.Create(node, "Link ToOK", (int)Node.LinkType.ToOK);

        node.Init();


        return node;
    }


#if UNITY_EDITOR
    public override void DrawNode()
    {
        GUILayout.BeginHorizontal();

        GUILayoutOption op = GUILayout.MinWidth(300);

        Condition = (Controller_Conditions)EditorGUILayout.EnumPopup("Condition : ", Condition, op);

        // From Linker Node
        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.From])
                Linkers[(int)Node.LinkType.From].SetRect(new Rect(
                    rect.x + rect.width / 2,
            rect.y - 16,
            16, 16
                    ));
        }

        // ToKO Linker Node

        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.ToKO])
                Linkers[(int)Node.LinkType.ToKO].SetRect(new Rect(
                    rect.x + rect.width / 4,
            rect.y + rect.height,
            16, 16
                    ));
        }

        // ToOK Linker Node

        if (Event.current.type == EventType.Repaint)
        {
            if (Linkers[(int)Node.LinkType.ToOK])
                Linkers[(int)Node.LinkType.ToOK].SetRect(new Rect(
                    rect.x + rect.width / 4 + rect.width / 2,
            rect.y + rect.height,
            16, 16
                    ));
        }


        GUILayout.EndHorizontal();

        ConditionFunction.DrawGUI();

        if (GUI.changed)
        {
            Node_Editor.editor.RecalculateFrom(this);

            if (oldCondition != Condition)
            {
                var type = System.Type.GetType(Controller_ConditionsDico[(int)Condition]);
                ConditionFunction = (ConditionNodeFunctions)System.Activator.CreateInstance(type);
                oldCondition = Condition;
            }
        }
    }

#endif

    public override bool Calculate()
    {
        return true;
    }
}
