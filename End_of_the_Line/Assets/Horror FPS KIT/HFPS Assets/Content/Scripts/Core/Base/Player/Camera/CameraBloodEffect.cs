using UnityEngine;

namespace HFPS.Player
{
    public class CameraBloodEffect : MonoBehaviour
    {
        private Material material = null;

        public Texture2D bloodTexture;
        public Texture2D bloodNormalMap;

        [Range(0.0f, 1.0f)]
        public float bloodAmount = 0.0f;

        [Range(0.0f, 1.0f)]
        public float distortion = 1.0f;

        public Shader bloodShader = null;

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            if (bloodShader == null) return;
            if (material == null)
            {
                material = new Material(bloodShader);
            }

            if (material == null) return;

            if (bloodTexture != null)
            {
                material.SetTexture("_BloodTex", bloodTexture);
            }

            if (bloodNormalMap != null)
            {
                material.SetTexture("_BloodBump", bloodNormalMap);
            }

            material.SetFloat("_Distortion", distortion);
            material.SetFloat("_BloodAmount", bloodAmount);
            Graphics.Blit(source, dest, material);
        }
    }
}