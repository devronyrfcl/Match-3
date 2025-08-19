using DG.Tweening; // ✅ Needed for DOTween animations
using System.Collections; // ✅ Needed for coroutines
using System.IO;
using TMPro; // ✅ Needed for text display
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ Needed for scene loading
using UnityEngine.UI; // ✅ Needed for UI components

public class StageManager : MonoBehaviour
{
    [Header("Level Button References (Order matters)")]
    public LevelButtonManager[] levelButtons; // Assign in order in Inspector

    [Header("JSON Save File")]
    public string fileName = "playerdata.json";

    public TMP_Text TotalStar;
    public TMP_Text TotalXP;
    public TMP_Text Name;
    public GameObject EmojisImage; // Reference to the emojis image GameObject





    private PlayerData playerData;

    private string SavePath => Path.Combine(Application.dataPath, fileName);


    private const string SelectedLevelIndexKey = "SelectedLevelIndex";

    private int selectedLevelIndex = 0; // 0-based, for the clicked button only



    void Start()
    {
        LoadPlayerData();
        ApplyDataToButtons();
        ShowTotalXPandTotalStars();
        
        
    }

    private void LoadPlayerData()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("StageManager: Player data loaded.");
        }
        else
        {
            Debug.LogWarning("StageManager: Save file not found at " + SavePath);
            playerData = new PlayerData(); // empty fallback
        }
    }

    private void ApplyDataToButtons()
    {
        if (playerData == null || playerData.Levels == null)
        {
            Debug.LogError("StageManager: No level data available.");
            return;
        }

        int currentLevelIndex = GetCurrentLevelIndex();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonManager btn = levelButtons[i];
            btn.SetLevelId(i + 1); // Levels start from 1

            LevelInfo levelInfo = playerData.Levels.Find(l => l.LevelID == btn.levelId);
            if (levelInfo != null)
            {
                btn.SetStar(levelInfo.Stars);
                btn.SetLocked(levelInfo.LevelLocked == 1);
            }
            else
            {
                // If level not found in JSON, default: locked & 0 stars
                btn.SetStar(0);
                btn.SetLocked(true);
            }

            // ✅ Ensure only one current level is active
            bool isCurrent = (i == currentLevelIndex);
            btn.SetCurrentLevel(isCurrent);
        }
    }

    private int GetCurrentLevelIndex()
    {
        // Example: first unlocked level with <3 stars
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (!levelButtons[i].isLocked)
            {
                return i;
            }
        }

        // If all locked, fallback to first one
        return 0;
    }

    IEnumerator EmojiLoading()
    {
        RectTransform emojiRect = EmojisImage.GetComponent<RectTransform>();


        // ✅ Move EmojisImage into view (Y: 2150 → -1777)
        yield return emojiRect.DOAnchorPosY(-1777f, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();

        // ✅ Wait 1 second
        yield return new WaitForSeconds(1f);



    }
    public void SelectLevel(LevelButtonManager clickedButton)
    {
        StartCoroutine(SelectLevelCoroutine(clickedButton));
    }

    private IEnumerator SelectLevelCoroutine(LevelButtonManager clickedButton)
    {
        // Run Emoji animation first
        yield return StartCoroutine(EmojiLoading());

        int clickedIndex = -1;

        // Find index of clicked button
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == clickedButton)
            {
                clickedIndex = i;
                break;
            }
        }

        if (clickedIndex == -1)
            yield break; // safety check

        // Check if the level is locked
        LevelInfo levelInfo = playerData.Levels.Find(l => l.LevelID == clickedButton.levelId);
        bool isLocked = levelInfo != null && levelInfo.LevelLocked == 1;

        if (isLocked)
        {
            OnLockedLevelClicked(clickedButton.levelId); // call separate function
            yield break;
        }

        // ✅ Level is unlocked → save and load scene
        selectedLevelIndex = clickedIndex;
        PlayerPrefs.SetInt(SelectedLevelIndexKey, clickedButton.levelId - 1);
        PlayerPrefs.Save();

        Debug.Log($"StageManager: Selected level saved as {clickedButton.levelId - 1}");

        SceneManager.LoadScene("MainGame");
    }



    void OnLockedLevelClicked(int levelId)
    {
        // Handle locked level click (e.g., show message)
        Debug.Log($"StageManager: Level {levelId} is locked. Please unlock it first.");
        // You can also show a UI message or popup here
    }

    void ShowTotalXPandTotalStars()
    {
        if (playerData == null)
        {
            Debug.LogError("StageManager: No player data available.");
            return;
        }
        int totalXP = 0;
        int totalStars = 0;
        foreach (LevelInfo level in playerData.Levels)
        {
            totalXP += level.XP;
            totalStars += level.Stars;
        }
        TotalXP.text = $"{totalXP}";
        TotalStar.text = $"{totalStars}";
        Name.text = playerData.Name; // Display player name
    }


    

    
}
