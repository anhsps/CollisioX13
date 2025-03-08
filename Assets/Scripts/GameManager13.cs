using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

public class GameManager13 : Singleton<GameManager13>
{
    public static int level;
    [SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private GameObject nextBtn_win, lvBtn_win;//
    [SerializeField] private GameObject winMenu, /*loseMenu, */pauseMenu, lvMenu;
    [SerializeField] private RectTransform winPanel, /*losePanel, */pausePanel, lvPanel;
    [SerializeField] private float topPosY = 250f, middlePosY, tweenDuration = 0.3f;
    private Button[] btns;
    private int maxLV;
    private bool playMaxLV;

    [Header("Grid")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject[] gridPrefabs;

    protected override void Awake()
    {
        base.Awake();
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LoadLevel(level);
    }

    async void Start()
    {
        btns = FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            if (btn.name == "PauseButton" || btn.name == "RetryButton")
                btn.interactable = false;
        }

        //retryBtn.interactable = false;
        //pauseBtn.interactable = false;
        Invoke(nameof(ActiveButton), 1f);

        await HidePanel(winMenu, winPanel);
        //await HidePanel(loseMenu, losePanel);
        await HidePanel(pauseMenu, pausePanel);
        await HidePanel(lvMenu, lvPanel);
    }

    public void Hack() => GameWin();

    private void LoadLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > gridPrefabs.Length) levelIndex = 1;

        maxLV = gridPrefabs.Length;
        if (levelIndex == maxLV) playMaxLV = true;
        nextBtn_win.SetActive(!playMaxLV);
        lvBtn_win.SetActive(playMaxLV);

        PlayerPrefs.SetInt("CurrentLevel", levelIndex);

        if (lvText) lvText.text = "LEVEL " + (levelIndex < 10 ? "0" + levelIndex : levelIndex);

        CreateGrid(levelIndex);
        Vector2Int gridSize = GetGridSize(gridParent);
        UpdateCamera(gridSize);
    }

    private void CreateGrid(int levelIndex)
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        if (gridPrefabs[levelIndex - 1] != null)
            Instantiate(gridPrefabs[levelIndex - 1], gridParent);
    }

    private Vector2Int GetGridSize(Transform gridParent)
    {
        var walls = gridParent.GetComponentsInChildren<Transform>()
            .Where(b => b.gameObject.layer == LayerMask.NameToLayer("Ground"))
            .ToList();

        if (walls.Count == 0) return Vector2Int.zero;

        int minX = walls.Min(b => Mathf.RoundToInt(b.position.x));
        int minZ = walls.Min(b => Mathf.RoundToInt(b.position.z));
        int maxX = walls.Max(b => Mathf.RoundToInt(b.position.x));
        int maxZ = walls.Max(b => Mathf.RoundToInt(b.position.z));

        int width = maxX - minX + 1;
        int height = maxZ - minZ + 1;

        return new Vector2Int(width, height);
    }

    private void UpdateCamera(Vector2Int gridSize)
    {
        float size = Mathf.Max(gridSize.x, gridSize.y) - 2;
        Camera.main.transform.rotation = Quaternion.Euler(60, 0, 0);
        Camera.main.transform.position = new Vector3(0, size, -0.7f * size);
    }

    private void ActiveButton()
    {
        foreach (var btn in btns)
        {
            if (btn.name == "PauseButton" || btn.name == "RetryButton")
                btn.interactable = true;
        }
    }

    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void NextLV()
    {
        PlayerPrefs.SetInt("CurrentLevel", level + 1);
        PlayerPrefs.Save();
        Retry();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PauseGame() => OpenMenu(pauseMenu, pausePanel, 1);

    public async void ResumeGame()
    {
        SoundManager13.Instance.SoundClick();
        await HidePanel(pauseMenu, pausePanel);
        Time.timeScale = 1f;
    }

    public void GameWin()
    {
        UnlockNextLevel();
        OpenMenu(winMenu, winPanel, 2);
    }

    //public void GameLose() => OpenMenu(loseMenu, losePanel, 3);

    private void OpenMenu(GameObject menu, RectTransform panel, int soundIndex)
    {
        SoundManager13.Instance.PlaySound(soundIndex);
        ShowPanel(menu, panel);
    }

    public async void OpenLVMenu()
    {
        await HidePanel(pauseMenu, pausePanel);
        OpenMenu(lvMenu, lvPanel, 1);
    }

    public async void CloseLVMenu()
    {
        await HidePanel(lvMenu, lvPanel);
        if (level != maxLV) OpenMenu(pauseMenu, pausePanel, 1);
    }

    public void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (level >= unlockedLevel && level <= maxLV)
            PlayerPrefs.SetInt("UnlockedLevel", level + 1);
    }

    //public void SetCurrentLV(int levelIndex) => SceneManager.LoadScene((level = levelIndex).ToString());
    public void SetCurrentLV(int levelIndex)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelIndex);
        PlayerPrefs.Save();
        Retry();
    }

    private void ShowPanel(GameObject menu, RectTransform panel)
    {
        menu.SetActive(true);
        Time.timeScale = 0f;
        menu.GetComponent<CanvasGroup>().DOFade(1, tweenDuration).SetUpdate(true);
        panel.DOAnchorPosY(middlePosY, tweenDuration).SetUpdate(true);
    }

    private async Task HidePanel(GameObject menu, RectTransform panel)
    {
        if (menu == null || panel == null) return;

        menu.GetComponent<CanvasGroup>().DOFade(0, tweenDuration).SetUpdate(true);
        await panel.DOAnchorPosY(topPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        if (menu) menu.SetActive(false);
    }
}
