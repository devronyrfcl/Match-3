using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class loadscene : MonoBehaviour
{
    public Image frame;
    public Image frame1;
    public float fillSpeed = 0.5f; 
    public GameObject frame84;     
    public Button nextButton;      
    public GameObject newButton;   

    void Start()
    {
        frame.fillAmount = 0f;
     //   frame1.fillAmount = 0f; // Reset fill amount to 0
        frame84.SetActive(false);
        newButton.SetActive(false); 

        StartCoroutine(FillBar());

        
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
            Debug.Log("? Next button listener attached!");
        }
        else
        {
            Debug.LogError("? Next button is not assigned in Inspector!");
        }
    }

    IEnumerator FillBar()
    {
        while (frame.fillAmount < 1f)
        {
            frame.fillAmount += fillSpeed * Time.deltaTime;
            yield return null;
        }

       // Debug.Log("? Loading bar filled completely!");
        frame84.SetActive(true);
    }

    void OnNextClicked()
    {
       // Debug.Log("?? Next button was clicked!");
        frame84.SetActive(false);     
        frame1.gameObject.SetActive(false);
        newButton.SetActive(true);
       // Debug.Log("? Frame84 hidden, new button shown!");
    }
}
