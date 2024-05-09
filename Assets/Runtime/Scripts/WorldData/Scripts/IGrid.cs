using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrid
{
    public LevelGraph.Node node { get; set; }
    public LevelGraph.Node.NodeType previousType { get; set; }

    public void ChangeGirdPosition();
    public void SetNewNode();

}
