using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AI_ActionNode : ActionNode {

    public enum AI_Actions
    {
        DisplayText,
        DoNothing,
        Move,
    }

    public AI_Actions Action;
    private AI_Actions oldAction;

    //public static Dictionary<Actions, string> ActionsDico = new Dictionary<Actions, string>();
    public List<string> AI_ActionsDico = new List<string>();

    public static AI_ActionNode Create(Rect NodeRect)
    {
        AI_ActionNode node = ScriptableObject.CreateInstance<AI_ActionNode>();

        node.name = "Action";
        node.rect = NodeRect;

        node.AI_ActionsDico.Add("ActionDisplayText");
        node.AI_ActionsDico.Add("ActionDoNothing");
        node.AI_ActionsDico.Add("ActionMove");

        var type = System.Type.GetType(node.AI_ActionsDico[(int)node.Action]);
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

        Action = (AI_Actions)EditorGUILayout.EnumPopup("Action : ", Action, op);

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
                var type = System.Type.GetType(AI_ActionsDico[(int)Action]);
                ActionFunction = (ActionNodeFunctions)System.Activator.CreateInstance(type);
                oldAction = Action;
            }
        }
    }
#endif

    public void OnGUI()
    {

        Texture2D tex;

        tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, new Color(0.25f, 0.4f, 0.25f));
        tex.Apply();

        GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill);
    }

    public override bool Calculate()
    {
        return true;
    }

}
