using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class TaskWander : Node
    {
        Transform transform;
        Transform[] waypoints;

        bool isWaiting;
        float waitCounter = 0;
        float waitDuration;

        int wayPointIndex = 0;

        float speed = 10f;


        public TaskWander(Transform transform, Transform[] waypoints)
        {
            this.transform = transform;
            this.waypoints = waypoints;
        }

        public override NodeState Evaluate()
        {
            if (isWaiting)
            {
                waitCounter += Time.fixedDeltaTime; // since we are not in a monobehaviour dont rely on the compiler conversion
                if(waitCounter >= waitDuration)
                {
                    isWaiting = false;
                }
                else
                {
                    if((transform.position - waypoints[wayPointIndex].position).magnitude < 0.01f)
                    {
                        transform.position = waypoints[wayPointIndex].position;
                        isWaiting = true;
                        wayPointIndex++;
                        wayPointIndex = (wayPointIndex + 1) % waypoints.Length;
                    }
                    else
                    {
                        transform.position = Vector3.MoveTowards(transform.position, waypoints[wayPointIndex].position, speed * Time.fixedDeltaTime);
                        transform.forward = (transform.position - waypoints[wayPointIndex].position).normalized;
                    }
                }

            }
            return NodeState.Performing;
        }
    }
}
