using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml.Serialization;

using Object = UnityEngine.Object;

[System.Serializable, 
XmlInclude(typeof(RootNode)),
XmlInclude(typeof(EndNode)),
XmlInclude(typeof(StartStepNode)),
XmlInclude(typeof(EndStepNode)),

XmlInclude(typeof(AI_ActionNode)),

XmlInclude(typeof(AI_ConditionNode)),
XmlInclude(typeof(EnemyInFrontFireArcAtLessThanDist)),
XmlInclude(typeof(EnemyInFrontFireArcAtMoreThanDist)),
XmlInclude(typeof(EnemyInRangeOfBioWeapons)),
XmlInclude(typeof(EnemyInRearFireArc)),

XmlInclude(typeof(Controller_ActionNode)),
XmlInclude(typeof(Controller_Move)),

XmlInclude(typeof(Controller_ConditionNode)),
XmlInclude(typeof(Controller_ButtonPressed)),
]
public abstract class Node : ScriptableObject
{
	public Rect rect = new Rect ();
    public int ID;
    public bool Displayed = true;

    public enum LinkType
    {
        From = 0,
        To,
        ToOK,
        ToKO,
    }

    //[XmlIgnore]
    public NodeLinker[] Linkers = new NodeLinker[4] {null, null, null, null};

    public virtual void InitCustom()
    {

    }

	/// <summary>
	/// Init this node. Has to be called when creating a child node of this
	/// </summary>
	protected void Init () 
	{
        //for (int i = 0; i < 4; ++i)
        //{
        //    Linkers[i] = null;
        //}
            Calculate();

            ID = Node_Editor.editor.nodeCanvas.currentId;
            Node_Editor.editor.nodeCanvas.currentId++;

#if UNITY_EDITOR
        Node_Editor.editor.nodeCanvas.nodes.Add(this);
        if (!String.IsNullOrEmpty(AssetDatabase.GetAssetPath(Node_Editor.editor.nodeCanvas)))
        {
            AssetDatabase.AddObjectToAsset(this, Node_Editor.editor.nodeCanvas);

            for (int i = 0; i < 4; ++i)
            {
                if (Linkers[i])
                {
                    Debug.Log("Bonjour");
                    AssetDatabase.AddObjectToAsset(Linkers[i], this);
                }
            }


            var n = this as ActionNode;
            if (n)
            {
                AssetDatabase.AddObjectToAsset(n.ActionFunction, this);
            }
            else
            {
                var n1 = this as ConditionNode;
                if (n1)
                {
                    AssetDatabase.AddObjectToAsset(n1.ConditionFunction, this);
                }
                else
                {

                }
            }

            AssetDatabase.ImportAsset(Node_Editor.editor.openedCanvasPath);
            AssetDatabase.Refresh();
        }
#endif
	}

	/// <summary>
	/// Function implemented by the children to draw the node
	/// </summary>
    /// 
#if UNITY_EDITOR
	public abstract void DrawNode ();
#endif

	/// <summary>
	/// Function implemented by the children to calculate their outputs
	/// Should return Success/Fail
	/// </summary>
	public abstract bool Calculate ();

	/// <summary>
	/// Draws the node curves as well as the knobs	
	/// </summary>
    /// 

#if UNITY_EDITOR
	public void DrawConnectors () 
	{
        for (int i = 0; i < 4; ++i)
        {
            if (Linkers[i])
             GUI.DrawTexture(Linkers[i].GetKnob(), Node_Editor.ConnectorKnob);
        }

        for (int i = 1; i < 4; ++i)
        {
            if (Linkers[i])
            {
                if (Linkers[i].connection)
                {
                    Node_Editor.DrawNodeCurve(Linkers[i].GetKnob().center, Linkers[i].connection.body.Linkers[0].GetKnob().center);
                }
            }
        }
	}
#endif

