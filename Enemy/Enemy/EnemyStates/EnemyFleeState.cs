using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/FleeState"), InlineEditor]
    public class EnemyFleeState : AState<Enemy>
    {
        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _owner.animator.speed = 0.1f / _owner.enemyData.FleeTravelTime;
            EscapeRoute myRoute;
            myRoute = new EscapeRoute(_owner.storedEscapeRoute.destination, _owner.storedEscapeRoute.curveHeight);
            _owner.storedEscapeRoute.destination = null;
            _owner.storedEscapeRoute.curveHeight = 0;
            _owner.StartStateCoroutine(Flee(myRoute.destination, myRoute.curveHeight, _owner.myEnemyPoint));
            _owner.fleeForceTimer = 0.0f;
        }

        public override void Exit()
        {
            _owner.fleeForceTimer = 0.0f;
            _owner.fleeCooldownTimer = 0.0f;
            _owner.animator.speed = 1.0f;
            _owner.transform.position = _owner.myEnemyPoint.transform.position;
        }

        public override bool CheckStateChanges()
        {
            return false;
        }

        public override void Tick(float deltaTime)
        {
            _owner.animator.speed = 0.1f;
        }

        public IEnumerator Flee(EnemyPoint destination, float arcHeight, EnemyPoint source)
        {
            Vector3 sourcePoint = source.transform.position;
            _owner.ClaimEnemyPoint(destination);

            float jumpTimer = 0.0f;
            float curvePosition = 0.0f;
            Vector3 nextCurveLocation = _owner.transform.position;
            float curveLengthY = destination.transform.position.y - _owner.transform.position.y;
            while (jumpTimer < _owner.enemyData.FleeTravelTime)
            {
                // Wrap things that update every frame in a pause-checker
                while (_owner.ShouldPauseCoroutines())
                {
                    yield return null;
                }
                jumpTimer += Time.deltaTime;
                curvePosition = jumpTimer / _owner.enemyData.FleeTravelTime;
                nextCurveLocation = Vector3.Lerp(sourcePoint, destination.transform.position, curvePosition);
                nextCurveLocation.y = sourcePoint.y + (curvePosition < 0.5f ? (curvePosition * 10 * arcHeight) + (curvePosition * curveLengthY) 
                    : ((1 - curvePosition) * 10 * arcHeight) + (curvePosition * curveLengthY));
                while (_owner.ShouldPauseCoroutines())
                {
                    yield return null;
                }
                _owner.transform.position = nextCurveLocation;
                yield return null;
            }
            _owner.transform.position = destination.transform.position;
            _owner.animator.speed = 1.0f;
            _owner.fleeCooldownTimer = 0.0f;
            _owner.animator.ResetTrigger("IsJumping");
            _owner.animator.SetTrigger("IsLanding");
            _owner._onRangedSniperLand.Raise(_owner.gameObject);
            _owner.EnemyStateMachine.ChangeState(_owner.initialState.GetType());
        }
    }
}
