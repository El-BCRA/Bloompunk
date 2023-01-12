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
        [Space(10)]
        [Title("Scriptable Objects")]
        [SerializeField, InlineEditor]
        public EnemyData enemyData;
        [SerializeField, InlineEditor]
        public BoidData boidData;

        [Space(10)]
        [Title("State Machine Fields")]
        [SerializeField] private List<AState<Enemy>> enemyStates;
        [SerializeField] private AState<Enemy> initialState;
        [HideInInspector] public RootStateMachine<Enemy> EnemyStateMachine;

        [Space(10)]
        [Title("Movement Fields")]
        [SerializeField] public NavMeshAgent navMeshAgent;
        [SerializeField] public GameObject rotatablePortion;
        [HideInInspector] public Rigidbody enemyRigidBody;
        [HideInInspector] public BoidValues myBoidValues;
        [HideInInspector] public bool isBoid;

        [Space(10)]
        [Title("Health Fields")]
        [SerializeField] GameObject _healthBarUI;
        [SerializeField] Slider _healthBarSlider;
        [HideInInspector] float _currentHealth;
        [HideInInspector] public bool critWaiting = false;

        [Space(10)]
        [Title("Events")]
        [SerializeField] private GameObjectGameEvent _onEnemySpawn;
        [SerializeField] private GameObjectGameEvent _onEnemyDeath;
        [SerializeField] private GameObjectGameEvent _onCriticalHit;
        [SerializeField] public GameObjectGameEvent _onSpawnInterrupt;
        [SerializeField] public GameObjectGameEvent _onAttackInterrupt;
        [SerializeField] private GameEvent _onEnemySelfDestruct;
        [SerializeField] private GameEvent _onEnemyDeathByType;

        [Space(10)]
        [Title("Whatever TF A VisibilityCP Is")]
        [SerializeField] private GameObject visibilityCP;
        #endregion variables

        private void Awake()
        {
            if (boidData is not null)
            {
                myBoidValues = new BoidValues();
                myBoidValues.cachedTransform = transform;
                myBoidValues.position = myBoidValues.cachedTransform.position;
                myBoidValues.forward = myBoidValues.cachedTransform.forward;

                float startSpeed = (boidData.minSpeed + boidData.maxSpeed) / 2;
                myBoidValues.velocity = transform.forward * startSpeed;

                isBoid = true;
            }
        }

        private void Start()
        {
            enemyRigidBody = GetComponent<Rigidbody>();
            enemyRigidBody.isKinematic = true;
            navMeshAgent = GetComponent<NavMeshAgent>();

            _currentHealth = enemyData.MaxHealth;
            _healthBarSlider.value = CalculateHealth();

            // Instatiate the state machine and populate it with the given enemyStates
            // (which should be added in the inspector)
            EnemyStateMachine = new RootStateMachine<Enemy>("Enemy State", this);
            EnemyStateMachine.Initialize(enemyStates);
            EnemyStateMachine.ChangeState(initialState.GetType());
        }

        private void Update()
        {
            if (PauseManager.Instance.Paused)
            {
                // navMeshAgent.enabled = false;
                return;
            } else
            {
                // navMeshAgent.enabled = true;
                EnemyStateMachine.GetCurrentState().Tick(Time.deltaTime);
                EnemyStateMachine.GetCurrentState().CheckStateChanges();
            }

            _healthBarSlider.value = CalculateHealth();
            if (_currentHealth < enemyData.MaxHealth)
            {
                _healthBarUI.SetActive(true);
                // UIFacePlayer();
            }
        }

        public void LookTowards()
        {
            Vector3 relativePos = Player.Instance.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);

            Quaternion current = transform.localRotation;

            transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime
                * enemyData.RotationSpeed);
        }

        public void UIFacePlayer()
        {
            Vector3 relativePos = Player.Instance.transform.position - _healthBarUI.transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);

            Quaternion current = _healthBarUI.transform.localRotation;

            _healthBarUI.transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime
                * enemyData.RotationSpeed);
        }

        public void Damage(float dmgAmount, ACombatant source)
        {
            if (critWaiting == true)
            {
                _currentHealth -= dmgAmount * enemyData.CriticalHitModifier;
                // Debug.Log("Dealt " + (dmgAmount * enemyData.CriticalHitModifier) + " damage.");
                // Debug.Log("Normally would've dealt " + (dmgAmount) + " damage.");
                critWaiting = false;
            } else
            {
                _currentHealth -= dmgAmount;
            }

            if (_currentHealth <= 0 && EnemyStateMachine.GetCurrentState() is not EnemyDeathState)
            {
                EnemyStateMachine.ChangeState(typeof(EnemyDeathState));
            }
        }

        public void Spawn()
        {
            _onEnemySpawn.Raise(this.gameObject);
        }

        public void Die()
        {
            if (_currentHealth > 0)
            {
                _onEnemySelfDestruct.Raise();
            } else
            {
                _onEnemyDeathByType.Raise();
            }

            _onEnemyDeath.Raise(this.gameObject);

            if (isBoid)
            {
                BoidManager.Instance.boids.Remove(this);
            }

            Destroy(this.gameObject);
        }

        public float CurrentHealth()
        {
            return _currentHealth;
        }
        public void CriticalHit()
        {
            _onCriticalHit.Raise(transform.gameObject);
            critWaiting = true;
        }

        public void ActivateSlowed()
        {
            enemyData.MovementModifier = 0.25f;
        }

        public void DeactivateSlowed()
        {
            enemyData.MovementModifier = 1.0f;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.AttackRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyData.InteractionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.AlertRadius);
        }

        //to help states to use coroutine
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
            } else
            {
                return false;
            }
        }

        public void InterruptAttack()
        {
            _onAttackInterrupt.Raise(this.gameObject);
        }

        public void InterruptSpawn()
        {

        }

        public bool HasVisibilityCheckPoint() {
            return visibilityCP != null;
        }

        public GameObject GetVisibilityCheckPoint() {
            if (HasVisibilityCheckPoint()) {
                return visibilityCP;
            } else {
                return null;
            }
        }

        public float CalculateHealth()
        {
            return _currentHealth / enemyData.MaxHealth;
        }
    }
}
