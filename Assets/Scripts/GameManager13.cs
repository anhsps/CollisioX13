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
    [SerializeField] private GameObject nextBtn_win, lvBtn_win;
    [SerializeField] private GameObject winMenu, pauseMenu, lvMenu;
    [SerializeField] private RectTransform winPanel, pausePanel, lvPanel;
    [SerializeField] private float topPosY = 250f, middlePosY, tweenDuration = 0.2f;
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

        Invoke(nameof(ActiveButton), 1f);

        await HidePanel(winMenu, winPanel);
        await HidePanel(pauseMenu, pausePanel);
        await HidePanel(lvMenu, lvPanel);
    }

    public void Hack() => GameWin();

    private void LoadLevel(int levelIndex)
    {
        maxLV = gridPrefabs.Length;
        if (levelIndex < 1 || levelIndex > maxLV) levelIndex = 1;
        if (levelIndex == maxLV) playMaxLV = true;
        nextBtn_win.SetActive(!playMaxLV);
        lvBtn_win.SetActive(playMaxLV);

        PlayerPrefs.SetInt("CurrentLevel", levelIndex);

        if (lvText) lvText.text = "LEVEL " + levelIndex.ToString("00");

        if (gridPrefabs.Length > 0) StartCoroutine(CreateGrid(levelIndex));
    }

    private IEnumerator CreateGrid(int levelIndex)
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);
        yield return null;// wait destroy complete

        if (gridPrefabs[levelIndex - 1] != null)
        {
            Instantiate(gridPrefabs[levelIndex - 1], gridParent);
            UpdateCamera(GetGridSize(gridParent));

            JellyMerge.Instance.GetJellyBlocks();
        }
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
    public void NextLV() => SetCurrentLV(level + 1);

    public void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (level >= unlockedLevel && level < maxLV)
            PlayerPrefs.SetInt("UnlockedLevel", level + 1);
    }

    public void SetCurrentLV(int levelIndex)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("1");
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

    private void ShowPanel(GameObject menu, RectTransform panel)
    {
        Time.timeScale = 0f;
        menu.SetActive(true);
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
