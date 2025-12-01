using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelBooking : PanelController<FormatPanelBooking, string>
    {
        [Header("Guidance Section")]

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private TextMeshProUGUI _textMessage;
        [SerializeField] private ButtonInteractive _buttonYes;
        [SerializeField] private ButtonInteractive _buttonNo;

        protected override void ShowPanel(FormatPanelBooking contentData, Action<string> callback = null, Action onClosePanel = null)
        {
            if (_panelContainer != null)
                _panelContainer.SetActive(true);

            _textMessage.text = $"Are you sure want to book {contentData.facilityName}?";
            _buttonYes.onLeftMouseDown.RemoveAllListeners();
            _buttonYes.onLeftMouseDown.AddListener(() => callback?.Invoke(contentData.urlBooking));

            _buttonNo.onLeftMouseDown.RemoveAllListeners();
            _buttonNo.onLeftMouseDown.AddListener(() =>
            {
                onClosePanel();
            });
        }

        public override void HidePanel()
        {
            if (_panelContainer != null)
                _panelContainer.SetActive(false);
        }
    }
}
