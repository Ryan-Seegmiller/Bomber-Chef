using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine
{
    public static class ColliderArrayExtension
    {
        public static Collider[] SortCollidersArray(this Collider[] colliders, Transform tr)
        {
            Vector3[] positions = new Vector3[colliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                positions[i] = colliders[i].transform.position;
            }

            //While not sorted sort the array based off of closest to farthest
            while (!IsArraySoreted(positions, tr))
            {
                for (int i = 0; i < positions.Length - 1; i++)
                {
                    if ((positions[i + 1] - tr.position).magnitude < (positions[i] - tr.position).magnitude)
                    {
                        var pos = positions[i];
                        positions[i] = positions[i + 1];
                        positions[i + 1] = pos;
                        Collider col = colliders[i];
                        colliders[i] = colliders[i + 1];
                        colliders[i + 1] = col;
                    }
                }
            }
            return colliders;
        }
        static bool IsArraySoreted(Vector3[] positions, Transform tr)
        {
            //Checks if the array is sorted
            for (int i = 0; i < positions.Length - 1; i++)
            {
                if ((positions[i + 1] - tr.position).magnitude < (positions[i] - tr.position).magnitude)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
