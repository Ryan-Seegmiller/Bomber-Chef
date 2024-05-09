using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    public delegate void RemoveHeart();
    public RemoveHeart removeHeart = null;

    public void RemoveThisHeart()
    {
        removeHeart.Invoke();
    }
}
