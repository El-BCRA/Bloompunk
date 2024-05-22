using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.AI;
using Utilities.StateMachine;

namespace Bloompunk
{
    [CreateAssetMenu(menuName = "States/Enemy/ChaseState"), InlineEditor]
    public class EnemyChaseState : AState<Enemy>
    {
        [SerializeField] private AState<Enemy> postChaseState;
        private Transform _chaseTarget;
        private float _distToPlayer;

        private bool _nearby;
        [SerializeField] private GameObjectGameEvent _onNearby;
        [SerializeField] private GameObjectGameEvent _onNotNearby;

        public override void Enter()
        {
            _owner = Parent.GetOwner();
            
            if (_owner.isBoid)
            {
                BoidManager.Instance.boids.Add(this._owner);
                _owner.enemyRigidBody.isKinematic = false;
            }

            _chaseTarget = Player.Instance.transform;
            _distToPlayer = Vector3.Distance(_owner.transform.position, _chaseTarget.position);
        }

        public override void Exit()
        {

        }

        public override bool CheckStateChanges()
        {
            if (_distToPlayer < _owner.enemyData.AttackRadius)
            {
                Parent.ChangeState(postChaseState.GetType());
                return true;
            }
            return false;
        }
        public override void Tick(float deltaTime)
        {
            _chaseTarget = Player.Instance.transform;
            _distToPlayer = Vector3.Distance(_owner.transform.position, _chaseTarget.position);
            
            if (!_nearby && _distToPlayer <= _owner.enemyData.NearbyRadius)
            {
                _nearby = true;
                _onNearby.Raise(_owner.gameObject);
            }
            else if (_nearby && _distToPlayer > _owner.enemyData.NearbyRadius)
            {
                _nearby = false;
                _onNotNearby.Raise(_owner.gameObject);
            }

            if (!_owner.isBoid)
            {
                _owner.LookTowards();
            }
        }

        #region boidHelperFunctions
        public void UpdateBoid()
        {
            Vector3 acceleration = Vector3.zero;

            if (_chaseTarget != null)
            {
                Vector3 offsetToTarget = (_chaseTarget.transform.position - _owner.myBoidValues.position);
                acceleration = SteerTowards(offsetToTarget) * _owner.boidData.targetWeight;
            }

            if (_owner.myBoidValues.numPerceivedFlockmates != 0)
            {
                _owner.myBoidValues.centreOfFlockmates /= _owner.myBoidValues.numPerceivedFlockmates;

                Vector3 offsetToFlockmatesCentre = (_owner.myBoidValues.centreOfFlockmates - _owner.myBoidValues.position);

                var alignmentForce = SteerTowards(_owner.myBoidValues.avgFlockHeading) * _owner.boidData.alignWeight;
                var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * _owner.boidData.cohesionWeight;
                var seperationForce = SteerTowards(_owner.myBoidValues.avgAvoidanceHeading) * _owner.boidData.seperationWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidDir = ObstacleRays();
                Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * _owner.boidData.avoidCollisionWeight;
                acceleration += collisionAvoidForce;
            }

            _owner.myBoidValues.velocity += acceleration * Time.deltaTime;
            float speed = _owner.myBoidValues.velocity.magnitude;
            Vector3 dir = _owner.myBoidValues.velocity / speed;
            speed = Mathf.Clamp(speed, _owner.boidData.minSpeed, _owner.boidData.maxSpeed);
            _owner.myBoidValues.velocity = dir * speed;

            _owner.myBoidValues.cachedTransform.position += _owner.myBoidValues.velocity * Time.deltaTime;
            _owner.myBoidValues.cachedTransform.forward = dir;
            _owner.myBoidValues.position = _owner.myBoidValues.cachedTransform.position;
            _owner.myBoidValues.forward = dir;
        }

        bool IsHeadingForCollision()
        {
            RaycastHit hit;
            if (Physics.SphereCast(_owner.myBoidValues.position, _owner.boidData.boundsRadius, _owner.myBoidValues.forward, out hit, _owner.boidData.collisionAvoidDst, _owner.boidData.obstacleMask))
            {
                return true;
            }
            else { }
            return false;
        }

        Vector3 ObstacleRays()
        {
            Vector3[] rayDirections = BoidHelper.directions;

            for (int i = 0; i < rayDirections.Length; i++)
            {
                Vector3 dir = _owner.myBoidValues.cachedTransform.TransformDirection(rayDirections[i]);
                Ray ray = new Ray(_owner.myBoidValues.position, dir);
                if (!Physics.SphereCast(ray, _owner.boidData.boundsRadius, _owner.boidData.collisionAvoidDst, _owner.boidData.obstacleMask))
                {
                    return dir;
                }
            }

            return _owner.myBoidValues.forward;
        }

        Vector3 SteerTowards(Vector3 vector)
        {
            Vector3 v = vector.normalized * _owner.boidData.maxSpeed - _owner.myBoidValues.velocity;
            return Vector3.ClampMagnitude(v, _owner.boidData.maxSteerForce);
        }
        #endregion
    }
}