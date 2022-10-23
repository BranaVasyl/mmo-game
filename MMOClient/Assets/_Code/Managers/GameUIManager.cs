using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameObject gameUi;

    [Header("Ineraction")]
    public GameObject interactionUi;
    public TextMeshProUGUI interactionText;

    [Header("Enemy Ui")]
    public GameObject enemyUIPrefab;

    public void Init()
    {
        Show();
    }

    public void Show()
    {
        gameUi.SetActive(true);
    }

    public void Hide()
    {
        gameUi.SetActive(false);
    }
    
    public static GameUIManager singleton;
    void Awake()
    {
        singleton = this;
    }
}
