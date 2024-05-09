using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class DevController : MonoBehaviour
{
    public static DevController instace;
    [HideInInspector] public bool showConsole;
    [HideInInspector] public bool showHelp;
    [HideInInspector] public string input = string.Empty;

    public List<CommandData> commands = new List<CommandData>();
    [NonSerialized] List<DevCommandBase> devCommands = new List<DevCommandBase>();
    #region Setup
    public void Start()
    {
        if(instace == null)
        {
            instace = this;
        }
        else
        {
            Destroy(gameObject);
        }
        SetupCommands();
    }
    public void SetupCommands()
    {
        foreach (CommandData data in commands)
        {
            devCommands.Add(new DevCommand(data.commndID, data.commandDecription, data.commandFormat, () => { data.command.Invoke(); }));
        }
    }
    #endregion

   

    Vector2 scroll;
    private void OnGUI()
    {
        if (!showConsole) { return; }
        
        float y = 0f;

        if (showHelp)
        {
            //draw a box
            GUI.Box(new Rect(0, y, Screen.width, 100), "");
            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * devCommands.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y, Screen.width, 90), scroll, viewport);

            for(int i = 0; i < devCommands.Count; i++)
            {
                string label = $"{devCommands[i].commandFormat} - {devCommands[i].commandDescription}";
                Rect labelRect = new Rect(5, 20 * i, viewport.width - 100, 20);
                GUI.Label(labelRect, label);
            }
            GUI.EndScrollView();
            y += 100;
     
        }


        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
    }

    /*public void HandleInput()
    {
        for (int i = 0; i < devCommands.Count; i++)
        {
            if (input.Contains(devCommands[i].commandID))
            {
                (devCommands[i] as DevCommand).Invoke();
            }
        }
    }*/
    public void HandleInput()
    {
        string[] properties = input.Split(" ");

        for (int i = 0; i < devCommands.Count; i++)
        {
            DevCommandBase command = devCommands[i];

            if (!input.Contains(command.commandID)) { continue; }

            if (command as DevCommand<int> != null)
            {
                (command as DevCommand<int>).Invoke(int.Parse(properties[1]));
            }
            else if(command as DevCommand != null)
            {
                (command as DevCommand).Invoke();
            }

        }
    }
    public void ShowHelp()
    {
        showHelp = !showHelp;
    }
    

}
[System.Serializable]
public struct CommandData
{
    public string commndID;
    public string commandDecription;
    public string commandFormat;
    public UnityEvent command;
}