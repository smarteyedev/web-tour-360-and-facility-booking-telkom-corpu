using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelInfoLocationMaps : PanelController<FormatPanelLocationMapsAsset, string>
    {
        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Image _ImageMaps;
        [SerializeField] private Image _ImageDescription;
        [SerializeField] private Button buttonClose;

        protected override void ShowPanel(FormatPanelLocationMapsAsset mapsAsset, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            if (mapsAsset == null)
            {
                Debug.LogWarning($"{name}: mapsAsset kosong.");
                return;
            }

            _panelContainer.SetActive(true);

            if (_ImageMaps != null && mapsAsset.mapsSprite != null)
                _ImageMaps.sprite = mapsAsset.mapsSprite;

            if (_ImageDescription != null && mapsAsset.DescriptionSprite != null)
                _ImageDescription.sprite = mapsAsset.DescriptionSprite;

            buttonClose.onClick.RemoveAllListeners();
            buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());
        }


        public override void HidePanel()
        {
            _panelContainer.SetActive(false);
        }
    }
}
