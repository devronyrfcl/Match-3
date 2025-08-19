using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using System.IO; // Needed for file operations



public class PlayFabManager : MonoBehaviour
{
    
    
    /*public string playerName;
    public string playerID;
    
    public loadscene loadScene; // Reference to the loadscene script

    public string playerNameFromPlayerJSON;
    private string savePath;
    public PlayerData playerData;


    public static PlayFabManager Instance { get; private set; }

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


    // Start is called before the first frame update
    void Start()
    {
        savePath = Path.Combine(Application.dataPath, "playerdata.json");

        if (File.Exists(savePath))
        {
            
            Debug.Log("PlayFab: Existing player data loaded.");
            LoadPlayerData();
        }
        else
        {
            
            Debug.Log("PlayFab: No save found. Default player created.");
        }

        LoginAsGuest();
        UpdateUI();

        //load player Name From PlayerJSON
        
        if (playerData != null && !string.IsNullOrEmpty(playerData.Name))
        {
            playerNameFromPlayerJSON = playerData.Name;
            Debug.Log("Player name loaded from PlayerData: " + playerNameFromPlayerJSON);
        }
        else
        {
            playerNameFromPlayerJSON = "Guest";
            Debug.LogWarning("Player name not found in PlayerData, using default: " + playerNameFromPlayerJSON);
        }
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
            
        }
    }

    public void UploadPlayerDataToPlayFab()
    {
        if (playerData == null)
        {
            Debug.LogWarning("No player data found to upload!");
            return;
        }

        // Convert playerData object into JSON string
        string jsonData = JsonUtility.ToJson(playerData);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "PlayerData", jsonData } // save the entire json under one key
        }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log("✅ PlayerData uploaded to PlayFab successfully.");
            },
            error =>
            {
                Debug.LogError("❌ Failed to upload PlayerData: " + error.GenerateErrorReport());
            }
        );
    }


    public void DownloadPlayerDataFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("PlayerData"))
                {
                    string jsonData = result.Data["PlayerData"].Value;
                    playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                    PlayerDataManager.Instance.SetWholeJSON(playerData); // Set the player data in PlayerDataManager

                    Debug.Log("✅ PlayerData downloaded and loaded.");
                }
                else
                {
                    Debug.LogWarning("⚠️ No PlayerData found on PlayFab.");
                }
            },
            error =>
            {
                Debug.LogError("❌ Failed to download PlayerData: " + error.GenerateErrorReport());
            }
        );
    }



    // Update is called once per frame
    void Update()
    {

    }

    //send playerdata to playfab server


    //if playerData name is not matched with PlayFabManager player name, then set PlayfabManagers player name using SetName(string name) method
    
    public void CheckAndSetPlayerName()
    {
        //if PlayerNameFromPlayerJSON is not matched with PlayFabManager playerName, then send PlayFabManager playerName to PlayerDataManager using SetUserName()
        if (playerNameFromPlayerJSON != playerName)
        {
            Debug.Log("Player name mismatch, updating PlayerDataManager with PlayFabManager player name: " + playerName);
            PlayerDataManager.Instance.SetName(playerName);
            PlayerDataManager.Instance.SavePlayerData(); // Save the updated player data
        }
        else
        {
            Debug.Log("Player names match or PlayerData is null, no update needed.");
        }

    }

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

        playerID = result.PlayFabId;
        GetUserName();

        // Get user name

        //CheckAndSetPlayerName();

        Invoke("CheckAndSetPlayerName", 2f); // Delay to ensure playerName is set after login
        PlayerDataManager.Instance.isLaunched = true;

        DownloadPlayerDataFromPlayFab();


    }
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error during PlayFab operation: " + error.GenerateErrorReport());
        // Handle error
    }

    public void UpdateUI()
    {
        


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
            playerName = result.AccountInfo.TitleInfo.DisplayName;
            Debug.Log("User name retrieved: " + playerName);
            UpdateUI();
            loadScene.isFoundName = true; // Set the flag to true if name is found
        }
        else
        {
            Debug.LogWarning("User name not found, setting isFoundName to false");
            loadScene.isFoundName = false; // Set the flag to false if name is not found
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
        playerName = result.DisplayName;
        Debug.Log("User name updated to: " + playerName);
        UpdateUI();
    }*/

}