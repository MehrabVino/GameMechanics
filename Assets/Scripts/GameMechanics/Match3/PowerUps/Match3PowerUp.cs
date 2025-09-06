using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Power-up types available in the Match-3 game.
    /// </summary>
    public enum PowerUpType
    {
        None = 0,
        Shuffle = 1,        // Shuffles the board
        Hammer = 2,         // Destroys a single tile
        ColorBomb = 3,      // Destroys all tiles of one color
        ExtraMoves = 4,     // Adds extra moves
        ScoreMultiplier = 5 // Multiplies score for next match
    }

    /// <summary>
    /// Data structure for power-ups.
    /// </summary>
    [System.Serializable]
    public struct PowerUp
    {
        public PowerUpType type;
        public int uses;
        public float effectValue;

        public PowerUp(PowerUpType type, int uses = 1, float effectValue = 1f)
        {
            this.type = type;
            this.uses = uses;
            this.effectValue = effectValue;
        }

        public bool IsValid => type != PowerUpType.None && uses > 0;
    }

    /// <summary>
    /// Power-up system for Match-3 game.
    /// </summary>
    public sealed class Match3PowerUp : MonoBehaviour
    {
        [Header("Power-up Settings")]
        [SerializeField] private int maxPowerUps = 3;
        [SerializeField] private float powerUpCooldown = 1f;

        // Power-up inventory
        private PowerUp[] powerUpInventory;
        private float[] powerUpCooldowns;
        private bool isPowerUpActive;

        // Events
        public System.Action<PowerUpType> OnPowerUpUsed { get; set; }
        public System.Action<PowerUpType> OnPowerUpActivated { get; set; }
        public System.Action<PowerUpType> OnPowerUpDeactivated { get; set; }

        #region Unity Lifecycle

        private void Awake()
        {
            InitializePowerUps();
        }

        private void Update()
        {
            UpdateCooldowns();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the power-up system.
        /// </summary>
        private void InitializePowerUps()
        {
            powerUpInventory = new PowerUp[maxPowerUps];
            powerUpCooldowns = new float[maxPowerUps];
            
            for (int i = 0; i < maxPowerUps; i++)
            {
                powerUpInventory[i] = new PowerUp(PowerUpType.None, 0, 0f);
                powerUpCooldowns[i] = 0f;
            }
        }

        #endregion

        #region Power-up Management

        /// <summary>
        /// Add a power-up to the inventory.
        /// </summary>
        public bool AddPowerUp(PowerUpType type, int uses = 1, float effectValue = 1f)
        {
            for (int i = 0; i < maxPowerUps; i++)
            {
                if (powerUpInventory[i].type == PowerUpType.None)
                {
                    powerUpInventory[i] = new PowerUp(type, uses, effectValue);
                    Debug.Log($"Match3PowerUp: Added {type} power-up with {uses} uses");
                    return true;
                }
            }
            
            Debug.LogWarning("Match3PowerUp: Inventory is full, cannot add power-up");
            return false;
        }

        /// <summary>
        /// Use a power-up from the inventory.
        /// </summary>
        public bool UsePowerUp(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxPowerUps)
            {
                Debug.LogError($"Match3PowerUp: Invalid slot index {slotIndex}");
                return false;
            }

            if (powerUpCooldowns[slotIndex] > 0f)
            {
                Debug.LogWarning("Match3PowerUp: Power-up is on cooldown");
                return false;
            }

            PowerUp powerUp = powerUpInventory[slotIndex];
            if (!powerUp.IsValid)
            {
                Debug.LogWarning("Match3PowerUp: No valid power-up in slot");
                return false;
            }

            if (isPowerUpActive)
            {
                Debug.LogWarning("Match3PowerUp: Another power-up is already active");
                return false;
            }

            // Use the power-up
            powerUp.uses--;
            if (powerUp.uses <= 0)
            {
                powerUpInventory[slotIndex] = new PowerUp(PowerUpType.None, 0, 0f);
            }
            else
            {
                powerUpInventory[slotIndex] = powerUp;
            }

            powerUpCooldowns[slotIndex] = powerUpCooldown;
            isPowerUpActive = true;

            OnPowerUpUsed?.Invoke(powerUp.type);
            OnPowerUpActivated?.Invoke(powerUp.type);

            Debug.Log($"Match3PowerUp: Used {powerUp.type} power-up, {powerUp.uses} uses remaining");
            return true;
        }

        /// <summary>
        /// Deactivate the current power-up.
        /// </summary>
        public void DeactivatePowerUp()
        {
            if (isPowerUpActive)
            {
                isPowerUpActive = false;
                OnPowerUpDeactivated?.Invoke(PowerUpType.None);
            }
        }

        /// <summary>
        /// Get power-up from inventory slot.
        /// </summary>
        public PowerUp GetPowerUp(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxPowerUps)
                return new PowerUp(PowerUpType.None, 0, 0f);
            
            return powerUpInventory[slotIndex];
        }

        /// <summary>
        /// Get cooldown time for a power-up slot.
        /// </summary>
        public float GetCooldown(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxPowerUps)
                return 0f;
            
            return powerUpCooldowns[slotIndex];
        }

        /// <summary>
        /// Check if a power-up slot is available.
        /// </summary>
        public bool IsSlotAvailable(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxPowerUps)
                return false;
            
            return powerUpInventory[slotIndex].type == PowerUpType.None;
        }

        /// <summary>
        /// Check if any power-up is currently active.
        /// </summary>
        public bool IsPowerUpActive => isPowerUpActive;

        #endregion

        #region Power-up Effects

        /// <summary>
        /// Apply shuffle power-up effect.
        /// </summary>
        public void ApplyShuffleEffect(Match3Board board)
        {
            if (board == null) return;
            
            // Shuffle the board by swapping random tiles
            for (int i = 0; i < 100; i++)
            {
                int x1 = Random.Range(0, board.Width);
                int y1 = Random.Range(0, board.Height);
                int x2 = Random.Range(0, board.Width);
                int y2 = Random.Range(0, board.Height);
                
                // Swap tiles
                int temp = board.GetTile(x1, y1);
                board.SetTile(x1, y1, board.GetTile(x2, y2));
                board.SetTile(x2, y2, temp);
            }
            
            Debug.Log("Match3PowerUp: Applied shuffle effect");
        }

        /// <summary>
        /// Apply hammer power-up effect.
        /// </summary>
        public void ApplyHammerEffect(Match3Board board, Vector2Int position)
        {
            if (board == null) return;
            
            // Clear the tile at the specified position
            board.SetTile(position.x, position.y, -1);
            board.SetSpecialTile(position.x, position.y, new SpecialTile(-1, SpecialTileType.None));
            
            Debug.Log($"Match3PowerUp: Applied hammer effect at {position}");
        }

        /// <summary>
        /// Apply color bomb power-up effect.
        /// </summary>
        public void ApplyColorBombEffect(Match3Board board, int targetColor)
        {
            if (board == null) return;
            
            // Clear all tiles of the target color
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.GetTile(x, y) == targetColor)
                    {
                        board.SetTile(x, y, -1);
                        board.SetSpecialTile(x, y, new SpecialTile(-1, SpecialTileType.None));
                    }
                }
            }
            
            Debug.Log($"Match3PowerUp: Applied color bomb effect for color {targetColor}");
        }

        #endregion

        #region Cooldown Management

        /// <summary>
        /// Update power-up cooldowns.
        /// </summary>
        private void UpdateCooldowns()
        {
            for (int i = 0; i < maxPowerUps; i++)
            {
                if (powerUpCooldowns[i] > 0f)
                {
                    powerUpCooldowns[i] -= Time.deltaTime;
                    if (powerUpCooldowns[i] < 0f)
                    {
                        powerUpCooldowns[i] = 0f;
                    }
                }
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Debug method to add random power-ups.
        /// </summary>
        [ContextMenu("Add Random Power-ups")]
        public void AddRandomPowerUps()
        {
            for (int i = 0; i < maxPowerUps; i++)
            {
                if (IsSlotAvailable(i))
                {
                    PowerUpType randomType = (PowerUpType)Random.Range(1, System.Enum.GetValues(typeof(PowerUpType)).Length);
                    AddPowerUp(randomType, Random.Range(1, 4), Random.Range(1f, 3f));
                }
            }
        }

        #endregion
    }
}
