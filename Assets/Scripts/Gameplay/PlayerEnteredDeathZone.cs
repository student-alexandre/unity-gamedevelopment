using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnteredDeathZone : Simulation.Event<PlayerEnteredDeathZone>
    {
        public DeathZone deathzone;

        public override void Execute()
        {
            var model = Simulation.GetModel<PlatformerModel>();
            var player = model.player;

            // Morte instantânea
            player.health.Die();
        }
    }
}
