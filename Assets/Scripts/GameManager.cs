using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public TMP_Text heightText;
    public TMP_Text timerText;
    public GameObject winPanel;
    public TMP_Text winTimeText;
    public GameObject pausePanel;

    [Header("Health")]
    public int maxHealth = 3;
    public Image[] healthSegments;       // drag the 3 segments in, left to right
    public Color fullColor = new Color(0.85f, 0.15f, 0.15f);
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    public GameObject gameOverPanel;

    [Header("Audio (optional, Phase 9)")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip gameOverSound;

    float startY;
    float timer;
    int health;
    bool won;
    bool paused;
    bool gameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        startY = player.transform.position.y;
        health = maxHealth;
        UpdateHealthBar();
        winPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && !won && !gameOver)
        {
            if (kb.escapeKey.wasPressedThisFrame) TogglePause();
            if (kb.rKey.wasPressedThisFrame && !paused) PlayAgain(); // R restarts the whole run
        }

        if (won || paused || gameOver) return;

        timer += Time.deltaTime;
        float height = Mathf.Max(0f, player.transform.position.y - startY);
        heightText.text = $"Height: {height:0} m";
        timerText.text = FormatTime(timer);
    }

    // Called by PlayerController each time a lizard or bee hits the player
    public void TakeDamage()
    {
        if (won || gameOver) return;
        health = Mathf.Max(0, health - 1);
        UpdateHealthBar();
        if (health == 0) GameOver();
    }

    void UpdateHealthBar()
    {
        // Segments to the left of the current health stay red; the rest turn grey
        for (int i = 0; i < healthSegments.Length; i++)
            healthSegments[i].color = i < health ? fullColor : emptyColor;
    }

    void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(true);
        if (audioSource != null && gameOverSound != null) audioSource.PlayOneShot(gameOverSound);
        Time.timeScale = 0f;
    }

    public void Win()
    {
        if (won || gameOver) return;
        won = true;
        winPanel.SetActive(true);
        winTimeText.text = "Time: " + FormatTime(timer);
        if (audioSource != null && winSound != null) audioSource.PlayOneShot(winSound);
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        paused = !paused;
        pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return $"{minutes}:{seconds:00.00}";
    }
}