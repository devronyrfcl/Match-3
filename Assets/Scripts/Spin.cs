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

    public UnityEvent onSpinComplete;

    [Header("Spin Limit")]
    public int spinCount = 5;         // Total spins allowed
    public TextMeshProUGUI spinLeftText; // UI text to show remaining spins

    private bool isSpinning = false;
    private float currentTime;
    private float totalAngle;

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
        if (isSpinning)
        {
            currentTime += Time.deltaTime;

            float progress = Mathf.Clamp01(currentTime / spinDuration);

            // Ease out (slows down smoothly)
            float easedProgress = 1 - Mathf.Pow(1 - progress, 3);

            float currentAngle = easedProgress * totalAngle;
            transform.rotation = Quaternion.Euler(0, 0, -currentAngle);

            if (progress >= 1f)
            {
                isSpinning = false;
                onSpinComplete.Invoke(); // Trigger the event when spin is complet
            }


        }
    }

    private void UpdateSpinText()
    {
        if (spinLeftText != null)
        {
            spinLeftText.text = "Spin left: " + spinCount;
        }
    }
}
