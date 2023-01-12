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
        public override bool CheckStateChanges()
        {
            return false;
        }

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _onStartDeathAnim.Raise(_owner.gameObject);
            // Eventually there should be an animation coroutine of some kind here,
            // but for now enemies will just insta-die
            _owner.Die();
        }

        public override void Exit()
        {

        }

        public override void Tick(float deltaTime)
        {
            // Check if the death-animation is done, destroy self if yes
        }
    }
}
