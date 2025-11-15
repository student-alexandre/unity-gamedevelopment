using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.Mechanics
{
    public class HealthHUD : MonoBehaviour
    {
        public static HealthHUD Instance;
        public TextMeshProUGUI healthText;
        private int currentHP;
        private int maxHP;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // Buscar player no modelo global
            var model = Platformer.Core.Simulation.GetModel<Platformer.Model.PlatformerModel>();
            var player = model.player;

            if (player != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    // Sincroniza HUD com valores reais do player
                    SetMaxHealth(health.maxHP);
                    SetHealth(health.maxHP);
                }
            }
        }
        public void SetMaxHealth(int value)
        {
            maxHP = value;
            UpdateHUD();
        }

        public void SetHealth(int value)
        {
            currentHP = value;
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (healthText != null)
                healthText.text = "Vida: " + currentHP + " / " + maxHP;
        }
    }
}
