using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Right Hand Weapon")]
        public GameObject rightHandPivot;
        public ItemWeaponData rightHandData;
        [HideInInspector]
        public GameObject rightHandObject;
        [HideInInspector]
        public WeaponHook rightHandWeaponHook;

        [Header("Left Hand Weapon")]
        public GameObject leftHandPivot;
        public ItemWeaponData leftHandData;
        [HideInInspector]
        public GameObject leftHandObject;
        [HideInInspector]
        public WeaponHook leftHandWeaponHook;

        [Header("Selected Spell")]
        public ItemWeaponData currentSpellData;
        [HideInInspector]
        public GameObject currentSpellParticle;

        private StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            UpdateLeftHand(leftHandData);
            UpdateRightHand(rightHandData);
        }

        public void OpenDamageColliders()
        {

        }

        public void CloseDamageColliderss()
        {

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
            leftHandObject.transform.localPosition = new Vector3(0, 0, 0);
            leftHandObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

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
            rightHandObject.transform.localPosition = new Vector3(0, 0, 0);
            rightHandObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            rightHandWeaponHook = rightHandObject.GetComponent<WeaponHook>();

            UpdateActions();
            EquipWeapon(rightHandData, false);
            CloseAllDamageColliders();
        }
    }
}
