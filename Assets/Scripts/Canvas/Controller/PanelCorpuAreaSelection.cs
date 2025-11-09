using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebTourCorpu.DataManager;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelCorpuAreaSelection : PanelController<List<TelkomCorpuAreaCard>>
    {
        [Space(10f)]
        [Header("Corpu Area Selection Configuration")]
        [Header("Content Vieport")]
        public List<GameObject> contentPanels;

        [Header("Pagination Buttons")]
        public Button nextButton;
        public Button prevButton;

        [Header("Page Settings")]
        public bool useTimer = false;
        public bool isLimitedSwipe = false;
        public float autoMoveTime = 5f;
        private float timer;
        public int currentIndex = 0;
        public float swipeThreshold = 50f;
        private Vector2 touchStartPos;

        protected override void Start()
        {
            /* nextButton.onClick.AddListener(NextContent);
                        prevButton.onClick.AddListener(PreviousContent); */
        }

        protected override void ShowPanel(List<TelkomCorpuAreaCard> cardData)
        {
            Debug.Log($"masuk ke selection, nama area : {cardData[0].name}");
        }

        protected override void HidePanel()
        {

        }

        public void SetCurrentIndex(int newIndex)
        {
            if (newIndex >= 0 && newIndex < contentPanels.Count)
            {
                currentIndex = newIndex;
                ShowContent();
            }
        }

        void NextContent()
        {
            currentIndex = (currentIndex + 1) % contentPanels.Count;
            ShowContent();
        }

        void PreviousContent()
        {
            currentIndex = (currentIndex - 1 + contentPanels.Count) % contentPanels.Count;
            ShowContent();
        }

        void ShowContent()
        {
            // Activate the current panel and deactivate others
            for (int i = 0; i < contentPanels.Count; i++)
            {
                bool isActive = i == currentIndex;
                contentPanels[i].SetActive(isActive);

                if (isActive)
                {
                    // Reset timer and fill amount when the content is swiped
                    timer = autoMoveTime;
                }
            }
        }
    }
}