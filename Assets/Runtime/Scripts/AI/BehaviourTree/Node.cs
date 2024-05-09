using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace BehaviourTree
{
    public class Node
    {
        protected NodeState state;
        protected List<Node> children = new List<Node>();
        Node parent = null;//this allows for some backtracking

        Dictionary<string, object> data = new Dictionary<string, object>();

        public Node()
        {
            parent = null;
        }
        public Node(List<Node> children)
        {
            foreach(Node node in children)
            {
                Attach(node);
            }
        }
        
        public void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        //each derived type can make their own evaluate method
        public virtual NodeState Evaluate() => NodeState.Failure;

        public void AddData(string key, object value)
        {
            data.Add(key, value);
        }
        public object GetData(string key)
        {
            object value = null;

            if(data.TryGetValue(key, out value))
            {
                return value;
            }
            Node parent = this.parent;

            while(parent != null)
            {
                value = parent.GetData(key);
                if(value != null)
                {
                    return value;
                }
                parent = parent.parent;//Gets the parent of the parent
            }

            return null;
        }

        public bool RemoveData(string key)
        {
            bool dataErased = false;

            if (data.ContainsKey(key))
            {
                data.Remove(key);
                return true;
            }
            Node parent = this.parent;

            while (parent != null)
            {
                dataErased = parent.RemoveData(key);

                if (dataErased)
                {
                    return dataErased;
                }
                
                parent = parent.parent;//Gets the parent of the parent
            }
            return dataErased;
        }
    }

    public enum NodeState
    {
        Performing, Success, Failure
    }
}
