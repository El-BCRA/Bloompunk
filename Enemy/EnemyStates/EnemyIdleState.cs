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

        public override bool CheckStateChanges()
        {
            _distToPlayer = Vector3.Distance(_owner.transform.position, Player.Instance.transform.position);

            if (_distToPlayer <= _owner.enemyData.AlertRadius)
            {
                Parent.ChangeState(postIdleState.GetType());
                return true;
            }
            return false;
        }

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _distToPlayer = Vector3.Distance(_owner.transform.position, Player.Instance.transform.position);
        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
            // Any idle animations or behaviors go here
        }
    }
}