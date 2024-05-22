using System;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using Utilities.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Bloompunk
{
    // Information for meleeChasers(hornets). Exists here for ease of access.
    public struct BoidValues
    {
        // Only modified by the BoidManager
        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 forward;
        [HideInInspector]
        public Vector3 velocity;

        // Modified by native functions and the BoidManager
        [HideInInspector]
        public Vector3 acceleration;
        [HideInInspector]
        public Vector3 avgFlockHeading;
        [HideInInspector]
        public Vector3 avgAvoidanceHeading;
        [HideInInspector]
        public Vector3 centreOfFlockmates;
        [HideInInspector]
        public int numPerceivedFlockmates;

        // Modified by all chase events
        [HideInInspector]
        public Transform cachedTransform;
    }

    public class Enemy : MonoBehaviour, ACombatant
    {
        #region variables
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
        [Space(10)]
        private string ad = "Tutorial";
        [FoldoutGroup("$ad")][SerializeField] private bool isTutorialEnemy;

        [Space(10)]
        private string ae = "Scriptable Objects";
        [FoldoutGroup("$ae")][SerializeField, InlineEditor] public EnemyData enemyData;
        [FoldoutGroup("$ae")][SerializeField, InlineEditor] public BoidData boidData;
        [FoldoutGroup("$ae")][SerializeField, InlineEditor] public ProjectileData enemyMissileData;

        [Space(10)]
        private string af = "State Machine Fields";
        [FoldoutGroup("$af")][HideInInspector] public RootStateMachine<Enemy> EnemyStateMachine;
        [FoldoutGroup("$af")][SerializeField] private List<AState<Enemy>> enemyStates;
        [FoldoutGroup("$af")] public AState<Enemy> initialState;
        [PropertyTooltip("Should be undefined for all non-sniper enemies.")]
        [FoldoutGroup("$af")] public AState<Enemy> fleeState;

        [Space(10)]
        private string ag = "Movement Fields";
        [FoldoutGroup("$ag")] public GameObject rotatablePortion;
        [FoldoutGroup("$ag")][HideInInspector] public Rigidbody enemyRigidBody;
        [FoldoutGroup("$ag")][HideInInspector] public BoidValues myBoidValues;
        [FoldoutGroup("$ag")][HideInInspector] public bool isBoid;
        [FoldoutGroup("$ag")][HideInInspector] public float fleeForceTime;
        [FoldoutGroup("$ag")][HideInInspector] public float fleeForceTimer;
        [FoldoutGroup("$ag")][HideInInspector] public float fleeCooldownTime;
        [FoldoutGroup("$ag")][HideInInspector] public float fleeCooldownTimer;
        [FoldoutGroup("$ag")][HideInInspector] public float attackCooldown;
        [HideInInspector]

        [Space(10)]
        private string ah = "Health Fields";
        [FoldoutGroup("$ah")][SerializeField] public GameObject _healthBarUIPrefab;
        [FoldoutGroup("$ah")] private GameObject _healthBarUI;
        //prefab that handles disintegrate behaviour
        [FoldoutGroup("$ah")][SerializeField] private GameObject _disintegratePrefab;
        [FoldoutGroup("$ah")][SerializeField] private GameObject _disintegrateParentTransformReference;
        [FoldoutGroup("$ah")] private ScreenSpaceEnemyHealthBar _SSHealthBar;
        [FoldoutGroup("$ah")][HideInInspector] float _currentHealth;
        [FoldoutGroup("$ah")][HideInInspector] public bool critWaiting = false;

        [Space(10)]
        private string ac = "Game Events";
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onEnemySpawn;
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onEnemyDeath;
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onEnemyDeathByType;
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onEnemySelfDestruct;
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onEnemyDespawnByType;
        [FoldoutGroup("$ac")][SerializeField] public GameObjectGameEvent _onRangedSniperStartJumpAnim;
        [FoldoutGroup("$ac")][SerializeField] public GameObjectGameEvent _onRangedSniperJump;
        [FoldoutGroup("$ac")][SerializeField] public GameObjectGameEvent _onRangedSniperLand;
        [FoldoutGroup("$ac")][SerializeField] private Vector3GameEvent _onEnemyDeathByTypeV3;
        [FoldoutGroup("$ac")][SerializeField] private Vector3GameEvent _onEnemySelfDestructV3;
        [FoldoutGroup("$ac")][SerializeField] private Vector4GameEvent _onEnemyHit;
        [FoldoutGroup("$ac")][SerializeField] private Vector4GameEvent _onEnemyCriticalHit;
        [FoldoutGroup("$ac")][SerializeField] public GameObjectGameEvent _onLaunchAttack;
        [FoldoutGroup("$ac")][SerializeField] private GameObjectGameEvent _onCriticalHit;
        [FoldoutGroup("$ac")][SerializeField] private GameEvent _onDespawnEnemies;
        [FoldoutGroup("$ac")] public GameObjectGameEvent _onAttackInterrupt;
        [FoldoutGroup("$ac")] public GameObjectGameEvent _onSpawnInterrupt;

        [Space(10)]
        private string ai = "Enemy Points";
        [InfoBox("The field myEnemyPoint should always be defined, whereas mySpawnPoint will " +
            "only be defined in the instances where myEnemyPoint is also a SpawnPoint")]
        [FoldoutGroup("$ai")] public EnemyPoint myEnemyPoint;
        [FoldoutGroup("$ai")] public SpawnPoint mySpawnPoint;
        [FoldoutGroup("$ai")] [HideInInspector] public EscapeRoute storedEscapeRoute;

        [Space(10)]
        private string aj = "Animations";
        [PropertyTooltip("Should be undefined for all non-sniper enemies. Auto-assigns on Awake().")]
        public Transform _gunTip;
        [FoldoutGroup("$aj")] public Animator animator;
        [FoldoutGroup("$aj")] private float animatorSpeed = 0.0f;

        [Space(10)]
        private string ak = "Visibility Checkpoint";
        [FoldoutGroup("$ak")][SerializeField] private GameObject visibilityCP;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
        #endregion variables

        private void Awake()
        {
            // Add a listener to the abilityHUD event
            _onDespawnEnemies.AddListener(LevelClearDestroy);
        }

        private void Start()
        {
            fleeCooldownTime = enemyData.FleeCooldownTime;
            fleeCooldownTimer = 15.0f;
            fleeForceTime = enemyData.ForcedFleeTime;
            fleeForceTimer = 0.0f;

            float cooldownAdjustment = UnityEngine.Random.Range(-1.0f * enemyData.FleeVariance, enemyData.FleeVariance);
            fleeCooldownTime += cooldownAdjustment;
            fleeForceTime += 2 * cooldownAdjustment;

            if (boidData is not null)
            {
                // Boid (hornet) initialization fields
                myBoidValues = new BoidValues();
                myBoidValues.cachedTransform = transform;
                myBoidValues.position = myBoidValues.cachedTransform.position;
                myBoidValues.forward = myBoidValues.cachedTransform.forward;

                float startSpeed = (boidData.minSpeed + boidData.maxSpeed) / 2;
                myBoidValues.velocity = transform.forward * startSpeed;

                isBoid = true;

                EnemyManager.Instance.AddChaser();

                Spawn();
            }
            else if (_onSpawnInterrupt is not null)
            {
                // Enemy is a spawner
                EnemyManager.Instance.AddSpawner();
                animatorSpeed = animator.speed;
            }
            else
            {
                // Enemy is a sniper
                if (transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip") is not null)
                {
                    _gunTip = transform.Find("RIG_SniperbugRetopology/Bug_Gun/GunTip");
                }
                storedEscapeRoute = new EscapeRoute(null, 0);
                EnemyManager.Instance.AddSniper();
                animatorSpeed = animator.speed;
            }

            // General enemy initialization field
            _currentHealth = enemyData.MaxHealth;
            enemyRigidBody = GetComponent<Rigidbody>();
            enemyRigidBody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;

            // Instatiate the state machine and populate it with the given enemyStates
            // (which should be added in the inspector)
            EnemyStateMachine = new RootStateMachine<Enemy>("Enemy State", this);
            EnemyStateMachine.Initialize(enemyStates);
            EnemyStateMachine.ChangeState(initialState.GetType());
        }

        private void Update()
        {
            if (!PauseManager.Instance.Paused)
            {
                EnemyStateMachine.GetCurrentState().Tick(Time.deltaTime);
                EnemyStateMachine.GetCurrentState().CheckStateChanges();

                // Enemy is a sniper
                if (fleeState is not null)
                {
                    transform.Find("RIG_SniperbugRetopology").transform.localPosition = new Vector3(0.0f, transform.Find("RIG_SniperbugRetopology").transform.localPosition.y, 0.0f);
                    animator.speed = animatorSpeed;
                    fleeForceTimer += Time.deltaTime;
                    fleeCooldownTimer += Time.deltaTime;
                    if (fleeForceTimer > fleeForceTime)
                    {
                        FindEscapeRoute();
                        if (storedEscapeRoute.destination is not null && storedEscapeRoute.destination.myEnemy is null)
                        {
                            _onRangedSniperStartJumpAnim.Raise(this.gameObject);
                            animator.SetTrigger("IsJumping");
                            fleeForceTimer = 0.0f;
                        }
                    }
                } else if (!isBoid)
                {
                    animator.speed = animatorSpeed;
                    animatorSpeed = animator.speed;
                }
            } else
            {
                if (!isBoid)
                {
                    animator.speed = 0.0f;
                    if (_onSpawnInterrupt is not null && EnemyStateMachine.CurrentState.GetType() == typeof(EnemySpawnState))
                    {
                        EnemySpawnState mine = (EnemySpawnState) EnemyStateMachine.GetCurrentState();
                        mine.PauseVFX();
                    }
                }
            }

            // Claims the current spawnPoint (if this enemy has recently spawned)
            if (myEnemyPoint is not null)
            {
                if (myEnemyPoint is SpawnPoint && mySpawnPoint is null)
                {
                    mySpawnPoint = (SpawnPoint)myEnemyPoint;
                }
            }

            // Ensures that tutorial enemys also turn to face the player.
            if (isTutorialEnemy)
            {
                LookTowards();
            }
        }

        private void OnDestroy()
        {
            _onEnemyDespawnByType.Raise(this.gameObject);
            _onDespawnEnemies.RemoveListener(LevelClearDestroy);
            if (_healthBarUI != null)
            {
                Destroy(_healthBarUI);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.AttackRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.AlertRadius);
        }

        public void LevelClearDestroy()
        {
            StopAllCoroutines();

            if (this.gameObject == null) return;

            // Update the relevant singletons
            if (isBoid)
            {
                BoidManager.Instance.boids.Remove(this);
                EnemyManager.Instance.RemoveChaser();
            }
            else if (_onSpawnInterrupt is not null)
            {
                EnemyManager.Instance.RemoveSpawner();
                ClearSpawn();
            }
            else
            {
                EnemyManager.Instance.RemoveSniper();
                ClearSpawn();
            }

            Destroy(this.gameObject);
        }

        public bool HasVisibilityCheckPoint()
        {
            return visibilityCP != null;
        }

        public GameObject GetVisibilityCheckPoint()
        {
            if (HasVisibilityCheckPoint())
            {
                return visibilityCP;
            }
            else
            {
                return gameObject;
            }
        }

        public float CalculateHealth()
        {
            return _currentHealth / enemyData.MaxHealth;
        }

        public void Damage(float dmgAmount, ACombatant source)
        {
            // Activate healthbar UI (if not already active from previous combat)
            if (_healthBarUI == null && _healthBarUIPrefab != null)
            {
                _healthBarUI = Instantiate(_healthBarUIPrefab);
                _SSHealthBar = _healthBarUI.GetComponent<ScreenSpaceEnemyHealthBar>();
                _SSHealthBar.followTarget = gameObject;
                if (_healthBarUI != null && Player.Instance != null)
                    _healthBarUI.transform.SetParent(Player.Instance.enemyHealthBarCanvas.transform);
            }

            // Apply critical hit modifier data
            if (critWaiting == true)
            {
                int critDmg = (int)(dmgAmount * enemyData.CriticalHitModifier);
                _currentHealth -= critDmg;

                // Add critical hit damage UI number
                Vector4 posDamage = new Vector4(transform.position.x, transform.position.y, transform.position.z, critDmg);
                if (_onEnemyCriticalHit != null) _onEnemyCriticalHit.Raise(posDamage);

                critWaiting = false;
            }
            else
            {
                _currentHealth -= dmgAmount;

                // Add standard damage UI number
                Vector4 posDamage = new Vector4(transform.position.x, transform.position.y, transform.position.z, dmgAmount);
                if (_onEnemyHit != null)
                {
                    Debug.Log("Enemy Hit");
                    _onEnemyHit.Raise(posDamage);
                }
            }

            if (_currentHealth <= 0 && EnemyStateMachine.GetCurrentState() is not EnemyDeathState)
            {
                StopAllCoroutines();
                EnemyStateMachine.ChangeState(typeof(EnemyDeathState));
            }


            if (_SSHealthBar != null)
            {
                _SSHealthBar.setHealthSlider(CalculateHealth());
                _SSHealthBar.resetCountDown();
            }
        }

        public void Die()
        {
            if (_currentHealth > 0)
            {
                // This is a hornet self-destructing
                _onEnemySelfDestruct.Raise(this.gameObject);
                _onEnemySelfDestructV3.Raise(transform.position);
            }
            else
            {
                // This is any enemy being killed by the player
                _onEnemyDeathByType.Raise(this.gameObject);
                _onEnemyDeathByTypeV3.Raise(transform.position);
            }

            // Regardless of circumstance, this is an enemy that is dying
            _onEnemyDeath.Raise(this.gameObject);

            // Update the relevant singletons
            if (isBoid)
            {
                BoidManager.Instance.boids.Remove(this);
                EnemyManager.Instance.RemoveChaser();
            }
            else if (_onSpawnInterrupt is not null)
            {
                EnemyManager.Instance.RemoveSpawner();
                ClearSpawn();
            }
            else
            {
                EnemyManager.Instance.RemoveSniper();
                ClearSpawn();
            }

            // Destroy things
            //spawn disintegrate object
            //_disintegratePrefab
            if (_disintegratePrefab != null)
            {
                Instantiate(_disintegratePrefab, _disintegrateParentTransformReference.transform.position, _disintegrateParentTransformReference.transform.rotation);
            }
            //destroy healthbar
            if (_healthBarUI != null)
            {
                Destroy(_healthBarUI);
            }
            Destroy(this.gameObject);
        }

        public void LookTowards()
        {
            Vector3 relativePos = Player.Instance.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);

            Quaternion current = transform.localRotation;

            transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime
                * enemyData.RotationSpeed);

            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }

        public void Spawn()
        {
            _onEnemySpawn.Raise(this.gameObject);
        }

        public void ClearSpawn()
        {
            if (mySpawnPoint is not null)
            {
                mySpawnPoint.ClearSpawn();
            }
            else if (myEnemyPoint is not null)
            {
                myEnemyPoint.ClearSpawn();
            }
            mySpawnPoint = null;
            myEnemyPoint = null;
        }

        public float CurrentHealth()
        {
            return _currentHealth;
        }

        public void CriticalHit()
        {
            critWaiting = true;
            _onCriticalHit.Raise(transform.gameObject);
        }

        public void InterruptAttack()
        {
            animator.SetTrigger("ChargeInterrupt");
            _onAttackInterrupt.Raise(this.gameObject);
        }

        public void InterruptSpawn()
        {
            /* DEFINE THIS FUNCTION YOU LAZY FUCK */
        }

        public void ActivateSlowed()
        {
            enemyData.MovementModifier = enemyData.ThornwallSlow;
        }

        public void DeactivateSlowed()
        {
            enemyData.MovementModifier = 1.0f;
        }

        public void FindEscapeRoute()
        {
            if (fleeCooldownTimer < fleeCooldownTime)
            {
                storedEscapeRoute = new EscapeRoute(null, 0);
            } else if (!myEnemyPoint.hasBeenPopulated)
            {
                myEnemyPoint.PopulateAccessibleJumpPoints();
            }

            storedEscapeRoute = myEnemyPoint.FindAJumpPoint();
        }

        public void ClaimEnemyPoint(EnemyPoint newPoint)
        {
            if (!isBoid)
            {
                ClearSpawn();
                myEnemyPoint = newPoint;
                newPoint.myEnemy = this;
                if (_onSpawnInterrupt is not null)
                {
                    newPoint.myEnemyType = EnemyPoint.heldEnemy.Spawner;
                }
                else
                {
                    newPoint.myEnemyType = EnemyPoint.heldEnemy.Sniper;
                }
                EnemyPointManager.Instance.openJumpPoints.Remove(newPoint);
            }
        }

        public void PlayAnimation(String animation)
        {
            animator.Play(animation);
        }

        public float ChargePercent()
        {
            if (EnemyStateMachine.GetCurrentState() is not EnemyAttackState)
            {
                return -1.0f;
            }
            else
            {
                EnemyAttackState currentState = (EnemyAttackState)EnemyStateMachine.CurrentState;
                if (currentState.lockOnTimer <= 0.0f)
                {
                    return -1.0f;
                }
                else
                {
                    return currentState.lockOnTimer / enemyData.LockOnDelay;
                }

            }
        }

        // To help states to use coroutine
        public void StartStateCoroutine(IEnumerator coroutineMethod)
        {
            StartCoroutine(coroutineMethod);
        }

        // Call to check if the game is in a state where code execution should 
        // be paused (AKA, during menus, transitions, etc.)
        public bool ShouldPauseCoroutines()
        {
            if (PauseManager.Instance.Paused)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
