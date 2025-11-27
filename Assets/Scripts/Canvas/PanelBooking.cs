using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelBooking : PanelController<string, string>
    {
        [DllImport("__Internal")]
        private static extern void OpenInSameTab(string url);

        [Header("Guidance Section")]

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Button _buttonClose;

        protected override void ShowPanel(
            string contentData,
            Action<string> callbackUsingDocumentId = null,
            Action onClosePanel = null)
        {
            if (_panelContainer != null)
                _panelContainer.SetActive(true);

            _buttonClose.onClick.AddListener(() =>
            {
                HidePanel();
            });

            callbackUsingDocumentId?.Invoke(contentData);
        }

        public override void HidePanel()
        {
            if (_panelContainer != null)
                _panelContainer.SetActive(false);
        }


        public void OpenLink(string targetUrl)
        {
            if (string.IsNullOrEmpty(targetUrl))
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            OpenInSameTab(targetUrl);
#else
            Application.OpenURL(targetUrl);
#endif
        }
    }
}
