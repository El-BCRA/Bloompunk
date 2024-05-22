using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/IdleState"), InlineEditor]
    public class EnemyIdleState : AState<Enemy>
    {
        [SerializeField] private AState<Enemy> postIdleState;
        private float _distToPlayer;

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _distToPlayer = Vector3.Distance(_owner.transform.position, Player.Instance.transform.position);
        }

        public override void Exit()
        {

        }

        public override bool CheckStateChanges()
        {
            _distToPlayer = Vector3.Distance(_owner.transform.position, Player.Instance.transform.position);

            // Account for the potential case where an enemy transitions directly from Idle to Flee
            if (_distToPlayer <= _owner.enemyData.FleeRadius)
            {
                _owner.FindEscapeRoute();
                if (_owner.storedEscapeRoute.destination is not null)
                {
                    _owner._onRangedSniperStartJumpAnim.Raise(_owner.gameObject);
                    _owner.animator.SetTrigger("IsJumping");
                    return true;
                }
            }
            else if (_distToPlayer <= _owner.enemyData.AttackRadius)
            {
                Parent.ChangeState(postIdleState.GetType());
                return true;
            }
            return false;
        }

        public override void Tick(float deltaTime)
        {
            _owner.LookTowards();
        }
    }
}