using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text; // Needed for StringBuilder

public class AIPathfindingQ : MonoBehaviour
{
    public Node startNode;
    public Node endNode;
    public GameObject player;
    private Dictionary<(Node, Node), float> QValues = new Dictionary<(Node, Node), float>();
    public float learningRate = 0.8f;
    public float discountFactor = 0.9f;
    public float explorationRate = 0.2f;
    private List<Node> path = new List<Node>();
    public float speed = 5.0f;
    private int resetCounter = 0; // Counter to track the number of resets

    public float explorationDecay = 0.995f;

    public float minExplorationRate = 0.05f; // Minimum exploration rate

    void Start()
    {
        InitializeQValues();
        StartCoroutine(ContinuousLearning());
    }

    void InitializeQValues()
    {
        Node[] allNodes = FindObjectsOfType<Node>();
        foreach (Node node in allNodes)
        {
            foreach (Node.Neighbor neighbor in node.neighbors)
            {
                if (!QValues.ContainsKey((node, neighbor.node)))
                {
                    QValues[(node, neighbor.node)] = 0f; // Initialize all Q-values to zero
                }
            }
        }
    }

    void UpdateExplorationRate()
{
    explorationRate *= explorationDecay;
    explorationRate = Mathf.Max(explorationRate, minExplorationRate);
}

    IEnumerator ContinuousLearning()
    {
        while (true) // Loop indefinitely
        {
            yield return StartCoroutine(LearnToNavigate());
            BuildAndFollowPath();
            LogPath(); // Log the path taken
            ResetEnvironment(); // Reset the environment for the next run
            UpdateExplorationRate(); // Decrease exploration rate after each run

        }
    }

    IEnumerator LearnToNavigate()
{
    Node currentNode = startNode;
    HashSet<Node> visitedNodes = new HashSet<Node>(); // Track visited nodes
    visitedNodes.Add(currentNode); // Start node is visited at the beginning

    while (currentNode != endNode)
    {
        Node nextNode = ChooseNextNode(currentNode);
        float reward = -currentNode.neighbors.Find(n => n.node == nextNode).cost; // Negative cost as reward

        if (visitedNodes.Contains(nextNode))
        {
            reward -= 500; // Apply a penalty for revisiting a node
        }
        else
        {
            visitedNodes.Add(nextNode); // Add to visited if it's a new node
        }

        float oldQValue = QValues[(currentNode, nextNode)];
        float maxQ = float.MinValue;

        foreach (var neighbor in nextNode.neighbors)
        {
            float qValue = QValues[(nextNode, neighbor.node)];
            if (qValue > maxQ)
            {
                maxQ = qValue;
            }
        }

        float newQValue = oldQValue + learningRate * (reward + discountFactor * maxQ - oldQValue);
        QValues[(currentNode, nextNode)] = newQValue;

        currentNode = nextNode;

        yield return null;
    }
}


    Node ChooseNextNode(Node currentNode)
    {
        if (Random.value < explorationRate)
        {
            return currentNode.neighbors[Random.Range(0, currentNode.neighbors.Count)].node;
        }
        else
        {
            return GetBestNextNode(currentNode);
        }
    }

    Node GetBestNextNode(Node currentNode)
    {
        float maxQ = float.MinValue;
        Node bestNode = null;
        foreach (var neighbor in currentNode.neighbors)
        {
            float qValue = QValues[(currentNode, neighbor.node)];
            if (qValue > maxQ)
            {
                maxQ = qValue;
                bestNode = neighbor.node;
            }
        }
        return bestNode ?? currentNode.neighbors[Random.Range(0, currentNode.neighbors.Count)].node; // Fallback to random if no best node
    }

    void ResetEnvironment()
    {
        player.transform.position = startNode.transform.position; // Reset player position to the start node
        resetCounter++; // Increment the counter each time the environment is reset
        Debug.Log("Environment has been reset " + resetCounter + " times.");
    }

    void BuildAndFollowPath()
    {
        Node currentNode = startNode;
        path.Clear();
        while (currentNode != endNode)
        {
            path.Add(currentNode);
            currentNode = ChooseNextNode(currentNode);
        }
        path.Add(endNode);
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        foreach (Node node in path)
        {
            Vector3 startPosition = player.transform.position;
            Vector3 endPosition = node.transform.position;
            while (Vector3.Distance(player.transform.position, endPosition) > 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, endPosition, speed * Time.deltaTime);
                yield return null;
            }
        }
        Debug.Log("Arrived at end.");
    }

    void LogPath()
    {
        StringBuilder pathLog = new StringBuilder("Current Path: ");
        foreach (Node node in path)
        {
            pathLog.Append(node.name + " -> ");
        }
        pathLog.Append("End");
        Debug.Log(pathLog.ToString());
    }
}
