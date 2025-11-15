using Platformer.Core;
using Platformer.Mechanics;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class HealthIsZero : Simulation.Event<HealthIsZero>
    {
        public Health health;

        public override void Execute()
        {
            var player = health.GetComponent<PlayerController>();
            if (player != null)
            {
                // Forçar morte do player
                var ev = Schedule<PlayerDeath>();
                ev.player = player;
            }
        }
    }
}
