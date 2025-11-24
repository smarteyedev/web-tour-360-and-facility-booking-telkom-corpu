using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Tour360TelkomCorpu.HotspotHandler
{
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

        [Header("Hotspot Handler")]
        [Header("Hotspot Handler | Configuration")]
        public HotspotType hotspotType;
        [Range(0f, 90f)] public float maxVisibleAngle = 15f;
        public bool hideWhenBehind = true;

        [Header("Hotspot Handler | Component References")]
        [SerializeField] private Image _imageOutline;
        [SerializeField] private CanvasGroup _canvasGroupHotspot;
        [SerializeField] private CanvasGroup _canvasGroupPlankName;
        [SerializeField] private RectTransform _rectTransform;

        private RectTransform canvasRect;
        private Canvas uiCanvas;

        private GameObject m_targetPosition;
        private Camera m_cam;

        // cache tween supaya bisa di-Kill
        private Tween m_outlineTween;
        private Tween m_hoverTween;


        #region Unity Lifecycle

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();

            OutlineAnimation();
            HoverAnimation(false);

            onHoverExit.AddListener(() => HoverAnimation(false));
            onHoverEnter.AddListener(() => HoverAnimation(true));
        }

        protected override void Enable()
        {
            base.Enable();

            HoverAnimation(false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HoverAnimation(false);
            _canvasGroupHotspot.alpha = 0f;
            m_targetPosition.SetActive(false);
        }

        private void Update()
        {
            if (m_targetPosition == null || m_cam == null || !m_targetPosition.activeSelf)
            {
                _canvasGroupHotspot.alpha = 0f;
                return;
            }

            TrackingPosition();
        }

        #endregion


        #region Public API

        public void SetupHotspot(string hotspotName, Sprite iconSprite, Action action, Vector3 position, Canvas canvas, RectTransform rct, Camera cam)
        {
            _imageButton.sprite = iconSprite;
            _textButton.text = hotspotName;

            SetupDefaultAsset(iconSprite, _textButton.color);

            uiCanvas = canvas;
            canvasRect = rct;
            m_cam = cam;

            // penting untuk pooling:
            onLeftMouseDown.RemoveAllListeners();

            if (action != null)
                onLeftMouseDown.AddListener(() => action());

            if (m_targetPosition == null)
            {
                InstantiateTargetPosition(position);
            }
            else
            {
                m_targetPosition.transform.position = position;
                m_targetPosition.SetActive(true);

                Vector3 screenPoint = m_cam.WorldToScreenPoint(m_targetPosition.transform.position);
                _rectTransform.position = screenPoint;
            }
        }

        #endregion


        #region World Target & Tracking

        private void InstantiateTargetPosition(Vector3 worldPos)
        {
            GameObject target = new GameObject($"[TargetPosition] for hotspot {_textButton.text}");
            target.transform.position = worldPos;

            m_targetPosition = target;
        }

        private void TrackingPosition()
        {
            // Cek apakah target berada di depan kamera (z > 0)
            Vector3 screenPoint = m_cam.WorldToScreenPoint(m_targetPosition.transform.position);
            if (screenPoint.z <= 0f)
            {
                if (hideWhenBehind) _canvasGroupHotspot.alpha = 0f;
                return;
            }

            // Cek sudut
            Vector3 toTarget = m_targetPosition.transform.position - m_cam.transform.position;
            float angle = Vector3.Angle(m_cam.transform.forward, toTarget);
            if (angle > maxVisibleAngle) { _canvasGroupHotspot.alpha = 0f; return; }

            _canvasGroupHotspot.alpha = 1f;

            if (canvasRect != null && uiCanvas != null)
            {
                Vector2 localPoint;
                bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, m_cam, out localPoint);
                if (ok) _rectTransform.anchoredPosition = localPoint;
                else _rectTransform.position = screenPoint;
            }
            else
            {
                _rectTransform.position = screenPoint;
            }
        }

        #endregion


        #region Animations
        private void OutlineAnimation()
        {
            m_outlineTween?.Kill();

            if (_imageOutline == null)
                return;

            _imageOutline.color = new Color(1f, 1f, 1f, 1f);
            m_outlineTween = _imageOutline
                .DOFade(0f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void HoverAnimation(bool isHover)
        {
            m_hoverTween?.Kill();

            float hoverScale = 1.2f;
            float scaleDuration = 0.2f;
            float textFadeDuration = 0.3f;
            Vector3 originalScale = Vector3.one;

            if (isHover)
            {
                if (_canvasGroupPlankName != null)
                {
                    _canvasGroupPlankName.gameObject.SetActive(true);
                    m_hoverTween = _canvasGroupPlankName.DOFade(1f, textFadeDuration);
                }

                transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);
            }
            else
            {
                if (_canvasGroupPlankName != null)
                {
                    m_hoverTween = _canvasGroupPlankName
                        .DOFade(0f, textFadeDuration)
                        .OnComplete(() =>
                        {
                            _canvasGroupPlankName.gameObject.SetActive(false);
                        });
                }

                transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InBack);
            }
        }
        #endregion
    }
}
