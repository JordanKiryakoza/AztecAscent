using UnityEngine;
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

    [Header("Audio (optional, Phase 9)")]
    public AudioSource audioSource;
    public AudioClip winSound;

    float startY;
    float timer;
    bool won;
    bool paused;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        startY = player.transform.position.y;
        winPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && !won)
        {
            if (kb.escapeKey.wasPressedThisFrame) TogglePause();
            if (kb.rKey.wasPressedThisFrame && !paused) PlayAgain(); // R restarts the whole run
        }

        if (won || paused) return;

        timer += Time.deltaTime;
        float height = Mathf.Max(0f, player.transform.position.y - startY);
        heightText.text = $"Height: {height:0} m";
        timerText.text = FormatTime(timer);
    }

    public void Win()
    {
        if (won) return;
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
        SceneManager.LoadScene("Title"); // the Title scene is made in Phase 9
    }

    static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t % 60f;
        return $"{minutes}:{seconds:00.00}";
    }
}