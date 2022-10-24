using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    public class BlurController : MonoBehaviour
    {
        public string blurProperty = "_BlurSize";
        public float blurSpeed = 3f;

        private float blurSize = 0;
        private bool canBlur = false;

        private Material temp;

        void Start()
        {
            if(gameObject.HasComponent(out Image img))
            {
                temp = new Material(img.material);

                if (temp.HasProperty(blurProperty))
                {
                    blurSize = temp.GetFloat(blurProperty);
                    canBlur = true;
                }
                else
                {
                    Debug.LogError($"[BlurController] Material shader does not have \"{blurProperty}\" property!");
                }

                GetComponent<Image>().material = temp;
            }
            else
            {
                throw new System.NullReferenceException("Could not find Image component attached to the " + gameObject.name);
            }
        }

        public void BlurMaterial(float size)
        {
            if (canBlur)
            {
                StopAllCoroutines();
                StartCoroutine(DoBlurMaterial(size));
            }
        }

        IEnumerator DoBlurMaterial(float size)
        {
            while (!Mathf.Approximately(blurSize, size))
            {
                blurSize = temp.GetFloat(blurProperty);
                temp.SetFloat(blurProperty, Mathf.MoveTowards(blurSize, size, Time.deltaTime * blurSpeed));
                yield return null;
            }

            temp.SetFloat(blurProperty, size);
        }
    }
}
