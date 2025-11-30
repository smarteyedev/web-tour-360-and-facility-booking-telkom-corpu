using UnityEngine;
using UnityEngine.UI;
using System;
using Tour360TelkomCorpu.DataManager;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelInfoLocationMaps : PanelController<LocationDataModel, string>
    {
        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Image _ImageMaps;
        [SerializeField] private Image _ImageDescription;
        [SerializeField] private Button buttonClose;

        protected override void ShowPanel(LocationDataModel mapsAsset, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            if (mapsAsset == null)
            {
                Debug.LogWarning($"{name}: mapsAsset kosong.");
                return;
            }

            _panelContainer.SetActive(true);

            if (_ImageMaps != null && mapsAsset.maps_image.GetSpriteImage() != null)
                _ImageMaps.sprite = mapsAsset.maps_image.GetSpriteImage();

            if (_ImageDescription != null && mapsAsset.description_image.GetSpriteImage() != null)
                _ImageDescription.sprite = mapsAsset.description_image.GetSpriteImage();

            buttonClose.onClick.RemoveAllListeners();
            buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());
        }


        public override void HidePanel()
        {
            _panelContainer.SetActive(false);
        }
    }
}
