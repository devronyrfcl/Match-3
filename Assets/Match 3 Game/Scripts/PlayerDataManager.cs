using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class PlayerDataManager : MonoBehaviour
{
    private string savePath;
    public PlayerData playerData;

    public static PlayerDataManager Instance { get; private set; }

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

        // Example usage
        SetAllData(1, 0, 2, 50); // 🔥 Example: setting level 1, unlocked, 2 stars, 50 XP
        SetAllData(2, 1, 3, 10); // 🔥 Example: setting level 2, locked, 3 stars, 100 XP


        SetCurrentLevel(1); // 🔥 Example: setting level 1 as current

        SavePlayerData();
    }

    public void CreateNewPlayer(string name, string playerId)
    {
        playerData = new PlayerData
        {
            Name = name,
            PlayerID = playerId,
            TotalXP = 0,
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

        playerData.TotalXP += xp;
        Debug.Log($"Level {levelId} updated: Stars={stars}, XP={xp}, TotalXP={playerData.TotalXP}");
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
}
