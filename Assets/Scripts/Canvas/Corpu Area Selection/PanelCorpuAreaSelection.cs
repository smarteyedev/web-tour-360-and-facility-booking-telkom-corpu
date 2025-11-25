using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tour360TelkomCorpu.DataManager;
using UnityEngine.EventSystems;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelCorpuAreaSelection : PanelController<List<TelkomCorpuAreaCard>, Action>, IBeginDragHandler, IEndDragHandler
    {
        [Space(10f)]
        [Header("Corpu Area Selection Configuration")]

        [Header("Carousel Settings")]
        [SerializeField] private float _swipeThreshold = 50f;
        [SerializeField] private float _snapSpeed = 10f;
        [SerializeField] private bool _loop = false;

        private int m_currentIndex = 0;
        private int m_pageCount = 0;
        private float[] m_pagePositions;
        private Vector2 m_dragStartPos;

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private RectTransform _rectContentParent;
        [SerializeField] private CorpuSelectionCard _prefabCorpuSelectionCard;
        private readonly List<CorpuSelectionCard> m_cardPooling = new List<CorpuSelectionCard>();

        private Coroutine m_snapRoutine;

        protected override void Awake()
        {
            if (!_scrollRect)
                _scrollRect = GetComponentInChildren<ScrollRect>();
        }

        protected override void ShowPanel(List<TelkomCorpuAreaCard> cardData, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            _panelContainer.gameObject.SetActive(true);

            if (cardData == default) return;

            for (int i = 0; i < cardData.Count; i++)
            {
                TelkomCorpuAreaCard D = cardData[i];

                CorpuSelectionCard card;
                if (i < m_cardPooling.Count)
                {
                    card = m_cardPooling[i];
                }
                else
                {
                    card = Instantiate(_prefabCorpuSelectionCard, _rectContentParent, false);
                    if (i >= m_cardPooling.Count) m_cardPooling.Add(card);
                }

                if (!card.gameObject.activeSelf) card.gameObject.SetActive(true);
                card.SetupCard(
                    bgCard: D.thumbnail_image.GetSpriteImage(),
                    cardName: D.thumbnail_name,
                    address: D.address,
                    isOpenForVisitor: D.open_for_visitor,
                    onClickAction: () => callbackUsingDocumentId?.Invoke($"{D.documentId}")
                );
                if (card.transform.GetSiblingIndex() != i) card.transform.SetSiblingIndex(i);
            }

            for (int i = cardData.Count; i < m_cardPooling.Count; i++)
                if (m_cardPooling[i] && m_cardPooling[i].gameObject.activeSelf) m_cardPooling[i].gameObject.SetActive(false);

            SetupCarousel();
        }

        public override void HidePanel()
        {
            _panelContainer.gameObject.SetActive(false);
        }

        private void SetupCarousel()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectContentParent);

            if (_rectContentParent.childCount <= 0)
            {
                Debug.LogWarning("PanelCorpuAreaSeleciton: Content tidak punya child.");
                enabled = false;
                return;
            }

            RectTransform firstCard = (RectTransform)_rectContentParent.GetChild(0);
            float cardWidth = firstCard.rect.width;
            float spacing = _rectContentParent.GetComponent<HorizontalLayoutGroup>().spacing;

            float viewportWidth = _scrollRect.viewport != null
                    ? _scrollRect.viewport.rect.width
                    : ((RectTransform)_scrollRect.transform).rect.width;

            float contentWidth = _rectContentParent.rect.width;
            float scrollRange = Mathf.Max(0f, contentWidth - viewportWidth);

            float stepWorld = cardWidth + spacing;
            int maxStepIndex = Mathf.CeilToInt(scrollRange / stepWorld);
            m_pageCount = Mathf.Max(1, maxStepIndex + 1);

            m_pagePositions = new float[m_pageCount];

            float stepNormalized = stepWorld / scrollRange;

            for (int i = 0; i < m_pageCount; i++)
            {
                float p = i * stepNormalized;

                // pastikan step terakhir benar-benar 1 (ujung kanan)
                if (i == m_pageCount - 1)
                    p = 1f;

                m_pagePositions[i] = Mathf.Clamp01(p);
            }

            // posisi awal
            m_currentIndex = 0;
            SetPageImmediate(0);

            // tombol
            if (_nextButton) _nextButton.onClick.AddListener(NextPage);
            if (_prevButton) _prevButton.onClick.AddListener(PreviousPage);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_dragStartPos = eventData.position;

            // hentikan snap yang mungkin masih jalan
            if (m_snapRoutine != null)
            {
                StopCoroutine(m_snapRoutine);
                m_snapRoutine = null;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float deltaX = eventData.position.x - m_dragStartPos.x;

            if (Mathf.Abs(deltaX) > _swipeThreshold)
            {
                if (deltaX < 0)
                    NextPage();
                else
                    PreviousPage();
            }
            else
            {
                // swipe pendek → snap ke page terdekat
                int nearest = GetNearestPage(_scrollRect.horizontalNormalizedPosition);
                SetPage(nearest);
            }
        }

        int GetNearestPage(float currentPos)
        {
            int nearest = 0;
            float smallest = Mathf.Abs(currentPos - m_pagePositions[0]);

            for (int i = 1; i < m_pageCount; i++)
            {
                float distance = Mathf.Abs(currentPos - m_pagePositions[i]);
                if (distance < smallest)
                {
                    smallest = distance;
                    nearest = i;
                }
            }

            return nearest;
        }

        public void NextPage()
        {
            SetPage(m_currentIndex + 1);
        }

        public void PreviousPage()
        {
            SetPage(m_currentIndex - 1);
        }

        void SetPage(int index)
        {
            if (_loop)
            {
                if (index < 0) index = m_pageCount - 1;
                if (index >= m_pageCount) index = 0;
            }
            else
            {
                index = Mathf.Clamp(index, 0, m_pageCount - 1);
            }

            m_currentIndex = index;

            float targetPos = m_pagePositions[m_currentIndex];
            _scrollRect.velocity = Vector2.zero;

            // mulai coroutine snap (hanya jalan saat perlu)
            if (m_snapRoutine != null) StopCoroutine(m_snapRoutine);
            m_snapRoutine = StartCoroutine(SnapTo(targetPos));
        }

        void SetPageImmediate(int index)
        {
            index = Mathf.Clamp(index, 0, m_pageCount - 1);
            m_currentIndex = index;

            float targetPos = m_pagePositions[m_currentIndex];

            if (m_snapRoutine != null) StopCoroutine(m_snapRoutine);
            m_snapRoutine = null;

            _scrollRect.horizontalNormalizedPosition = targetPos;
            _scrollRect.velocity = Vector2.zero;
        }

        private IEnumerator SnapTo(float targetPos)
        {
            while (Mathf.Abs(_scrollRect.horizontalNormalizedPosition - targetPos) > 0.001f)
            {
                float pos = Mathf.Lerp(
                    _scrollRect.horizontalNormalizedPosition,
                    targetPos,
                    Time.deltaTime * _snapSpeed
                );

                _scrollRect.horizontalNormalizedPosition = pos;
                yield return null;
            }

            _scrollRect.horizontalNormalizedPosition = targetPos;
            m_snapRoutine = null;
        }
    }
}