using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCommand : DevCommandBase
{
    private Action command;
    public DevCommand(string commndID, string commandDecription, string commandFormat, Action command) : base(commndID, commandDecription, commandFormat)
    {
        this.command = command; 
    }
    public void Invoke()
    {
        command.Invoke();
    }
}
public class DevCommand<T1> : DevCommandBase
{
    private Action<T1> command;

    public DevCommand(string commndID, string commandDecription, string commandFormat, Action<T1> command) : base(commndID, commandDecription, commandFormat)
    {
        this.command = command;
    }
    public void Invoke(T1 value)
    {
        command.Invoke(value);
    }
}
