using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/AttackState"), InlineEditor]
    public class EnemyAttackState : AState<Enemy>
    {
        #region variables
        [SerializeField] private AState<Enemy> postAttackState;
        [SerializeField] public ProjectileData enemyMissileData;
        [SerializeField] private bool _usesLockSights;

        [SerializeField] private GameObjectGameEvent _onStartCharge;
        [SerializeField] private GameObjectGameEvent _onLaunchAttack;

        private float _lockOnTimer = 0.0f;
        private bool _isAttacking;
        private bool _hasAttacked;
        private float _attackAnimLength;
        #endregion

        public override bool CheckStateChanges()
        {
            if (_hasAttacked)
            {
                Parent.ChangeState(postAttackState.GetType());
                return true;
            }
            return false;
        }

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _hasAttacked = false;
            _isAttacking = false;
            _attackAnimLength = 0.0f;
        }

        public override void Exit()
        {
            if (_usesLockSights && (_isAttacking && !_hasAttacked))
            {
                _owner.InterruptAttack();
            }
            // Make sure animation is finished playing, I assume
        }

        public override void Tick(float deltaTime)
        {
            if (!_isAttacking && !_hasAttacked)
            {
                if (_usesLockSights)
                {
                    _owner.StartStateCoroutine(LockOnAttack());
                } else
                {
                    _owner.StartStateCoroutine(Attack());
                }
            }
        }

        public IEnumerator Attack()
        {
            _isAttacking = true;
            _onLaunchAttack.Raise(_owner.gameObject);
            while (PauseManager.Instance.Paused)
            {
                yield return null;
            }
            yield return new WaitForSeconds(_attackAnimLength);
            while (PauseManager.Instance.Paused)
            {
                yield return null;
            }
            Player.Instance.Damage(_owner.enemyData.AttackDamage, _owner);
            _isAttacking = false;
            _hasAttacked = true;
        }

        public IEnumerator LockOnAttack()
        {
            _isAttacking = true;
            RaycastHit objectHit;
            Vector3 toPlayer = Player.Instance.transform.position - _owner.transform.position;

            _onStartCharge.Raise(_owner.gameObject);

            // Loop until the sniper has kept the player in LOS for LockOnDelay seconds
            while (_lockOnTimer <= _owner.enemyData.LockOnDelay)
            {
                _owner.LookTowards();
                // do a visibility raycast
                toPlayer = Player.Instance.CameraHolder.transform.position - _owner.transform.position;
                if (Physics.Raycast(_owner.transform.position, toPlayer, out objectHit, _owner.enemyData.AttackRadius))
                {
                    // Only continute incrementing if the RayCast hits the player
                    if (objectHit.transform.CompareTag("Player") && !PauseManager.Instance.Paused)
                    {
                        _lockOnTimer += Time.deltaTime;
                        if (_lockOnTimer >= _owner.enemyData.LockOnDelay)
                        {
                            break;
                        }
                    }
                    else
                    {
                        _lockOnTimer = 0.0f;
                        _isAttacking = false;
                        _owner.InterruptAttack();
                        break;
                    }
                }
                yield return null;
            }

            // Determine which breakpoint was triggered
            if (_lockOnTimer >= _owner.enemyData.LockOnDelay)
            {
                // Enemy has locked-on and is launching attack
                _onLaunchAttack.Raise(_owner.gameObject);

                toPlayer = Player.Instance.CameraHolder.transform.position - _owner.transform.position;
                ProjectileManager.Instance.SpawnProjectile(enemyMissileData, _owner.transform.position, toPlayer, toPlayer.normalized, _owner);

                // Wrap the attackCooldown in "pause-menu catchers"
                while (_owner.ShouldPauseCoroutines())
                {
                    yield return null;
                }
                yield return new WaitForSeconds(_owner.enemyData.AttackCooldown);
                while (_owner.ShouldPauseCoroutines())
                {
                    yield return null;
                }

                // Lets the tick function know that the enemy is free to attack again
                _isAttacking = false;
                _hasAttacked = true;
            } else
            {
                _isAttacking = false;
            }
        }
    }
}