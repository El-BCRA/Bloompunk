using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class AnimationEventListener : MonoBehaviour
    {
        public Enemy myEnemy;

        public void FireProjectile()
        {
            // Enemy has locked-on and is launching attack
            myEnemy._onLaunchAttack.Raise(this.gameObject);

            EnemyAttackState blah = (EnemyAttackState) myEnemy.EnemyStateMachine.GetCurrentState();
            blah.StopVFX();
            blah.StartShootVFX();

            Vector3 toPlayer = Player.Instance.CameraHolder.transform.position - myEnemy._gunTip.position;
            ProjectileManager.Instance.SpawnProjectile(myEnemy.enemyMissileData, myEnemy._gunTip.position, toPlayer, myEnemy.enemyMissileData.GetRadius(), toPlayer.normalized, myEnemy);

            myEnemy.animator.ResetTrigger("IsCharging");
            myEnemy.animator.ResetTrigger("IsAttacking");
            myEnemy.animator.SetTrigger("HasAttacked");
        }

        public void EnterFleeState()
        {
            if (myEnemy.EnemyStateMachine.CurrentState.GetType() == typeof(EnemyAttackState))
            {
                myEnemy.InterruptAttack();
            }
            myEnemy.EnemyStateMachine.ChangeState(myEnemy.fleeState.GetType());
            myEnemy._onRangedSniperJump.Raise(myEnemy.gameObject);
        }

        public void EndAttack()
        {
            myEnemy.animator.ResetTrigger("IsAttacking");
            myEnemy.animator.SetTrigger("HasAttacked");
        }

        public void EnemySpawn()
        {
            EnemySpawnState spawnyBoy = (EnemySpawnState) myEnemy.EnemyStateMachine.CurrentState;
            spawnyBoy.Spawn();
        }
        
        public void SpawnCooldown()
        {
            myEnemy.PlayAnimation("SpawnCooldown");
        }

        public void ResetSpawnLoop()
        {
            EnemySpawnState spawnyBoy = (EnemySpawnState) myEnemy.EnemyStateMachine.CurrentState;
            spawnyBoy.StopVFX();
            spawnyBoy.StartWindupVFX();
            spawnyBoy.canSpawn = true;
        }
    }
}
