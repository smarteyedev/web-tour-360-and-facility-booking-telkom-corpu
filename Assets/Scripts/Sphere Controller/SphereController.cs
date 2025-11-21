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
    }
}
