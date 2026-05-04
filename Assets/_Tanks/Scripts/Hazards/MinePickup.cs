using UnityEngine;

namespace Tanks.Complete
{
    /// <summary>
    /// A collectible object placed on the map. When a tank drives over it (OnTriggerEnter),
    /// the tank receives one mine (via TankMinePlacer) and this pickup is destroyed.
    /// </summary>
    public class MinePickup : MonoBehaviour
    {
        [Tooltip("How many mines this pickup gives to the tank.")]
        public int m_MineAmount = 1;

        [Tooltip("Speed of the idle rotation (degrees per second).")]
        public float m_RotateSpeed = 50f;

        [Tooltip("Optional Particle System prefab to instantiate when this pickup is collected.")]
        public ParticleSystem m_CollectFX;

        [Tooltip("Optional AudioClip to play on collection.")]
        public AudioClip m_CollectSound;


        private void Update()
        {
            // Slowly rotate the pickup so it's visually noticeable on the battlefield.
            transform.Rotate(Vector3.up, m_RotateSpeed * Time.deltaTime, Space.World);
        }


        private void OnTriggerEnter(Collider other)
        {
            // Use attachedRigidbody so we find the root tank object regardless
            // of which sub-collider actually triggered the overlap.
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null)
                return;

            // Only tanks can pick up mines. We check for TankMinePlacer, which is the component
            // responsible for holding and deploying mines.
            TankMinePlacer minePlacer = rb.GetComponent<TankMinePlacer>();

            if (minePlacer == null)
                return;

            // Give the tank mines.
            minePlacer.AddMine(m_MineAmount);

            // Spawn collection visual effects if assigned.
            if (m_CollectFX != null)
            {
                ParticleSystem fx = Instantiate(m_CollectFX, transform.position, Quaternion.identity);
                ParticleSystem.MainModule main = fx.main;
                Destroy(fx.gameObject, main.duration);
            }

            // Play collection sound if assigned (play at position so it's spatialized).
            if (m_CollectSound != null)
            {
                AudioSource.PlayClipAtPoint(m_CollectSound, transform.position);
            }

            // Destroy the pickup object.
            Destroy(gameObject);
        }
    }
}
