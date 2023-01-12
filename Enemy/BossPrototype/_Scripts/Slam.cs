using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bloompunk
{
    public class Slam : MonoBehaviour, ACombatant
    {

        [SerializeField] GameObject slammingObject;
        [SerializeField] float liftTime;
        [SerializeField] float pauseTime;
        [SerializeField] float slamTime;
        [SerializeField] float cooldown;
        [SerializeField] float liftHeight;
        [SerializeField] float slamDamage = 10f;

        [SerializeField] GameObject slamIndicator;
        public bool insideHitCollider;
        Vector3 defaultLocation;
        Vector3 targetLocation;
        Vector3 defaultScale;
        Vector3 targetScale;
        
        float elapsedTime;
        

        // Start is called before the first frame update
        void Start()
        {
            defaultLocation = slammingObject.transform.position;
            targetLocation = new Vector3(defaultLocation.x, defaultLocation.y + liftHeight, defaultLocation.z);

            defaultScale = slamIndicator.transform.localScale;
            targetScale = new Vector3(defaultScale.x + 162, defaultScale.y, defaultScale.z + 162);
        }


        IEnumerator LiftMotion()
        {
            StartCoroutine(IndicatorMotion());
            elapsedTime = Time.deltaTime;
            float percentComplete = elapsedTime / liftTime;
            while (percentComplete < 1)
            {
                elapsedTime += Time.deltaTime;
                percentComplete = elapsedTime / liftTime;
                slammingObject.transform.position = Vector3.Lerp(defaultLocation, targetLocation, percentComplete);
                yield return new WaitForSeconds(0);
            }

            yield return new WaitForSeconds(pauseTime);
            StartCoroutine(SlamMotion());
        }

        IEnumerator SlamMotion()
        {
            elapsedTime = Time.deltaTime;
            float percentComplete = elapsedTime / slamTime;
            while (percentComplete < 1)
            {
                elapsedTime += Time.deltaTime;
                percentComplete = elapsedTime / slamTime;

                slammingObject.transform.position = Vector3.Lerp(targetLocation, defaultLocation, percentComplete);
                yield return new WaitForSeconds(0);
            }

            CheckColliderHit();
            slamIndicator.transform.localScale = defaultScale;
            yield return new WaitForSeconds(cooldown);
            StartCoroutine(LiftMotion());
        }

        IEnumerator IndicatorMotion()
        {
            elapsedTime = Time.deltaTime;
            float percentComplete = elapsedTime / (slamTime+pauseTime+liftTime -0.7f);//the -0.7 is to correct some weird offset, might need to talk to adrian or someone about this later - Bob,  11/11/2022
            while (percentComplete < 1)
            {
                elapsedTime += Time.deltaTime;
                percentComplete = elapsedTime / (slamTime + pauseTime + liftTime -0.7f);
                Debug.Log(percentComplete);
                slamIndicator.transform.localScale = Vector3.Lerp(defaultScale, targetScale, percentComplete);
                yield return new WaitForSeconds(0);
            }
            Debug.Log("done");
            
        }

        //check for hit
        void CheckColliderHit()
        {
            if (insideHitCollider)
            {
                Player.Instance.Damage(slamDamage, this);
            }
        }

        public void Damage(float dmgAmount, ACombatant source)
        {
            //having this here just so boss can do damage to players
        }

        public void StartSlamAttack()
        {
            StartCoroutine(LiftMotion());
        }
    }
}

