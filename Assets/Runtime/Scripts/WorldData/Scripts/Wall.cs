using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode()]
public class Wall : MonoBehaviour, IGrid
{
    #region Declerations
    public GameManager gameManager => GameManager.instance;
    public LevelGraph.Node node { get; set; }
    public LevelGraph.Node.NodeType previousType { get; set; }

    public WallType type;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if(TryGetComponent(out Health health))
        {
            health.death += Death;
        }
        SetNewNode();
    }
    public void ChangeGirdPosition()
    {
        if(node == null) { return; }
        node.SetOccupied(previousType);
    }
 
    /// <summary>
    /// Sets the new node to the world position
    /// </summary>
    public void SetNewNode()
    {
        ChangeGirdPosition();
        node = gameManager.playableArea.grid.GetNodeFromWorldPosition(transform.position);
        previousType = node.typeOfOccupaition;
        node.SetOccupied(GetTileType(type));
    }


   
    public static LevelGraph.Node.NodeType GetTileType(WallType type)
    {
        switch (type)
        {
            case WallType.Breakable:
               return LevelGraph.Node.NodeType.Breakable;
            case WallType.Unbreakable:
                return LevelGraph.Node.NodeType.Unbreakable;
            default:
                //For oven and conveyour
                return LevelGraph.Node.NodeType.Empty;
        }
    }

    public void OnDestroy()
    {
        ChangeGirdPosition();
    }
    public void Death()
    {
        node.SetOccupied(LevelGraph.Node.NodeType.Path);
    }

    public enum WallType
    {
        Breakable,
        Unbreakable,
        Empty

    }
}