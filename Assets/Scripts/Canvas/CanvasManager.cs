using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tour360TelkomCorpu.DataManager;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class CanvasManager : MonoBehaviour
    {
        public LoadingScreenHandler loadingScreen;
        [SerializeField] private List<MonoBehaviour> _panelComponentList;
        private Dictionary<PanelType, IPanel> m_panelControllerDictionary = new Dictionary<PanelType, IPanel>();

        private void Awake()
        {
            SetupPanelDict();
        }

        private void Start()
        {
            /* List<TelkomCorpuAreaCard> tesdata = new List<TelkomCorpuAreaCard>();
            TelkomCorpuAreaCard d1 = new TelkomCorpuAreaCard();
            d1.name = $"telkom samarinda";
            tesdata.Add(d1);

            OpenPanelCorpuAreaSelection(IPanel.PanelType.CorpuAreaSelection, tesdata, (documentId) => Debug.Log($"PanelCorpuAreaSelection: masuk ke selection, nama area : {documentId}")); */
        }

        private void SetupPanelDict()
        {
            m_panelControllerDictionary.Clear();
            if (_panelComponentList == null) return;

            foreach (var p in _panelComponentList)
            {
                if (p == null) continue;

                if (p is IPanel ip)
                {
                    var key = ip.panelIdentity();
                    if (m_panelControllerDictionary.ContainsKey(key))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"CanvasManager: Duplicate registration for panel key '{key}'. Existing will be kept and this one ignored. Object: {p.name}", p);
#endif
                        // jika ingin overwrite, gunakan panelControllerDict[key] = ip;
                        continue;
                    }
                    m_panelControllerDictionary[key] = ip;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"CanvasManager: component '{p.name}' does not implement IPanel and will be ignored.", p);
#endif
                }
            }

#if UNITY_EDITOR
            Debug.Log($"CanvasManager: Registered {m_panelControllerDictionary.Count} panels.");
#endif
        }

        public void OpenPanel(PanelType panelType, object data, Action<string> callback)
        {
            if (data == null)
            {
#if UNITY_EDITOR
                Debug.Log($"data is null, please input data");
#endif
                return;
            }

            if (m_panelControllerDictionary.TryGetValue(panelType, out var panel))
            {
                panel.ShowPanel(data, callback);
            }
            else
            {
                Debug.LogError($"CanvasManager: panel {panelType.ToString()} is not registered");
            }
        }
    }
}