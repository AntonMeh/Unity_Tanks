using UnityEngine;
using UnityEngine.InputSystem;

namespace Tanks.Complete
{
    /// <summary>
    /// Attached to a tank. Allows the player to place a mine behind their tank when they have
    /// collected one via MinePickup. The mine is instantiated behind the tank (negative forward).
    /// Uses the Input System action "PlaceMine" (must be added to the project's Input Actions).
    /// </summary>
    public class TankMinePlacer : MonoBehaviour
    {
        [Tooltip("The mine prefab to instantiate behind the tank. Must have a Landmine component.")]
        public GameObject m_MinePrefab;
        [Tooltip("How far behind the tank (in local units) the mine is placed.")]
        public float m_SpawnOffset = 2.5f;
        [Tooltip("The height offset from the ground at which the mine is placed.")]
        public float m_SpawnHeightOffset = 0.1f;

        [HideInInspector]
        public TankInputUser m_InputUser;       // Reference to the Input User component on this tank.

        private int m_MineCount = 0;            // How many mines the tank currently holds.
        private InputAction m_PlaceMineAction;  // The InputAction bound to the "PlaceMine" action.

        /// <summary>
        /// Returns the current number of mines the tank is holding.
        /// </summary>
        public int MineCount => m_MineCount;


        private void Awake()
        {
            m_InputUser = GetComponent<TankInputUser>();
            if (m_InputUser == null)
                m_InputUser = gameObject.AddComponent<TankInputUser>();
        }


        private void OnEnable()
        {
            // Reset the mine count at the start of each round,
            // so players don't carry mines between rounds.
            m_MineCount = 0;
        }


        private void Start()
        {
            // Try to find a "PlaceMine" action in the local action asset.
            // If the user hasn't added one yet, we fall back to null and log a warning.
            m_PlaceMineAction = m_InputUser.ActionAsset.FindAction("PlaceMine");

            if (m_PlaceMineAction != null)
            {
                m_PlaceMineAction.Enable();
            }
            else
            {
                Debug.LogWarning("TankMinePlacer: No 'PlaceMine' action found in Input Actions. " +
                                 "Add a 'PlaceMine' action to your Input Action Asset or use AddMine() from another script.");
            }
        }


        private void Update()
        {
            // Only place a mine if the player pressed the button this frame and they have mines.
            if (m_PlaceMineAction != null && m_PlaceMineAction.WasPressedThisFrame() && m_MineCount > 0)
            {
                PlaceMine();
            }
        }


        /// <summary>
        /// Called by MinePickup to add mines to this tank's inventory.
        /// </summary>
        public void AddMine(int amount)
        {
            m_MineCount += amount;
        }


        /// <summary>
        /// Instantiates a mine behind the tank and decrements the mine count.
        /// </summary>
        private void PlaceMine()
        {
            if (m_MinePrefab == null)
            {
                Debug.LogError("TankMinePlacer: m_MinePrefab is not assigned!");
                return;
            }

            // Calculate spawn position: behind the tank at a fixed height.
            Vector3 spawnPosition = transform.position - transform.forward * m_SpawnOffset;
            spawnPosition.y = m_SpawnHeightOffset;

            // Instantiate the mine with no rotation (mines are flat on the ground).
            Instantiate(m_MinePrefab, spawnPosition, Quaternion.identity);

            // Consume one mine from the inventory.
            m_MineCount--;
        }
    }
}
