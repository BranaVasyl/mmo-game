using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        public bool IsOpen()
        {
            return isOpen;
        }

        public void OpenMenu()
        {
            inventoryManager = SampleSceneManager.singleton.currentPlayerGameObject.GetComponent<InventoryManager>();
            
            if (inventoryManager == null)
            {
                return;
            }

            Spell[] reversedSpells = (Spell[])inventoryManager.quickSpells.Clone();
            Array.Reverse(reversedSpells);

            pieMenu.SetSpellData(reversedSpells);
            pieMenu.gameObject.SetActive(true);
            isOpen = true;
        }

        public void CloseMenu()
        {
            if (inventoryManager != null)
            {
                if (pieMenu.selection < 4)
                {
                    int spellIndex = 3 - pieMenu.selection;
                    inventoryManager.UpdateCurrentSpell(spellIndex);
                }
                else
                {
                    int spellIndex = pieMenu.selection - 4;

                    Debug.Log("Update Item: " + spellIndex);
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
