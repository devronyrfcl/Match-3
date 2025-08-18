using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        UpdateSpinText();
    }

    public void StartSpin()
    {
        if (!isSpinning && spinCount > 0)
        {
            spinCount--;
            UpdateSpinText();

            isSpinning = true;

            // Random final angle
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


                });
            onWatchComplete.Invoke();
        }
        else
        {
            onSpinComplete.Invoke();
            AddBonusSpin();
            //onWatchComplete.Invoke();
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
        // normalize angle (0 - 360)
        angle = (angle + 360f) % 360f;

        if (angle >= 0 && angle < 45)
        {
            Result_1();
        }
        else if (angle >= 45 && angle < 90)
        {
            Result_2();
        }
        else if (angle >= 90 && angle < 135)
        {
            Result_3();
        }

        else if (angle >= 135 && angle < 180)
        {
            Result_4();
        }

        else if (angle >= 180 && angle < 225)
        {
            Result_5();
        }
        else if (angle >= 225 && angle < 270)
        {
            Result_6();
        }
        else if (angle >= 270 && angle < 315)
        {
            Result_7();
        }
        else if (angle >= 315 && angle < 360)
        {
            Result_8();
        }
    }

    void Result_1() { 
        Debug.Log("Spin completed!"); 
    }
    void Result_2() 
    { 
        Debug.Log("Another spin completed!"); 
    }
    void Result_3() 
    {
        Debug.Log("Result 3 completed!"); 
    }
    void Result_4()
    {
        Debug.Log("Result 4 completed!"); 
    }
    void Result_5()
    {
        Debug.Log("Result 5 completed!");
    }
    void Result_6()
    {
        Debug.Log("Result 6 completed!"); 
    }
    void Result_7()
    {
        Debug.Log("Result 7 completed!"); 
    }
    void Result_8()
    {
        Debug.Log("Result 8 completed!"); 
    }

    public void AddBonusSpin()
    {
        spinCount += 1;
        UpdateSpinText();

        // Show spin UI again
        if (spinLeftText != null) spinLeftText.gameObject.SetActive(true);

        checkSpinCountForAds();
    }

    void checkSpinCountForAds()
    {
        //if spin count 0 then show ad button
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

}
