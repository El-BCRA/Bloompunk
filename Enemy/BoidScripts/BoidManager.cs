using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.StateMachine;
using UnityEngine;

namespace Bloompunk
{
    public class BoidManager : MonoBehaviour
    {
        public static BoidManager Instance;

        const int threadGroupSize = 1024;

        // public MeleeChaserData settings;
        public ComputeShader compute;
        public List<Enemy> boids;
        [SerializeField] GameObject target;

        void Awake()
        {
            if (Instance == null || Instance.Equals(null))
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(transform.parent.gameObject);
            }
        }

        private void Start()
        {
            OnLevelStart();
        }

        public void OnLevelStart()
        {
            boids.Clear();
        }

        void Update()
        {
            if (boids != null && boids.Count > 0)
            {

                int numBoids = boids.Count;
                var boidData = new BoidData[numBoids];

                for (int i = 0; i < boids.Count; i++)
                {
                    boidData[i].position = boids[i].myBoidValues.position;
                    boidData[i].direction = boids[i].myBoidValues.forward;
                }

                var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
                boidBuffer.SetData(boidData);

                compute.SetBuffer(0, "boids", boidBuffer);
                compute.SetInt("numBoids", boids.Count);
                compute.SetFloat("viewRadius",  boids[0].boidData.perceptionRadius);
                compute.SetFloat("avoidRadius", boids[0].boidData.avoidanceRadius);

                int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
                compute.Dispatch(0, threadGroups, 1, 1);

                boidBuffer.GetData(boidData);

                for (int i = 0; i < boids.Count; i++)
                {
                    if (PauseManager.Instance.Paused)
                    {
                        boids[i].enemyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    else
                    {
                        boids[i].enemyRigidBody.constraints = RigidbodyConstraints.None;
                        boids[i].myBoidValues.avgFlockHeading = boidData[i].flockHeading;
                        boids[i].myBoidValues.centreOfFlockmates = boidData[i].flockCentre;
                        boids[i].myBoidValues.avgAvoidanceHeading = boidData[i].avoidanceHeading;
                        boids[i].myBoidValues.numPerceivedFlockmates = boidData[i].numFlockmates;

                        AState<Enemy> tempState = boids[i].EnemyStateMachine.GetCurrentState();
                        if (tempState is EnemyChaseState)
                        {
                            EnemyChaseState boidChaseState = (EnemyChaseState)tempState;
                            boidChaseState.UpdateBoid();
                        }
                        // boids[i].EnemyStateMachine.GetCurrentState().CheckStateChanges();
                    }
                }
                boidBuffer.Release();
            }

        }

        public struct BoidData
        {
            public Vector3 position;
            public Vector3 direction;

            public Vector3 flockHeading;
            public Vector3 flockCentre;
            public Vector3 avoidanceHeading;
            public int numFlockmates;

            public static int Size
            {
                get
                {
                    return sizeof(float) * 3 * 5 + sizeof(int);
                }
            }
        }
    }
}