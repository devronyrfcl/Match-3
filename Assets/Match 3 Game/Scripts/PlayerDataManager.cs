using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.UIElements;



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
    public bool isFoundName = false; // Flag to check if player name is found
    public bool isOnline = false; // Flag to check if online mode is active

    public bool isNameSame = false; // Flag to check if playerData name is same as PlayFabManager player name

    public string PlayFabPlayerID; // PlayFab Player ID
    public string PlayFabPlayerName; // PlayFab Player Name

    private StageManager stageManager;



    public int currentLevel = 1; // 🔥 Universal current level tracker

    public static PlayerDataManager Instance { get; private set; }

    

    public int TotalXP
    {
        get { return playerData != null ? playerData.Levels.Sum(l => l.XP) : 0; }
    }
    public int TotalStars
    {
        get { return playerData != null ? playerData.Levels.Sum(l => l.Stars) : 0; }
    }


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
        //CheckForOnline();

        stageManager = FindObjectOfType<StageManager>();

    }

    void Start()
    {
        //save path will be in android/data/com.companyname.match3game/files/playerdata.json
        savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

        //savePath = Path.Combine(Application.dataPath, "playerdata.json");

        LoginAsGuest();

        if (File.Exists(savePath))
        {
            LoadPlayerData();
            
            Debug.Log("Existing player data loaded.");
        }
        else
        {
            CreateNewPlayer("Temp", Guid.NewGuid().ToString());
            SavePlayerData();
            
            Debug.Log("No save found. Default player created.");
        }

        stageManager = FindObjectOfType<StageManager>();


        SavePlayerData();
        GetCurrentLevel(); // Initialize current level from player data

        

    }

    private string XorEncryptDecrypt(string data, string key = "Heil")
    {
        char[] result = new char[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (char)(data[i] ^ key[i % key.Length]);
        }
        return new string(result);
    }




    /*public void CreateNewPlayer(string name, string playerId)
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
            {
                new LevelInfo { LevelID = 1, Stars = 0, XP = 0, LevelLocked = 0 }, // Start with level 1 unlocked
                new LevelInfo { LevelID = 2, Stars = 0, XP = 0, LevelLocked = 1 }, // Level 2 locked by default
                new LevelInfo { LevelID = 3, Stars = 0, XP = 0, LevelLocked = 1 } // Level 3 locked by default
            }
        };

        SendPlayerDataToPlayFab();
    }*/

    public void CreateNewPlayer(string name, string playerId)
    {
        playerData = new PlayerData
        {
            Name = name,
            PlayerID = playerId,
            PlayerBombAbilityCount = 20,
            PlayerColorBombAbilityCount = 20,
            PlayerExtraMoveAbilityCount = 20,
            CurrentLevelId = 1,
            Levels = new List<LevelInfo>()
        {
            new LevelInfo { LevelID = 1, Stars = 0, XP = 0, LevelLocked = 0 },
            new LevelInfo { LevelID = 2, Stars = 0, XP = 0, LevelLocked = 1 },
            new LevelInfo { LevelID = 3, Stars = 0, XP = 0, LevelLocked = 1 }
        }
        };

        if (isOnline) // 🔥 Only send if online
        {
            SendPlayerDataToPlayFab();
        }
    }

    public void GetCurrentLevel()
    {
        if (playerData != null)
        {
            currentLevel = playerData.CurrentLevelId;
            Debug.Log("Current Level: " + currentLevel);
        }
        else
        {
            Debug.LogWarning("Player data is null, cannot get current level.");
        }
    }

    /*public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Player data saved: " + savePath);

        SendPlayerDataToPlayFab(); // Send data to PlayFab after saving locally

    }

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Player data saved locally: " + savePath);

        if (isOnline) // 🔥 Only send to PlayFab if online
        {
            SendPlayerDataToPlayFab();
        }
    }*/

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);

        // Encrypt before saving
        string encryptedJson = XorEncryptDecrypt(json);
        File.WriteAllText(savePath, encryptedJson);

        Debug.Log("Player data saved (encrypted) locally: " + savePath);

        if (isOnline)
        {
            SendPlayerDataToPlayFab();
        }
    }



    /*public void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Player data loaded.");
            GetCurrentLevel(); // Initialize current level from loaded data
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new player...");
            CreateNewPlayer("Temp", Guid.NewGuid().ToString());
            SavePlayerData();
            GetCurrentLevel(); // Initialize current level after creating new player

        }
    }*/

    public void LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            string encryptedJson = File.ReadAllText(savePath);

            // Decrypt before loading
            string decryptedJson = XorEncryptDecrypt(encryptedJson);

            playerData = JsonUtility.FromJson<PlayerData>(decryptedJson);
            Debug.Log("Player data loaded (decrypted).");

            GetCurrentLevel();
            //isLaunched = true;
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new player...");
            CreateNewPlayer("Temp", Guid.NewGuid().ToString());
            SavePlayerData();
            GetCurrentLevel();
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
    /*public void SetName(string newName)
    {
        playerData.Name = newName;
        SetUserName(newName);
    }*/

    public void SetName(string newName)
    {
        playerData.Name = newName;
        if (isOnline) // 🔥 Only update PlayFab if online
        {
            SetUserName(newName);
        }
    }

    //On Name Update debug log the new name
    void OnUpdateUserNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        PlayFabPlayerName = result.DisplayName;
        Debug.Log("User name updated to: " + PlayFabPlayerName);

        stageManager.UserNameUpdated();
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


    public void AddColorBombAbility(int count)
    {
        playerData.PlayerColorBombAbilityCount += count;
        //Debug.Log($"Added {count} Color Bombs. Total: {playerData.PlayerColorBombAbilityCount}");
    }
    public void AddBombAbility(int count)
    {
        playerData.PlayerBombAbilityCount += count;
        //Debug.Log($"Added {count} Bombs. Total: {playerData.PlayerBombAbilityCount}");
    }
    public void AddExtraMoveAbility(int count)
    {
        playerData.PlayerExtraMoveAbilityCount += count;
        //Debug.Log($"Added {count} Extra Moves. Total: {playerData.PlayerExtraMoveAbilityCount}");
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
    /*void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in as guest");
        // Handle successful login

        PlayFabPlayerID = result.PlayFabId;
        Invoke("CheckAndSetPlayerName", 2f); // Delay to ensure playerName is set after login
        isLaunched = true;
        isOnline = true;
    }*/

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in as guest");

        PlayFabPlayerID = result.PlayFabId;

        Invoke("CheckAndSetPlayerName", 2f);
        isLaunched = true;
        isOnline = true;

        // 🔥 When login succeeds, always sync local JSON data
        LoadPlayerData();
        SendPlayerDataToPlayFab();
        CheckAndSetPlayerName();

        //get player display name and set PlayFabPlayerName
        var getRequest = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(getRequest, result =>
        {
            PlayFabPlayerName = result.AccountInfo.TitleInfo.DisplayName;
            Debug.Log("Fetched PlayFab player name: " + PlayFabPlayerName);
            CheckAndSetPlayerName(); // Ensure names are checked after fetching
        }, OnError);
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error during PlayFab operation: " + error.GenerateErrorReport());
        // Handle error
        isOnline = false;
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


    /*void OnUpdateUserNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        PlayFabPlayerName = result.DisplayName;
        Debug.Log("User name updated to: " + PlayFabPlayerName);
        
    }*/


    // send player level data and PlayerExtraMoveAbilityCount, PlayerColorBombAbilityCount, PlayerBombAbilityCount to PlayFab
    /*public void SendPlayerDataToPlayFab()
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
    }*/

    public void SendPlayerDataToPlayFab()
    {
        if (!isOnline)
        {
            Debug.Log("Offline mode: Skipping PlayFab upload.");
            return;
        }

        if (playerData == null)
        {
            Debug.LogError("Player data is null, cannot send to PlayFab.");
            return;
        }

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
        if (isOnline)
        {
            // 🔥 Online mode checks
            if (string.IsNullOrEmpty(PlayFabPlayerName))
            {
                isFoundName = false;
                Debug.Log("PlayFab player name is empty → isFoundName = false");
            }
            else if (string.IsNullOrEmpty(playerData.Name))
            {
                isFoundName = false;
                Debug.Log("Local playerData name is empty → isFoundName = false");
            }
            else if (PlayFabPlayerName == playerData.Name)
            {
                isFoundName = true;
                Debug.Log("Names match → isFoundName = true");
            }
            else
            {
                // Names differ → prioritize PlayFab name
                playerData.Name = PlayFabPlayerName;
                SavePlayerData(); // Save updated name locally
                isFoundName = true;
                Debug.Log($"Names differed. Local name updated to PlayFab name: {PlayFabPlayerName} → isFoundName = true");
            }
        }
        else
        {
            // 🔥 Offline mode checks
            if (string.IsNullOrEmpty(playerData.Name) || playerData.Name == "Temp")
            {
                isFoundName = false;
                Debug.Log("Offline: Local player name is empty or 'Temp' → isFoundName = false");
            }
            else
            {
                isFoundName = true;
                Debug.Log("Offline: Local player name is valid → isFoundName = true");
            }
        }
    }



    /*public void SendLeaderboard(int TotalXP)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "XP",
                    Value = TotalXP
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }
    
     
     
     public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "XP",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
        //ShowNameOnLeaderboard();
    }*/


    public void SendLeaderboard(int TotalXP)
    {
        if (!isOnline)
        {
            Debug.Log("Offline mode: Skipping leaderboard update.");
            return;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
        {
            new StatisticUpdate
            {
                StatisticName = "XP",
                Value = TotalXP
            }
        }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    public void GetLeaderboard()
    {
        if (!isOnline)
        {
            Debug.Log("Offline mode: Leaderboard not available.");
            return;
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = "XP",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully updated leaderboard");
    }

    

    public void OnLeaderboardGet(GetLeaderboardResult result)
    {

        //Show leaderboard entries
        if (result.Leaderboard != null && result.Leaderboard.Count > 0)
        {
            foreach (var entry in result.Leaderboard)
            {
                Debug.Log($"Rank: {entry.Position + 1}, Player: {entry.DisplayName}, XP: {entry.StatValue}");
            }
        }
        else
        {
            Debug.Log("No leaderboard entries found.");
        }

    }

    void CheckForOnline()
    {
        PlayFabClientAPI.GetTitleData(
            new GetTitleDataRequest(),
            OnSuccess,
            OnError
        );
        
    }

    

    void OnSuccess(GetTitleDataResult result)
    {
        isOnline = true;
        Debug.Log("Online mode active. Syncing local data to PlayFab...");

        // 🔥 Sync latest local data to PlayFab
        LoadPlayerData();  // Make sure JSON is loaded
        SendPlayerDataToPlayFab();
    }



    #endregion

    //send xp of specific level
    /*public void SendXP(int levelId, int xp)
    {
        LevelInfo level = playerData.Levels.Find(l => l.LevelID == levelId);
        if (level != null)
        {
            level.XP += xp;
            playerData.TotalXP += xp;
            Debug.Log($"XP for Level {levelId} updated: {xp}, Total XP: {playerData.TotalXP}");
        }
        else
        {
            Debug.LogWarning($"Level {levelId} not found to send XP.");
        }
    }*/

    

    public void SendBombAbility(int bombCount)
    {
        playerData.PlayerBombAbilityCount = bombCount;
        Debug.Log($"Bomb Ability Count updated: {bombCount}");
    }

    public void SendColorBombAbility(int colorBombCount)
    {
        playerData.PlayerColorBombAbilityCount = colorBombCount;
        Debug.Log($"Color Bomb Ability Count updated: {colorBombCount}");
    }

    public void SendExtraMoveAbility(int extraMoveCount)
    {
        playerData.PlayerExtraMoveAbilityCount = extraMoveCount;
        Debug.Log($"Extra Move Ability Count updated: {extraMoveCount}");
    }

}
