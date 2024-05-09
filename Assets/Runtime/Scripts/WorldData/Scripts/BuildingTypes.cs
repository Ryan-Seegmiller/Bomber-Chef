using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Builidng Pallete", menuName = "Grid/Bulding Pallete")]
public class BuildingTypes : ScriptableObject
{
    public const string pathID = "Path";
    public const string emptyID = "Empty";
    public Dictionary<string, Building> buidlingsRefernce;
    public Building[] buildings = new Building[1];
    [NonSerialized]public Building path = new Building
        (
            pathID,
            null,
            Color.green,
            LevelGraph.Node.NodeType.Path
        );
    [NonSerialized] public Building empty = new Building
        (
            emptyID,
            null,
            Color.cyan,
            LevelGraph.Node.NodeType.Empty
        );

    public void SetupDictionary()
    {
        buidlingsRefernce = new Dictionary<string, Building>();
        foreach(Building building in buildings)
        {
            buidlingsRefernce.Add(building.buildingID, building);
        }
        buidlingsRefernce.Add(path.buildingID, path);
        buidlingsRefernce.Add(empty.buildingID, empty);
    }
    public GameObject GetBuildingObject(string key)
    {
        return buidlingsRefernce[key].buildingObject;
    }

}
[System.Serializable]
public struct Building
{
    public string buildingID;
    public GameObject buildingObject;
    public Color buildingGizmosColor;
    public LevelGraph.Node.NodeType nodeType;

    public Building(string buildingID, GameObject buildingObject, Color buildingGizmosColor, LevelGraph.Node.NodeType nodeType)
    {
        this.buildingID = buildingID;
        this.buildingObject = buildingObject;
        this.buildingGizmosColor = buildingGizmosColor;
        this.nodeType = nodeType;
    }
}
