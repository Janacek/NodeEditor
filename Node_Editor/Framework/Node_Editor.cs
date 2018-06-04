#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Xml.Serialization;

using Object = UnityEngine.Object;

public class Node_Editor : EditorWindow
{
    public Node_Canvas_Object nodeCanvas;
    public static Node_Editor editor;

    public const string editorPath = "Assets/Node_Editor/";
    public string openedCanvas = "New Canvas";
    public string openedCanvasPath;

    public int sideWindowWidth = 400;
    public int knobSize = 16;

    public Node activeNode; // Handled by Unity. For new Windowing System
    public bool dragNode = false; // Handled by Unity. For new Windowing System
    public NodeLinker connectOutput;
    public bool navigate = false;
    public bool scrollWindow = false;
    public Vector2 mousePos;

    public bool creatingLink = false;
    public NodeLinker linkFrom = null;

    public static Texture2D ConnectorKnob;
    public static Texture2D Background;
    public static GUIStyle nodeBase;
    public static GUIStyle nodeBox;
    public static GUIStyle nodeLabelBold;
    public static GUIStyle nodeButton;

    private bool initiated;

    public void checkInit()
    {
        if (!initiated || nodeCanvas == null)
        {
            ConnectorKnob = EditorGUIUtility.Load("icons/animationkeyframe.png") as Texture2D;
            Background = AssetDatabase.LoadAssetAtPath(editorPath + "background.png", typeof(Texture2D)) as Texture2D;

            nodeBase = new GUIStyle(GUI.skin.box);
            nodeBase.normal.background = ColorToTex(new Color(0.5f, 0.5f, 0.5f));
            nodeBase.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            nodeBox = new GUIStyle(nodeBase);
            nodeBox.margin = new RectOffset(8, 8, 5, 8);
            nodeBox.padding = new RectOffset(8, 8, 8, 8);

            nodeLabelBold = new GUIStyle(nodeBase);
            nodeLabelBold.fontStyle = FontStyle.Bold;
            nodeLabelBold.wordWrap = false;

            nodeButton = new GUIStyle(GUI.skin.button);
            nodeButton.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

            NewNodeCanvas();

            // Example of creating Nodes and Connections through code
            //			CalcNode calcNode1 = CalcNode.Create (new Rect (200, 200, 200, 150));
            //			CalcNode calcNode2 = CalcNode.Create (new Rect (600, 200, 200, 150));
            //			Node.ApplyConnection (calcNode1.Outputs [0], calcNode2.Inputs [0]);

            initiated = true;
        }
    }

    [MenuItem("Window/Node Editor")]
    static void CreateEditor()
    {
        Node_Editor.editor = EditorWindow.GetWindow<Node_Editor>();
        Node_Editor.editor.minSize = new Vector2(800, 600);
    }

    public void OnAwake()
    {
        Node_Editor.editor = EditorWindow.GetWindow<Node_Editor>();
        Node_Editor.editor.minSize = new Vector2(800, 600);
        Debug.Log("Pàp");
    }

    #region GUI

    public void OnGUI()
    {
        checkInit();

        InputEvents();

        // draw the nodes
        BeginWindows();

        for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++)
        {
            Color saveCol = GUI.color;

            if (nodeCanvas.nodes[nodeCnt].name == "Action")
                GUI.color = Color.red;
            if (nodeCanvas.nodes[nodeCnt].name == "Condition")
                GUI.color = Color.blue;

            if (nodeCanvas.nodes[nodeCnt].name == "Start Step" ||
                nodeCanvas.nodes[nodeCnt].name == "End Step")
                GUI.color = Color.green;



            //DrawNode (nodeCanvas.nodes [nodeCnt]);
            if (nodeCanvas.nodes[nodeCnt] != null && nodeCanvas.nodes[nodeCnt].Displayed)
            {
                //if (nodeCanvas.nodes[nodeCnt].name != "Start Step")
                nodeCanvas.nodes[nodeCnt].rect = GUILayout.Window(nodeCnt, nodeCanvas.nodes[nodeCnt].rect, DrawNode, nodeCanvas.nodes[nodeCnt].name);

            }
            GUI.color = saveCol;
        }
        EndWindows();

