using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    
    
    public string userName;

    public int userLevel;
    public int userXP;
    public int userStars;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLevelButtonClick()
    {
        // This method is called when the "Add Level" button is clicked
        Debug.Log("Add Level button clicked!");
        //PlayerDataManager.Instance.SetName(userName);

        //wait for a second and save data
        SaveDataAfterDelay();
        // Here you can add logic to handle the button click, such as opening a level editor or adding a new level
    }

    private void SaveDataAfterDelay()
    {

        PlayerDataManager.Instance.SavePlayerData();
        Debug.Log("User data saved after delay.");
    }

    public void OnStarButtonClick()
    {
        PlayerDataManager.Instance.SetLevelStars(userLevel, userStars, userXP); // Example: Set level 1 with 3 stars and 50 XP
        SaveDataAfterDelay();
    }


}
