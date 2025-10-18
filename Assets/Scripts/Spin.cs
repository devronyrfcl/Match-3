using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Spin : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinDuration = 5f;
    public int spinRounds = 5;
    public GameObject spinObject;
    public UnityEvent onSpinComplete;
    public UnityEvent onWatchComplete;
    public GameObject spinNowBtn;
    public GameObject watchAdBtn;

    [Header("Spin Limit")]
    public int spinCount = 5;
    public TextMeshProUGUI spinLeftText;

    private bool isSpinning = false;
    private float finalAngle;

    [Header("Reward UI")]
    public GameObject winPanel;
    public GameObject bombImage;
    public TextMeshProUGUI bombText;
    public GameObject colorBombImage;
    public TextMeshProUGUI colorBombText;
    public GameObject extraMoveImage;
    public TextMeshProUGUI extraMoveText;

    void Start()
    {
        UpdateSpinText();
        ResetRewardUI();

        //if no spins saved, set to default spin count = 5
        LoadSpinCount();

    }

    //Load spin count from PlayerPrefs
    void LoadSpinCount()
    {
        if (PlayerPrefs.HasKey("SpinCount"))
        {
            spinCount = PlayerPrefs.GetInt("SpinCount");
        }
        else
        {
            spinCount = 3; // default value
            SaveSpinCount();
        }
        UpdateSpinText();
        checkSpinCountForAds();
    }


    //savve spin count to PlayerPrefs   
    void SaveSpinCount()
    {
        PlayerPrefs.SetInt("SpinCount", spinCount);
        PlayerPrefs.Save();
    }

    public void StartSpin()
    {
        if (isSpinning) return; // 🔒 block spam clicks

        
        if (spinCount > 0)
        {
            spinCount--;
            UpdateSpinText();

            isSpinning = true;
            spinNowBtn.SetActive(false);

            float randomAngle = Random.Range(0f, 360f);
            float totalAngle = (360f * spinRounds) + randomAngle;

            spinObject.transform.DOKill();

            spinObject.transform
                .DORotate(new Vector3(0, 0, totalAngle), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    isSpinning = false;
                    finalAngle = spinObject.transform.eulerAngles.z;

                    HandleResult(finalAngle);
                    checkSpinCountForAds();

                    onSpinComplete.Invoke();
                });

            onWatchComplete.Invoke();
        }
        else
        {
            Debug.Log("No spins left! Watch ad for more.");
            checkSpinCountForAds();
        }
    }

    private void UpdateSpinText()
    {
        if (spinLeftText != null)
        {
            spinLeftText.text = "Spin left: " + spinCount;
        }
    }

    private void HandleResult(float angle)
    {
        ResetRewardUI(); // hide old rewards
        winPanel.SetActive(true); // show win panel for every result

        // normalize angle (0 - 360)
        //angle = (angle + 360f) % 360f;

        // shift everything by 22.5 degrees
        angle = (angle + 22.5f) % 360f;

        if (angle >= 0 && angle < 45) Result_1();
        else if (angle >= 45 && angle < 90) Result_2();
        else if (angle >= 90 && angle < 135) Result_3();
        else if (angle >= 135 && angle < 180) Result_4();
        else if (angle >= 180 && angle < 225) Result_5();
        else if (angle >= 225 && angle < 270) Result_6();
        else if (angle >= 270 && angle < 315) Result_7();
        else if (angle >= 315 && angle < 360) Result_8();
    }

    void Result_1()
    {
        Debug.Log("Won Color Bomb x2!");
        colorBombImage.SetActive(true);
        colorBombText.text = "2";
        PlayerDataManager.Instance.AddColorBombAbility(2); // Add 2 color bombs to player data
    }
    void Result_2()
    {
        Debug.Log("Won Moves x2!");
        extraMoveImage.SetActive(true);
        extraMoveText.text = "2";
        PlayerDataManager.Instance.AddExtraMoveAbility(2); // Add 2 extra moves to player data

    }
    void Result_3()
    {
        /*Debug.Log("Won Moves x1");
        extraMoveImage.SetActive(true);
        extraMoveText.text = "1";
        PlayerDataManager.Instance.AddExtraMoveAbility(1); // Add 1 extra move to player data*/

        Debug.Log("won Bomb x1");
        bombImage.SetActive(true);
        bombText.text = "1";
        PlayerDataManager.Instance.AddBombAbility(1); // Add 1 bomb to player data
    }
    void Result_4()
    {
        Debug.Log("Won Bomb and Color Bomb");
        bombImage.SetActive(true);
        bombText.text = "2";
        colorBombImage.SetActive(true);
        colorBombText.text = "2";
        PlayerDataManager.Instance.AddBombAbility(2); // Add 1 bomb to player data
        PlayerDataManager.Instance.AddColorBombAbility(2); // Add 1 color bomb to player data
    }
    void Result_5()
    {
        Debug.Log("Won Extra Move 2");
        extraMoveImage.SetActive(true);
        extraMoveText.text = "2";
        PlayerDataManager.Instance.AddExtraMoveAbility(5); // Add 5 extra moves to player data
    }
    void Result_6()
    {
        Debug.Log("Won Color Bomb x1");
        colorBombImage.SetActive(true);
        colorBombText.text = "1";
        PlayerDataManager.Instance.AddColorBombAbility(1); // Add 1 color bomb to player data
    }
    void Result_7()
    {
        Debug.Log("Won Bomb x1");
        extraMoveImage.SetActive(true);
        extraMoveText.text = "1";
        PlayerDataManager.Instance.AddBombAbility(1); // Add 1 bomb to player data
    }
    void Result_8()
    {
        /*Debug.Log("Extra Moves x1");
        extraMoveImage.SetActive(true);
        extraMoveText.text = "1";
        PlayerDataManager.Instance.AddExtraMoveAbility(1); // Add 1 bomb to player data*/

        Debug.Log("Won Bomb x2!");
        bombImage.SetActive(true);
        bombText.text = "2";
        PlayerDataManager.Instance.AddBombAbility(2); // Add 2 bombs to player data
    }

    public void AddBonusSpin()
    {
        spinCount += 1;
        UpdateSpinText();
        if (spinLeftText != null) spinLeftText.gameObject.SetActive(true);
        checkSpinCountForAds();
    }

    void checkSpinCountForAds()
    {
        if (spinCount <= 0)
        {
            if (spinNowBtn != null) spinNowBtn.SetActive(false);
            if (watchAdBtn != null) watchAdBtn.SetActive(true);
        }
        else
        {
            if (spinNowBtn != null) spinNowBtn.SetActive(true);
            if (watchAdBtn != null) watchAdBtn.SetActive(false);
        }
    }

    void ResetRewardUI()
    {
        winPanel.SetActive(false);
        bombImage.SetActive(false);
        colorBombImage.SetActive(false);
        extraMoveImage.SetActive(false);
    }

    
    public void CloseWinPanel()
    {
        winPanel.SetActive(false);
        bombImage.SetActive(false);
        colorBombImage.SetActive(false);
        extraMoveImage.SetActive(false);
        spinNowBtn.SetActive(true);
        PlayerDataManager.Instance.SavePlayerData(); // Save player data after rewards are given
        SaveSpinCount();
    }

}
