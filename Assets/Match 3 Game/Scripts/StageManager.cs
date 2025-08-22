using DG.Tweening; // ✅ Needed for DOTween animations
using System.Collections; // ✅ Needed for coroutines
using System.IO;
using TMPro; // ✅ Needed for text display
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ Needed for scene loading
using UnityEngine.UI; // ✅ Needed for UI components
using PlayFab;
using PlayFab.ClientModels;

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

    public TMP_Text bombAbilityCount;
    public TMP_Text colorBombAbilityCount;
    public TMP_Text extraMoveAbilityCount;

    public GameObject namePanel; // Reference to the name panel GameObject

    public int currentLevel;

    public HomeButtonManager mapHomeButton; // Reference to the HomeButtonManager
    public HomeButtonManager spinButton;

    public GameObject shopUI;

    public int totalStars;
    public int totalXP;
    public TMP_InputField userNameInput; // Reference to the input field for username



    private PlayerData playerData;

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private const string SelectedLevelIndexKey = "SelectedLevelIndex";

    private int selectedLevelIndex = 0; // 0-based, for the clicked button only



    void Start()
    {
        mapHomeButton.ShowButton(); // Show the map home button

        LoadPlayerData();
        ApplyDataToButtons();
        ShowTotalXPandTotalStars();
        
    }


    private void Update()
    {
        GetCurrentLevelInt();
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

        

        /*//enable btn.isCurrentLevel=true if current level
        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonManager btn = levelButtons[i];
            btn.isCurrentLevel = (i + 1 == currentLevel); // Levels start from 1
            btn.SetInteractable(!btn.isCurrentLevel); // Disable interaction for current level button
        }*/


        //int currentLevelIndex = GetCurrentLevelIndex();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonManager btn = levelButtons[i];
            btn.SetLevelId(i + 1); // Levels start from 1

            btn.isCurrentLevel = (i + 1 == currentLevel); // Levels start from 1
            //btn.SetInteractable(!btn.isCurrentLevel); // Disable interaction for current level button
            LevelInfo levelInfo = playerData.Levels.Find(l => l.LevelID == btn.levelId);
            if (levelInfo != null)
            {
                btn.SetStar(levelInfo.Stars);
                btn.SetLocked(levelInfo.LevelLocked == 1);

                // 🔥 Make button not interactable if locked
                btn.GetComponent<Button>().interactable = (levelInfo.LevelLocked == 0);
            }
            else
            {
                // If level not found in JSON, default: locked & 0 stars
                btn.SetStar(0);
                btn.SetLocked(true);

                btn.GetComponent<Button>().interactable = false; // 🔒
            }

            SendDataToLeaderBoard();





        }
    }

    /*private int GetCurrentLevelIndex()
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
    }*/

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

        //reset sce

        
        // You can also show a UI message or popup here
    }

    public void ShowTotalXPandTotalStars()
    {
        if (playerData == null)
        {
            Debug.LogError("StageManager: No player data available.");
            return;
        }
        totalXP = 0;
        totalStars = 0;
        foreach (LevelInfo level in playerData.Levels)
        {
            totalXP += level.XP;
            totalStars += level.Stars;
        }
        TotalXP.text = $"{totalXP}";
        TotalStar.text = $"{totalStars}";
        Name.text = playerData.Name; // Display player name


        //show ability counts
        bombAbilityCount.text = playerData.PlayerBombAbilityCount.ToString();
        colorBombAbilityCount.text = playerData.PlayerColorBombAbilityCount.ToString();
        extraMoveAbilityCount.text = playerData.PlayerExtraMoveAbilityCount.ToString();
    }




    //if PlayerDataManager.isFoundName = false , then show name panel
    public void CheckAndShowNamePanel()
    {
        if (playerData == null || string.IsNullOrEmpty(playerData.Name))
        {
            namePanel.SetActive(true); // Show name panel if no name found
        }
        else
        {
            namePanel.SetActive(false); // Hide if name exists
        }
    }


    public void RefreashData()
    {
               // Reload player data and update buttons
        LoadPlayerData();
        ApplyDataToButtons();
        ShowTotalXPandTotalStars();
        CheckAndShowNamePanel(); // Ensure name panel visibility is updated
    }

    void GetCurrentLevelInt()
    {
        // Get the current level from PlayerPrefs
        currentLevel = PlayerDataManager.Instance.currentLevel;
    }

    public void OnClickAbilityButton()
    {
        //if bomb or color bomb or extra move ability count is 0 then debug log "No abilities left"
        if (playerData.PlayerBombAbilityCount <= 0 && playerData.PlayerColorBombAbilityCount <= 0 && playerData.PlayerExtraMoveAbilityCount <= 0)
        {
            spinButton.ShowButton(); // Show the spin button
            return;
        }
        else
        {
            shopUI.SetActive(true); // Show the shop UI
        }

    }


    public void SendDataToLeaderBoard()
    {
        PlayerDataManager.Instance.SendLeaderboard(totalXP); // Send the score to the leaderboard
    }

    public void GetDataFromLeaderboard()
    {
        PlayerDataManager.Instance.GetLeaderboard();

    }

    public void SetUserName()
    {

        string userName = userNameInput.text.Trim();
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }

        PlayerDataManager.Instance.SetName(userName);
        PlayerDataManager.Instance.SavePlayerData();
        RefreashData();
    }
}
