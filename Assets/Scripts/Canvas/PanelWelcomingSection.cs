using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tour360TelkomCorpu.CanvasManager
{
    using Tour360TelkomCorpu.DataManager;
    using UnityEngine.UI;

    public class PanelWelcomingSection : PanelController<List<TelkomCorpuAreaCard>, string>
    {
        [Space(10f)]
        [Header("Welcoming Section")]
        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Button _buttonStart;

        protected override void ShowPanel(List<TelkomCorpuAreaCard> contentData, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            _panelContainer.gameObject.SetActive(true);
            _buttonStart.onClick.AddListener(() => onClosePanel?.Invoke());
        }

        public override void HidePanel()
        {
            _panelContainer.gameObject.SetActive(false);
        }
    }
}