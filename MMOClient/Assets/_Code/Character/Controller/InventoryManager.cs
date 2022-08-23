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
        public GameObject rightHandObject;
        [HideInInspector]
        public WeaponHook rightHandWeaponHook;

        [Header("Left Hand Weapon")]
        public GameObject leftHandPivot;
        public ItemWeaponData leftHandData;
        public GameObject leftHandObject;
        [HideInInspector]
        public WeaponHook leftHandWeaponHook;

        private StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            UpdateRightHand(rightHandData);
            UpdateRightHand(leftHandData);
        }

        public void OpenDamageColliders()
        {
            if (rightHandObject == null)
            {
                return;
            }

            rightHandObject.GetComponent<WeaponHook>().OpenDamageColliders();
        }

        public void CloseDamageColliders()
        {
            if (leftHandObject != null)
            {
                leftHandWeaponHook.CloseDamageColliders();
            }

            if (rightHandObject != null)
            {
                leftHandWeaponHook.CloseDamageColliders();
            }

        }

        public void UpdateLeftHand(ItemWeaponData? newItem)
        {
            if (leftHandObject != null)
            {
                leftHandObject.SetActive(false);
                Destroy(leftHandObject, 0.1f);
                leftHandObject = null;
            }

            if (newItem == null || newItem.weaponModel == null)
            {
                return;
            }

            leftHandData = newItem;

            leftHandObject = Instantiate(leftHandData.weaponModel, leftHandPivot.transform);
            leftHandObject.transform.localPosition = new Vector3(0, 0, 0);
            leftHandObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

            leftHandWeaponHook = leftHandObject.GetComponent<WeaponHook>();
            CloseDamageColliders();
            Debug.Log("UpdateLeftHand");
        }

        public void UpdateRightHand(ItemWeaponData? newItem)
        {
            if (rightHandObject != null)
            {
                rightHandObject.SetActive(false);
                Destroy(rightHandObject, 0.1f);
                rightHandObject = null;
            }

            if (newItem == null || newItem.weaponModel == null)
            {
                return;
            }

            rightHandData = newItem;

            rightHandObject = Instantiate(rightHandData.weaponModel, rightHandPivot.transform);
            rightHandObject.transform.localPosition = new Vector3(0, 0, 0);
            rightHandObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

            rightHandWeaponHook = rightHandObject.GetComponent<WeaponHook>();
            CloseDamageColliders();

            if (states.isTwoHanded)
            {
                states.actionManager.UpdateActionsTwoHanded();
            }
            else
            {
                states.actionManager.UpdateActionsOneHanded();
            }
            Debug.Log("UpdateRightHand");
        }
    }
}
