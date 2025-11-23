using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


namespace Tour360TelkomCorpu.SphereController
{
    public class SphereController : MonoBehaviour
    {
        [SerializeField] private Material _Material; //ambil nilai ini 
        [SerializeField] private Transform _sphereTransform; // assign sphere GameObject
        [SerializeField] private float _surfaceOffset = 0.01f; // small offset to avoid clipping

        public void ChangeTextureWithFade(Texture targetTexture, Action onStartTransition, Action onFinishTransition)
        {
            float transitionDuration = 1.0f;
            Tween currentTween;
            bool useFadeTransition = true;

            onStartTransition?.Invoke();

            if (useFadeTransition)
            {
                float blendValue = _Material.GetFloat("_Blend");
                //Debug.Log("Current Blend Value: " + blendValue);

                bool usingTexture = blendValue <= 0.5f;
                float endBlend = usingTexture ? 1 : 0;

                if (usingTexture)
                {
                    _Material.SetTexture("_Texture_B", targetTexture);
                }
                else
                {
                    _Material.SetTexture("_Texture_A", targetTexture);
                }

                currentTween = DOTween.To(
                    () => _Material.GetFloat("_Blend"),
                    x => _Material.SetFloat("_Blend", x),
                    endBlend,
                    transitionDuration
                )
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    usingTexture = !usingTexture;
                    _Material.SetFloat("_Blend", usingTexture ? 0 : 1);
                    Debug.Log("Transition Completed. Final Blend Value: " + _Material.GetFloat("_Blend"));
                    onFinishTransition?.Invoke();
                });
            }
            else
            {
                Debug.Log("Transisi Zoom dijalankan!");

                Vector3 startScale = transform.localScale;
                Vector3 zoomOut = startScale * 1.2f;
                Vector3 zoomIn = startScale;

                transform.DOScale(zoomOut, transitionDuration / 2)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        // Ganti tekstur saat di tengah animasi zoom
                        _Material.SetTexture("_Texture_A", targetTexture);
                        _Material.SetTexture("_Texture_B", targetTexture);

                        transform.DOScale(zoomIn, transitionDuration / 2)
                            .SetEase(Ease.InOutQuad)
                            .OnComplete(() =>
                            {
                                Debug.Log("Zoom Transition Completed.");
                                onFinishTransition?.Invoke();
                            });
                    });
            }
        }

        // Hitung radius world sphere
        public float GetSphereRadius()
        {
            if (_sphereTransform == null) return 1f;
            MeshFilter mf = _sphereTransform.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Bounds b = mf.sharedMesh.bounds;
                float localMax = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
                Vector3 lossy = _sphereTransform.lossyScale;
                float maxScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
                return localMax * maxScale;
            }
            // fallback for unit sphere
            Vector3 ls = _sphereTransform.lossyScale;
            return 0.5f * Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.y), Mathf.Abs(ls.z));
        }

        /// <summary>
        /// Convert UV (0..1) to world position on/near sphere surface.
        /// u = x, v = y.
        /// </summary>
        public Vector3 UVToWorldPosition(float u, float v, float extraOffset = 0f)
        {
            if (_sphereTransform == null) return Vector3.zero;

            u = Mathf.Repeat(u, 1f);
            v = Mathf.Clamp01(v);

            float theta = u * Mathf.PI * 2f;
            float phi = (0.5f - v) * Mathf.PI; // flip vertical if needed: phi = (v - 0.5f) * PI

            float cosPhi = Mathf.Cos(phi);
            Vector3 dirLocal = new Vector3(
                cosPhi * Mathf.Sin(theta),
                Mathf.Sin(phi),
                cosPhi * Mathf.Cos(theta)
            ).normalized;

            float radius = GetSphereRadius();
            Debug.Log($"radius: {radius}");
            float r = radius + _surfaceOffset + extraOffset;

            // transform direction to world (handles sphere rotation)
            Vector3 dirWorld = _sphereTransform.TransformDirection(dirLocal);
            Vector3 center = _sphereTransform.position;

            return center + dirWorld * r;
        }
    }
}
