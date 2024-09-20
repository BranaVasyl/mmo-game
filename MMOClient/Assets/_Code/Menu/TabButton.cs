using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BV
{

    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public TabGroup tabGroup;
        [HideInInspector]
        public Image background;
        [HideInInspector]
        public Image icon;

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
        }

        void Awake()
        {
            icon = transform.GetChild(0).GetComponent<Image>();
            background = GetComponent<Image>();

            Debug.Log(this.transform.GetSiblingIndex());
            tabGroup.Subscribe(this, this.transform.GetSiblingIndex());
        }
    }
}
