using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;

[Serializable]
public class LevelGraph
{
    #region Decleration
    public int width = 10;
    public int height = 10;

    public float cellSize = 1f;

    [HideInInspector]public Vector3 center;

    [HideInInspector]public Node[] storedNode;
    //for simplity we will store it in a 2d Array
    [HideInInspector]public Node[,] nodes;
    [HideInInspector]public Node[,] runtimeNodes;


    public Node GetNode(int x, int y)
    {
        return (!Application.isPlaying) ? nodes[x, y] : runtimeNodes[x,y];
    }
    
    #endregion

    public LevelGraph(int width, int height, float cellSize, Vector3 center)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        nodes = new Node[width, height];
     
        this.center = center;

        Init();
    }

    public void Init()
    {
        SetupArray();
        SetNeightbors(nodes);
    }
   
    
    public Color GetNodeColor(string key, BuildingTypes buildPallete)
    {
        if(buildPallete.buidlingsRefernce == null)
        {
            buildPallete.SetupDictionary();
        }
        return buildPallete.buidlingsRefernce[key].buildingGizmosColor;
    }
    public void SetNeightbors(Node[,] nodes)
    {
        // set neighbors, depends on gameplay style
        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                if (nodes[x, y].neighbors == null)
                {
                    nodes[x,y].neighbors = new List<Node>();
                }
                //Horizontal neighbors -----
                if (x > 0)
                {
                    nodes[x, y].neighbors.Add(nodes[x - 1, y]);
                }
                if (x < width - 1)
                {
                    nodes[x, y].neighbors.Add(nodes[x + 1, y]);
                }
                //Vertical neighbors ||||
                if (y > 0)
                {
                    nodes[x, y].neighbors.Add(nodes[x, y - 1]);
                }
                if (y < height - 1)
                {
                    nodes[x, y].neighbors.Add(nodes[x, y + 1]);
                }
            }
        }
    }

    public void SetupArray()
    {
        nodes = new Node[width, height];
        storedNode = new Node[width * height];
        int halfWidth = width / 2;
        int halfHeight = height / 2;

        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                nodes[x, y] = new Node(x - halfWidth, y - halfHeight);
               
                storedNode[(x * width) + y] = nodes[x, y];
            }
        }
    }
    public void StoreNodes()
    {
        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                storedNode[(x * width) + y] = nodes[x, y];
            }
        }
    }
    public void Reset2DArray()
    {
        nodes = new Node[width, height];
        runtimeNodes = new Node[width, height];
        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                Node node = storedNode[(x * width) + y];
                nodes[x, y] = storedNode[(x * width) + y];

                runtimeNodes[x, y] = new Node(node.x, node.y, node.typeOfOccupaition, node.buildingID, node.direction);
            }
        }
       
        SetNeightbors(nodes);
        SetNeightbors(runtimeNodes);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return center + new Vector3(x, 0, y) * cellSize;
    }
    public Vector3 GetWorldPosition(Vector3 position)
    {
        return center + position * cellSize;
    }
    public Vector3 GetWorldPositionCenter(Vector3 position)
    {
        return (center + position * cellSize) + new Vector3(cellSize/2, 0, cellSize/2);
    }

    public Node GetNodeFromWorldPosition(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / cellSize) + width / 2;
        int y = Mathf.FloorToInt(pos.z / cellSize) + height / 2;

        try
        {
            return (!Application.isPlaying) ? nodes[x, y] : runtimeNodes[x, y];
        }
        catch
        {
            //Debug.LogWarning("Out of world bounds");
            return null;
        }
    }

    [Serializable]
    public class Node
    {
        public int x;
        public int y;
        public Vector3 location { get { return new Vector3(x, 0, y); } private set { } }

        [NonSerialized] public List<Node> neighbors = new List<Node>();

        public bool occupied => IsOccupied();
        public bool isPath => IsPath();

        public bool inDanger;
        
        public NodeType typeOfOccupaition = NodeType.Empty;
        public NodeType previousOcupation = NodeType.Empty;

        public Vector3 direction = Vector3.forward;
        public string buildingID = BuildingTypes.emptyID;

        #region Pathfinding scores
        public int G, H = 0;



        public int F
        {
            get { return G + H; }
            private set { }
        }
        public Node previousNode = null;

        public void Reset()
        {
            G = int.MaxValue;
            previousNode = null;
        }
        #endregion

        public void ResetOccupied()
        {
            typeOfOccupaition = previousOcupation;
        }
        public void SetOccupied(NodeType type)
        {
            previousOcupation = typeOfOccupaition;
            typeOfOccupaition = type;
        }

        public bool CheckIfNeighbor(Node node)
        {
            return neighbors.Contains(node);
        }
        public Node CheckForMutualNeighbors(Node node)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (node.neighbors.Contains(neighbors[i]))
                {
                    return node;
                }
            }
            return null;
        }


        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Node(int x, int y, NodeType type)
        {
            this.x = x;
            this.y = y;

            this.typeOfOccupaition = type;
        }
        public Node(int x, int y, NodeType type, string buildingID, Vector3 direction)
        {
            this.x = x;
            this.y = y;

            this.typeOfOccupaition = type;
            this.buildingID = buildingID;
            this.direction = direction;
        }

        private bool IsOccupied()
        {
            return !(typeOfOccupaition == NodeType.Empty || typeOfOccupaition == NodeType.Path);
        }
        private bool IsPath()
        {
            return (typeOfOccupaition == NodeType.Path);
        }
        public override string ToString()
        {
            return "Node: (" + x + ", " + y + ")";
        }

        [Serializable]
        public enum NodeType
        {
            Empty = 0,
            Path = 1,
            Breakable = 2,
            Unbreakable = 3,
        }
    }
}
