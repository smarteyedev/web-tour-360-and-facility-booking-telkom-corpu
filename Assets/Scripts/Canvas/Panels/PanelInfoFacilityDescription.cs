using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelInfoFacilityDescription : PanelController<FormatPanelDescriptionAsset, bool>
    {
        [Header("Guidance Section")]
        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private TextMeshProUGUI _TextTitle;
        [SerializeField] private TextMeshProUGUI _TextDescription;
        [SerializeField] private Image _ImageDetail;
        [SerializeField] private Button _buttonClose;
        [SerializeField] private Button _buttonBooking;
        [SerializeField] private Toggle _toggleShowPanelAutomatically;


        protected override void ShowPanel(FormatPanelDescriptionAsset descriptionAsset, Action<bool> callback = null, Action onClosePanel = null)
        {
            if (descriptionAsset == null)
            {
                Debug.LogWarning($"[{name}]: descriptionAsset kosong.");
                return;
            }

            _panelContainer.SetActive(true);

            string finalText = descriptionAsset.descriptionText;
            if (!string.IsNullOrEmpty(finalText) && finalText.Length > 1300)
            {
                finalText = finalText.Substring(0, 1300);
                //Debug.Log("[FacilityDesc] Teks dipotong");
            }

            if (_TextDescription != null)
                _TextDescription.text = finalText;

            if (_TextTitle != null)
                _TextTitle.text = descriptionAsset.titleText;

            if (_ImageDetail != null && descriptionAsset.facilityDetailSprite != null)
                _ImageDetail.sprite = descriptionAsset.facilityDetailSprite;

            _buttonClose.onClick.RemoveAllListeners();
            _buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());

            if (descriptionAsset.isCanBook)
            {
                _buttonBooking.gameObject.SetActive(true);
                _buttonBooking.onClick.RemoveAllListeners();
                _buttonBooking.onClick.AddListener(() => descriptionAsset.onOpenPanelBooking?.Invoke());
            }
            else
            {
                _buttonBooking.gameObject.SetActive(false);
            }

            _toggleShowPanelAutomatically.isOn = descriptionAsset.isAutoShow;
            _toggleShowPanelAutomatically.onValueChanged.RemoveAllListeners();
            _toggleShowPanelAutomatically.onValueChanged.AddListener((bool val) => callback?.Invoke(val));
        }


        public override void HidePanel()
        {
            _panelContainer.SetActive(false);
        }
    }
}
