using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject gameMenu;
        private bool isOpen;

        [Header("Header Section")]
        public GameObject header;

        [Header("Inventory Section")]
        public GameObject inventory;
        InventoryController inventoryController;

        public void Init() {
            HideMenu();
        }

        public bool IsOpen() {
            return isOpen;
        }        

        public void ShowMenu() {
            gameMenu.SetActive(true);
            isOpen = true;

            inventoryController.Deinit();
        }

        public void HideMenu() {
            inventoryController = inventory.GetComponent<InventoryController>();
            inventoryController.Init();

            gameMenu.SetActive(false);
            isOpen = false;

        }

        public static MenuManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
