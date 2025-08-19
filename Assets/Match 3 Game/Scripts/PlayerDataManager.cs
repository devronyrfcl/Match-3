using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;


public class PlayerDataManager : MonoBehaviour
{
    private string savePath;
    public PlayerData playerData;

    public bool isLaunched = false; // Flag to check if the game has been launched
    public string PlayFabPlayerID; // PlayFab Player ID
    public string PlayFabPlayerName; // PlayFab Player Name

    public static PlayerDataManager Instance { get; private set; }


    #region "Offline JSON"
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject); // Avoid duplicates
        }
    }

    void Start()
    {
        savePath = Path.Combine(Application.dataPath, "playerdata.json");

        LoginAsGuest();

        if (File.Exists(savePath))
        {
            LoadPlayerData();
            
            Debug.Log("Existing player data loaded.");
        }
        else
        {
            CreateNewPlayer("Player", Guid.NewGuid().ToString());
            SavePlayerData();
            
            Debug.Log("No save found. Default player created.");
        }

    }

    

    public void CreateNewPlayer(string name, string playerId)
    {
        playerData = new PlayerData
        {
            Name = name,
            PlayerID = playerId,
            //TotalXP = 0,
            PlayerBombAbilityCount = 0,
            PlayerColorBombAbilityCount = 0,
            PlayerExtraMoveAbilityCount = 0,
            CurrentLevelId = 1, // 🔥 Default start at level 1
            Levels = new List<LevelInfo>()
        };
    }

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Player data saved: " + savePath);
        
    }

    public void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Player data loaded.");
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new player...");
            CreateNewPlayer("Player", Guid.NewGuid().ToString());
            SavePlayerData();
        }
    }


    //if playerData name is not matched with PlayFabManager player name, then set PlayfabManager player name to playerData name


    // 🔥 Set level stars and XP
    public void SetLevelStars(int levelId, int stars, int xp)
    {
        if (stars < 0) stars = 0;
        if (stars > 3) stars = 3;

        LevelInfo level = playerData.Levels.Find(l => l.LevelID == levelId);
        if (level == null)
        {
            level = new LevelInfo { LevelID = levelId, Stars = stars, XP = xp, LevelLocked = 1 }; // Default locked
            playerData.Levels.Add(level);
        }
        else
        {
            if (stars > level.Stars) level.Stars = stars;
            level.XP += xp;
        }

        //playerData.TotalXP += xp;
        Debug.Log($"Level {levelId} updated: Stars={stars}, XP={xp}");
    }

    public void SetLevelLocked(int levelId, int lockedValue)
    {
        if (lockedValue != 0 && lockedValue != 1)
        {
            Debug.LogWarning("Locked value must be 0 (unlocked) or 1 (locked).");
            return;
        }

        LevelInfo level = playerData.Levels.Find(l => l.LevelID == levelId);
        if (level == null)
        {
            // If level doesn't exist, create it with 0 stars and locked state
            level = new LevelInfo { LevelID = levelId, Stars = 0, XP = 0, LevelLocked = lockedValue };
            playerData.Levels.Add(level);
        }
        else
        {
            level.LevelLocked = lockedValue;
        }

        Debug.Log($"Level {levelId} lock state changed to: {(lockedValue == 1 ? "Locked" : "Unlocked")}");
    }

    // 🔥 NEW - Set universal current level
    public void SetCurrentLevel(int levelId)
    {
        playerData.CurrentLevelId = levelId;
        Debug.Log($"Current Level set to: {levelId}");
    }

    // Ability setters
    public void SetName(string newName)
    {
        playerData.Name = newName;
    }

    public void SetPlayerID(string newPlayerID)
    {
        playerData.PlayerID = newPlayerID;
    }

    public void SetPlayerBombAbilityCount(int count)
    {
        playerData.PlayerBombAbilityCount = count;
    }

    public void SetPlayerColorBombAbilityCount(int count)
    {
        playerData.PlayerColorBombAbilityCount = count;
    }

    public void SetPlayerExtraMoveAbilityCount(int count)
    {
        playerData.PlayerExtraMoveAbilityCount = count;
    }

    public void SetAllData(int levelId, int lockedValue, int stars, int xp)
    {
        SetLevelLocked(levelId, lockedValue);
        SetLevelStars(levelId, stars, xp);
    }

    //send xp of specific level
    public void SendXP(int levelId, int xp)
    {
        LevelInfo level = playerData.Levels.Find(l => l.LevelID == levelId);
        if (level != null)
        {
            level.XP = xp;
            //playerData.TotalXP += xp;
            Debug.Log($"XP for Level {levelId} updated: {xp}");
        }
        else
        {
            Debug.LogWarning($"Level {levelId} not found to send XP.");
        }
    }

    #endregion


    #region "PlayFab Integration"
    void LoginAsGuest()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);

        //GetUserName();
    }
    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in as guest");
        // Handle successful login

        PlayFabPlayerID = result.PlayFabId;
        GetUserName();

        // Get user name

        //CheckAndSetPlayerName();

        Invoke("CheckAndSetPlayerName", 2f); // Delay to ensure playerName is set after login
        PlayerDataManager.Instance.isLaunched = true;


    }
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error during PlayFab operation: " + error.GenerateErrorReport());
        // Handle error
    }

    

    //get user name. if user name not found, then loadscene.isFoundName = false;
    public void GetUserName()
    {
        var request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, OnGetUserNameSuccess, OnError);
    }
    void OnGetUserNameSuccess(GetAccountInfoResult result)
    {
        if (result.AccountInfo.TitleInfo != null && !string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
        {
            PlayFabPlayerName = result.AccountInfo.TitleInfo.DisplayName;
            Debug.Log("User name retrieved: " + PlayFabPlayerName);
            
            
        }
        else
        {
            Debug.LogWarning("User name not found, setting isFoundName to false");
            
        }
    }


    // Set user name using PlayFab API from another script (e.g., loadscene.cs)
    public void SetUserName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = name
            };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdateUserNameSuccess, OnError);
        }
        else
        {
            Debug.LogError("User name input is empty");
        }
    }


    void OnUpdateUserNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        PlayFabPlayerName = result.DisplayName;
        Debug.Log("User name updated to: " + PlayFabPlayerName);
        
    }

    #endregion
}
