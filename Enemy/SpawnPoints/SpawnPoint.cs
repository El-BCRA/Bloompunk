using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class SpawnPoint : EnemyPoint
    {
        #region variables
        [Space(10)]
        [Title("Spawning Data")]
        [SerializeField, InlineEditor]
        public SpawnPointData spawnPointData;
        #endregion variables

        // Start is called before the first frame update
        void Start()
        {
            SpawnManager.Instance.availableSpawns.Add(this);
        }

        public void Spawn(GameObject enemyToSpawn)
        {
            GameObject temp = Instantiate(enemyToSpawn, this.gameObject.transform.position, 
                Quaternion.LookRotation(this.transform.forward));
            myEnemy = temp.GetComponent<Enemy>();
            myEnemy.myEnemyPoint = this;

            switch (myEnemyType)
            {
                case heldEnemy.Sniper:
                    {
                        SpawnManager.Instance.sniperFilledSpawns.Add(this);
                        SpawnManager.Instance.availableSpawns.Remove(this);
                        break;
                    }
                case heldEnemy.Spawner:
                    {
                        SpawnManager.Instance.spawnerFilledSpawns.Add(this);
                        SpawnManager.Instance.availableSpawns.Remove(this);
                        break;
                    }
                case heldEnemy.None:
                    {
                        break;
                    }
            }
        }

        public override void ClearSpawn()
        {
            switch (myEnemyType)
            {
                case heldEnemy.Sniper:
                    {
                        SpawnManager.Instance.sniperFilledSpawns.Remove(this);
                        SpawnManager.Instance.availableSpawns.Add(this);
                        break;
                    }
                case heldEnemy.Spawner:
                    {
                        SpawnManager.Instance.spawnerFilledSpawns.Remove(this);
                        SpawnManager.Instance.availableSpawns.Add(this);
                        break;
                    }
                case heldEnemy.None:
                    {
                        break;
                    }
            }
            base.ClearSpawn();
        }
    }
}
