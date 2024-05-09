using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class WandererTree : Tree
    {
        Transform[] waypoints;

        protected override Node SetupTree()
        {
            Node root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    //Sequence behavoirus

                })
                //Behaviours go here

            });
            return null;
        }
    }
}
