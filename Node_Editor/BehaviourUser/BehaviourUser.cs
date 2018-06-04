using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class BehaviourContainer
{
    public string BehaviourName;
    public Node_Canvas_Object canvas;
    public Node root;
    public Node currentNode;
}

public class BehaviourUser : MonoBehaviour 
{

    public const string editorPath = "Assets/Resources/Behaviours/";

    //public string BehaviourName;
    public List<string> BehaviourNames = new List<string>();

    //private Node_Canvas_Object canvas;
    //private Node root;
    //private Node currentNode;
    List<BehaviourContainer> Behaviours = new List<BehaviourContainer>();

    int currentBehaviour = 0;

    public bool execute = false;

	void Start () 
    {
        foreach (string s in BehaviourNames)
        {
            LoadBehaviour(s);
        }
    }
	
    bool isBehaviourLoaded(string behaviourName)
    {
        foreach (BehaviourContainer ctn in Behaviours)
        {
            if (ctn.BehaviourName == behaviourName)
            {
                Debug.Log("Behaviour \"" + behaviourName + "\" already loaded for the entity \"" + gameObject.name + "\"");
                return true;
            }
        }
        return false;
    }

    public void LoadNewBehaviour(string behaviourName)
    {
        if (isBehaviourLoaded(behaviourName) != true)
        {
            LoadBehaviour(behaviourName);
            BehaviourNames.Add(behaviourName);
        }
    }

    private void LoadBehaviour(string behaviourName)
    {
        if (isBehaviourLoaded(behaviourName))
        {
            return;
        }

        BehaviourContainer tmpContainer = new BehaviourContainer();

        tmpContainer.BehaviourName = behaviourName;

        XmlSerializer serializer = new XmlSerializer(typeof(Node_Canvas_Object));

        StreamReader lecteur = new StreamReader("Assets/Resources/Behaviours/" + behaviourName + ".xml");

        Node_Canvas_Object nodeCanvas = (Node_Canvas_Object)serializer.Deserialize(lecteur);

        foreach (Node n1 in nodeCanvas.nodes)
        {
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

        Behaviours.Add(tmpContainer);

        tmpContainer.canvas = nodeCanvas;

        foreach (Node node in tmpContainer.canvas.nodes)
        {
            if (node.name == "Root")
            {
                tmpContainer.root = node;
                tmpContainer.currentNode = tmpContainer.root.Linkers[(int)Node.LinkType.To].connection.body;
            }
        }

        Behaviours.Add(tmpContainer);
    }

	void Update () 
    {
        //Debug.Log(currentBehaviour);
        if (execute)
        {
            ExecuteNextStep();
        }
	}

    private void ResetBehaviour()
    {
        BehaviourContainer ctn = Behaviours[currentBehaviour];

        ctn.currentNode = ctn.root.Linkers[(int)Node.LinkType.To].connection.body;
    }

    public void SwitchBehaviour(string behaviourName, bool restart = false)
    {
        int current = 0;

        foreach (BehaviourContainer ctn in Behaviours)
        {
            if (ctn.BehaviourName == behaviourName)
            {
                currentBehaviour = current;

                if (restart)
                {
                    ResetBehaviour();
                }
                break;
            }
            ++current;
        }
    }

    public void ExecuteBehaviour()
    {

    }

    bool inStep = false;

    public void ExecuteNextStep()
    {
        BehaviourContainer ctn = Behaviours[currentBehaviour];

        if (ctn.currentNode.name == "Start Step")
        {
            inStep = true;
            ctn.currentNode = ctn.currentNode.Linkers[(int)Node.LinkType.To].connection.body;
        }

        if (ctn.currentNode.name == "End Step")
        {
            inStep = false;
            ctn.currentNode = ctn.currentNode.Linkers[(int)Node.LinkType.To].connection.body;
        }

        if (ctn.currentNode.name == "Action")
        {
            ActionNode n = ctn.currentNode as ActionNode;

            n.ActionFunction.DoAction(this.gameObject);
            ctn.currentNode = ctn.currentNode.Linkers[(int)Node.LinkType.To].connection.body;

            if (inStep)
            {
                ExecuteNextStep();
            }

        }
        else if (ctn.currentNode.name == "Condition")
        {
            ConditionNode n = ctn.currentNode as ConditionNode;

            if (n.ConditionFunction.DoCondition(this.gameObject))
            {
                ctn.currentNode = ctn.currentNode.Linkers[(int)Node.LinkType.ToOK].connection.body;
            }
            else
            {
                ctn.currentNode = ctn.currentNode.Linkers[(int)Node.LinkType.ToKO].connection.body;
            }

            ExecuteNextStep();
        }
        else if (ctn.currentNode.name == "End")
        {
            EndNode endNode = ctn.currentNode as EndNode;

            if (endNode.Restart)
            {
                ctn.currentNode = ctn.root.Linkers[(int)Node.LinkType.To].connection.body;
            }
        }
    }

    public void ExecuteBehaviour(bool Execute)
    {
        execute = Execute;
    }
}
