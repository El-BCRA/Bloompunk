using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/KnockbackState"), InlineEditor]
    public class EnemyKnockbackState : AState<Enemy>
    {
        private bool _inHitStun;
        private bool _hasCompletedStun;

        public override bool CheckStateChanges()
        {
            if (_hasCompletedStun)
            {
                Parent.ChangeState(typeof(EnemyIdleState));
                return true;
            }
            return false;
        }

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            // Interrupt any relevant coroutines
            _owner.StopAllCoroutines();
            // Disable NavMesh agent & enable physics on the rigidBody
            // _owner.navMeshAgent.enabled = false;
            _owner.enemyRigidBody.isKinematic = false;
            _inHitStun = false;
            _hasCompletedStun = false;
        }

        public override void Exit()
        {
            // _owner.navMeshAgent.enabled = true;
            _owner.enemyRigidBody.isKinematic = true;
        }

        public override void Tick(float deltaTime)
        {
            if (!_inHitStun && !_hasCompletedStun)
            {
                _owner.StartStateCoroutine(HitStun());
            }
        }

        public IEnumerator HitStun()
        {
            _inHitStun = true;
            yield return new WaitForSeconds(_owner.enemyData.HitStunLength);
            _inHitStun = false;
            _hasCompletedStun = true;
        }
    }
}
