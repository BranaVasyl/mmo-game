using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BV
{
    public class EnemyUI : MonoBehaviour
    {
        public Slider healthSlider;
        public Transform enemyTransform;

        float closeTimer;

        public static EnemyUI sinleton;
        private void Awake()
        {
            sinleton = this;
        }

        private void Update()
        {
            if (enemyTransform == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            transform.position = enemyTransform.position;

            Vector3 v = Camera.main.transform.position - transform.position;
            v.x = v.z = 0.0f;
            transform.LookAt(Camera.main.transform.position - v);
            transform.Rotate(0, 180, 0);

            if (closeTimer < 4)
            {
                closeTimer += Time.deltaTime;
            }
            else
            {
                closeTimer = 0;
                gameObject.SetActive(false);
            }
        }
    }
}
