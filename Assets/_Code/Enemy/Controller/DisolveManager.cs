using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class DisolveManager : MonoBehaviour
    {
        [Header("Objects to Dissolve")]
        public List<GameObject> targets = new List<GameObject>();

        [Header("Shader Property")]
        public string dissolveProperty = "_DissolveAmount";

        [Header("Test Slider (-1 to 1)")]
        [Range(-1f, 1f)]
        public float testDissolveValue = -1f;

        [Header("Dissolve Animation")]
        public float dissolveSpeed = 1f;

        Dictionary<Renderer, MaterialPropertyBlock> blocks = new Dictionary<Renderer, MaterialPropertyBlock>();

        void Start()
        {
            CacheRenderers();
            ApplyDissolveToAll(testDissolveValue);
        }

        void OnValidate()
        {
            if (!Application.isPlaying) return;
            ApplyDissolveToAll(testDissolveValue);
        }

        void CacheRenderers()
        {
            blocks.Clear();

            foreach (var obj in targets)
            {
                if (obj == null) continue;

                Renderer rend = obj.GetComponentInChildren<Renderer>();
                if (rend == null) continue;

                if (!blocks.ContainsKey(rend))
                    blocks[rend] = new MaterialPropertyBlock();
            }
        }

        void ApplyDissolveToAll(float value)
        {
            foreach (var kvp in blocks)
            {
                Renderer rend = kvp.Key;
                MaterialPropertyBlock block = kvp.Value;

                rend.GetPropertyBlock(block);
                block.SetFloat(dissolveProperty, value);
                rend.SetPropertyBlock(block);
            }
        }

        public void Dissolve()
        {
            StopAllCoroutines();

            foreach (var obj in targets)
            {
                if (obj == null) continue;
                StartCoroutine(DissolveRoutine(obj));
            }
        }

        public void DissolveGameObject(GameObject obj)
        {
            StartCoroutine(DissolveRoutine(obj));
        }

        IEnumerator DissolveRoutine(GameObject obj)
        {
            if (obj == null) yield break;

            Renderer rend = obj.GetComponentInChildren<Renderer>();
            if (rend == null) yield break;

            if (!blocks.ContainsKey(rend))
                blocks[rend] = new MaterialPropertyBlock();

            MaterialPropertyBlock block = blocks[rend];

            float t = -1f;

            while (t < 1f)
            {
                t += Time.deltaTime * dissolveSpeed;

                rend.GetPropertyBlock(block);
                block.SetFloat(dissolveProperty, t);
                rend.SetPropertyBlock(block);

                yield return null;
            }
        }
    }
}
