using BehaviourTree;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    #region Declerations
    public static Pathfinding instance;
    

    public LevelsScriptableObject levels;
    public BuildingTypes buildingPallete;
    public GridScriptableObject path;

    [HideInInspector]public LevelGraph grid;

    #endregion

 

    private void Awake()
    {
        path = levels.levels[PlayerPrefs.GetInt(GameRules.levelSelectGameRuleKey)];
        buildingPallete.SetupDictionary();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GetGrid();
    }


    public List<LevelGraph.Node> FindPath(Vector3 startPos, Vector3 endPos, bool ignoreDanger = false)
    {

        LevelGraph.Node startNode = grid.GetNodeFromWorldPosition(startPos);
        LevelGraph.Node endNode = grid.GetNodeFromWorldPosition(endPos);
        

        if (startNode == null || endNode == null)
        {
            return null;
        }
        return AStar(startNode, endNode, ignoreDanger);
    }
    public void GetGrid()
    {
        if (path == null)
        {
            grid = new LevelGraph(10, 10, 1, transform.position);
        }
        else
        {
            grid = path.grid;
            grid.Reset2DArray();
        }
    }
    
    public Vector3 GetRandomPosition()
    {
        List<LevelGraph.Node> pathBlocks = new List<LevelGraph.Node>();

        foreach (LevelGraph.Node node in grid.nodes)
        {
            if (node.occupied) { continue; }
            pathBlocks.Add(node);
        }

        LevelGraph.Node randomNode = pathBlocks[UnityEngine.Random.Range(0, pathBlocks.Count)];

        return grid.GetWorldPositionCenter(randomNode.location);
        
    }
    /// <summary>
    /// Pathfinding algorithim
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private List<LevelGraph.Node> AStar(LevelGraph.Node startNode, LevelGraph.Node endNode, bool ignoreDanger)
    {
        List<LevelGraph.Node> openNodes = new List<LevelGraph.Node>() {startNode };
        List<LevelGraph.Node> closedNodes = new List<LevelGraph.Node>();

        ResetNodes();

        startNode.G = 0;
        startNode.H = CalculateManhattanDistance(startNode, endNode);

        while(openNodes.Count > 0)
        {
            LevelGraph.Node currentNode = GetFittestNode(openNodes, startNode, endNode, ignoreDanger); 
            if(currentNode == endNode)
            {
                //done!....ish
                return CalculatePath(endNode);
            }

            //first node has been searched
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            foreach(LevelGraph.Node neighbor in currentNode.neighbors)
            {
                if (closedNodes.Contains(neighbor) || ((neighbor.occupied || (!ignoreDanger && neighbor.inDanger)) && !(neighbor == startNode || neighbor == endNode))) { continue; }
                
                int gScore = currentNode.G + CalculateManhattanDistance(currentNode, neighbor);
                if(gScore < neighbor.G)
                {
                    neighbor.previousNode = currentNode;
                    neighbor.G = gScore;
                    neighbor.H = CalculateManhattanDistance(neighbor, endNode);
                    if (!openNodes.Contains(neighbor))
                    {
                        openNodes.Add(neighbor);
                    }

                }

            }

        }
        return null;

    }
    List<LevelGraph.Node> CalculatePath(LevelGraph.Node goal)
    {
        List<LevelGraph.Node> path = new List<LevelGraph.Node>();
        path.Add(goal);

        LevelGraph.Node currentNode = goal;
        while(currentNode.previousNode != null)
        {
            path.Add(currentNode.previousNode);
            currentNode = currentNode.previousNode;
        }
        return path;
    }

    private void ResetNodes()
    {
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                grid.GetNode(i, j).Reset();
            }
        }
    }

    private static LevelGraph.Node GetFittestNode(List<LevelGraph.Node> nodes, LevelGraph.Node startNode, LevelGraph.Node endNode, bool ignoreDanger)
    {
        LevelGraph.Node fittestNode = nodes[0];

        for(int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].inDanger && !ignoreDanger)
            {
                continue;
            }
            if (nodes[i].occupied)
            {
                continue;
            }
            if (nodes[i] == startNode || nodes[i] == endNode)
            {
                continue;
            }
            if(nodes[i].F < fittestNode.F)
            {
                fittestNode = nodes[i];
            }
        }
        return fittestNode;
    }

    private int CalculateManhattanDistance(LevelGraph.Node a, LevelGraph.Node b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        return xDistance + yDistance;
    }

    private void OnValidate()
    {
        
        GetGrid();
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        
        if (Application.isPlaying && !GameManager.instance.debugManager.showLevelLayout) { return; }
        
        Vector3 yPos = new Vector3(0, transform.position.y, 0);

        if (buildingPallete.buidlingsRefernce == null)
        {
            buildingPallete.SetupDictionary();
        }

        if (grid == null)
        {
            GetGrid();
        }

        if (grid.nodes == null) { grid.Reset2DArray(); }
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                LevelGraph.Node currentNode = grid.GetNode(x, y);
                //Draws a wire mesh instead of the normal
                
                if (currentNode.buildingID == BuildingTypes.emptyID)
                {
                    Gizmos.color = grid.GetNodeColor(currentNode.buildingID, buildingPallete);
                    Gizmos.DrawWireCube(grid.GetWorldPositionCenter(currentNode.location) + yPos, new Vector3(grid.cellSize, 0, grid.cellSize));
                }
                else
                {
                    Gizmos.color = grid.GetNodeColor(currentNode.buildingID, buildingPallete);
                    Gizmos.DrawCube(grid.GetWorldPositionCenter(currentNode.location) + yPos, new Vector3(grid.cellSize, 0, grid.cellSize));
                }
                if (currentNode.inDanger)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(grid.GetWorldPositionCenter(currentNode.location) + yPos, new Vector3(grid.cellSize, 0, grid.cellSize));
                }
            }
        }
        

    }
#endif
}
