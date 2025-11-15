using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.Mechanics
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;
        public int score = 0;
        public TextMeshProUGUI scoreText;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            UpdateHUD();
        }

        public void AddScore(int value)
        {
            score += value;
            UpdateHUD();
        }

        void UpdateHUD()
        {
            if (scoreText != null)
                scoreText.text = "Coins: " + score.ToString();
        }
    }
}
