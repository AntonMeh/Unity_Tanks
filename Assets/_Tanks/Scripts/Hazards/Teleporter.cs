using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tanks.Complete
{
    /// <summary>
    /// A portal that teleports any Rigidbody (Tanks or Shells) to a destination portal.
    /// </summary>
    public class Teleporter : MonoBehaviour
    {
        [Header("Teleport Settings")]
        [Tooltip("The destination portal where the object will appear.")]
        public Teleporter m_Destination;
        
        [Tooltip("Should the teleported object face the same direction as the destination portal?")]
        public bool m_MatchDestinationRotation = true;

        [Tooltip("Local offset at the destination. If Z=2, it spawns 2 units in front of the destination portal.")]
        public Vector3 m_ExitOffset = new Vector3(0, 0, 2f);

        [Header("Effects")]
        [Tooltip("Particle system prefab to instantiate when something enters or exits this portal.")]
        public GameObject m_TeleportFXPrefab;

        [Tooltip("Sound to play when a teleport occurs.")]
        public AudioClip m_TeleportSound;

        // Keep track of recently teleported objects so they don't instantly bounce back and forth
        private HashSet<Collider> m_RecentlyTeleported = new HashSet<Collider>();

        private void OnTriggerEnter(Collider other)
        {
            // If no destination is set, do nothing
            if (m_Destination == null) return;

            // Get the rigidbody of the object entering the portal (tank or shell)
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            // If this object just arrived from another portal, ignore it temporarily
            if (m_RecentlyTeleported.Contains(other)) return;

            // Perform the teleportation
            TeleportObject(rb, other);
        }

        private void TeleportObject(Rigidbody rb, Collider col)
        {
            // Play departure effects at THIS portal
            PlayEffects();

            // Calculate new position at the destination
            Vector3 newPosition = m_Destination.transform.position + (m_Destination.transform.rotation * m_ExitOffset);

            // Move the physical object
            rb.position = newPosition;
            rb.transform.position = newPosition;

            // Align rotation if enabled
            if (m_MatchDestinationRotation)
            {
                rb.rotation = m_Destination.transform.rotation;
                rb.transform.rotation = m_Destination.transform.rotation;
                
                // If it's a shell, we also need to change its velocity direction so it flies out of the new portal properly!
                if (rb.linearVelocity.sqrMagnitude > 0.1f)
                {
                    float currentSpeed = rb.linearVelocity.magnitude;
                    rb.linearVelocity = m_Destination.transform.forward * currentSpeed;
                }
            }

            // Play arrival effects at the DESTINATION portal
            m_Destination.PlayEffects();

            // Tell the destination portal to ignore this object for 1 second, so it doesn't instantly teleport back
            m_Destination.IgnoreColliderTemporarily(col, 1.0f);
            
            // Also ignore it on this portal just in case
            IgnoreColliderTemporarily(col, 1.0f);
        }

        public void PlayEffects()
        {
            if (m_TeleportFXPrefab != null)
            {
                // Instantiate the effect at the portal's position
                GameObject fxInstance = Instantiate(m_TeleportFXPrefab, transform.position, transform.rotation);
                ParticleSystem particles = fxInstance.GetComponent<ParticleSystem>();
                
                if (particles != null)
                {
                    particles.Play();
                    // Destroy the effect object after the particle system finishes playing
                    Destroy(fxInstance, particles.main.duration);
                }
                else
                {
                    // Fallback if the prefab has no ParticleSystem on the root
                    Destroy(fxInstance, 3f);
                }
            }
            
            if (m_TeleportSound != null)
            {
                // Create a temporary audio source to play the sound at the portal's position
                AudioSource.PlayClipAtPoint(m_TeleportSound, transform.position);
            }
        }

        public void IgnoreColliderTemporarily(Collider col, float duration)
        {
            if (!m_RecentlyTeleported.Contains(col))
            {
                m_RecentlyTeleported.Add(col);
                StartCoroutine(RemoveFromIgnoreList(col, duration));
            }
        }

        private IEnumerator RemoveFromIgnoreList(Collider col, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (col != null)
            {
                m_RecentlyTeleported.Remove(col);
            }
        }
    }
}
