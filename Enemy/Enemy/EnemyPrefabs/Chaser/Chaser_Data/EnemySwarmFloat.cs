using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class EnemySwarmFloat : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.5f;
        [SerializeField] private float frequency = 1f;

        Vector3 posOffset = new Vector3();
        Vector3 tempPos = new Vector3();

        void Start()
        {
            posOffset = transform.localPosition;
        }

        void Update()
        {
            // Float up/down with a Sin()
            tempPos = posOffset;
            tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

            transform.localPosition = tempPos;
        }
    }
}
