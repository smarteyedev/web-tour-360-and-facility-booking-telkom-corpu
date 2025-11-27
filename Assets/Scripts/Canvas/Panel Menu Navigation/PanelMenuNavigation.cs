using System;
using System.Collections;
using System.Collections.Generic;
using Tour360TelkomCorpu.DataManager;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelMenuNavigation : PanelController<List<LocationDataModel>, string>
    {
        [SerializeField] private List<LocationDataModel> _allData;
        private int m_itemsPerPage = 8;
        private int m_currentPage = 0;
        private int m_totalPages = 0;


        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Button _buttonClose;
        [SerializeField] private List<SelectionCard> _selectionCardList;
        [SerializeField] private TextMeshProUGUI _textPanelTitle;

        [Space(8f)]
        [SerializeField] private Button _buttonPrev;
        [SerializeField] private Button _buttonNext;
        [SerializeField] private Button _pageButtonPrefab;
        [SerializeField] private Transform _pageButtonsParent;
        [SerializeField] private Sprite _pageButtonActiveSprite;
        [SerializeField] private Sprite _pageButtonNonactiveSprite;
        private readonly List<Button> m_pageButtons = new List<Button>();

        private Action<string> m_onClickingCardAction = null;

        protected override void ShowPanel(List<LocationDataModel> contentData, Action<string> callback = null, Action onClosePanel = null)
        {
            _panelContainer.gameObject.SetActive(true);

            if (_allData == null || !_allData.SequenceEqual(contentData))
            {
                // Simpan copy dari data baru supaya aman dari perubahan luar
                _allData = new List<LocationDataModel>(contentData);
            }

            _textPanelTitle.text = _panelIndentity == PanelType.MenuNavigationToFacility ? $"Facilities" : $"Building Category";

            // Hitung jumlah halaman
            m_totalPages = Mathf.CeilToInt(_allData.Count / (float)m_itemsPerPage);
            m_currentPage = 0;

            // Setup UI
            SetupPageButtons();
            ShowPage(m_currentPage);

            m_onClickingCardAction = callback;

            _buttonClose.onClick.RemoveAllListeners();
            _buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());

            // Prev / Next
            _buttonPrev.onClick.RemoveAllListeners();
            _buttonNext.onClick.RemoveAllListeners();

            _buttonPrev.onClick.AddListener(OnClickPrevPage);
            _buttonNext.onClick.AddListener(OnClickNextPage);
        }

        public override void HidePanel()
        {
            _panelContainer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Membuat tombol-tombol nomor halaman (1,2,3,...)
        /// </summary>
        private void SetupPageButtons()
        {
            if (m_totalPages <= 0)
            {
                for (int i = 0; i < m_pageButtons.Count; i++)
                    m_pageButtons[i].gameObject.SetActive(false);

                return;
            }

            _buttonPrev.transform.SetAsFirstSibling();

            for (int i = 0; i < m_totalPages; i++)
            {
                int pageIndex = i;
                if (i >= m_pageButtons.Count)
                {
                    var newBtn = Instantiate(_pageButtonPrefab, _pageButtonsParent);
                    m_pageButtons.Add(newBtn);
                }

                Button pageBtn = m_pageButtons[i];
                pageBtn.gameObject.SetActive(true);

                var btnText = pageBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = (pageIndex + 1).ToString();

                pageBtn.onClick.RemoveAllListeners();
                pageBtn.onClick.AddListener(() => OnClickPage(pageIndex));

                pageBtn.transform.SetSiblingIndex(i + 1);
            }

            for (int i = m_totalPages; i < m_pageButtons.Count; i++)
                m_pageButtons[i].gameObject.SetActive(false);

            _buttonNext.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Menampilkan data pada halaman tertentu.
        /// </summary>
        private void ShowPage(int pageIndex)
        {
            if (m_totalPages == 0)
            {
                // Tidak ada data, semua card dimatikan
                for (int i = 0; i < _selectionCardList.Count; i++)
                    _selectionCardList[i].gameObject.SetActive(false);

                _buttonPrev.interactable = false;
                _buttonNext.interactable = false;
                return;
            }

            m_currentPage = Mathf.Clamp(pageIndex, 0, m_totalPages - 1);

            int startIndex = m_currentPage * m_itemsPerPage;

            for (int i = 0; i < _selectionCardList.Count; i++)
            {
                int dataIndex = startIndex + i;

                if (dataIndex < _allData.Count)
                {
                    // Ada data untuk card ini
                    _selectionCardList[i].gameObject.SetActive(true);
                    LocationDataModel data = _allData[dataIndex];

                    // Asumsikan SelectionCard punya fungsi Setup / BindData
                    _selectionCardList[i].SetupCard(
                        bgCard: data.thumbnail_image.GetSpriteImage(),
                        cardName: data.thumbnail_name,
                        onClickAction: () => m_onClickingCardAction?.Invoke($"{data.documentId}")
                    );

                    //Debug.Log($"[PanelMenuNavigation] Show dataIndex: {dataIndex} on card: {i}");
                }
                else
                {
                    // Tidak ada data di index ini -> hide card
                    _selectionCardList[i].gameObject.SetActive(false);
                }
            }

            // Update tombol prev/next
            _buttonPrev.interactable = m_currentPage > 0;
            _buttonNext.interactable = m_currentPage < m_totalPages - 1;

            for (int i = 0; i < m_pageButtons.Count; i++)
            {
                Sprite buttonSprite = i == m_currentPage ? _pageButtonActiveSprite : _pageButtonNonactiveSprite;
                m_pageButtons[i].image.sprite = buttonSprite;
            }
        }

        private void OnClickPrevPage()
        {
            int targetPage = m_currentPage - 1;
            if (targetPage >= 0)
            {
                ShowPage(targetPage);
            }
        }

        private void OnClickNextPage()
        {
            int targetPage = m_currentPage + 1;
            if (targetPage < m_totalPages)
            {
                ShowPage(targetPage);
            }
        }

        private void OnClickPage(int pageIndex)
        {
            if (pageIndex == m_currentPage) return;

            ShowPage(pageIndex);
        }
    }
}