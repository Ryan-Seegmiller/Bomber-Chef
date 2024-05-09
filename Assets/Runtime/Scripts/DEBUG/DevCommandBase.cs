using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevCommandBase
{
    private string _commndID;
    private string _commandDecription;
    private string _commandFormat;

    public string commandID
    {
        get { return _commndID; }
    }
    public string commandDescription
    {
        get { return _commandDecription; }
    }
    public string commandFormat
    {
        get { return _commandFormat; }
    }

    public DevCommandBase(string commndID, string commandDecription, string commandFormat)
    {
        _commndID = commndID;
        _commandDecription = commandDecription;
        _commandFormat = commandFormat;
    }
}


