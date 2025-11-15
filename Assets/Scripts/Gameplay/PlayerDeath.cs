using System.Collections;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        public Mechanics.PlayerController player; // 👈 agora o evento tem a referência direta

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            // Usa o player passado pelo evento se existir
            var p = player != null ? player : model.player;

            if (p == null) return;
            if (p.health.IsAlive) return;

            model.virtualCamera.Follow = null;
            model.virtualCamera.LookAt = null;

            p.controlEnabled = false;

            if (p.audioSource && p.ouchAudio)
                p.audioSource.PlayOneShot(p.ouchAudio);

            p.animator.SetTrigger("hurt");
            p.animator.SetBool("dead", true);

            // Agenda respawn após 2 segundos
            Simulation.Schedule<PlayerSpawn>(2);
        }
    }
}
