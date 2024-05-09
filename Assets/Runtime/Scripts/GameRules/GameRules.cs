using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GameRules
{
    public const string levelSelectGameRuleKey = "CurrentLevel";
    public const string healthGameRuleKey = "Health";
    public const string CookTimeKey = "CookTime";
    public const string durationKey = "GameDuration";
    public const string respawnTimeKey = "RespawnTime";

    public int health;
    public float cookTime;
    public float duration;
    public float respawnTime;
    public GridScriptableObject map;
}
