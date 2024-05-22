using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bloompunk
{
    public class EscapeRoute
    {
        public EscapeRoute(EnemyPoint dest, int cH)
        {
            destination = dest;
            curveHeight = cH;
        }

        public EnemyPoint destination;
        public int curveHeight;
    }

    public class EnemyPoint : MonoBehaviour
    {
        [HideInInspector] public List<EnemyPoint> validJumpPoints;
        [HideInInspector] public List<EscapeRoute> possibleEscapeRoutes;
        [HideInInspector] public bool hasBeenPopulated = false;

        public enum heldEnemy
        {
            Sniper,
            Spawner,
            None
        }

        [HideInInspector] public Enemy myEnemy;
        [HideInInspector] public heldEnemy myEnemyType = heldEnemy.None;

        private void Awake()
        {
            possibleEscapeRoutes = new List<EscapeRoute>();
            validJumpPoints = new List<EnemyPoint>();
        }

        // Start is called before the first frame update
        void Start()
        {
            EnemyPointManager.Instance.openJumpPoints.Add(this);
            EnemyPointManager.Instance.possibleJumpPoints.Add(this);
        }

        public float getDistanceTo(Vector3 location)
        {
            return Vector3.Distance(location, transform.position);
        }

        public EscapeRoute FindAJumpPoint()
        {
            GetValidJumpPoints();
            List<EscapeRoute> potentialJumpPoints = new List<EscapeRoute>();
            foreach (EscapeRoute p in possibleEscapeRoutes)
            {
                if (validJumpPoints.Contains(p.destination))
                {
                    potentialJumpPoints.Add(p);
                }
            }

            if (potentialJumpPoints.Count == 0)
            {
                return new EscapeRoute(null, 0);
            } else
            {
                int index = UnityEngine.Random.Range(0, potentialJumpPoints.Count);
                return potentialJumpPoints[index];
            }
        }

        public List<EnemyPoint> GetValidJumpPoints()
        {
            if (myEnemyType == heldEnemy.None)
            {
                return new List<EnemyPoint>();
            } else
            {
                validJumpPoints.Clear();
                foreach (EnemyPoint p in EnemyPointManager.Instance.openJumpPoints)
                {
                    float distToThisPoint = Vector3.Distance(p.transform.position, transform.position);
                    if (distToThisPoint <= myEnemy.enemyData.FleeDistanceMax && distToThisPoint >= myEnemy.enemyData.FleeDistanceMin)
                    {
                        validJumpPoints.Add(p);
                    }
                }
                return validJumpPoints;
            }
        }

        public void PopulateAccessibleJumpPoints()
        {
            hasBeenPopulated = true;
            foreach (EnemyPoint p in EnemyPointManager.Instance.possibleJumpPoints)
            {
                // Don't add an object to its own accessible list
                if (this.gameObject == p.gameObject)
                {
                    continue;
                }

                bool nextPoint = false;
                EnemyPoint destinationPoint = p;
                for (int i = 0; i < 8; i++)
                {
                    if (nextPoint) { break; }
                    Vector3 currentCurveLocation = transform.position;
                    Vector3 nextCurveLocation = transform.position;
                    float curveLengthY = destinationPoint.transform.position.y - transform.position.y;
                    for (int j = 0; j < 10; j++)
                    {
                        nextCurveLocation = Vector3.Lerp(transform.position, destinationPoint.transform.position, (float)(j + 1) / 10.0f);
                        nextCurveLocation.y += j > 4 ? (i / 5) + (curveLengthY / 10) : (curveLengthY / 10) - (i / 5);
                        if (Physics.Raycast(currentCurveLocation, nextCurveLocation - currentCurveLocation, out RaycastHit hit, 
                            (nextCurveLocation - currentCurveLocation).magnitude, LayerMask.GetMask("Default", "Ground")) && j != 9)
                        {
                            break;
                        }
                        else
                        {
                            // Only reachable if this enemy has made 10 successful sphercasts
                            // signifying that this is a valid travel curve
                            if (j == 9)
                            {
                                EscapeRoute escape = new EscapeRoute(destinationPoint, i);
                                possibleEscapeRoutes.Add(escape);
                                nextPoint = true;
                                break;
                            }
                            currentCurveLocation = nextCurveLocation;
                        }
                    }
                }
            }
        }

        public virtual void ClearSpawn()
        { 
            myEnemy = null;
            myEnemyType = heldEnemy.None;
            EnemyPointManager.Instance.openJumpPoints.Add(this);
        }
    }
}
