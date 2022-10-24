/*
 * NPCHealth.cs - by ThundereWire Studio
 * ver. 2.0
 * 
*/

using UnityEngine;
using System.Linq;
using HFPS.Player;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    /// <summary>
    /// Script for NPC Health
    /// </summary>
    public class NPCHealth : MonoBehaviour
    {
        public enum CorpseRemoveType { None, Disable, Destroy }

        INPCReaction[] hitReactions;

        [Header("Setup")]
        public Transform Hips;
        public string FleshTag = "Flesh";

        [Header("Character Health")]
        public int Health;
        public int MaxHealth;
        public AudioClip HtAudio;
        public bool disableHealth;

        [Header("Headshot")]
        public Collider head;
        public bool allowHeadshot;
        public int headshotDamage;

        [Header("On Death")]
        public CorpseRemoveType corpseRemoveType = CorpseRemoveType.None;
        public float corpseTime = 10;

        [Header("Eye Glow")]
        public bool enableEyesGlow;
        public SkinnedMeshRenderer eyeRenderer;
        public string emission = "_EmissionColor";

        private BodyPart[] bodyParts;
        private bool isDead = false;

        void Start()
        {
            hitReactions = GetComponents<INPCReaction>();

            bodyParts = (from rb in Hips.GetComponentsInChildren<Rigidbody>(true)
                         let col = rb.GetComponent<Collider>()
                         let bp = col.gameObject.AddComponent<NPCBodyPart>()
                         select new BodyPart(rb, col, bp)).ToArray();

            foreach (BodyPart bodyPart in bodyParts)
            {
                bodyPart.bodyPart.health = this;
                bodyPart.collider.gameObject.tag = FleshTag;

                if (allowHeadshot && head && bodyPart.collider == head)
                {
                    bodyPart.bodyPart.isHead = true;
                }
            }

            if (enableEyesGlow && eyeRenderer)
            {
                eyeRenderer.material.EnableKeyword("_EMISSION");
                eyeRenderer.material.SetColor("_EmissionColor", new Color(1f, 1f, 1f));
            }

            EnableRagdoll(false);
        }

        void Update()
        {
            if (disableHealth) return;

            if (Health <= 0)
            {
                Health = 0;
            }
            else if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public void Damage(int damage)
        {
            if (hitReactions.Length > 0)
            {
                foreach (var hit in hitReactions)
                {
                    hit.HitReaction();
                }
            }

            if (Health <= 0 || disableHealth) return;

            Health -= damage;

            if (Health <= 0)
            {
                if (gameObject.HasComponent(out NPCEntity entity)) entity.OnDeath();
                if (gameObject.HasComponent(out CapsuleCollider collider)) collider.enabled = false;
                EnableRagdoll(true);

                //You can put your custom events after death here.

                if (enableEyesGlow && eyeRenderer)
                {
                    eyeRenderer.material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
                }

                if (corpseRemoveType != CorpseRemoveType.None)
                {
                    if (corpseRemoveType == CorpseRemoveType.Destroy)
                    {
                        SaveGameHandler.Instance.RemoveSaveableObject(gameObject, false, false);
                    }

                    Invoke("CorpseRemove", corpseTime);
                }
            }

            if (HtAudio)
            {
                AudioSource.PlayClipAtPoint(HtAudio, transform.position, 1.0f);
            }
        }

        public void CorpseRemove()
        {
            if (corpseRemoveType == CorpseRemoveType.Destroy)
            {
                SaveGameHandler.Instance.RemoveSaveableObject(gameObject, true, true);
            }
            else if (corpseRemoveType == CorpseRemoveType.Disable)
            {
                gameObject.SetActive(false);
            }
        }

        void EnableRagdoll(bool enabled)
        {
            foreach (BodyPart bodyPart in bodyParts)
            {
                //Physics.IgnoreCollision(bodyPart.collider, PlayerController.Instance.gameObject.GetComponent<Collider>());

                if (enabled)
                {
                    bodyPart.rigidbody.isKinematic = false;
                    bodyPart.rigidbody.useGravity = true;
                    bodyPart.collider.isTrigger = false;
                }
                else
                {
                    bodyPart.rigidbody.isKinematic = true;
                    bodyPart.rigidbody.useGravity = false;
                    bodyPart.collider.isTrigger = true;
                }
            }

            if (!isDead && enabled)
            {
                if (GetComponent<TriggerObjective>())
                {
                    GetComponent<TriggerObjective>().OnTrigger();
                }

                isDead = true;
            }
        }

        struct BodyPart
        {
            public Rigidbody rigidbody;
            public Collider collider;
            public NPCBodyPart bodyPart;

            public BodyPart(Rigidbody rb, Collider col, NPCBodyPart bpart)
            {
                rigidbody = rb;
                collider = col;
                bodyPart = bpart;
            }
        }
    }
}