        // draw their connectors
        for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++)
        {
            if (nodeCanvas.nodes[nodeCnt].Displayed)
                nodeCanvas.nodes[nodeCnt].DrawConnectors();
        }

        sideWindowWidth = Math.Min(600, Math.Max(200, (int)(position.width / 5)));
        GUILayout.BeginArea(sideWindowRect, nodeBox);
        DrawSideWindow();
        GUILayout.EndArea();
    }

    public void DrawSideWindow()
    {
        GUILayout.Label(new GUIContent("Node Editor (" + openedCanvas + ")", "The currently opened canvas in the Node Editor"), nodeLabelBold);
        GUILayout.Label(new GUIContent("Do note that changes will be saved automatically!", "All changes are automatically saved to the currently opened canvas (see above) if it's present in the Project view."), nodeBase);
        if (GUILayout.Button(new GUIContent("Save Canvas", "Saves the canvas as a new Canvas Asset File in the Assets Folder"), nodeButton))
        {
            SaveNodeCanvas(EditorUtility.SaveFilePanelInProject("Save Node Canvas", "Node Canvas", "xml", "Saving to a file is only needed once.", editorPath + "Saves/"));
        }
        if (GUILayout.Button(new GUIContent("Load Canvas", "Loads the canvas from a Canvas Asset File in the Assets Folder"), nodeButton))
        {
            string path = EditorUtility.OpenFilePanel("Load Node Canvas", editorPath, "xml");
            //if (!path.Contains (Application.dataPath)) 
            //{
            //    if (path != String.Empty)
            //        ShowNotification (new GUIContent ("You should select an asset inside your project folder!"));
            //    return;
            //}
            path = path.Replace(Application.dataPath, "Assets");
            LoadNodeCanvas(path);
        }
        if (GUILayout.Button(new GUIContent("New Canvas", "Creates a new Canvas (remember to save the previous one to a referenced Canvas Asset File at least once before! Else it'll be lost!)"), nodeButton))
        {
            NewNodeCanvas();
        }
        if (GUILayout.Button(new GUIContent("Recalculate All", "Starts to calculate from the beginning off."), nodeButton))
        {
            RecalculateAll();
        }
        knobSize = EditorGUILayout.IntSlider(new GUIContent("Handle Size", "The size of the handles of the Node Inputs/Outputs"), knobSize, 8, 32);
    }

    #endregion

    #region Calculation

    List<Node> workList;

    /// <summary>
    /// Recalculate from every Input Node.
    /// Usually does not need to be called at all, the smart calculation system is doing the job just fine
    /// </summary>
    public void RecalculateAll()
    {
        //workList = new List<Node> ();
        //for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++) 
        //{
        //    if (nodeCanvas.nodes [nodeCnt].Inputs.Count == 0) 
        //    {
        //        workList.Add (nodeCanvas.nodes [nodeCnt]);
        //        ClearChildrenInput (nodeCanvas.nodes [nodeCnt]);
        //    }
        //}
        //Calculate ();
    }

    /// <summary>
    /// Recalculate from node. 
    /// Usually does not need to be called manually
    /// </summary>
    public void RecalculateFrom(Node node)
    {
        workList = new List<Node> { node };
        ClearChildrenInput(node);
        Calculate();
    }

    /// <summary>
    /// Iterates through the worklist and calculates everything, including children
    /// </summary>
    private void Calculate()
    {
        //// this blocks iterates through the worklist and starts calculating
        //// if a node returns false state it stops and adds the node to the worklist
        //// later on, this worklist is reworked
        //bool limitReached = false;
        //for (int roundCnt = 0; !limitReached; roundCnt++)
        //{ // Runs until every node that can be calculated are calculated
        //    limitReached = true;
        //    for (int workCnt = 0; workCnt < workList.Count; workCnt++) 
        //    {
        //        Node node = workList [workCnt];
        //        if (node.Calculate ())
        //        { // finished Calculating, continue with the children
        //            for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
        //            {
        //                NodeOutput output = node.Outputs [outCnt];
        //                for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
        //                    ContinueCalculation (output.connections [conCnt].body);
        //            }
        //            if (workList.Contains (node))
        //                workList.Remove (node);
        //            limitReached = false;
        //        }
        //        else if (!workList.Contains (node)) 
        //        { // Calculate returned false state (due to missing inputs / whatever), add it to check later
        //            workList.Add (node);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// A recursive function to clear all inputs that depend on the outputs of node. 
    /// Usually does not need to be called manually
    /// </summary>
    private void ClearChildrenInput(Node node)
    {
        //node.Calculate ();
        //for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
        //{
        //    NodeOutput output = node.Outputs [outCnt];
        //    output.value = null;
        //    for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
        //        ClearChildrenInput (output.connections [conCnt].body);
        //}
    }

    /// <summary>
    /// Continues calculation on this node to all the child nodes
    /// Usually does not need to be called manually
    /// </summary>
    private void ContinueCalculation(Node node)
    {
        //if (node.Calculate ())
        //{ // finished Calculating, continue with the children
        //    for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
        //    {
        //        NodeOutput output = node.Outputs [outCnt];
        //        for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
        //        {
        //            ContinueCalculation (output.connections [conCnt].body);
        //        }
        //    }
        //}
        //else if (!workList.Contains (node))
        //    workList.Add (node);
    }
    #endregion

    #region Events

    /// <summary>
    /// Processes input events
    /// </summary>
    private void InputEvents()
    {
        Event e = Event.current;
        mousePos = e.mousePosition;

        Node clickedNode = null;
        if (e.type == EventType.MouseDown || e.type == EventType.MouseUp)
            clickedNode = NodeAtPosition(e.mousePosition);

        if (e.type == EventType.Repaint)
        { // Draw background when repainting
            Vector2 offset = new Vector2(nodeCanvas.scrollOffset.x % Background.width - Background.width,
                                          nodeCanvas.scrollOffset.y % Background.height - Background.height);
            int tileX = Mathf.CeilToInt((position.width + (Background.width - offset.x)) / Background.width);
            int tileY = Mathf.CeilToInt((position.height + (Background.height - offset.y)) / Background.height);

            for (int x = 0; x < tileX; x++)
            {
                for (int y = 0; y < tileY; y++)
                {
                    Rect texRect = new Rect(offset.x + x * Background.width,
                                             offset.y + y * Background.height,
                                             Background.width, Background.height);
                    GUI.DrawTexture(texRect, Background);
                }
            }
        }

        if (e.type == EventType.MouseDown)
        {
            activeNode = clickedNode;
            connectOutput = null;

            if (clickedNode != null)
            { // A click on a node
                if (e.button == 1)
                { // Right click -> Node Context Click
                    GenericMenu menu = new GenericMenu();

                    if (clickedNode.name != "Root" && clickedNode.name != "End")
                        menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");
                    else
                        menu.AddDisabledItem(new GUIContent("Delete Node"));

                    if (clickedNode.name == "Action" || clickedNode.name == "Root")
                    {
                        menu.AddSeparator(" ");
                        if (clickedNode.Linkers[(int)Node.LinkType.To].connection)
                            menu.AddItem(new GUIContent("Delete Link To"), false, ContextCallback, "deleteLinkTo");
                        else
                            menu.AddDisabledItem(new GUIContent("Delete Link To"));
                    }

                    if (clickedNode.name == "Condition")
                    {
                        menu.AddSeparator(" ");
                        if (clickedNode.Linkers[(int)Node.LinkType.ToOK].connection)
                            menu.AddItem(new GUIContent("Delete Link Condition True"), false, ContextCallback, "deleteLinkToOK");
                        else
                            menu.AddDisabledItem(new GUIContent("Delete Link Condition True"));

                        if (clickedNode.Linkers[(int)Node.LinkType.ToKO].connection)
                            menu.AddItem(new GUIContent("Delete Link Condition False"), false, ContextCallback, "deleteLinkToKO");
                        else
                            menu.AddDisabledItem(new GUIContent("Delete Link Condition False"));
                    }

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (e.button == 0)
                {
                    /* // Handled by Unity. For new Windowing System
                    // Left click -> check for drag on the header and for transition edits, else let it pass for gui elements
                    if (new Rect (clickedNode.rect.x, clickedNode.rect.y, clickedNode.rect.width, 40).Contains (mousePos))
                    { // We clicked the header, so we'll drag the node
                        dragNode = true;
                        e.delta = new Vector2 (0, 0);
                    }*/

                    if (clickedNode.name == "Root" || clickedNode.name == "Action" ||
                        clickedNode.name == "Start Step" || clickedNode.name == "End Step")
                    {
                        Rect linkRect = new Rect(
                            clickedNode.rect.x,
                            clickedNode.rect.y + clickedNode.rect.height,
                            clickedNode.rect.width,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            creatingLink = true;
                            linkFrom = clickedNode.Linkers[(int)Node.LinkType.To];
                            connectOutput = linkFrom;
                            e.Use();
                        }

                        linkRect = new Rect(
                            clickedNode.rect.x,
                            clickedNode.rect.y - knobSize,
                            clickedNode.rect.width,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            connectOutput = clickedNode.Linkers[(int)Node.LinkType.From].connection;

                            clickedNode.Linkers[(int)Node.LinkType.From].connection = null;
                            clickedNode.Linkers[(int)Node.LinkType.From].connectionID = -1;
                            //connectOutput.connection = null;


                            e.Use();
                        }

                    }
                    else if (clickedNode.name == "Condition")
                    {
                        Rect linkRect = new Rect(
                            clickedNode.rect.x,
                            clickedNode.rect.y + clickedNode.rect.height,
                            clickedNode.rect.width / 2,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            creatingLink = true;
                            linkFrom = clickedNode.Linkers[(int)Node.LinkType.ToKO];
                            connectOutput = linkFrom;
                            e.Use();
                        }

                        linkRect = new Rect(
                            clickedNode.rect.x,
                            clickedNode.rect.y - knobSize,
                            clickedNode.rect.width,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            connectOutput = clickedNode.Linkers[(int)Node.LinkType.From].connection;

                            clickedNode.Linkers[(int)Node.LinkType.From].connection = null;
                            clickedNode.Linkers[(int)Node.LinkType.From].connectionID = -1;
                            connectOutput.connection = null;
                            connectOutput.connectionID = -1;


                            e.Use();
                        }








                        linkRect = new Rect(
                            clickedNode.rect.x + clickedNode.rect.width / 2,
                            clickedNode.rect.y + clickedNode.rect.height,
                            clickedNode.rect.width / 2,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            creatingLink = true;
                            linkFrom = clickedNode.Linkers[(int)Node.LinkType.ToOK];
                            connectOutput = linkFrom;
                            e.Use();
                        }

                        linkRect = new Rect(
                            clickedNode.rect.x,
                            clickedNode.rect.y - knobSize,
                            clickedNode.rect.width,
                            knobSize
                            );

                        if (linkRect.Contains(mousePos))
                        {
                            connectOutput = clickedNode.Linkers[(int)Node.LinkType.From].connection;

                            clickedNode.Linkers[(int)Node.LinkType.From].connection = null;
                            clickedNode.Linkers[(int)Node.LinkType.From].connectionID = -1;
                            connectOutput.connection = null;
                            connectOutput.connectionID = -1;


                            e.Use();
                        }
                    }
                }
            }
            else if (!sideWindowRect.Contains(mousePos))
            { // A click on the empty canvas
                if (e.button == 2 || e.button == 0)
                { // Left/Middle Click -> Start scrolling
                    scrollWindow = true;
                    e.delta = new Vector2(0, 0);
                }
                else if (e.button == 1)
                { // Right click -> Editor Context Click
                    GenericMenu menu = new GenericMenu();

                    //menu.AddItem(new GUIContent("Add Root Node"), false, ContextCallback, "rootNode");
                    menu.AddItem(new GUIContent("Action Node/AI Action Node"), false, ContextCallback, "AI_actionNode");
                    menu.AddItem(new GUIContent("Action Node/Controller Action Node"), false, ContextCallback, "Controller_actionNode");
                    menu.AddItem(new GUIContent("Condition Node/AI Condition Node"), false, ContextCallback, "AI_conditionNode");
                    menu.AddItem(new GUIContent("Condition Node/Controller Condition Node"), false, ContextCallback, "Controller_conditionNode");
                    //menu.AddItem(new GUIContent("Add End Node"), false, ContextCallback, "endNode");
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Step Node/Start Step"), false, ContextCallback, "Start_StepNode");
                    menu.AddItem(new GUIContent("Step Node/End Step"), false, ContextCallback, "End_StepNode");

                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }
        else if (e.type == EventType.MouseUp)
        {





            if (connectOutput != null)
            { // Apply a connection if theres a clicked input
                //if (clickedNode != null /*&& !clickedNode.Outputs.Contains(connectOutput)*/)
                {	// If an input was clicked, it'll will now be connected
                    //NodeLinker clickedLinker = clickedNode.GetLinkFromAtPos(mousePos);
                    Node n = NodeAtPosition(mousePos);
                    if (n)
                    {
                        if (n.Linkers[0])
                        {
                            NodeLinker clickedLinker = NodeAtPosition(mousePos).Linkers[0];

                            //if (Node.CanApplyConnection(connectOutput, clickedLinker))
                            { // If it can connect (type is equals, it does not cause recursion, ...)
                                //Node.ApplyConnection(connectOutput, clickedLinker);
                                clickedLinker.connection = connectOutput;
                                clickedLinker.connectionID = connectOutput.body.ID;
                                connectOutput.connection = clickedLinker;
                                connectOutput.connectionID = clickedLinker.body.ID;
                            }
                        }
                    }
                    else
                    {
                        connectOutput = null;
                    }

                }
                //e.Use();
            }



            if (clickedNode != null)
            {
                //e.Use();
            }
            else if (e.button == 2 || e.button == 0)
            { // Left/Middle click up -> Stop scrolling
                scrollWindow = false;
            }
            connectOutput = null;






        }
        else if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.N) // Start Navigating (curve to origin)
                navigate = true;
        }
        else if (e.type == EventType.KeyUp)
        {
            if (e.keyCode == KeyCode.N) // Stop Navigating
                navigate = false;
        }
        else if (e.type == EventType.Repaint)
        {
            if (navigate)
            { // Draw a curve to the origin/active node for orientation purposes
                DrawNodeCurve(nodeCanvas.scrollOffset, (activeNode != null ? activeNode.rect.center : e.mousePosition));
                Repaint();
            }
            if (connectOutput != null)
            { // Draw the currently drawn connection
                DrawNodeCurve(connectOutput.GetKnob().center, e.mousePosition);
                Repaint();
            }
        }
        if (scrollWindow)
        { // Scroll everything with the current mouse delta
            nodeCanvas.scrollOffset += e.delta / 2;
            for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++)
                nodeCanvas.nodes[nodeCnt].rect.position += e.delta / 2;
            Repaint();
        }
        /* // Handled by Unity. For new Windowing System
        if (dragNode) 
        { // Drag the active node with the current mouse delt
            activeNode.rect.position += e.delta / 2;
            Repaint ();
        }*/
    }

    /// <summary>
    /// Context Click selection. Here you'll need to register your own using a string identifier
    /// </summary>
    public void ContextCallback(object obj)
    {
        switch (obj.ToString())
        {


            case "AI_actionNode":
                AI_ActionNode actionNode = AI_ActionNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "Controller_actionNode":
                Controller_ActionNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "AI_conditionNode":
                AI_ConditionNode conditionNode = AI_ConditionNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "Controller_conditionNode":
                Controller_ConditionNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "Start_StepNode":
                StartStepNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "End_StepNode":
                EndStepNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50));
                break;

            case "deleteLinkTo":
                Node n = NodeAtPosition(mousePos);

                n.Linkers[(int)Node.LinkType.To].connection = null;
                n.Linkers[(int)Node.LinkType.To].connectionID = -1;
                break;

            case "deleteLinkToOK":
                Node n1 = NodeAtPosition(mousePos);

                n1.Linkers[(int)Node.LinkType.ToOK].connection = null;
                n1.Linkers[(int)Node.LinkType.ToOK].connectionID = -1;
                break;

            case "deleteLinkToKO":
                Node n2 = NodeAtPosition(mousePos);

                n2.Linkers[(int)Node.LinkType.ToKO].connection = null;
                n2.Linkers[(int)Node.LinkType.ToKO].connectionID = -1;
                break;

            case "deleteNode":
                Node node = NodeAtPosition(mousePos);
                if (node != null && (node.name != "Root" && node.name != "End"))
                {
                    nodeCanvas.nodes.Remove(node);
                    node.OnDelete();
                }
                break;
        }
    }

    #endregion

    #region GUI Functions

    public Rect sideWindowRect
    {
        get { return new Rect(position.width - sideWindowWidth, 0, sideWindowWidth, position.height); }
    }

    public static Texture2D ColorToTex(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(1, 1, col);
        tex.Apply();
        return tex;
    }
    public static Texture2D Tint(Texture2D tex, Color col)
    {
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, tex.GetPixel(x, y) * col);
            }
        }
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Returns the node at the position
    /// </summary>
    public Node NodeAtPosition(Vector2 pos)
    {
        if (sideWindowRect.Contains(pos))
            return null;
        // Check if we clicked inside a window (or knobSize pixels left or right of it at outputs, for easier knob recognition)
        for (int nodeCnt = nodeCanvas.nodes.Count - 1; nodeCnt >= 0; nodeCnt--)
        { // From top to bottom because of the render order (though overwritten by active Window, so be aware!)
            Rect NodeRect = new Rect(nodeCanvas.nodes[nodeCnt].rect);
            NodeRect = new Rect(
                NodeRect.x - knobSize,
                NodeRect.y - knobSize,
                NodeRect.width + knobSize * 2,
                NodeRect.height + knobSize * 2
                );
            if (NodeRect.Contains(pos))
                return nodeCanvas.nodes[nodeCnt];
        }
        return null;
    }

    /// <summary>
    /// Draws the node
    /// </summary>
    private void DrawNode(int id)
    {
        nodeCanvas.nodes[id].DrawNode();
        GUI.DragWindow();

        /* // Handled by Unity. For new Windowing System
        Rect headerRect = new Rect (node.rect.x, node.rect.y, node.rect.width, 20);
        Rect bodyRect = new Rect (node.rect.x, node.rect.y + 20, node.rect.width, node.rect.height - 40);
        GUI.Label (headerRect, new GUIContent (node.name));
        //GUI.Box (bodyRect, GUIContent.none, GUI.skin.box);
        GUILayout.BeginArea (bodyRect, nodeBox);
        node.DrawNode ();
        GUILayout.EndArea ();
        */
    }

    /// <summary>
    /// Draws a node curve from start to end (with three shades of shadows! :O )
    /// </summary>
    public static void DrawNodeCurve(Vector2 start, Vector2 end)
    {
        Vector3 startPos = new Vector3(start.x, start.y);
        Vector3 endPos = new Vector3(end.x, end.y);
        Vector3 startTan = startPos + Vector3.up * 50;
        Vector3 endTan = endPos + Vector3.down * 50;
        Color shadowColor = new Color(0, 0, 0, 0.1f);

        for (int i = 0; i < 3; i++) // Draw a shadow with 3 shades
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowColor, null, (i + 1) * 4); // increasing width for fading shadow
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2);
    }

    #endregion

    #region Node Canvas

    /// <summary>
    /// Saves the current node canvas as a new asset
    /// </summary>
    public void SaveNodeCanvas(string path)
    {
        XmlSerializer serialize = new XmlSerializer(typeof(Node_Canvas_Object));

        Debug.Log(path);

        StreamWriter Node_Canvas = new StreamWriter(path, false);

        serialize.Serialize(Node_Canvas, nodeCanvas);

        Node_Canvas.Close();
        Repaint();
    }

    /// <summary>
    /// Loads the a node canvas from an asset
    /// </summary>
    public void LoadNodeCanvas(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Node_Canvas_Object));

        StreamReader lecteur = new StreamReader(path);

        nodeCanvas = (Node_Canvas_Object)serializer.Deserialize(lecteur);

        foreach (Node n1 in nodeCanvas.nodes)
        {
            StartStepNode stepNode = n1 as StartStepNode;
            if (stepNode)
                stepNode.InitCustom();

            if (n1.Linkers[0])
            {
                n1.Linkers[0].body = n1;
            }
            for (int i = 1; i < 4; ++i)
            {
                if (n1.Linkers[i])
                {
                    n1.Linkers[i].body = n1;
                    foreach (Node n2 in nodeCanvas.nodes)
                    {
                        if (n1.Linkers[i].connectionID == n2.ID)
                        {
                            n1.Linkers[i].connection = n2.Linkers[(int)Node.LinkType.From];
                            n2.Linkers[(int)Node.LinkType.From].connection = n1.Linkers[i];
                        }
                    }
                }
            }
        }

        lecteur.Close();
        Repaint();
    }

    /// <summary>
    /// Creates and opens a new empty node canvas
    /// </summary>
    public void NewNodeCanvas()
    {
        nodeCanvas = ScriptableObject.CreateInstance<Node_Canvas_Object>();
        nodeCanvas.nodes = new List<Node>();
        openedCanvas = "New Canvas";
        openedCanvasPath = "";

        RootNode rootNode = RootNode.Create(new Rect(0, 0, 100, 50));
        EndNode endNode = EndNode.Create(new Rect(0, 200, 100, 50));

        rootNode.Linkers[(int)Node.LinkType.To].connection = endNode.Linkers[(int)Node.LinkType.From];
        rootNode.Linkers[(int)Node.LinkType.To].connectionID = endNode.ID;
        endNode.Linkers[(int)Node.LinkType.From].connection = rootNode.Linkers[(int)Node.LinkType.To];
        endNode.Linkers[(int)Node.LinkType.From].connectionID = rootNode.ID;

    }

    #endregion
}

#endif