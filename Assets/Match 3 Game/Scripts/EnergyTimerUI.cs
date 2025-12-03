using UnityEngine;
using TMPro;
using System.Collections;

public class EnergyTimerUI : MonoBehaviour
{
    public TMP_Text energyTimerText; // Shows "Next energy in: 04:32"
    public TMP_Text currentEnergyText; // Shows "5/10"

    private void Start()
    {
        StartCoroutine(UpdateEnergyTimerUI());
    }

    private IEnumerator UpdateEnergyTimerUI()
    {
        while (true)
        {
            if (PlayerDataManager.Instance != null)
            {
                int currentEnergy = PlayerDataManager.Instance.GetEnergyCount();
                int maxEnergy = PlayerDataManager.Instance.playerData.MaxEnergy;

                // Update energy count display
                if (currentEnergyText != null)
                {
                    currentEnergyText.text = $"{currentEnergy}/{maxEnergy}";
                }

                // Update timer display
                if (energyTimerText != null)
                {
                    if (currentEnergy >= maxEnergy)
                    {
                        energyTimerText.text = "Energy Full!";
                    }
                    else
                    {
                        string timeRemaining = PlayerDataManager.Instance.GetFormattedTimeUntilNextEnergy();
                        energyTimerText.text = $"Next in: {timeRemaining}";
                    }
                }
            }

            yield return new WaitForSeconds(1f); // Update every second
        }
    }
}