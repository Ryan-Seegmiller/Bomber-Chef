using Mono.CompilerServices.SymbolWriter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class Selector : Node
    {
        public Selector() : base() {  }

        public Selector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            foreach(Node child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Performing:
                        state = NodeState.Performing;
                        return state;
                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;
                    case NodeState.Failure:
                        continue;
                    default:
                        continue;
                }
            }
            state = NodeState.Failure;
            return state;
        }

    }
}
