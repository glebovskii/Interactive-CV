using UnityEngine;

namespace Dissolver.Demo
{
    public class CameraMover : MonoBehaviour
    {
        public float moveSpeed = 2f;
        private bool shouldMove = false;
        private Vector3 targetPosition;
        public void MoveTo(ExampleHolder exampleHolder)
        {
            targetPosition = new Vector3(exampleHolder.Centre.x, transform.position.y, transform.position.z);
            shouldMove = true;
        }

        void Update()
        {
            if (shouldMove)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.01f)
                {
                    transform.position = targetPosition;
                    shouldMove = false;
                }
            }
        }
    }
}