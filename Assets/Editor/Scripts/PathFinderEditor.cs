using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System.Drawing.Printing;
using BehaviourTree;
using Codice.CM.Client.Differences;
using log4net.Core;

[EditorTool("Level Builder Tool", typeof(Pathfinding))]
public class PathFinderEditor : EditorTool
{
    #region Classes
    Pathfinding m_Pathfinding;
    LevelGraph m_LevelGraph = null;
    BuildingTypes m_BuildingPallete;
    LevelGraph.Node[,] startingGrid;
    #endregion

    string[] buildingNames = null;
    Color[] buildingColors = null;

    bool changeBounds = false;
    bool dragToDraw = false;
    bool guiChange = false;

    Vector3? hitPoint;

    bool forceQuit;

    string widthInput;
    string heightInput;
    string cellSizeInput;

    #region ToggleWindow
    Rect togglesWindowRect = new Rect(150, 20, 120, 50);
    Rect boundsWindowRect = new Rect(200, 20, 120, 50);
    Rect boundsDisplayWindowRect = new Rect(300, 20, 400, 20);

    int toggleIndex = 0;
    GUIContent[] toggles = { };

    #endregion

    public override GUIContent toolbarIcon => EditorGUIUtility.IconContent("AvatarPivot");

    #region Setup
    // Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
    // so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
    // to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
    public override void OnActivated()
    {  //setup
        m_Pathfinding = (Pathfinding)target;
        m_LevelGraph = m_Pathfinding.grid;
        m_Pathfinding.GetGrid();

        forceQuit = m_Pathfinding.path == null || m_Pathfinding.buildingPallete == null;

        SetupArrays();

        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Grid Layout Tool"), .1f);
        SaveStartNodes();
    }
    void SetupArrays()
    {
        //Array setup
        int arrayLength = m_Pathfinding.buildingPallete.buildings.Length + 2;
        buildingNames = new string[arrayLength];
        buildingColors = new Color[arrayLength];
        toggles = new GUIContent[arrayLength];

        //Gets the buidling pallete
        m_BuildingPallete = m_Pathfinding.buildingPallete;

        //Adds the path block
        buildingNames[0] = m_BuildingPallete.path.buildingID;
        buildingColors[0] = m_BuildingPallete.path.buildingGizmosColor;
        toggles[0] = new GUIContent(buildingNames[0]);

        //Adds the empty block
        buildingNames[1] = m_BuildingPallete.empty.buildingID;
        buildingColors[1] = m_BuildingPallete.empty.buildingGizmosColor;
        toggles[1] = new GUIContent(buildingNames[1]);

        //Sets up the building names
        for (int i = 2; i < arrayLength; i++)
        {
            Building building = m_BuildingPallete.buildings[i - 2];

            buildingNames[i] = building.buildingID;
            buildingColors[i] = building.buildingGizmosColor;

            toggles[i] = new GUIContent(buildingNames[i]);
        }
    }
    void SaveStartNodes()
    {
        if(m_Pathfinding.grid.nodes == null)
        {
            Debug.LogWarning("Nodes have been erased");
            m_Pathfinding.grid.Init();
        }
        startingGrid = new LevelGraph.Node[m_Pathfinding.grid.nodes.GetLength(0), m_Pathfinding.grid.nodes.GetLength(1)];
        for (int i = 0; i < m_Pathfinding.grid.width; i++)
        {
            for (int j = 0; j < m_Pathfinding.grid.height; j++)
            {
                LevelGraph.Node node = m_Pathfinding.grid.nodes[i, j];
                startingGrid[i, j] = new LevelGraph.Node(node.x, node.y, node.typeOfOccupaition, node.buildingID, node.direction);
            }
        }
    }
    #endregion

