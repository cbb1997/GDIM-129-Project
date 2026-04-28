using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UiManagerTest : MonoBehaviour
{
    [Header("UI")]
    public UnityEngine.UI.Image bloodiedScreen;
    public UnityEngine.UI.Image reticle;
    public CanvasGroup playerStatus;
    public CanvasGroup[] inventoryUI;

    private bool statusVisible;
    private bool inventoryVisible;

    private bool nearDeath;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) reticle.enabled = true;
        if (Input.GetKeyDown(KeyCode.Q)) ToggleInvenotryUI();
        if (Input.GetKeyDown(KeyCode.W)) ToggleStatusUI();

        if(nearDeath == true)
        {
            bloodiedScreen.enabled = false;
        }
    }


    // hides or shows health and stamina bars
    private void ToggleStatusUI()
    {
        if (statusVisible == true)
        {
            playerStatus.alpha = 0f;
        }
        else
        {
            playerStatus.alpha = 1f;
        }

    }

    //toggle inventory ui (interactability not set up)
    private void ToggleInvenotryUI()
    {
        foreach (CanvasGroup canGroup in inventoryUI)
        {
            if(inventoryVisible == true)
            {
                canGroup.alpha = 0f;
                canGroup.blocksRaycasts = false;
                canGroup.interactable = false;
            }
            else
            {
                canGroup.alpha = 1f;
                canGroup.blocksRaycasts = true;
                canGroup.interactable = true;
            }

        }

    }

}
