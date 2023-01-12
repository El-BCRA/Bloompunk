using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class BossCritSpot : MonoBehaviour
    {
        BossCritManager manager;
        public enum Phases
        {
            phase1, phase2, phase3, core
        };
        [SerializeField] Phases phase;

        // Start is called before the first frame update
        void Start()
        {
            manager = FindObjectOfType<BossCritManager>();

            switch (phase)
            {
                case Phases.phase1:
                    manager.AddToGroup(1);
                    break;
                case Phases.phase2:
                    manager.AddToGroup(2);
                    break;
                case Phases.phase3:
                    manager.AddToGroup(3);
                    break;
            }
        }


        private void OnDestroy()
        {
            switch (phase)
            {
                case Phases.phase1:
                    manager.DeleteFromGroup(1);
                    break;
                case Phases.phase2:
                    manager.DeleteFromGroup(2);
                    break;
                case Phases.phase3:
                    manager.DeleteFromGroup(3);
                    break;
                case Phases.core:
                    manager.CoreDestroyed();
                    break;
            }
        }

    }
}

