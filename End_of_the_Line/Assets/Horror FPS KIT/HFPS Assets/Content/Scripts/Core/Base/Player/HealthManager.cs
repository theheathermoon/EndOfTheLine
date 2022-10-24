/*
 * HealthManager.cs - by ThunderWire Studio
 * ver. 2.0
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ThunderWire.Helpers;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Main Player Health Script
    /// </summary>
    public class HealthManager : DamageBehaviour
    {
        private ScriptManager scriptManager;
        private HFPS_GameManager gameManager;
        private CameraBloodEffect bloodEffect;
        private PlayerController player;
        private readonly RandomHelper rand = new RandomHelper();

        [Header("Health Settings")]
        public float Health = 100.0f;
        public float maximumHealth = 200.0f;
        public float lowHealth = 15f;
        public float maxRegenerateHealth = 100.0f;

        [Header("Player Low Health")]
        public bool lowHealthSettings;
        public float lowWalkSpeed;
        public float lowRunSpeed;

        private float normalWalk;
        private float normalRun;

        [Header("Regeneration")]
        public bool regeneration = false;
        public float minHelathForRegen = 10;
        public float regenerationSpeed;
        public float timeWaitAfterDamage;

        [Header("Pain")]
        public float painWait = 2f;
        public float painStopTime;
        public float maxPainAmount;

        [Header("Audio Settings")]
        public AudioClip[] DamageSounds;
        [Range(0, 1)]
        public float Volume = 1f;

        [Header("Colors")]
        public Color HealthColor = new Color(255, 255, 255);
        public Color AddHealthColor = new Color(0, 0.8f, 0);
        public Color LowHealthColor = new Color(0.9f, 0, 0);

        private Text HealthText;

        private Color CurColor = new Color(0, 0, 0);

        private bool lowHealthMode;

        [HideInInspector]
        public bool isMaximum;

        [HideInInspector]
        public bool isDead;

        private Coroutine PainFadeCoroutine;
        private Coroutine RegenCoroutine;

        void Awake()
        {
            scriptManager = ScriptManager.Instance;
            gameManager = HFPS_GameManager.Instance;
            player = GetComponent<PlayerController>();
            bloodEffect = scriptManager.MainCamera.GetComponent<CameraBloodEffect>();
        }

        void Start()
        {
            HealthText = gameManager.userInterface.HealthText;
            CurColor = HealthColor;
            bloodEffect.bloodAmount = 0;
            normalWalk = player.basicSettings.walkSpeed;
            normalRun = player.basicSettings.runSpeed;
        }

        void Update()
        {
            if (isDead) return;

            if (HealthText)
            {
                HealthText.text = System.Convert.ToInt32(Health).ToString();
                HealthText.color = CurColor;
            }

            if (Health <= lowHealth)
            {
                CurColor = Color.Lerp(CurColor, LowHealthColor, (Seno(6.0f, 0.1f, 0.0f) * 5) + 0.5f);
                if (!lowHealthMode && bloodEffect.bloodAmount != maxPainAmount)
                {
                    if(PainFadeCoroutine != null)
                        StopCoroutine(PainFadeCoroutine);

                    bloodEffect.bloodAmount = maxPainAmount;

                    if (lowHealthSettings)
                    {
                        player.basicSettings.walkSpeed = lowWalkSpeed;
                        player.basicSettings.runSpeed = lowRunSpeed;
                    }

                    lowHealthMode = true;
                }
            }
            else if (lowHealthMode)
            {
                if (PainFadeCoroutine != null)
                    StopCoroutine(PainFadeCoroutine);

                PainFadeCoroutine = StartCoroutine(PainFade(2));
                CurColor = Color.Lerp(CurColor, HealthColor, (Seno(6.0f, 0.1f, 0.0f) * 5) + 0.5f);

                if (lowHealthSettings)
                {
                    player.basicSettings.walkSpeed = normalWalk;
                    player.basicSettings.runSpeed = normalRun;
                }

                lowHealthMode = false;
            }
            else
            {
                CurColor = Color.Lerp(CurColor, HealthColor, Time.deltaTime * 0.0075f);
            }

            if (Health <= 0 || Health <= 0.9)
            {
                Health = 0f;
                gameManager.ShowDeadPanel();

                isDead = true;
            }

            if (Health >= maximumHealth)
            {
                Health = maximumHealth;
                isMaximum = true;
            }
            else
            {
                isMaximum = false;
            }
        }

        /// <summary>
        /// Damage Player
        /// </summary>
        public override void ApplyDamage(int damageAmount)
        {
            if (Health <= 0) return;
            Health -= damageAmount;

            if (DamageSounds.Length > 0)
            {
                GetComponent<AudioSource>().PlayOneShot(DamageSounds[rand.Range(0, DamageSounds.Length)], Volume);
            }

            if (Health <= lowHealth)
            {
                if (PainFadeCoroutine != null)
                    StopCoroutine(PainFadeCoroutine);

                bloodEffect.bloodAmount = maxPainAmount;

                if (lowHealthSettings)
                {
                    player.basicSettings.walkSpeed = lowWalkSpeed;
                    player.basicSettings.runSpeed = lowRunSpeed;
                }

                lowHealthMode = true;
            }
            else
            {
                float pain = bloodEffect.bloodAmount + 0.1f;

                if (pain >= maxPainAmount)
                {
                    pain = maxPainAmount;
                    bloodEffect.bloodAmount = pain;
                }
                else
                {
                    bloodEffect.bloodAmount = pain;
                }

                if (PainFadeCoroutine != null)
                    StopCoroutine(PainFadeCoroutine);

                PainFadeCoroutine = StartCoroutine(PainFade(painWait));
            }

            if (regeneration)
            {
                if (RegenCoroutine != null)
                    StopCoroutine(RegenCoroutine);

                if (Health > minHelathForRegen)
                    RegenCoroutine = StartCoroutine(Regenerate());
            }
        }

        public override bool IsAlive()
        {
            return Health > 0;
        }

        /// <summary>
        /// Instantly kill Player
        /// </summary>
        public void InstantDeath()
        {
            StopAllCoroutines();
            bloodEffect.bloodAmount = maxPainAmount;
            Health = 0;
        }

        /// <summary>
        /// Heal Player
        /// </summary>
        public void ApplyHeal(float heal)
        {
            if (Health > 0 && !isMaximum)
            {
                Health += heal;
                CurColor = AddHealthColor;
            }

            if (isMaximum)
            {
                gameManager.ShowQuickMessage("You have maximum health", "MaxHealth", true);
            }
        }

        float Seno(float rate, float amp, float offset = 0.0f)
        {
            return Mathf.Cos((Time.time + offset) * rate) * amp;
        }

        IEnumerator PainFade(float wait)
        {
            yield return new WaitForSeconds(wait);

            var currentValue = bloodEffect.bloodAmount;
            var t = 0f;

            while (t < 1)
            {
                t += Time.deltaTime / (painStopTime * 10);
                bloodEffect.bloodAmount = Mathf.Lerp(currentValue, 0f, t);
                yield return null;
            }
        }

        IEnumerator Regenerate()
        {
            yield return new WaitForSeconds(timeWaitAfterDamage);

            while (Health <= maxRegenerateHealth)
            {
                Health += Time.deltaTime * regenerationSpeed;
                yield return null;
            }

            Health = maxRegenerateHealth;
        }
    }
}