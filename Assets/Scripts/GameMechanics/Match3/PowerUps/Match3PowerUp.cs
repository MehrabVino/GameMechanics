using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Power-up types available in the Match-3 game.
    /// </summary>
    public enum PowerUpType
    {
        None = 0,
        Shuffle = 1,
        Hammer = 2,
        ColorBomb = 3,
        ExtraMoves = 4,
        ScoreMultiplier = 5
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
        [SerializeField] private PowerUpConfig powerUpConfig;

        private PowerUp[] inventory;
        private float[] cooldowns;
        private bool isActive;

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

        private void InitializePowerUps()
        {
            inventory = new PowerUp[powerUpConfig.MaxPowerUps];
            cooldowns = new float[powerUpConfig.MaxPowerUps];
        }

        #endregion

        #region Power-up Management

        /// <summary>
        /// Add a power-up to the inventory.
        /// </summary>
        public bool AddPowerUp(PowerUpType type, int uses = 1, float effectValue = 1f)
        {
            for (int i = 0; i < powerUpConfig.MaxPowerUps; i++)
            {
                if (inventory[i].type == PowerUpType.None)
                {
                    inventory[i] = new PowerUp(type, uses, effectValue);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Use a power-up from the inventory.
        /// </summary>
        public bool UsePowerUp(int slot)
        {
            if (slot < 0 || slot >= powerUpConfig.MaxPowerUps || cooldowns[slot] > 0f || isActive) return false;

            PowerUp pu = inventory[slot];
            if (!pu.IsValid) return false;

            pu.uses--;
            if (pu.uses <= 0)
            {
                inventory[slot] = new PowerUp(PowerUpType.None);
            }
            else
            {
                inventory[slot] = pu;
            }

            cooldowns[slot] = powerUpConfig.Cooldown;
            isActive = true;

            OnPowerUpUsed?.Invoke(pu.type);
            OnPowerUpActivated?.Invoke(pu.type);

            return true;
        }

        /// <summary>
        /// Deactivate the current power-up.
        /// </summary>
        public void DeactivatePowerUp()
        {
            if (isActive)
            {
                isActive = false;
                OnPowerUpDeactivated?.Invoke(PowerUpType.None);
            }
        }

        public PowerUp GetPowerUp(int slot) => (slot >= 0 && slot < powerUpConfig.MaxPowerUps) ? inventory[slot] : new PowerUp();
        public float GetCooldown(int slot) => (slot >= 0 && slot < powerUpConfig.MaxPowerUps) ? cooldowns[slot] : 0f;
        public bool IsSlotAvailable(int slot) => (slot >= 0 && slot < powerUpConfig.MaxPowerUps) && inventory[slot].type == PowerUpType.None;
        public bool IsPowerUpActive => isActive;

        #endregion

        #region Power-up Effects

        public void ApplyShuffleEffect(Match3Board board)
        {
            if (board == null) return;
            
            for (int i = 0; i < 100; i++)
            {
                int x1 = Random.Range(0, board.Width);
                int y1 = Random.Range(0, board.Height);
                int x2 = Random.Range(0, board.Width);
                int y2 = Random.Range(0, board.Height);
                
                int temp = board.GetTile(x1, y1);
                board.SetTile(x1, y1, board.GetTile(x2, y2));
                board.SetTile(x2, y2, temp);
            }
        }

        public void ApplyHammerEffect(Match3Board board, Vector2Int position)
        {
            if (board == null) return;
            
            board.SetTile(position.x, position.y, -1);
            board.SetSpecialTile(position.x, position.y, new SpecialTile(-1, SpecialTileType.None));
        }

        public void ApplyColorBombEffect(Match3Board board, int targetColor)
        {
            if (board == null) return;
            
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
        }

        #endregion

        #region Cooldown Management

        private void UpdateCooldowns()
        {
            for (int i = 0; i < powerUpConfig.MaxPowerUps; i++)
            {
                if (cooldowns[i] > 0f)
                {
                    cooldowns[i] -= Time.deltaTime;
                    cooldowns[i] = Mathf.Max(cooldowns[i], 0f);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// ScriptableObject for power-up configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpConfig", menuName = "MechanicGames/PowerUp Config", order = 3)]
    public class PowerUpConfig : ScriptableObject
    {
        public int MaxPowerUps = 3;
        public float Cooldown = 1f;
    }
}