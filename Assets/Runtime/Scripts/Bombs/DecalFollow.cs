using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalFollow : MonoBehaviour
{
    public Transform followTr;

    private void Update()
    {
        transform.position = followTr.position;   
    }
}
