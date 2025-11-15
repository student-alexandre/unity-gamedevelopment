using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            if (player == null) return;

            // 🔹 Reativa o collider e desativa controle temporariamente
            player.collider2d.enabled = true;
            player.controlEnabled = false;

            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);

            // Restaurar a vida
            var health = player.GetComponent<Health>();
            if (health != null)
            {
                // Zerar qualquer valor anterior do boneco
                for (int i = 0; i < health.maxHP; i++)
                    health.Increment();

                // Atualiza o HUD de vida
                HealthHUD.Instance?.SetMaxHealth(health.maxHP);
                HealthHUD.Instance?.SetHealth(health.maxHP);
            }

            // Volter para o spawn
            player.Teleport(model.spawnPoint.transform.position);
            player.jumpState = PlayerController.JumpState.Grounded;
            player.animator.SetBool("dead", false);

            // Reativar câmera
            model.virtualCamera.Follow = player.transform;
            model.virtualCamera.LookAt = player.transform;

            // 2 segundos
            Simulation.Schedule<EnablePlayerInput>(2f);
        }

    }
}