using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelGuidanceSection : PanelController<string, string>
    {

        [Header("Guidance Section")]
        [SerializeField] private List<Sprite> m_guidanceSpriteList;

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Image _Imageoverlay;
        [SerializeField] private Button _buttonNext;
        [SerializeField] private Button _buttonClose;

        private int m_currentIndex = 0;
        private Action m_onFinished = null;


        protected override void ShowPanel(string assetSpriteList, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            if (_panelContainer != null)
            {
                _panelContainer.SetActive(true);
            }

            m_onFinished = () =>
            {
                onClosePanel?.Invoke();
                callbackUsingDocumentId?.Invoke("");
            };

            _buttonClose.gameObject.SetActive(false);
            _buttonClose.onClick.RemoveAllListeners();
            _buttonClose.onClick.AddListener(() => m_onFinished?.Invoke());

            _buttonNext.onClick.RemoveAllListeners();
            _buttonNext.onClick.AddListener(() => OnClickNextGuidance());

            m_currentIndex = 0;
            SetupAsset(m_guidanceSpriteList[m_currentIndex]);
        }

        public override void HidePanel()
        {
            if (_panelContainer != null)
            {
                _panelContainer.SetActive(false);
            }
        }

        protected void OnClickNextGuidance()
        {
            int totalSlides = m_guidanceSpriteList.Count;

            if (m_currentIndex < totalSlides - 1)
            {
                m_currentIndex++;
                SetupAsset(m_guidanceSpriteList[m_currentIndex]);

                _buttonClose.gameObject.SetActive(true);
            }
            else if (m_currentIndex == totalSlides - 1)
            {
                m_onFinished?.Invoke();
            }
        }

        protected void SetupAsset(Sprite assetSprite)
        {
            if (_Imageoverlay != null && assetSprite != null)
            {
                _Imageoverlay.sprite = assetSprite;
            }
        }
    }
}