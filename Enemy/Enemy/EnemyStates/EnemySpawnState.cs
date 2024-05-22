using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/SpawnState"), InlineEditor]
    public class EnemySpawnState : AState<Enemy>
    {
        [SerializeField] private GameObject _meleeChaserPrefab;
        [SerializeField] private List<Transform> _spawnLocations;
        [SerializeField] private float _spawnAmount = 2.0f;
        [SerializeField] private GameObjectGameEvent _onStartSpawnAnimation;
        private float initialSpawnDelayAmount;
        private float initialSpawnDelay = 0.0f;
        private float spawnTimer = 0.0f;

        private bool _isSpawning;
        private bool _hasSpawned;
        [HideInInspector] public bool canSpawn = true;

        public override void Enter()
        {
            initialSpawnDelayAmount = UnityEngine.Random.Range(3.0f, 6.0f);
            _owner = Parent.GetOwner();
            _spawnLocations.Clear();
            Transform spawnpoints = _owner.transform.Find("Spawnpoints");
            for (int i = 0; i < spawnpoints.childCount; i++)
            {
                _spawnLocations.Add(spawnpoints.GetChild(i));
            }
            _isSpawning = false;
            _hasSpawned = false;
        }

        public override void Exit()
        {
            StopVFX();
            _spawnLocations.Clear();
            if (_isSpawning && !_hasSpawned)
            {
                _owner._onSpawnInterrupt.Raise(_owner.gameObject);
            }
            // Make sure animation is finished playing, I assume
        }

        public override bool CheckStateChanges()
        {
            if (_hasSpawned)
            {
                Parent.ChangeState(typeof(EnemyIdleState));
                return true;
            }
            return false;
        }

        public override void Tick(float deltaTime)
        {
            spawnTimer += deltaTime;
            if (initialSpawnDelay < initialSpawnDelayAmount)
            {
                initialSpawnDelay += deltaTime;
                StopVFX();
            } 
            else
            {
                if (canSpawn && !_isSpawning)
                {
                    StartWindupVFX();
                }
            }
            // Call LookTowards() unless the enemy is in SpawnCooldown
            // (AKA, _hasSpawned && !_isSpawning, which is not an accesible 
            // variable state in the current implementation)
            _owner.LookTowards();
            if (EnemyManager.Instance.numEnemies < SpawnManager.Instance.currentEnemyLevelData.enemyLimit
                && canSpawn && initialSpawnDelay >= initialSpawnDelayAmount && !_isSpawning && !_hasSpawned && EnemyManager.Instance.numChasers 
                < LevelManager.Instance.currentLevelData.enemyConfiguration.maxChasers  && spawnTimer > _owner.enemyData.AttackCooldown)
            {
                canSpawn = false;
                _onStartSpawnAnimation.Raise(_owner.gameObject);
                _isSpawning = true;
                _owner.PlayAnimation("Spawning");
                // _owner.StartStateCoroutine(SpawnAnimation());
            }
        }

        public void Spawn()
        {
            StopVFX();
            int spawnIndex;
            // Magic number that can be made adjustable depending on future changes to
            // enemy-spawning difficulty scaling
            for (int i = 0; i < _spawnAmount; i++)
            {
                spawnIndex = Random.Range(0, _spawnLocations.Count);
                Vector3 facing = _spawnLocations[spawnIndex].transform.position - _owner.transform.position;
                StartSpawnVFX();
                GameObject newEnemy = Instantiate(_meleeChaserPrefab, _spawnLocations[spawnIndex].transform.position, Quaternion.LookRotation(facing));
                _spawnLocations.RemoveAt(spawnIndex);
                // _owner.transform.localScale = _owner._spawnStartPoint;
            }
            _hasSpawned = true;
            _isSpawning = false;
            spawnTimer = 0.0f;
        }

        public void StartWindupVFX()
        {
            Transform windupVFX = _owner.transform.Find("vfx_SpawnerWindup");
            foreach (Transform child in windupVFX)
            {
                if (!child.GetComponent<ParticleSystem>().isEmitting || !child.GetComponent<ParticleSystem>().isPlaying)
                {
                    child.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        public void StartSpawnVFX()
        {
            Transform spawnVFX = _owner.transform.Find("vfx_SpawnerPlopingChasersOut");
            foreach (Transform child in spawnVFX)
            {
                if (!child.GetComponent<ParticleSystem>().isEmitting || !child.GetComponent<ParticleSystem>().isPlaying)
                {
                    child.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        public void PauseVFX()
        {
            Transform windupVFX = _owner.transform.Find("vfx_SpawnerWindup");
            foreach (Transform child in windupVFX)
            {
                child.GetComponent<ParticleSystem>().Pause();
            }

            Transform spawnVFX = _owner.transform.Find("vfx_SpawnerPlopingChasersOut");
            foreach (Transform child in spawnVFX)
            {
                child.GetComponent<ParticleSystem>().Pause();
            }
        }

        public void StopChargeVFX()
        {
            Transform windupVFX = _owner.transform.Find("vfx_SpawnerWindup");
            foreach (Transform child in windupVFX)
            {
                child.GetComponent<ParticleSystem>().Stop();
            }
        }

        public void StopVFX()
        {
            StopChargeVFX();

            Transform spawnVFX = _owner.transform.Find("vfx_SpawnerPlopingChasersOut");
            foreach (Transform child in spawnVFX)
            {
                child.GetComponent<ParticleSystem>().Stop();
            }
        }
    }
}