using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelGuidanceSection : PanelController<List<Sprite>, Action<string>>
    {

        [Header("Guidance Section")]
        [SerializeField] private List<Sprite> m_guidanceSpriteList;

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Image _Imageoverlay;

        private int _currentIndex = 0;


        protected override void ShowPanel(List<Sprite> assetSpriteList, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            if (_panelContainer != null)
            {
                _panelContainer.SetActive(true);
            }

            if (assetSpriteList != null && assetSpriteList.Count > 0)
            {
                m_guidanceSpriteList = assetSpriteList;
                _currentIndex = 0;

                SetupAsset(m_guidanceSpriteList[_currentIndex]);
            }
            else
            {
                Debug.LogWarning("[Guidance] Daftar sprite panduan kosong.");
                HidePanel();
            }
        }

        public override void HidePanel()
        {
            if (_panelContainer != null)
            {
                _panelContainer.SetActive(false);
            }

            if (_Imageoverlay != null)
            {
                _Imageoverlay.sprite = null;
            }

            Debug.Log("[Guidance] Panel disembunyikan.");
        }

        protected void OnClickNextGuidance()
        {
            int totalSlides = m_guidanceSpriteList.Count;

            if (_currentIndex < totalSlides - 1)
            {
                _currentIndex++;
                SetupAsset(m_guidanceSpriteList[_currentIndex]);
            }
            else if (_currentIndex == totalSlides - 1)
            {
                HidePanel();
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