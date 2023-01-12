using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ScriptableObjectArchitecture;

namespace Bloompunk
{
    public class CritSpot : MonoBehaviour
    {
        [SerializeField] public Enemy parent;

        private void Awake()
        {
            if (parent is null)
            {
                parent = GetComponentInParent<Enemy>();
            }
        }
    }
}
