using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Tree : MonoBehaviour
    {
        private Node root = null;



        void Start()
        {
            SetupTree();
        }

        void FixedUpdate()
        {
            if(root != null)
            {
                root.Evaluate();
            }
        }
        protected abstract Node SetupTree();
        
    }
    
}
