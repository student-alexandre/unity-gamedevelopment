using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;

            // Toca animação de vitória
            player.animator.SetTrigger("victory");

            // Travar controle enquanto comemora
            player.controlEnabled = false;

            // Trocar para o PRÓXIMO LEVEL após 1 segundo
            Simulation.Schedule<LoadNextLevel>(1f);
        }
    }
}
