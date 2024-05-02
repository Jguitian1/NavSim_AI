using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Node startNode;
    public Node endNode;
    public GameObject player;
    private List<Node> path = new List<Node>();
    public float speed = 5.0f; // Speed at which the player will move

    void Start()
    {
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        var unvisited = new List<Node>(FindObjectsOfType<Node>());
        var dist = new Dictionary<Node, float>();
        var previous = new Dictionary<Node, Node>();

        foreach (Node node in unvisited)
        {
            dist[node] = float.MaxValue;
            previous[node] = null;
        }

        dist[startNode] = 0;

        while (unvisited.Count > 0)
        {
            Node current = null;
            float minDistance = float.MaxValue;

            foreach (Node n in unvisited)
            {
                if (dist[n] < minDistance)
                {
                    minDistance = dist[n];
                    current = n;
                }
            }

            if (current == null) break;

            unvisited.Remove(current);

            foreach (Node.Neighbor neighbor in current.neighbors)
            {
                Node neighborNode = neighbor.node;
                float alt = dist[current] + neighbor.cost;
                if (alt < dist[neighborNode])
                {
                    dist[neighborNode] = alt;
                    previous[neighborNode] = current;
                }
            }
        }

        // Build the path from the end node back to the start node
        Node step = endNode;

        while (step != null && step != previous[step])
        {
            path.Insert(0, step);
            step = previous[step];
        }
        path.Insert(0, startNode); // Add the start node at the beginning of the path

        // Move the player along the path
        foreach (Node node in path)
        {
            // Move towards each node one step at a time
            Vector3 startPosition = player.transform.position;
            Vector3 endPosition = node.transform.position;
            float journeyLength = Vector3.Distance(startPosition, endPosition);
            float startTime = Time.time;

            while (player.transform.position != endPosition)
            {
                float distCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distCovered / journeyLength;
                player.transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
                yield return null; // Wait until next frame before continuing execution
            }
            Debug.Log("Arrived at " + node.name);
        }
    }
}
