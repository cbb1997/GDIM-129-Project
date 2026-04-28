using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UiManagerTest : MonoBehaviour
{
    [Header("UI")]
    public GameObject bloodiedScreen;
    public GameObject reticle;
    public CanvasGroup playerStatus;
    public CanvasGroup[] inventoryUI;

    private bool statusVisible;
    private bool inventoryVisible;

    private bool nearDeath;
    private bool reticleEnabled;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) reticle.SetActive(true);
        if (Input.GetKeyDown(KeyCode.Q)) ToggleInvenotryUI();
        if (Input.GetKeyDown(KeyCode.W)) ToggleStatusUI();

        if(Input.GetKeyDown(KeyCode.E)) nearDeath = !nearDeath;

        if (nearDeath == true) bloodiedScreen.SetActive(true);
        else bloodiedScreen.SetActive(false);
    }


    // hides or shows reticle
    private void ToggleReticle()
    {
        reticleEnabled = !reticleEnabled;
        if(reticleEnabled == true) reticle.SetActive(true);
        else reticle.SetActive(false);
    }

    // hides or shows health and stamina bars
    private void ToggleStatusUI()
    {
        statusVisible = !statusVisible;
        if (statusVisible == true) playerStatus.alpha = 0f;
        else playerStatus.alpha = 1f;
    }

    //toggle inventory ui (interactability not set up)
    private void ToggleInvenotryUI()
    {
        inventoryVisible = !inventoryVisible;
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
