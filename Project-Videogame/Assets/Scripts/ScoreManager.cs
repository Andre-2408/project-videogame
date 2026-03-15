using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score = 0; // Contador de puntos
    public TextMeshProUGUI scoreText; // Referencia al texto en pantalla

    void Start()
    {
        UpdateScoreText();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Puntos: " + score;
    }
}
