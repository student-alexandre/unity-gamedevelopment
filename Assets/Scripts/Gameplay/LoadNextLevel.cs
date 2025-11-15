using Platformer.Core;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    public class LoadNextLevel : Simulation.Event<LoadNextLevel>
    {
        public override void Execute()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            switch (currentScene)
            {
                case "Level_1":
                    SceneManager.LoadScene("Level_2");
                    break;

                case "Level_2":
                    SceneManager.LoadScene("Level_3");
                    break;

                case "Level_3":
                    SceneManager.LoadScene("VictoryScene");
                    break;

                default:
                    break;
            }
        }
    }
}
