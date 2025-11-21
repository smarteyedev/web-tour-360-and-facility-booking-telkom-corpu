using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;

namespace Tour360TelkomCorpu.HotspotHandler
{
    using Tour360TelkomCorpu.DataManager;

    public class HotspotHandler : ButtonInteractive
    {
       
        [Header("Hotspot Handler Component")]
        public TargetHotspot hotspotType;

        private GameObject m_targetPosition;
        [SerializeField] private Image _imageOutline;

        [SerializeField] private Image _imageBgHotspotName;


        public void SetupHotspot(string hotspotName, Sprite iconSprite, Action action, Vector3 position)
        {
    
           
            _imageButton.sprite = iconSprite;
            _textButton.text = hotspotName;

            onLeftMouseDown.AddListener(() => action?.Invoke());

            InstantiateTargetPosition(position);
            TrackingPosition();

        }

        private void InstantiateTargetPosition(Vector3 position)
        {

            if (m_targetPosition != null && m_targetPosition.scene.IsValid())
            {
                Debug.Log($"⚠️ Target {hotspotType} sudah dibuat, lewati!");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("❌ MainCamera tidak ditemukan di scene!");
                return;
            }

            var hotspotColors = new Dictionary<TargetHotspot, Color>
        {
            { TargetHotspot.BUILDING, Color.green },
            { TargetHotspot.FACILITY, Color.cyan },
            { TargetHotspot.PANEL_NAVIGATION, Color.magenta },
            { TargetHotspot.PANEL_GALLERY, Color.yellow }
        };

            Vector3 pos = position;
            Color color = hotspotColors.ContainsKey(hotspotType) ? hotspotColors[hotspotType] : Color.white;

            float distanceFromCamera = 2f;

            Vector3 direction = cam.transform.forward;
            Vector3 offset = Quaternion.Euler(
                (pos.y - 0.5f) * 180f,
                (pos.x - 0.5f) * 360f,
                1) * direction;

            Vector3 spawnPos = cam.transform.position + offset * distanceFromCamera;

            GameObject worldTarget = new GameObject("Target Position");
            worldTarget.transform.position = spawnPos;
            worldTarget.transform.rotation = Quaternion.LookRotation(cam.transform.position - spawnPos);

            m_targetPosition = worldTarget;

            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.SetParent(worldTarget.transform, false);
            s.transform.localPosition = Vector3.zero;
            s.transform.localScale = Vector3.one * 0.05f;
            s.GetComponent<Renderer>().material.color = color;

            Debug.Log($"✅ Dibuat GameObject baru untuk target {hotspotType} di posisi {spawnPos}");
        }

        private void TrackingPosition()
        {
            if (m_targetPosition == null || Camera.main == null)
                return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(m_targetPosition.transform.position);
            RectTransform rect = GetComponent<RectTransform>();

            if (screenPos.z < 0)
            {
                if (rect != null)
                    rect.gameObject.SetActive(false);
                return;
            }

            if (rect != null)
            {
                rect.gameObject.SetActive(true);
                rect.position = screenPos;
            }
        }
  
        private void OutlineAnimation()
        {
           

            float startRingScale = 1.0f;
            float targetRingScale = 1.3f;
            float ringFadeDuration = 0.5f;
            float ringDelay = 0.2f;


            if (hotspotType == TargetHotspot.BUILDING)
            {
                _imageOutline.color = new Color(1, 1, 1, 1);
                _imageOutline.DOFade(0f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                Sequence ringSequence = DOTween.Sequence();
                ringSequence.Append(_imageOutline.rectTransform.DOScale(targetRingScale, ringFadeDuration))
                                .Join(_imageOutline.DOFade(1f, ringFadeDuration))
                                .SetEase(Ease.OutQuad);

                ringSequence.Append(_imageOutline.DOFade(0f, ringFadeDuration))
                            .SetEase(Ease.InQuad);

                ringSequence.AppendCallback(() =>
                {
                    _imageOutline.rectTransform.localScale = Vector3.one * startRingScale;
                    _imageOutline.color = new Color(_imageOutline.color.r, _imageOutline.color.g, _imageOutline.color.b, 0f);
                });

                ringSequence.AppendInterval(ringDelay);
                ringSequence.SetLoops(-1);
               
            }
            
           
        }
        private void HoverAnimation(bool isHover)
        {
            float hoverScale = 1.2f;
            float scaleDuration = 0.2f;
            float textFadeDuration = 0.3f;
            Vector3 originalScale = Vector3.one;

            Sequence ringSequence = DOTween.Sequence();

            if (isHover)
            {

                if (_imageBgHotspotName != null)
                {
                    _imageBgHotspotName.gameObject.SetActive(true);
                    _imageBgHotspotName.DOFade(1f, textFadeDuration);
                }

                transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);
            }
            else
            {
                ringSequence.Kill();


                if (_imageBgHotspotName != null)
                {
                    _imageBgHotspotName.DOFade(0f, textFadeDuration).OnComplete(() =>
                    {
                        _imageBgHotspotName.gameObject.SetActive(false);
                    });
                }

                transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InBack);
            }
        }

        protected override void Start()
        {
            base.Start();
            OutlineAnimation();
            HoverAnimation(false);
            onHoverExit.AddListener(() => HoverAnimation(false));
            onHoverEnter.AddListener(() => HoverAnimation(true));
        }
    }
}
