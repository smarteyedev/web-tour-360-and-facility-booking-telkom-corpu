using System;
using System.Collections;
using System.Collections.Generic;
using Tour360TelkomCorpu.DataManager;
using UnityEngine;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class PanelMenuNavigation : PanelController<List<LocationDataModel>, string>
    {
        [SerializeField] private List<LocationDataModel> _tempList;

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;

        protected override void ShowPanel(List<LocationDataModel> contentData, Action<string> callback = null, Action onClosePanel = null)
        {
            _panelContainer.gameObject.SetActive(true);

            _tempList = contentData;
        }

        public override void HidePanel()
        {

        }
    }
}