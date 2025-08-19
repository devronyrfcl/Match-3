using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Make sure you have DOTween installed

public class loadscene : MonoBehaviour
{
    public GameObject LoadingObject;

    public Image LoadingBar;
    public GameObject LoadingFrame;
    public float fillSpeed = 0.5f; 
    public GameObject namePanel;     
    public GameObject newButton;
    public GameObject EmojisImage;

    void Start()
    {
        LoadingBar.fillAmount = 0f;
        namePanel.SetActive(false);
        newButton.SetActive(false); 

        StartCoroutine(FillBar());

    }

    IEnumerator FillBar()
    {
        while (LoadingBar.fillAmount < 1f)
        {
            LoadingBar.fillAmount += fillSpeed * Time.deltaTime;
            yield return null;
        }

        // Debug.Log("? Loading bar filled completely!");
        namePanel.SetActive(true);
        LoadingFrame.gameObject.SetActive(false);
    }

    public void OnNextClicked()
    {
        
        namePanel.SetActive(false);     
        
        newButton.SetActive(true);
       
    }


    public void OnNewButtonClicked()
    {
        newButton.SetActive(false);
        StartCoroutine(SecondLoading());
    }
    IEnumerator SecondLoading()
    {
        RectTransform emojiRect = EmojisImage.GetComponent<RectTransform>();

        // ✅ Move EmojisImage into view (Y: 2150 → -1777)
        yield return emojiRect.DOAnchorPosY(-1777f, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();

        // ✅ Wait 1 second
        yield return new WaitForSeconds(1f);

        // ✅ Deactivate loading object during this time
        LoadingObject.SetActive(false);

        // ✅ Move EmojisImage back up (Y: -1777 → 2150)
        yield return emojiRect.DOAnchorPosY(2150f, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();
    }

}
