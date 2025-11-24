using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelInfoFacilityDescription : PanelController<FormatPanelDescriptionAsset, Action>
    {
        [Header("Guidance Section")]

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private TextMeshProUGUI _TextDescription;
        [SerializeField] private Image _ImageDetail;
        [SerializeField] private Button buttonClose;


        protected override void ShowPanel(FormatPanelDescriptionAsset descriptionAsset, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            if (descriptionAsset == null)
            {
                Debug.LogWarning($"{name}: descriptionAsset kosong.");
                return;
            }

            _panelContainer.SetActive(true);

            string finalText = descriptionAsset.descriptionText;

            if (!string.IsNullOrEmpty(finalText) && finalText.Length > 1300)
            {
                finalText = finalText.Substring(0, 1300);
                Debug.Log("[FacilityDesc] Teks dipotong");
            }

            if (_TextDescription != null)
                _TextDescription.text = finalText;

            if (_ImageDetail != null && descriptionAsset.facilityDetailSprite != null)
                _ImageDetail.sprite = descriptionAsset.facilityDetailSprite;

            buttonClose.onClick.RemoveAllListeners();
            buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());
        }


        public override void HidePanel()
        {
            _panelContainer.SetActive(false);
        }
    }
}
