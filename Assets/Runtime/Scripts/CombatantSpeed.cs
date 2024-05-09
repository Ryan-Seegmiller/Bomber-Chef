using System;

[Serializable]
public class CombatantSpeed
{
    public bool localSpeedToggle;
    public float localSpeed;

    public CombatantSpeedReference globalSpeed;

    public float Speed
    {
        get { return (localSpeedToggle) ? localSpeed : globalSpeed.speed; }
        
    }
}
