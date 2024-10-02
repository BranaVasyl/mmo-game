using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class InventoryManager : MonoBehaviour
    {
        public ItemsManager itemsManager;
        private List<CharacterInventoryData> playerEquipData;

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
            itemsManager = ItemsManager.Instance;

            characterModelProvider = st.activeModel.GetComponent<CharacterModelController>();
            if (characterModelProvider)
            {
                rightHandPivot = characterModelProvider.GetRightHandPivot();
                leftHandPivot = characterModelProvider.GetLeftHandPivot();
            }

            UpdatePlayerEquip();
        }

        public void SetPlayerEquip(List<CharacterInventoryData> pED)
        {
            playerEquipData = pED;

            if (characterModelProvider != null)
            {
                UpdatePlayerEquip();
            }
        }

        public void UpdatePlayerEquip()
        {
            if (playerEquipData == null)
            {
                Debug.LogWarning("Not found player equip data");
                return;
            }

            CharacterInventoryData leftHandGrid = playerEquipData.Find(x => x.gridId == "leftHandGrid");
            if (leftHandGrid != null)
            {
                List<InventoryItemData> items = leftHandGrid.items;
                ItemWeaponData item = null;
                if (items.Count > 0)
                {
                    string itemId = items[0].id;
                    item = itemsManager.GetItemById(itemId) as ItemWeaponData;
                }

                UpdateLeftHand(item);
            }

            CharacterInventoryData rightHandGrid = playerEquipData.Find(x => x.gridId == "rightHandGrid");
            if (rightHandGrid != null)
            {
                List<InventoryItemData> items = rightHandGrid.items;
                ItemWeaponData item = null;
                if (items.Count > 0)
                {
                    string itemId = items[0].id;
                    item = itemsManager.GetItemById(itemId) as ItemWeaponData;
                }

                UpdateRightHand(item);
            }

            for (int i = 0; i < 4; i++)
            {
                CharacterInventoryData quickSpellGrid = playerEquipData.Find(x => x.gridId == "quickSpellGrid" + (i + 1));
                if (quickSpellGrid != null)
                {
                    List<InventoryItemData> items = quickSpellGrid.items;
                    Spell item = null;
                    if (items.Count > 0)
                    {
                        string itemId = items[0].id;
                        item = itemsManager.GetItemById(itemId) as Spell;
                    }

                    UpdateQuickSpell(i, item);
                }
            }
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
            if (states.isTwoHanded)
            {
                states.isTwoHanded = false;
                states.HandleTwoHanded();
            }
            else
            {
                states.actionManager.UpdateActionsOneHanded();
            }
        }

        private void EquipWeapon(ItemWeaponData w, bool isLeft = false)
        {
            string targetIdle = w.oh_idle_name;
            if (targetIdle.Length > 0)
            {
                targetIdle += isLeft ? "_l" : "_r";
            }
            else
            {
                if (isLeft)
                {
                    targetIdle = "Empty Left";
                }
                else
                {
                    targetIdle = "Empty Right";
                }
            }

            states.anim.SetBool("mirror", isLeft);
            states.anim.Play("changeWeapon");
            states.anim.Play(targetIdle);
        }

        public void UpdateLeftHand(ItemWeaponData? newItem)
        {
            if (leftHandObject != null)
            {
                leftHandObject.SetActive(false);
                Destroy(leftHandObject);
                leftHandObject = null;
                leftHandData = null;
                UpdateActions();
            }

            if (newItem == null || newItem.weaponModel == null)
            {
                states.anim.Play("Empty Left");
                return;
            }

            leftHandData = newItem;
            leftHandObject = Instantiate(leftHandData.weaponModel, leftHandPivot.transform);
            leftHandObject.transform.localPosition = Vector3.zero;
            leftHandObject.transform.localRotation = Quaternion.identity;

            leftHandWeaponHook = leftHandObject.GetComponent<WeaponHook>();

            UpdateActions();
            EquipWeapon(leftHandData, true);
            CloseAllDamageColliders();
        }

        public void UpdateRightHand(ItemWeaponData? newItem)
        {
            if (rightHandObject != null)
            {
                rightHandObject.SetActive(false);
                Destroy(rightHandObject);
                rightHandObject = null;
                rightHandData = null;
                UpdateActions();
            }

            if (newItem == null || newItem.weaponModel == null)
            {
                states.anim.Play("Empty Right");
                return;
            }

            rightHandData = newItem;
            rightHandObject = Instantiate(rightHandData.weaponModel, rightHandPivot.transform);
            rightHandObject.transform.localPosition = Vector3.zero;
            rightHandObject.transform.localRotation = Quaternion.identity;
            rightHandWeaponHook = rightHandObject.GetComponent<WeaponHook>();

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
