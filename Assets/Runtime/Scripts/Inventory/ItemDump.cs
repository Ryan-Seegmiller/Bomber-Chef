using Collectible;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDump : MonoBehaviour
{
    public const int itemDumpID = 278;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 13)
        {
            other.gameObject.GetComponent<ItemHelper>().onPickUp.Invoke();
        }
    }
}
