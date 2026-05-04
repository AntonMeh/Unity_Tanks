using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Complete
{
    public class ToxicPuddle : MonoBehaviour
    {
        [Tooltip("The amount of damage dealt to a tank per second while it remains inside the puddle.")]
        public float m_DamagePerSecond = 15f;

        [Tooltip("Optional Particle System that plays while the puddle is active (e.g. toxic bubbles).")]
        public ParticleSystem m_PuddleVFX;

        [Tooltip("Sound clip to play while a tank is taking damage in the puddle.")]
        public AudioClip m_DamageSound;

        private AudioSource m_AudioSource;
        private HashSet<TankHealth> m_TanksInside = new HashSet<TankHealth>();


        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.loop = true;
            m_AudioSource.playOnAwake = false;
            m_AudioSource.spatialBlend = 0f;
        }

        private void Start()
        {
            if (m_PuddleVFX != null)
                m_PuddleVFX.Play();

            if (m_DamageSound != null)
                m_AudioSource.clip = m_DamageSound;
        }

        private void Update()
        {
            m_TanksInside.RemoveWhere(t => t == null || !t.gameObject.activeInHierarchy);

            if (m_TanksInside.Count > 0)
            {
                if (!m_AudioSource.isPlaying && m_DamageSound != null)
                    m_AudioSource.Play();
            }
            else
            {
                if (m_AudioSource.isPlaying)
                    m_AudioSource.Stop();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            TankHealth targetHealth = rb.GetComponent<TankHealth>();
            if (targetHealth != null)
            {
                m_TanksInside.Add(targetHealth);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            TankHealth targetHealth = rb.GetComponent<TankHealth>();
            if (targetHealth != null)
            {
                m_TanksInside.Remove(targetHealth);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null)
                return;

            TankHealth targetHealth = rb.GetComponent<TankHealth>();

            if (targetHealth == null)
                return;

            float damage = m_DamagePerSecond * Time.deltaTime;
            targetHealth.TakeDamage(damage);
        }
    }
}
