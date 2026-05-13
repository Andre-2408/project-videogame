using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    public int score { get; private set; }

    private float _startTime;

    public const string PrefsScore = "FinalScore";
    public const string PrefsTime  = "FinalTime";

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        _startTime = Time.time;
    }

    void Update()
    {
        if (timerText != null)
            timerText.text = FormatTime(GetElapsedTime());
    }

    public void AddPoints(int points)
    {
        score += points;
        if (scoreText != null)
            scoreText.text = "Puntos: " + score;
    }

    public float GetElapsedTime() => Time.time - _startTime;

    // Llama esto antes de cargar la WinnerScene
    public void SaveToPrefs()
    {
        PlayerPrefs.SetInt(PrefsScore, score);
        PlayerPrefs.SetFloat(PrefsTime, GetElapsedTime());
        PlayerPrefs.Save();
    }

    public static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return string.Format("{0:00}:{1:00}", m, s);
    }
}