    #region Deactivate
    // Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
    // destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
    public override void OnWillBeDeactivated()
    {
        if (forceQuit || Application.isPlaying) {
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Edit Requirements not yet met"), .5f);
            return; 
        }
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Grid Layout Tool"), .1f);
        int saved = EditorUtility.DisplayDialogComplex("Save", "Save this as a new level grid?", "Save New", "Discard", "Save");
        switch (saved)
        {
            case 0:
                SaveNew();
                break;
            case 2:
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Changes saved"), .5f);
                break;
            default:
                m_LevelGraph.nodes = startingGrid;//Cancels and resets the nodes back to before
                break;
        }

        string assetName = m_Pathfinding.path.name;
        string assetPath = $"Assets/Runtime/Prefabs/World/ScriptableObjects/{assetName}.asset";
        ScriptableObject scrobj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

        //Stores the nodes so it dosent reset on refresh
        m_LevelGraph.StoreNodes();
        EditorUtility.SetDirty(scrobj);
        AssetDatabase.Refresh();
    }
    private void SaveNew()
    {
        GridScriptableObject newObj = CreateInstance<GridScriptableObject>();
        newObj.grid = m_LevelGraph;
        ProjectWindowUtil.CreateAsset(newObj, "Assets/Runtime/Prefabs/World/ScriptableObjects/NewPath.asset");//Creates asset using the project window
        Undo.RegisterCreatedObjectUndo(newObj, "Created PathFinding grid");


        EditorUtility.SetDirty(newObj);
        AssetDatabase.Refresh();
    }
    #endregion

    #region Update
    // Equivalent to Editor.OnSceneGUI.
    public override void OnToolGUI(EditorWindow window)
    {
        if (forceQuit) {ToolManager.RestorePreviousTool();}
        if (guiChange) {guiChange = false; return; }
        if (!(window is SceneView sceneView))
            return;

        DrawWindows();

        if(m_LevelGraph.nodes == null) {return; }

        MouseCheck();

        CheckNodes();
       
    }

    #region Node Behaviour
    /// <summary>
    /// Checks the nodes
    /// </summary>
    private void CheckNodes()
    {
        foreach (LevelGraph.Node node in m_LevelGraph.nodes)
        {
            Vector3 point = m_LevelGraph.GetWorldPositionCenter(node.location);

            EditorGUI.BeginChangeCheck();
            //Makes sure there is a value on the object
            if (node.buildingID == string.Empty)
            {
                node.buildingID = BuildingTypes.emptyID;
            }

            //Handles the direction of the object
            Handles.color = m_LevelGraph.GetNodeColor(node.buildingID, m_BuildingPallete);
            Handles.Slider(point + node.direction, node.direction, .5f, Handles.ArrowHandleCap, 0);

            //Sets points within the buttons
            ChangeNode(point, node);
            if (EditorGUI.EndChangeCheck()) { }
        }
    }
    private void ChangeNode(Vector3 point, LevelGraph.Node node)
    {   //Makes sure a button is clicked
        if (!(Handles.Button(point, Quaternion.Euler(90, 0, 0), m_Pathfinding.grid.cellSize / 4, m_Pathfinding.grid.cellSize / 2, Handles.RectangleHandleCap) || MouseClicked(node))) { return; }
        Undo.RecordObject(m_Pathfinding, "Set Node Occupied");
        //Clears the tile       
        if (Event.current.shift)
        {
            node.buildingID = BuildingTypes.emptyID;
            return;
        }
        node.buildingID = buildingNames[toggleIndex];//Sets the tile
        node.typeOfOccupaition = m_BuildingPallete.buidlingsRefernce[buildingNames[toggleIndex]].nodeType;

        SetNodeDirection(node);//Sets the tile direction
    }
    private void SetNodeDirection(LevelGraph.Node node)
    {
        if (dragToDraw) { return; }
        //Sets teh direction for the directional blocks
        if (node.buildingID == buildingNames[toggleIndex])
        {
            node.direction = Quaternion.AngleAxis(90, Vector3.up) * node.direction;
        }
        if (node.direction == Vector3.zero)
        {   //Makes sure that the direction isnt zero
            node.direction = Vector3.forward;
        }
    }
    #endregion

