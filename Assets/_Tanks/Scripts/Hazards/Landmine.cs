using UnityEngine;

namespace Tanks.Complete
{
    /// <summary>
    /// A placeable mine that detonates when a tank enters its trigger.
    /// Explosion logic mirrors ShellExplosion: uses Physics.OverlapSphere to find all tanks
    /// in radius, applies AddExplosionForce, and deals distance-based damage via TankHealth.
    /// The mine has an arming delay to prevent the placing tank from immediately detonating it.
    /// </summary>
    public class Landmine : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [Tooltip("The amount of damage dealt if a tank is at the exact centre of the explosion.")]
        public float m_MaxDamage = 100f;
        [Tooltip("The explosion force in Newtons applied at the centre. Keep this at 500+.")]
        public float m_ExplosionForce = 1000f;
        [Tooltip("The radius of the explosion in Unity units. Tanks further than this are not affected.")]
        public float m_ExplosionRadius = 5f;
        [Tooltip("Layer mask to filter which objects the explosion affects. Set to 'Players' layer.")]
        public LayerMask m_TankMask;

        [Header("Timing")]
        [Tooltip("Delay in seconds before the mine becomes active after being placed. Prevents the placing tank from detonating it immediately.")]
        public float m_ArmingDelay = 1.0f;
        [Tooltip("Maximum lifetime of the mine in seconds. If nobody triggers it, it self-destructs after this time.")]
        public float m_MaxLifeTime = 30f;

        [Header("Effects")]
        [Tooltip("Particle System prefab to instantiate on explosion. A new copy is spawned, played, then auto-destroyed.")]
        public GameObject m_ExplosionPrefab;
        [Tooltip("AudioClip to play on explosion. Uses PlayClipAtPoint so it survives the mine's destruction.")]
        public AudioClip m_ExplosionSound;

        private bool m_IsArmed = false;     // Whether the mine is ready to detonate.
        private float m_ArmTimer;           // Timer counting down until the mine is armed.
        private bool m_HasExploded = false; // Prevents multiple detonations.


        private void Start()
        {
            m_ArmTimer = m_ArmingDelay;

            // Self-destruct after the maximum lifetime to avoid mines accumulating forever.
            Destroy(gameObject, m_MaxLifeTime);
        }


        private void Update()
        {
            // Count down the arming delay.
            if (!m_IsArmed)
            {
                m_ArmTimer -= Time.deltaTime;
                if (m_ArmTimer <= 0f)
                {
                    m_IsArmed = true;
                }
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            // Don't detonate if the mine isn't armed yet or has already exploded.
            if (!m_IsArmed || m_HasExploded)
                return;

            // Only detonate when a tank drives over the mine.
            Rigidbody enteringRb = other.attachedRigidbody;
            if (enteringRb == null)
                return;

            TankHealth enteringHealth = enteringRb.GetComponent<TankHealth>();
            if (enteringHealth == null)
                return;

            // Proceed with detonation.
            Explode();
        }


        /// <summary>
        /// Performs the explosion: finds all tanks in radius, applies force and damage,
        /// plays VFX/SFX, then destroys this mine. Logic mirrors ShellExplosion.
        /// </summary>
        private void Explode()
        {
            m_HasExploded = true;

            // Collect all colliders in the explosion radius on the tank layer.
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                // Find the rigidbody of each affected object.
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                if (!targetRigidbody)
                    continue;

                // Apply explosion force.
                targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

                // Find the TankHealth to apply damage.
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                if (!targetHealth)
                    continue;

                // Calculate distance-based damage (same formula as ShellExplosion).
                float damage = CalculateDamage(targetRigidbody.position);
                targetHealth.TakeDamage(damage);
            }

            // --- Visual Effects ---
            // Instantiate the explosion particles as a new object so they survive the mine's destruction.
            // We use Quaternion.Euler(-90, 0, 0) because Unity's default particles emit along the Z axis.
            // Rotating -90 degrees on X forces the Z axis to point straight UP.
            if (m_ExplosionPrefab != null)
            {
                GameObject explosionInstance = Instantiate(m_ExplosionPrefab, transform.position, Quaternion.Euler(0f, 0f, 0f));
                ParticleSystem particles = explosionInstance.GetComponent<ParticleSystem>();

                if (particles != null)
                {
                    particles.Play();
                    ParticleSystem.MainModule mainModule = particles.main;
                    Destroy(explosionInstance, mainModule.duration);
                }
                else
                {
                    // If there's no ParticleSystem, destroy the instance after a short time.
                    Destroy(explosionInstance, 3f);
                }
            }

            // --- Audio ---
            // Use PlayClipAtPoint which creates a temporary AudioSource in the world.
            // This ensures the explosion sound plays even after the mine GameObject is destroyed.
            if (m_ExplosionSound != null)
            {
                AudioSource.PlayClipAtPoint(m_ExplosionSound, transform.position);
            }

            // Destroy the mine GameObject.
            Destroy(gameObject);
        }


        /// <summary>
        /// Calculates damage based on the target's distance from the explosion centre.
        /// Identical formula to ShellExplosion.CalculateDamage.
        /// </summary>
        private float CalculateDamage(Vector3 targetPosition)
        {
            // Create a vector from the mine to the target.
            Vector3 explosionToTarget = targetPosition - transform.position;

            // Calculate the distance from the mine to the target.
            float explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance the target is away.
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            float damage = relativeDistance * m_MaxDamage;

            // Make sure that the minimum damage is always 0.
            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}
