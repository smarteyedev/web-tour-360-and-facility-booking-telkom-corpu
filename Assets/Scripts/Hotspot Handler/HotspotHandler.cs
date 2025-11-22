using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Tour360TelkomCorpu.HotspotHandler
{
    using Tour360TelkomCorpu.DataManager;

    public class HotspotHandler : ButtonInteractive
    {
        [Serializable]
        public enum HotspotType
        {
            BasicNavigation = 1,
            PointerNavigation = 2,
            OpenPanelNavigation = 3,
            OpenPanelGallery = 4
        }

        // Static dictionary: dibuat sekali saja, dipakai semua instance
        private static readonly Dictionary<HotspotType, Color> HotspotColors = new()
        {
            { HotspotType.BasicNavigation, Color.green },
            { HotspotType.PointerNavigation, Color.cyan },
            { HotspotType.OpenPanelNavigation, Color.magenta },
            { HotspotType.OpenPanelGallery, Color.yellow }
        };

        [Header("Hotspot Handler Component")]
        public HotspotType hotspotType;

        [SerializeField] private Image _imageOutline;
        [SerializeField] private CanvasGroup _canvasGroupHotspotName;
        [SerializeField] private RectTransform _rectTransform;

        private GameObject _targetPosition;
        private Camera _cam;

        // state tracking
        private bool _isTracking;

        // cache tween supaya bisa di-Kill
        private Tween _outlineTween;
        private Tween _hoverTween;


        #region Unity Lifecycle

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _cam = Camera.main;
        }

        protected override void Start()
        {
            base.Start();

            OutlineAnimation();
            HoverAnimation(false);

            onHoverExit.AddListener(() => HoverAnimation(false));
            onHoverEnter.AddListener(() => HoverAnimation(true));
        }

        private void LateUpdate()
        {
            //if (_isTracking)
            //    TrackingPosition();
        }

        #endregion


        #region Public API

        public void SetupHotspot(string hotspotName, Sprite iconSprite, Action action, Vector3 position)
        {
            _imageButton.sprite = iconSprite;
            _textButton.text = hotspotName;

            // penting untuk pooling:
            onLeftMouseDown.RemoveAllListeners();

            if (action != null)
                onLeftMouseDown.AddListener(() => action());

            //InstantiateTargetPosition(position);

            // tracking hanya aktif kalau target berhasil dibuat
            //_isTracking = _targetPosition != null;
        }

        /// <summary>
        /// Matikan hotspot: berhenti tracking, matikan target world & UI,
        /// tapi TIDAK destroy target hotspot.
        /// </summary>
        public void StopHotspot()
        {
            _isTracking = false;

            if (_targetPosition != null)
                _targetPosition.SetActive(false);

            if (_rectTransform != null)
                _rectTransform.gameObject.SetActive(false);

            // ⬅️ Ini penting untuk pooling:
            gameObject.SetActive(false);
        }

        #endregion


        #region World Target & Tracking

        private void InstantiateTargetPosition(Vector3 position)
        {
            // pastikan camera ada
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null)
                {
                    Debug.LogError("❌ MainCamera tidak ditemukan di scene!");
                    return;
                }
            }

            // hitung world position baru dari parameter 'position'
            float distanceFromCamera = 2f;

            Vector3 pos = position;
            Vector3 direction = _cam.transform.forward;
            Vector3 offset = Quaternion.Euler(
                (pos.y - 0.5f) * 180f,
                (pos.x - 0.5f) * 360f,
                1f) * direction;

            Vector3 spawnPos = _cam.transform.position + offset * distanceFromCamera;

            // ⚠️ Jika target SUDAH ada → cukup update posisinya kalau BERBEDA + hidupkan lagi
            if (_targetPosition != null && _targetPosition.scene.IsValid())
            {
                Transform t = _targetPosition.transform;

                if (t.position != spawnPos)
                {
                    t.position = spawnPos;
                    t.rotation = Quaternion.LookRotation(_cam.transform.position - spawnPos);
                }

                if (!_targetPosition.activeSelf)
                    _targetPosition.SetActive(true);

                return; // tidak buat GameObject baru
            }

            // ✅ Kalau target BELUM ada → buat GameObject baru
            _targetPosition = new GameObject("Target Position");
            Transform targetTransform = _targetPosition.transform;
            targetTransform.position = spawnPos;
            targetTransform.rotation = Quaternion.LookRotation(_cam.transform.position - spawnPos);

            Color color = HotspotColors.TryGetValue(hotspotType, out var c)
                ? c
                : Color.white;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(targetTransform, false);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.05f;

            var renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }

        private void TrackingPosition()
        {
            if (_rectTransform == null)
            {
                _isTracking = false;
                return;
            }

            if (_targetPosition == null || _cam == null)
            {
                if (_rectTransform.gameObject.activeSelf)
                    _rectTransform.gameObject.SetActive(false);

                _isTracking = false;
                return;
            }

            Vector3 screenPos = _cam.WorldToScreenPoint(_targetPosition.transform.position);

            /* if (screenPos.z <= 0f)
            {
                if (_rectTransform.gameObject.activeSelf)
                    _rectTransform.gameObject.SetActive(false);
                return;
            }

            if (!_rectTransform.gameObject.activeSelf)
                _rectTransform.gameObject.SetActive(true); */

            _rectTransform.position = screenPos;
        }

        #endregion


        #region Animations

        private void OutlineAnimation()
        {
            _outlineTween?.Kill();

            if (_imageOutline == null)
                return;

            _imageOutline.color = new Color(1f, 1f, 1f, 1f);
            _outlineTween = _imageOutline
                .DOFade(0f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void HoverAnimation(bool isHover)
        {
            _hoverTween?.Kill();

            float hoverScale = 1.2f;
            float scaleDuration = 0.2f;
            float textFadeDuration = 0.3f;
            Vector3 originalScale = Vector3.one;

            if (isHover)
            {
                if (_canvasGroupHotspotName != null)
                {
                    _canvasGroupHotspotName.gameObject.SetActive(true);
                    _hoverTween = _canvasGroupHotspotName.DOFade(1f, textFadeDuration);
                }

                transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);
            }
            else
            {
                if (_canvasGroupHotspotName != null)
                {
                    _hoverTween = _canvasGroupHotspotName
                        .DOFade(0f, textFadeDuration)
                        .OnComplete(() =>
                        {
                            _canvasGroupHotspotName.gameObject.SetActive(false);
                        });
                }

                transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InBack);
            }
        }

        #endregion
    }
}
