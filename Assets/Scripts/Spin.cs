using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Spin : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinDuration = 3f;   // Time for spin
    public int spinRounds = 5;        // Full rotations per spin

    public GameObject spinObject; // The object to spin

    public UnityEvent onSpinComplete;

    [Header("Spin Limit")]
    public int spinCount = 5;         // Total spins allowed
    public TextMeshProUGUI spinLeftText; // UI text to show remaining spins

    private bool isSpinning = false;
    private float currentTime;
    private float totalAngle;

    private float finalAngle;

    void Start()
    {
        totalAngle = 360f * spinRounds;

        // Show initial spin count
        UpdateSpinText();
    }

    public void StartSpin()
    {
        if (!isSpinning && spinCount > 0)
        {
            spinCount--;           // decrease spin count
            UpdateSpinText();      // update UI
            isSpinning = true;
            currentTime = 0f;
        }
    }

    void Update()
    {
        OnSpining();
    }

    private void UpdateSpinText()
    {
        if (spinLeftText != null)
        {
            spinLeftText.text = "Spin left: " + spinCount;
        }
    }


    void OnSpining()
    {
        //do spining and final roation  agle will be random totally. from 0 to 360 also it will slow smoothly

        
        if (isSpinning)
        {
            currentTime += Time.deltaTime;
            float angle = Mathf.Lerp(0, totalAngle + Random.Range(0f, 360f), currentTime / spinDuration);
            spinObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            if (currentTime >= spinDuration)
            {
                isSpinning = false; // stop spinning
                finalAngle = angle % 360f; // get the final angle after spin
                onSpinComplete.Invoke(); // invoke the event
            }
        }

        // Handle the result based on the final angle
        if (!isSpinning && finalAngle != 0)
        {
            HandleResult(finalAngle);
            finalAngle = 0; // reset final angle for next spin
        }

    }
    private void HandleResult(float angle)
    {
        // Determine the result based on the final angle
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




    void Result_1()
    {         // This method can be used to handle the result of the spin
        // For example, you can check the final angle and determine a result based on it
        Debug.Log("Spin completed!");
    }
    void Result_2()
    {         // Another method to handle the result of the spin
        Debug.Log("Another spin completed!");
    }

    //result 3 to 8
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

}