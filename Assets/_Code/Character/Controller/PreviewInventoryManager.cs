using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class PreviewInventoryManager : MonoBehaviour
    {
        private Animator anim;
        private CharacterModelController characterModelController;

        GameObject rightHandObject;
        ItemWeaponData rightHandItem;

        GameObject leftHandObject;
        ItemWeaponData leftHandItem;

        public void SetPlayerEquip(PlayerData playerData, GameObject character)
        {
            anim = character.GetComponent<Animator>();
            characterModelController = character.GetComponent<CharacterModelController>();

            if (characterModelController == null || anim == null)
            {
                return;
            }

            ChangeRightHandItem(GetEquipWeapon("rightHandGrid", playerData.playerEquipData));
            ChangeLeftHandItem(GetEquipWeapon("leftHandGrid", playerData.playerEquipData));
            HandleTwoHanded(playerData.isTwoHanded);

            Clean();
        }

        private ItemWeaponData? GetEquipWeapon(string gridId, List<InventoryGridData> playerEquipData)
        {
            InventoryGridData grid = playerEquipData.Find(x => x.gridId == gridId);
            if (grid != null && grid.items.Count > 0)
            {
                string itemId = grid.items[0].item.code;
                return ItemsManager.Instance.GetItemById(itemId) as ItemWeaponData;
            }

            return null;
        }

        private void ChangeRightHandItem(ItemWeaponData? item)
        {
            if (characterModelController != null)
            {
                characterModelController.ClearRightHand();
                GameObject rightHandPivot = characterModelController.GetRightHandPivot();
            }

            if (item == null || characterModelController == null)
            {
                anim.Play("Empty Right");
                return;
            }

            rightHandItem = item;

            rightHandObject = Instantiate(item.weaponModel, characterModelController.GetRightHandPivot().transform);
            rightHandObject.transform.localPosition = Vector3.zero;
            rightHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, false);
        }

        private void ChangeLeftHandItem(ItemWeaponData? item)
        {
            if (characterModelController != null)
            {
                characterModelController.ClearLeftHand();
                GameObject leftHandPivot = characterModelController.GetLeftHandPivot();
            }

            if (item == null || characterModelController == null)
            {
                anim.Play("Empty Left");
                return;
            }

            leftHandItem = item;

            leftHandObject = Instantiate(item.weaponModel, characterModelController.GetLeftHandPivot().transform);
            leftHandObject.transform.localPosition = Vector3.zero;
            leftHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, true);
        }

        private void EquipWeapon(ItemWeaponData w, bool isLeft = false)
        {
            if (!anim)
            {
                return;
            }

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

            anim.SetBool("mirror", isLeft);
            anim.Play(targetIdle);
        }

        public void HandleTwoHanded(bool isTwoHanded)
        {
            anim.SetBool("twoHanded", isTwoHanded);

            bool isRight = rightHandItem != null;
            ItemWeaponData currentWeapon = isRight ? rightHandItem : leftHandItem;

            if (currentWeapon == null)
            {
                return;
            }

            if (isTwoHanded)
            {
                anim.Play(currentWeapon.th_idle_name);

                if (isRight)
                {
                    if (leftHandObject != null)
                    {
                        leftHandObject.SetActive(false);
                    }
                }
                else
                {
                    if (rightHandObject != null)
                    {
                        rightHandObject.SetActive(false);
                    }
                }
            }
            else
            {
                anim.Play("Empty Both");
            }
        }

        void Clean()
        {
            rightHandItem = null;
            rightHandObject = null;

            leftHandItem = null;
            leftHandObject = null;
        }
    }
}