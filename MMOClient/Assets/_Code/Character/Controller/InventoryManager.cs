using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class InventoryManager : MonoBehaviour
    {
        private List<InventoryGridData> playerEquipData = new();

        [Header("Right Hand Weapon")]
        [HideInInspector]
        public GameObject rightHandPivot;
        public ItemWeaponData rightHandData;
        [HideInInspector]
        public GameObject rightHandObject;
        [HideInInspector]
        public WeaponHook rightHandWeaponHook;

        [Header("Left Hand Weapon")]
        [HideInInspector]
        public GameObject leftHandPivot;
        public ItemWeaponData leftHandData;
        [HideInInspector]
        public GameObject leftHandObject;
        [HideInInspector]
        public WeaponHook leftHandWeaponHook;

        [Header("Quick Spell Data")]
        public Spell[] quickSpells = new Spell[4];

        [Header("Current Spell")]
        public int currentSpelId = 0;
        public Spell currentSpellData;
        [HideInInspector]
        public GameObject currentSpellParticle;

        [Header("Inventory Camera")]
        public GameObject inventoryCameraHodler;

        private StateManager states;
        private CharacterModelController characterModelProvider;

        public void Init(StateManager st)
        {
            states = st;

            characterModelProvider = st.activeModel.GetComponent<CharacterModelController>();
            if (characterModelProvider)
            {
                rightHandPivot = characterModelProvider.GetRightHandPivot();
                leftHandPivot = characterModelProvider.GetLeftHandPivot();
            }

            if (st.networkIdentity.IsControlling())
            {
                InventoryController.singleton.SetUpdateEquipListener(SetPlayerEquip);
            }

            UpdatePlayerEquip(playerEquipData);
        }

        public void SetPlayerEquip(List<InventoryGridData> newEqupList)
        {
            foreach (InventoryGridData newItem in newEqupList)
            {
                InventoryGridData existingItem = playerEquipData.Find(x => x.gridId == newItem.gridId);
                if (existingItem != null)
                {
                    int index = playerEquipData.IndexOf(existingItem);
                    playerEquipData[index] = newItem;
                }
                else
                {
                    playerEquipData.Add(newItem);
                }
            }

            if (characterModelProvider != null)
            {
                UpdatePlayerEquip(newEqupList);
            }
        }

        public void UpdatePlayerEquip(List<InventoryGridData> equipList)
        {
            for (int i = 0; i < equipList.Count; i++)
            {
                switch (equipList[i].gridId)
                {
                    case "leftHandGrid":
                        UpdateLeftHand(GetEquipWeapon(equipList[i]));
                        break;
                    case "rightHandGrid":
                        UpdateRightHand(GetEquipWeapon(equipList[i]));
                        break;
                    case "quickSpellGrid1":
                        UpdateQuickSpell(0, GetEquipSpell(equipList[i]));
                        break;
                    case "quickSpellGrid2":
                        UpdateQuickSpell(1, GetEquipSpell(equipList[i]));
                        break;
                    case "quickSpellGrid3":
                        UpdateQuickSpell(2, GetEquipSpell(equipList[i]));
                        break;
                    case "quickSpellGrid4":
                        UpdateQuickSpell(3, GetEquipSpell(equipList[i]));
                        break;
                }
            }
        }

        private ItemWeaponData? GetEquipWeapon(InventoryGridData grid)
        {
            ItemWeaponData item = null;
            if (grid.items.Count > 0)
            {
                string itemId = grid.items[0].item.code;
                item = ItemsManager.Instance.GetItemById(itemId) as ItemWeaponData;
            }

            return item;
        }

        private Spell? GetEquipSpell(InventoryGridData grid)
        {
            Spell item = null;
            if (grid.items.Count > 0)
            {
                string itemId = grid.items[0].item.code;
                item = ItemsManager.Instance.GetItemById(itemId) as Spell;
            }

            return item;
        }

        public void OpenAllDamageColliders()
        {
            if (leftHandWeaponHook != null)
            {
                leftHandWeaponHook.OpenDamageColliders();
            }

            if (rightHandWeaponHook != null)
            {
                rightHandWeaponHook.OpenDamageColliders();
            }
        }

        public void CloseAllDamageColliders()
        {
            if (leftHandWeaponHook != null)
            {
                leftHandWeaponHook.CloseDamageColliders();
            }

            if (rightHandWeaponHook != null)
            {
                rightHandWeaponHook.CloseDamageColliders();
            }
        }

        private void UpdateActions()
        {
            if (states == null)
            {
                return;
            }

            if (states.isTwoHanded)
            {
                states.HandleTwoHanded();
            }
            else
            {
                states.actionManager.UpdateActionsOneHanded();
            }
        }


        private void EquipWeapon(ItemWeaponData w, bool isLeft = false)
        {
            if (states == null)
            {
                return;
            }

            states.UpdateEquip(w, isLeft);
        }

        public void UpdateLeftHand(ItemWeaponData? newItem)
        {
            if (newItem == leftHandData)
            {
                return;
            }

            if (leftHandObject != null)
            {
                Destroy(leftHandObject);
                leftHandObject = null;
                leftHandData = null;
            }

            if (newItem != null && newItem.weaponModel != null)
            {
                leftHandData = newItem;
                leftHandObject = Instantiate(leftHandData.weaponModel, leftHandPivot.transform);
                leftHandObject.transform.localPosition = Vector3.zero;
                leftHandObject.transform.localRotation = Quaternion.identity;

                leftHandWeaponHook = leftHandObject.GetComponent<WeaponHook>();
            }

            UpdateActions();
            EquipWeapon(leftHandData, true);
            CloseAllDamageColliders();
        }

        public void UpdateRightHand(ItemWeaponData? newItem)
        {
            if (newItem == rightHandData)
            {
                return;
            }

            if (rightHandObject != null)
            {
                Destroy(rightHandObject);
                rightHandObject = null;
                rightHandData = null;
            }

            if (newItem != null && newItem.weaponModel != null)
            {
                rightHandData = newItem;
                rightHandObject = Instantiate(rightHandData.weaponModel, rightHandPivot.transform);
                rightHandObject.transform.localPosition = Vector3.zero;
                rightHandObject.transform.localRotation = Quaternion.identity;
                rightHandWeaponHook = rightHandObject.GetComponent<WeaponHook>();
            }

            UpdateActions();
            EquipWeapon(rightHandData, false);
            CloseAllDamageColliders();
        }

        public void UpdateQuickSpell(int id, Spell? newItem)
        {
            quickSpells[id] = newItem;

            if (id == currentSpelId)
            {
                UpdateCurrentSpell(currentSpelId);
            }
        }

        public void UpdateCurrentSpell(int spellId)
        {
            if (currentSpellParticle != null)
            {
                currentSpellParticle.SetActive(false);
                Destroy(currentSpellParticle);
                currentSpellParticle = null;
                currentSpellData = null;
            }

            Spell newItem = quickSpells[spellId];
            if (newItem == null)
            {
                return;
            }

            currentSpelId = spellId;
            currentSpellData = newItem;
            currentSpellParticle = Instantiate(currentSpellData.particlePrefab) as GameObject;
            currentSpellParticle.SetActive(false);
        }

        public void CreateSpellParticle(bool isLeft = false)
        {
            if (currentSpellData == null || currentSpellParticle == null)
            {
                return;
            }

            Transform p = isLeft ? leftHandPivot.transform : rightHandPivot.transform;
            currentSpellParticle.transform.parent = p;
            currentSpellParticle.transform.localPosition = Vector3.zero;
            currentSpellParticle.transform.localRotation = Quaternion.identity;
            currentSpellParticle.SetActive(true);
        }
    }
}
