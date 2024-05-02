using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [System.Serializable]
    public struct Neighbor
    {
        public Node node;
        public float cost;
    }

    public List<Neighbor> neighbors = new List<Neighbor>();

    public void AddNeighbor(Node neighbor, float cost)
    {
        neighbors.Add(new Neighbor { node = neighbor, cost = cost });
    }
}
