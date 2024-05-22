using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Bloompunk
{
    public class BossCritManager : MonoBehaviour
    {
        [ReadOnly] int phase1CritSpots;
        [ReadOnly] int phase2CritSpots;
        [ReadOnly] int phase3CritSpots;

        [SerializeField] GameObject firstFloor;
        [SerializeField] GameObject secondFloor;
        [SerializeField] GameObject bossObject;
        Slam boss;

        private void Start()
        {
            boss = FindObjectOfType<Slam>();
        }

        public void AddToGroup(int phase)
        {
            switch (phase)
            {
                case 1:
                    phase1CritSpots++;
                    break;
                case 2:
                    phase2CritSpots++;
                    break;
                case 3:
                    phase3CritSpots++;
                    break;
            }
        }

        public void DeleteFromGroup(int phase)
        {
            switch (phase)
            {
                case 1:
                    phase1CritSpots--;
                    if (phase1CritSpots == 0)
                    {
                        Destroy(firstFloor);
                    }
                    break;
                case 2:
                    phase2CritSpots--;
                    if(phase2CritSpots == 0)
                    {
                        Destroy(secondFloor);
                    }
                    break;
                case 3:
                    phase3CritSpots--;
                    if (phase3CritSpots == 0)
                    {
                        boss.StartSlamAttack();
                    }
                    break;
            }
        }

        public void CoreDestroyed()
        {
            Destroy(bossObject);
        }
    }
}

