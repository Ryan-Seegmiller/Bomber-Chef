using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{

    public class Sequence : Node
    {
        public Sequence() : base() { }

        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool performing = false;
            foreach (Node child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Performing:
                        performing = true;
                        continue;
                    case NodeState.Success:
                        continue;
                    default:
                        return NodeState.Success;
                }
            }
            //None of the children failed
            return (performing)? NodeState.Performing : NodeState.Success;
        }
    
    }
}
