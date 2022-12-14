using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This handles all behaviors unique to the Gaurdian class
/// By using the EnemyAi attahce to the game component, it can detect when basic enemy behaviors should take place
/// and change them according to the gaurdians necessary behaviors
/// </summary>
namespace EnemySystem
{
    public class ShadowBehavior : MonoBehaviour
    {

        EnemyAi enemy;
        
        bool TeleStart = false;
        bool Teleporting = false;
        public int TeleTimer;
        public float TeleRange;
        public Vector3 TeleVector;

        public LayerMask whatIsground, whatIsplayer;

        public bool RandomTele;
        public Transform TelePoint;

        // Start is called before the first frame update
        void Start()
        {
            enemy = gameObject.GetComponent(typeof(EnemyAi)) as EnemyAi;
        }

        // Update is called once per frame
        void Update()
        {
            //Checks to see if he enemy is in the light
            if (enemy.InFlashLight == true && TeleStart == false)
            {
                
                //Debug.Log("TeleStart");
                //Frezzes it in place
                enemy.agent.isStopped = true;
                //Begins the teleport
                TeleStart = true;
                StartCoroutine(FadeOut());
                enemy.InFlashLight = false;

            }

            //After checking that the object is supposed to be teleporting, Create the teleport destination.
            if(Teleporting == true)
            {
                CreateTeleport();
            }
        }

        //fades the Opacity of the object until it is almost Transparent
        IEnumerator FadeOut()
        {
            Color c = GetComponent<MeshRenderer>().material.color;
            for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
            {
                Debug.Log("FadingOut");
                Debug.Log(c.a);
                c.a = alpha;
                GetComponent<MeshRenderer>().material.color = c;

                yield return new WaitForSeconds(.5f);
            }
            //Gives the code the "all clear" signal to start the teleport
            Teleporting = true;


        }

        //Fades back in the Opacity
        IEnumerator FadeIn()
        {

            Color c = this.GetComponent<MeshRenderer>().material.color;
            for (float alpha = 0f; alpha <= 1; alpha += 0.1f)
            {
                Debug.Log("FadingIn");
                c.a = alpha;
                this.GetComponent<MeshRenderer>().material.color = c;
                yield return new WaitForSeconds(.5f);

            }
            //Resumes motion and unlocks the object
            enemy.agent.isStopped = false;
            Debug.Log("Resume");
            TeleStart = false;

        }

        //
        public void CreateTeleport()
        {
            //Debug.Log("Teleporting");

            ///For Random Teleporting
            
            if(RandomTele == true)
            {
                ///Calculate random point in range
                float randomZ = Random.Range(-TeleRange, TeleRange);
                float randomX = Random.Range(-TeleRange, TeleRange);

                TeleVector = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

                if (Physics.Raycast(TeleVector, -transform.up, 2f, whatIsground))
                {
                    Teleporting = false;
                    StartCoroutine(FadeIn());
                    enemy.agent.Warp(TeleVector);
                }



            }
            ///For Non Random Teleporting

            if (RandomTele == false)
            {
                //Set Vector to preset position
                TeleVector = TelePoint.position;
                Teleporting = false;
                //Fade In
                StartCoroutine(FadeIn());
                //Teleport happens
                enemy.agent.Warp(TeleVector);
            }

        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TeleVector, new Vector3(1, 1, 1));
        }
    }

}
