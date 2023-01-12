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
        [SerializeField] private List<Vector3> _spawnLocations;
        [SerializeField] private float _spawnAnimLength;
        [SerializeField] private float _spawnAmount = 2.0f;

        [SerializeField] private GameObjectGameEvent _onStartSpawnAnimation;

        private bool _isSpawning;
        private bool _hasSpawned;

        public override bool CheckStateChanges()
        {
            if (_hasSpawned)
            {
                Parent.ChangeState(typeof(EnemyIdleState));
                return true;
            }
            return false;
        }

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            _hasSpawned = false;
            _isSpawning = false;
            _spawnAnimLength = _owner.enemyData.AttackCooldown;

            _spawnLocations.Add(new Vector3(_owner.transform.position.x + 2.0f, _owner.transform.position.y, _owner.transform.position.z + 2.0f));
            _spawnLocations.Add(new Vector3(_owner.transform.position.x - 2.0f, _owner.transform.position.y, _owner.transform.position.z + 2.0f));
            _spawnLocations.Add(new Vector3(_owner.transform.position.x + 2.0f, _owner.transform.position.y, _owner.transform.position.z - 2.0f));
            _spawnLocations.Add(new Vector3(_owner.transform.position.x - 2.0f, _owner.transform.position.y, _owner.transform.position.z - 2.0f));
        }

        public override void Exit()
        {
            _spawnLocations.Clear();
            if (_isSpawning && !_hasSpawned)
            {
                _owner._onSpawnInterrupt.Raise(_owner.gameObject);
            }
            // Make sure animation is finished playing, I assume
        }

        public override void Tick(float deltaTime)
        {
            // Call LookTowards() unless the enemy is in SpawnCooldown
            // (AKA, _hasSpawned && !_isSpawning, which is not an accesible 
            // variable state in the current implementation)
            if (!_isSpawning && !_hasSpawned)
            {
                _owner.LookTowards();
                _owner.StartStateCoroutine(SpawnAnimation());
            }
            else if (_isSpawning && !_hasSpawned)
            {
                _owner.LookTowards();
            }
        }

        public void Spawn()
        {
            List<Vector3> spawnLocations = new List<Vector3>(_spawnLocations);
            int spawnIndex;
            // Magic number that can be made adjustable depending on future changes to
            // enemy-spawning difficulty scaling
            for (int i = 0; i < _spawnAmount; i++)
            {
                spawnIndex = Random.Range(0, spawnLocations.Count);
                Vector3 facing = spawnLocations[spawnIndex] - _owner.transform.position;
                GameObject newEnemy = Instantiate(_meleeChaserPrefab, spawnLocations[spawnIndex], Quaternion.LookRotation(facing));
                spawnLocations.RemoveAt(spawnIndex);
            }
        }

        // Spawn occurs after animation has played to telegraph
        // the interaction to the player
        public IEnumerator SpawnAnimation()
        {
            _onStartSpawnAnimation.Raise(_owner.gameObject);
            _isSpawning = true;

            // Wrap the spawnAnimation in "pause-menu catchers"
            while (PauseManager.Instance.Paused)
            {
                yield return null;
            }
            yield return new WaitForSeconds(_spawnAnimLength);
            while (PauseManager.Instance.Paused)
            {
                yield return null;
            }
            Spawn();

            // Lets the tick function know that the enemy is free to spawn again
            _isSpawning = false;
            _hasSpawned = true;
        }
    }
}