using Platformer.Core;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    public class ChangeToVictoryScene : Simulation.Event<ChangeToVictoryScene>
    {
        public override void Execute()
        {
            SceneManager.LoadScene("VictoryScene");
        }
    }
}