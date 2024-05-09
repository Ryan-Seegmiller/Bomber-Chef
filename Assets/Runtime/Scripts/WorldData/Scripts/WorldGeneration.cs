using BehaviourTree;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [HideInInspector] public GameObject[] level;
    Dictionary<string, GameObject> groups = null;

    public GameObject world;

    public Pathfinding path;
    GameManager gameManager => GameManager.instance;
    


    // Start is called before the first frame update
    void Start()
    {
        EraseLevel();
        Instantiate(path.levels.levels[PlayerPrefs.GetInt(GameRules.levelSelectGameRuleKey)].level, transform);
    }
    
    public void GenerateWalls()
    {
        if(path.buildingPallete.buidlingsRefernce != null)
        {
            path.buildingPallete.SetupDictionary();
        }
        world = new GameObject("world");
        world.transform.parent = transform;

        Dictionary<string, GameObject> groups = new Dictionary<string, GameObject>();

        foreach (string name in path.buildingPallete.buidlingsRefernce.Keys)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = world.transform;
            groups.Add(name, go);

        }
        Vector3 raisedVector3 = new Vector3(0, .5f, 0);
        level = new GameObject[path.grid.width * path.grid.height];
        for (int x = 0; x < path.grid.width; x++)
        {
            for (int y = 0; y < path.grid.height; y++)
            {
                LevelGraph.Node currentNode = path.grid.nodes[x, y];
                
                Vector3 worldPos = path.grid.GetWorldPositionCenter(currentNode.location);

                GameObject objToInstatiate = path.buildingPallete.buidlingsRefernce[currentNode.buildingID].buildingObject;
                if(objToInstatiate == null) { continue; }
                GameObject obj = Instantiate(objToInstatiate);
                obj.transform.forward = currentNode.direction;
                  
                obj.transform.position = worldPos + raisedVector3;
                    
                obj.GetComponent<Wall>().SetNewNode();

                obj.transform.SetParent(groups[currentNode.buildingID].transform);

                level[(x * path.grid.width) + y] = obj;
            }
        }
    }
    public void EraseLevel()
    {
        if(world == null) { return; }
        if (Application.isPlaying)
        {
            world.SetActive(false);
        }
        else
        {
            DestroyImmediate(world);
        }
        world = null;
        groups = new Dictionary<string, GameObject>();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(WorldGeneration))]
public class WorldGenertationEditor : Editor
{
    WorldGeneration m_worldGeneration;
    public override void OnInspectorGUI()
    {
        if(m_worldGeneration == null)
        {
            m_worldGeneration = (WorldGeneration)target;
        }
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            if(m_worldGeneration.world == null)
            {
                m_worldGeneration.GenerateWalls();
            }
        }

        if (GUILayout.Button("Erase Level"))
        {
            int option = EditorUtility.DisplayDialogComplex("Erase", "WARNING: This will erase all saved data if not turned into a prefab", "Erase", "Cancel", "Dont Erase");
            switch (option)
            {
                case 0:
                    m_worldGeneration.EraseLevel();
                    break;
                default:
                    break;
            }
            
        }

    }
}
#endif