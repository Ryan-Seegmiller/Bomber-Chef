using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityEngine
{
    public static class RigidBodyExtension
    {
       public static void Stop(this Rigidbody rb)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