    #region Mouse clicked
    /// <summary>
    /// Checks if the mouse is currently held
    /// </summary>
    private void MouseCheck()
    {
        hitPoint = null;

        if (Event.current.type == EventType.MouseDrag && dragToDraw)
        {
            if (Event.current.button == 1)
            {
                //dragToDraw = false;
                return;
            }
           
            //This is how you get the scene view point
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, int.MaxValue))
            {
                hitPoint = hit.point;
            }
        }
    }
    /// <summary>
    /// Detects if the mouse was clicked
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool MouseClicked(LevelGraph.Node node)
    {
        return dragToDraw && hitPoint != null && m_LevelGraph.GetNodeFromWorldPosition((Vector3)hitPoint) == node;
    }
    #endregion

    #region Windows
    /// <summary>
    /// Draws the popup windows
    /// </summary>
    private void DrawWindows()
    {
        Handles.BeginGUI();
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //Window rect moves the window if the window is moved during the Toggle window method
                togglesWindowRect = GUILayout.Window(0, togglesWindowRect, ToggleWindow, "Tile Type");

                if (changeBounds)
                {
                    boundsWindowRect = GUILayout.Window(1, boundsWindowRect, BoundsWindow, "Bounds");
                }

                boundsDisplayWindowRect = GUILayout.Window(2, boundsDisplayWindowRect, BoundsDisplay, "");
            }

            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();
    }
    /// <summary>
    /// Draws the toggle window
    /// </summary>
    /// <param name="windowID"></param>
    private void ToggleWindow(int windowID)
    {
        // Make a very long rect that is 20 pixels tall.
        // This will make the window be resizable by the top
        // title bar - no matter how wide it gets.
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        EditorGUI.BeginChangeCheck();

        toggleIndex = GUILayout.SelectionGrid(toggleIndex, toggles, 1);
        dragToDraw = GUILayout.Toggle(dragToDraw, "Drag to Draw");
        //Activates the bounds change window
        if(GUILayout.Button("Change Bounds"))
        {
            changeBounds = !changeBounds;
            widthInput = $"{m_LevelGraph.width}";
            heightInput = $"{m_LevelGraph.height}";
            cellSizeInput = $"{m_LevelGraph.cellSize}";
            dragToDraw = false;
        }
        if (EditorGUI.EndChangeCheck())
        {   //Prevents any accidental level placement
            hitPoint = null;
            guiChange = true;
        }
    }
    /// <summary>
    /// Draws the bounds window
    /// </summary>
    /// <param name="windowID"></param>
    private void BoundsWindow(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        EditorGUI.BeginChangeCheck();
       

        GUILayout.Label("Width");
        widthInput = GUILayout.TextField(widthInput);
        GUILayout.Label("Height");
        heightInput = GUILayout.TextField(heightInput);
        GUILayout.Label("Cell Size");
        cellSizeInput = GUILayout.TextField(cellSizeInput);


        if (GUILayout.Button("Save"))
        {
            bool saveValues = EditorUtility.DisplayDialog("Save Bounds", "WARNING: Any saved path data on this will be lost", "Save", "Cancel");
            if(saveValues)
            {
                m_LevelGraph.width = Int32.Parse(widthInput);
                m_LevelGraph.height = Int32.Parse(heightInput);
                m_LevelGraph.cellSize = float.Parse(cellSizeInput);
                m_LevelGraph.Init();
            }
            changeBounds = !changeBounds;
        }
        if (EditorGUI.EndChangeCheck())
        {   //Error handling for anything other than the requied input
            #region String error Handling
            if (!Int32.TryParse(widthInput, out int reult))
            {
                if(widthInput == string.Empty)
                {
                    widthInput = "0";
                }
                else
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Enter A Int"), .3f);
                    widthInput = $"{m_LevelGraph.width}";
                }
            }
            if (!Int32.TryParse(heightInput, out int reult2))
            {
                if(heightInput == string.Empty)
                {
                    heightInput = "0";
                }
                else
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Enter A Int"), .3f);
                    heightInput = $"{m_LevelGraph.height}";
                }
            }
            if (!float.TryParse(cellSizeInput, out float result3))
            {
                if(cellSizeInput == string.Empty)
                {
                    cellSizeInput = "0";
                }
                else
                {
                    SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Enter A Float"), .3f);
                    cellSizeInput = $"{m_LevelGraph.cellSize}";
                }
            }
            #endregion
            hitPoint = null;

            guiChange = true;
        }

    }
    /// <summary>
    /// draws the display window
    /// </summary>
    /// <param name="WindowID"></param>
    private void BoundsDisplay(int WindowID)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 20));

        GUILayout.BeginHorizontal();

        int width = 80;
        int height = 20;
        Rect labelRect1 = new Rect(width /2, 0,width, height);
        Rect labelRect2 = new Rect(width + width/2,0,width, height);
        Rect labelRect3 = new Rect(width * 2 + width/2,0,width + 20, height);
        Rect labelRect4 = new Rect( width * 3 + width / 2,0,width + 20, height);

        GUI.Label(labelRect1,$"Width: {m_LevelGraph.width}");
        GUI.Label(labelRect2,$"Height: {m_LevelGraph.height}");
        GUI.Label(labelRect3, $"Cell Size: {m_LevelGraph.cellSize}");
        GUI.Label(labelRect4,$"Total Cells: {m_LevelGraph.width * m_LevelGraph.height}");

        GUILayout.EndHorizontal();
    }
    #endregion

    #endregion
}
