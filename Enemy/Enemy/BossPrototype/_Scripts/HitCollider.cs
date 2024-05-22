using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class HitCollider : MonoBehaviour
    {
        Slam slam;

        private void Start()
        {
            slam = FindObjectOfType<Slam>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                slam.insideHitCollider = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                slam.insideHitCollider = false;
            }
        }
    }
}

