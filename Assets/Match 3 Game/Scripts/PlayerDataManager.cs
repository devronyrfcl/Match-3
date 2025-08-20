using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;


[Serializable]
public class LevelListWrapper
{
    public List<LevelInfo> Levels;
}


public class PlayerDataManager : MonoBehaviour
{
    private string savePath;
    public PlayerData playerData;

    public bool isLaunched = false; // Flag to check if the game has been launched
    public string PlayFabPlayerID; // PlayFab Player ID
    public string PlayFabPlayerName; // PlayFab Player Name

    public bool isFoundName = false; // Flag to check if player name is found

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
            PlayerBombAbilityCount = 20,
            PlayerColorBombAbilityCount = 20,
            PlayerExtraMoveAbilityCount = 20,
            CurrentLevelId = 1, // 🔥 Default start at level 1
            Levels = new List<LevelInfo>()
        };

        SendPlayerDataToPlayFab();
    }

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Player data saved: " + savePath);

        SendPlayerDataToPlayFab(); // Send data to PlayFab after saving locally

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
        SetUserName(newName);
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
        Invoke("CheckAndSetPlayerName", 2f); // Delay to ensure playerName is set after login
        isLaunched = true;
    }
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error during PlayFab operation: " + error.GenerateErrorReport());
        // Handle error
    }

    

    //get user name. if user name not found, then loadscene.isFoundName = false;

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


    // send player level data and PlayerExtraMoveAbilityCount, PlayerColorBombAbilityCount, PlayerBombAbilityCount to PlayFab
    public void SendPlayerDataToPlayFab()
    {
        if (playerData == null)
        {
            Debug.LogError("Player data is null, cannot send to PlayFab.");
            return;
        }

        // 🔥 Serialize levels list to JSON
        string levelsJson = JsonUtility.ToJson(new LevelListWrapper { Levels = playerData.Levels }, true);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "PlayerName", playerData.Name },
            { "PlayerID", playerData.PlayerID },
            { "CurrentLevelId", playerData.CurrentLevelId.ToString() },
            { "PlayerBombAbilityCount", playerData.PlayerBombAbilityCount.ToString() },
            { "PlayerColorBombAbilityCount", playerData.PlayerColorBombAbilityCount.ToString() },
            { "PlayerExtraMoveAbilityCount", playerData.PlayerExtraMoveAbilityCount.ToString() },
            { "Levels", levelsJson }
        }
        };

        PlayFabClientAPI.UpdateUserData(request, OnUpdateUserDataSuccess, OnError);
    }

    void OnUpdateUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Player data sent to PlayFab successfully.");
    }

    //if playerData name is not matched with PlayFabManager player name, then set PlayfabManager player name to playerData name also isFoundName = false;
    public void CheckAndSetPlayerName()
    {
        if (string.IsNullOrEmpty(PlayFabPlayerName))
        {
            Debug.Log("Player name not found, setting default name.");
            SetUserName(playerData.Name);
            isFoundName = false; // Name not found
        }
        else
        {
            playerData.Name = PlayFabPlayerName;
            isFoundName = true; // Name found
            Debug.Log("Player name found: " + PlayFabPlayerName);
        }
    }




    #endregion
}
