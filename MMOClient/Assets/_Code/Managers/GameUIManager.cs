using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameObject loginPanel;

    public GameObject gameUI;

    [Header("Dialog")]
    public GameObject dialogUI;
    public bool alreadyInDialog = false;

    [Header("Chat")]
    public GameObject chatUI;

    [Header("Weather")]
    public GameObject weatherUI;
    public TextMeshProUGUI currentTimeText;

    [Header("Ineraction")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;
    public bool alreadyInteracted = false;

    [Header("Bag")]
    public GameObject bagUI;

    [Header("Enemy UI")]
    public GameObject enemyUIPrefab;

    public void Start()
    {
        HideInteractionUI();
        HideBagUI();

        loginPanel.SetActive(true);
    }

    public void Init()
    {
        Show();
    }

    public void Show()
    {
        gameUI.SetActive(true);
    }

    public void Hide()
    {
        gameUI.SetActive(false);
    }

    public void ShowDialogUI()
    {
        HideChatUI();
        HideWeatherUI();
        HideBagUI();
        HideInteractionUI();

        alreadyInDialog = true;
    }

    public void HideDialogUI()
    {
        ShowChatUI();
        ShowWeatherUI();

        alreadyInDialog = false;
    }

    public void ShowChatUI()
    {
        chatUI.SetActive(true);
    }

    public void HideChatUI()
    {
        chatUI.SetActive(false);
    }

    public void ShowWeatherUI()
    {
        weatherUI.SetActive(true);
    }

    public void HideWeatherUI()
    {
        weatherUI.SetActive(false);
    }

    public void ShowInteractionUI()
    {
        if (alreadyInteracted || alreadyInDialog)
        {
            return;
        }

        interactionUI.SetActive(true);
    }

    public void HideInteractionUI()
    {
        interactionUI.SetActive(false);
    }

    public bool IsAlreadyInteracted()
    {
        return alreadyInteracted;
    }

    public void ShowBagUI()
    {
        bagUI.SetActive(true);
        alreadyInteracted = true;
        HideInteractionUI();
    }

    public void HideBagUI()
    {
        bagUI.SetActive(false);
        alreadyInteracted = false;
    }

    public static GameUIManager singleton;
    void Awake()
    {
        singleton = this;
    }
}
