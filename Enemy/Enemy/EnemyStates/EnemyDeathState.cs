using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/DeathState"), InlineEditor]
    public class EnemyDeathState : AState<Enemy>
    {
        [SerializeField] private GameObjectGameEvent _onStartDeathAnim;

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _owner.enemyRigidBody.isKinematic = true;
            _onStartDeathAnim.Raise(_owner.gameObject);
            _owner.Die();
        }

        public override void Exit()
        {

        }

        public override bool CheckStateChanges()
        {
            return false;
        }

        public override void Tick(float deltaTime)
        {
            
        }
    }
}
