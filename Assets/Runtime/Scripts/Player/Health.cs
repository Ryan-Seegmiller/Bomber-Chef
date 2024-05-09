using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    #region Declerations
    private int _maxHealth = 3;
    public int maxHealth
    {
        get { return _maxHealth; }
        set 
        { 
            health = value;
            _maxHealth = value; 
        }
    }

    private int _health;
    public int health
    {
        get { return _health; }
        set 
        { 
            _health = value;
            if( _health <= 0)
            {
                Death();
            }
        }
    }

    public int setHealth = 0;
    
    public delegate void DeathDelegate();
    public DeathDelegate death = null;

    public delegate void RemoveHealth(int health);
    public RemoveHealth removeHealth = null;

    #endregion

    private void Start()
    {
        if(setHealth != 0)
        {
            maxHealth = setHealth;
        }
        removeHealth += (int health) => { this.health -= health; };
    }

    void Death()
    {
        if(death != null)
        {
            death.Invoke();
        }
        gameObject.SetActive(false);
    }
    
    public void SubtractHealth(int health)
    {
        removeHealth(health);
    }
}
