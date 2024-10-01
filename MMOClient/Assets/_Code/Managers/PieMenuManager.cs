using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class PieMenuManager : MonoBehaviour
    {
        public PieMenu pieMenu;
        private bool isOpen;
        private InventoryManager inventoryManager;

        void Start()
        {
            CloseMenu();
        }

        public void Init(SampleSceneManager mC)
        {
            inventoryManager = mC.currentPlayerGameObject.GetComponent<InventoryManager>();
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void OpenMenu()
        {
            pieMenu.SetSpellData(inventoryManager.quickSpells);
            pieMenu.gameObject.SetActive(true);
            isOpen = true;
        }

        public void CloseMenu()
        {
            if (inventoryManager != null)
            {
                if (pieMenu.selection < 4)
                {
                    inventoryManager.UpdateCurrentSpell(pieMenu.selection);
                }
                else
                {
                    Debug.Log("Update Item");
                }
            }

            pieMenu.Clean();
            pieMenu.gameObject.SetActive(false);
            isOpen = false;
        }

        public static PieMenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
