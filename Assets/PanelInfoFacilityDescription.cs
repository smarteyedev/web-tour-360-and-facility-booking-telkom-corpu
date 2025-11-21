using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelInfoFacilityDescription : PanelController<FormatPanelDescriptionAsset, Action>
    {
        [Header("Guidance Section")]
        [SerializeField] private FormatPanelDescriptionAsset mapsAsset;

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private TextMeshProUGUI _TextDescription;
        [SerializeField] private Image _ImageDetail;


        //private void Start()
        //{
        //    if (mapsAsset != null)
        //        ShowPanel(mapsAsset);
        //}


        protected override void ShowPanel(FormatPanelDescriptionAsset descriptionAsset, Action<string> callbackUsingDocumentId = null)
        {
            if (descriptionAsset == null)
            {
                Debug.LogWarning($"{name}: descriptionAsset kosong.");
                return;
            }

            mapsAsset = descriptionAsset;

            _panelContainer.SetActive(true);

            Debug.Log($"[FacilityDesc] ShowPanel() -> Text: {mapsAsset.descriptionText}, Sprite: {mapsAsset.facilityDetailSprite?.name}");

           
            string finalText = mapsAsset.descriptionText;

            if (!string.IsNullOrEmpty(finalText) && finalText.Length > 1300)
            {
                finalText = finalText.Substring(0, 1300);
                Debug.Log("[FacilityDesc] Teks dipotong");
            }

            if (_TextDescription != null)
                _TextDescription.text = finalText;

            if (_ImageDetail != null && mapsAsset.facilityDetailSprite != null)
                _ImageDetail.sprite = mapsAsset.facilityDetailSprite;
        }


        public override void HidePanel()
        {
            _panelContainer.SetActive(false);
        }
    }
}
