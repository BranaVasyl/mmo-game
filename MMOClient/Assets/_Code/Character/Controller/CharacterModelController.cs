using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class CharacterModelController : MonoBehaviour
    {
        public GameObject rightHandPivot;
        public GameObject leftHandPivot;

        public AvaibleCharacterCustomization avaibleCharacterCustomization;

        public GameObject GetRightHandPivot()
        {
            return rightHandPivot;
        }

        public GameObject GetLeftHandPivot()
        {
            return leftHandPivot;
        }

        public AvaibleCharacterCustomization GetAvaibleCharacterCustomization()
        {
            return avaibleCharacterCustomization;
        }

        public void UpdateCharacterCustomization(CharacterCustomizationData customization)
        {
            ApplyCharacterCustomization(customization);
        }

        public void SetCharacterCustomization(CharacterCustomizationData customization)
        {
            ApplyCharacterCustomization(customization, true);
        }

        private void ApplyCharacterCustomization(CharacterCustomizationData customization, bool removeAfterAply = false)
        {
            ApplyCharacterHairStyle(customization.hairId, removeAfterAply);
            ApplyCharacterHairColor(customization.hairColor);
        }

        private void ApplyCharacterHairStyle(string id, bool removeAfterAply = false)
        {
            GameObjectById[] hairList = avaibleCharacterCustomization.hairList;

            foreach (GameObjectById hair in hairList)
            {
                if (hair.gameObject == null)
                {
                    continue;
                }

                if (hair.id == id)
                {
                    hair.gameObject.SetActive(true);
                }
                else
                {
                    if (removeAfterAply)
                    {
                        Destroy(hair.gameObject);
                    }
                    else
                    {
                        hair.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void ApplyCharacterHairColor(string hexColor)
        {
            Color newColor;
            if (!string.IsNullOrEmpty(hexColor))
            {
                ColorUtility.TryParseHtmlString(hexColor, out newColor);
            }
            else
            {
                newColor = Color.white;
            }

            GameObjectById[] hairList = avaibleCharacterCustomization.hairList;
            foreach (GameObjectById hair in hairList)
            {
                if (hair.gameObject == null || !hair.gameObject.activeSelf)
                {
                    continue;
                }

                List<GameObject> gameObjectList = CollectRenderersFromGameObjects(hair.gameObject);
                foreach (GameObject gameObject in gameObjectList)
                {
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material instanceMaterial = new Material(materials[i])
                        {
                            color = newColor
                        };

                        materials[i] = instanceMaterial;
                    }

                    renderer.sharedMaterials = materials;
                }
            }
        }

        public List<GameObject> CollectRenderersFromGameObjects(GameObject obj)
        {
            List<GameObject> gameObjectList = new List<GameObject>();

            if (obj.activeSelf)
            {
                if (obj.GetComponent<Renderer>() != null)
                {
                    gameObjectList.Add(obj);
                }

                foreach (Transform child in obj.transform)
                {
                    gameObjectList.AddRange(CollectRenderersFromGameObjects(child.gameObject));
                }
            }

            return gameObjectList;
        }
    }

    [Serializable]
    public class CharacterCustomizationData
    {
        public string hairId;
        public string hairColor;
    }

    [Serializable]
    public class AvaibleCharacterCustomization
    {
        public GameObjectById[] hairList;
        public Color[] hairCollorPallete;
    }
}
