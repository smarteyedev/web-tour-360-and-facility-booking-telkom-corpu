using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebTourCorpu.DataManager;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> panelComponentList;
        private Dictionary<IPanel.PanelType, IPanel> panelControllerDict = new();

        private void Awake()
        {
            foreach (var p in panelComponentList)
                if (p is IPanel ip) panelControllerDict[ip.panelIdentity()] = ip;
        }

        private void Start()
        {

        }

        public void OpenPanelCorpuAreaSelection(IPanel.PanelType identity, object data)
        {
            if (data == null)
            {
                Debug.Log($"data is null, please input data");
                return;
            }

            if (panelControllerDict.TryGetValue(identity, out var panel))
            {
                panel.ShowPanel(data);
            }
        }
    }
}