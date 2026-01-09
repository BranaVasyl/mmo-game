using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Networking;

namespace BV
{
    public class PieMenuManager : Singleton<PieMenuManager>
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
            inventoryManager = NetworkClient.Instance.currentPlayerGameObject.GetComponent<InventoryManager>();
            
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
    }
}
