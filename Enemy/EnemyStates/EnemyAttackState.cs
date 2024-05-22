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
        [SerializeField] private bool _usesLockSights;

        [SerializeField] private GameObjectGameEvent _onStartCharge;

        [SerializeField] RaycastData lockOnVFX;
        [HideInInspector] public float lockOnTimer = 0.0f;
        [HideInInspector] public float delayTimer = 0.0f;
        private bool _isAttacking;
        private bool _hasAttacked;
        private float _attackAnimLength;
        private IEnumerator _shrinkRoutine;
        #endregion

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
                StopVFX();
                _owner.InterruptAttack();
                _owner.StopAllCoroutines();
            } else if (_usesLockSights)
            {
                StopVFX();
                _owner.animator.ResetTrigger("IsCharging");
                _owner.animator.ResetTrigger("IsAttacking");
                _owner.animator.ResetTrigger("HasAttacked");
                _owner.animator.ResetTrigger("ChargeInterrupt");
            }
        }

        public override bool CheckStateChanges()
        {
            float distToPlayer = Vector3.Distance(_owner.transform.position, Player.Instance.transform.position);
            if (distToPlayer <= _owner.enemyData.FleeRadius)
            {
                _owner.FindEscapeRoute();
                if (_owner.storedEscapeRoute.destination is not null && _owner.storedEscapeRoute.destination.myEnemy is null)
                {
                    _owner._onRangedSniperStartJumpAnim.Raise(_owner.gameObject);
                    _owner.animator.SetTrigger("IsJumping");
                    return true;
                } else
                {
                    return false;
                }
            }
            else if (_hasAttacked || distToPlayer > _owner.enemyData.AttackRadius)
            {
                Parent.ChangeState(postAttackState.GetType());
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Tick(float deltaTime)
        {
            _owner.LookTowards();
            if (!_isAttacking && !_hasAttacked)
            {
                if (_usesLockSights)
                {
                    _isAttacking = true;
                    _owner.animator.ResetTrigger("IsCharging");
                    _owner.animator.ResetTrigger("IsAttacking");
                    _owner.animator.ResetTrigger("HasAttacked");
                    _owner.animator.ResetTrigger("ChargeInterrupt");
                    _owner.StartStateCoroutine(LockOnAttack());
                } else
                {
                    _owner.StartStateCoroutine(Attack());
                }
            }
        }

        public IEnumerator Attack()
        {
            _owner._onLaunchAttack.Raise(_owner.gameObject);
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
            lockOnTimer = 0.0f;
            _isAttacking = true;
            RaycastHit objectHit;
            Vector3 toPlayer = Player.Instance.transform.position - _owner.transform.position;

            _onStartCharge.Raise(_owner.gameObject);
            _owner.animator.SetTrigger("IsCharging");
            // _owner.animator.Play("Charge");
            StartChargeVFX();

            // Loop until the sniper has kept the player in LOS for LockOnDelay seconds
            while (lockOnTimer <= _owner.enemyData.LockOnDelay)
            {
                if (!PauseManager.Instance.Paused)
                {
                    StartChargeVFX();
                    // do a visibility raycast
                    toPlayer = Player.Instance.CameraHolder.transform.position - _owner._gunTip.position;
                    if (Physics.Raycast(_owner._gunTip.position, toPlayer, out objectHit, _owner.enemyData.AttackRadius, LayerMask.GetMask("Player", "Ground", "Thornwall")))
                    {
                        // Only continute incrementing if the RayCast hits the player
                        if (objectHit.transform.CompareTag("Player") && !PauseManager.Instance.Paused)
                        {
                            delayTimer = 0.0f;
                            lockOnTimer += Time.deltaTime;
                            Vector3 bestPlaceToAim = Player.Instance.CameraHolder.transform.position;

                            // THIS IS A MAGIC NUMBER REGARDING THE SNIPER AIMING LASER.
                            // I AM AWARE THAT THIS IS POOR PRACTICE AND YET I WILL
                            // STUBBORNLY CARRY ON DOWN THIS PATH TOWARDS RUIN - BCRA
                            bestPlaceToAim.y -= 1.0f;
                            AimingLaser(bestPlaceToAim);

                            if (lockOnTimer >= _owner.enemyData.LockOnDelay)
                            {
                                StopVFX();
                                _owner.animator.ResetTrigger("IsCharging");
                                break;
                            }
                        }
                        else
                        {
                            delayTimer += Time.deltaTime;
                            if (delayTimer > 0.5f)
                            {
                                // Debug.Log("Sniper has broken contact with player");
                                lockOnTimer = 0.0f;
                                _isAttacking = false;
                                // _owner.StopCoroutine(_shrinkRoutine);
                                _owner.InterruptAttack();
                                StopVFX();
                                _owner.animator.SetTrigger("ChargeInterrupt");
                                break;
                            }
                        }
                    }
                } else
                {
                    PauseVFX();
                }
                yield return null;
            }

            // Determine which breakpoint was triggered
            if (lockOnTimer >= _owner.enemyData.LockOnDelay)
            {
                _owner.animator.SetTrigger("IsAttacking");

                // Wrap the attackCooldown in "pause-menu catchers"
                while (_owner.ShouldPauseCoroutines())
                {
                    PauseVFX();
                    yield return null;
                }
                StartChargeVFX();
                yield return new WaitForSeconds(_owner.enemyData.AttackCooldown);
                while (_owner.ShouldPauseCoroutines())
                {
                    PauseVFX();
                    yield return null;
                }
                StartChargeVFX();

                Parent.ChangeState(postAttackState.GetType());
                _isAttacking = false;
                _hasAttacked = true;
            }
        }

        void StartChargeVFX()
        {
            Transform muzzleVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleChargeSniper");
            foreach (Transform child in muzzleVFX)
            {   
                if (!child.GetComponent<ParticleSystem>().isPlaying)
                {
                    child.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        public void StartShootVFX()
        {
            Transform shootVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleSniperShot");
            foreach (Transform child in shootVFX)
            {
                if (!child.GetComponent<ParticleSystem>().isPlaying)
                {
                    child.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        void PauseVFX()
        {
            Transform muzzleVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleChargeSniper");
            foreach (Transform child in muzzleVFX)
            {
                child.GetComponent<ParticleSystem>().Pause();
            }

            Transform shootVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleSniperShot");
            foreach (Transform child in shootVFX)
            {
                child.GetComponent<ParticleSystem>().Pause();
            }
        }

        public void StopVFX()
        {
            Transform muzzleVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleChargeSniper");
            foreach (Transform child in muzzleVFX)
            {
                child.GetComponent<ParticleSystem>().Stop();
            }

            Transform shootVFX = _owner.transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip/vfx_MuzzleSniperShot");
            foreach (Transform child in shootVFX)
            {
                child.GetComponent<ParticleSystem>().Stop();
            }
        }

        void AimingLaser(Vector3 aimPoint)
        {
            GameObject bulletTrailEffect = Instantiate(lockOnVFX.trail.gameObject, _owner._gunTip.position, Quaternion.identity);

            LineRenderer lineR = bulletTrailEffect.GetComponent<LineRenderer>();

            lineR.SetPosition(0, _owner._gunTip.position);
            lineR.SetPosition(1, aimPoint);

            Destroy(bulletTrailEffect, Time.deltaTime);
        }
    }
}