	/// <summary>
	/// Callback when the node is deleted. Extendable by the child node, but always call base.OnDelete when overriding !!
	/// </summary>
	public virtual void OnDelete () 
	{
        //for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
        //{
        //    NodeOutput output = Outputs [outCnt];
        //    for (int conCnt = 0; conCnt < output.connections.Count; conCnt++) 
        //        output.connections [outCnt].connection = null;
        //}
        //for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
        //{
        //    if (Inputs [inCnt].connection != null)
        //        Inputs [inCnt].connection.connections.Remove (Inputs [inCnt]);
        //}

		DestroyImmediate (this, true);
#if UNITY_EDITOR

        if (!String.IsNullOrEmpty (Node_Editor.editor.openedCanvasPath)) 
		{
			AssetDatabase.ImportAsset (Node_Editor.editor.openedCanvasPath);
			AssetDatabase.Refresh ();
        }
#endif

    }

	#region Member Functions

	/// <summary>
	/// Checks if there are no unassigned and no null-value inputs.
	/// </summary>
	public bool allInputsReady () 
	{
        //for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
        //{
        //    if (Inputs [inCnt].connection == null || Inputs [inCnt].connection.value == null)
        //        return false;
        //}
		return true;
	}
	/// <summary>
	/// Checks if there are any unassigned inputs.
	/// </summary>
	public bool hasNullInputs () 
	{
        //for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
        //{
        //    if (Inputs [inCnt].connection == null)
        //        return true;
        //}
		return false;
	}
	/// <summary>
	/// Checks if there are any null-value inputs.
	/// </summary>
	public bool hasNullInputValues () 
	{
        //for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
        //{
        //    if (Inputs [inCnt].connection != null && Inputs [inCnt].connection.value == null)
        //        return true;
        //}
		return false;
	}

	/// <summary>
	/// Returns the input knob that is at the position on this node or null
	/// </summary>
	public NodeLinker GetLinkFromAtPos (Vector2 pos) 
	{
        //for (int inCnt = 0; inCnt < 4; inCnt++)
        { // Search for an input at the position
            if (Linkers[(int)Node.LinkType.From])
                if (Linkers[(int)Node.LinkType.From].GetKnob().Contains(new Vector3(pos.x, pos.y)))
                    return Linkers[(int)Node.LinkType.From];
        }
		return null;
	}
	/// <summary>
	/// Returns the output knob that is at the position on this node or null
	/// </summary>
	public NodeLinker GetOutputAtPos (Vector2 pos) 
	{
        for (int outCnt = 1; outCnt < 4; outCnt++)
        { // Search for an output at the position
            if (Linkers[outCnt].GetKnob().Contains(new Vector3(pos.x, pos.y)))
                return Linkers[outCnt];
        }
		return null;
	}

	/// <summary>
	/// Recursively checks whether this node is a child of the other node
	/// </summary>
	public bool isChildOf (Node otherNode)
	{
		if (otherNode == null)
			return false;
        //for (int cnt = 0; cnt < Inputs.Count; cnt++) 
        //{
        //    if (Inputs [cnt].connection != null) 
        //    {
        //        if (Inputs [cnt].connection.body == otherNode)
        //            return true;
        //        else if (Inputs [cnt].connection.body.isChildOf (otherNode)) // Recursively searching
        //            return true;
        //    }
        //}
		return false;
	}

	#endregion

	#region static Functions

	/// <summary>
	/// Check if an output and an input can be connected (same type, ...)
	/// </summary>
	public static bool CanApplyConnection (NodeLinker from, NodeLinker to)
	{
		if (from == null || to == null)
			return false;

		if (from.body == to.body || from.connection == to)
			return false;

		if (from.type != to.type)
			return false;

		if (to.body.isChildOf (from.body)) 
		{
#if UNITY_EDITOR
			Node_Editor.editor.ShowNotification (new GUIContent ("Recursion detected!"));
#endif
			return false;
		}
		return true;
	}

	/// <summary>
	/// Applies a connection between output and input. 'CanApplyConnection' has to be checked before
	/// </summary>
	public static void ApplyConnection (NodeLinker output, NodeLinker input)
	{
		if (input != null && output != null) 
		{
			if (input.connection != null) 
			{
                //input.connection.connections.Remove (input);
			}
			input.connection = output;
            //output.connections.Add (input);

#if UNITY_EDITOR
			Node_Editor.editor.RecalculateFrom (input.body);
#endif
		}
	}

	#endregion
}
