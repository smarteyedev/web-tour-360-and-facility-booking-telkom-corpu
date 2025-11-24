using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class CanvasManager : MonoBehaviour
    {
        [Header("Component Menu Bar")]
        [SerializeField] private TextMeshProUGUI _textLocationName;
        [SerializeField] private Button _buttonOpenPanelInformation;
        [SerializeField] private Button _buttonAutoRotation;

        public LoadingScreenHandler loadingScreen;
        [SerializeField] private List<MonoBehaviour> _panelComponentList;
        private Dictionary<PanelType, IPanel> m_panelControllerDictionary = new Dictionary<PanelType, IPanel>();
        private Dictionary<PanelType, IPanel> m_currentActivePanel = new Dictionary<PanelType, IPanel>();

        private void Awake()
        {
            SetupPanelDict();
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

        public void OpenPanel(PanelType panelType, object data, Action<string> callback, Action onClosePanel)
        {
            if (data == null)
            {
#if UNITY_EDITOR
                Debug.Log($"CanvasManager: data is null, please input data");
#endif
                return;
            }

            CloseAllPanel();

            if (m_panelControllerDictionary.TryGetValue(panelType, out var panel))
            {
                panel.ShowPanel(data, callback, () =>
                {
                    onClosePanel?.Invoke();

                    panel.HidePanel();
                    m_currentActivePanel.Remove(panelType);
                });

                m_currentActivePanel.Add(panelType, panel);
                Debug.Log($"CanvasManager: Panel {panelType.ToString()} is opened | Current active panel: {m_currentActivePanel.Count}");
            }
            else
            {
                Debug.LogError($"CanvasManager: panel {panelType.ToString()} is not registered");
            }
        }

        public void CloseAllPanel()
        {
            if (m_currentActivePanel.Count > 0)
            {
                // hide active panel
                foreach (var p in m_currentActivePanel.ToList())
                {
                    p.Value.HidePanel();
                    m_currentActivePanel.Remove(p.Key);
                }

#if UNITY_EDITOR
                Debug.Log($"CanvasManager: All panel are closed | Current active panel: {m_currentActivePanel.Count}");
#endif
            }
        }

        public void SetLocationPlank(string locationName, Action onClickPanelInfo)
        {
            _textLocationName.text = locationName;
            _buttonOpenPanelInformation.onClick.RemoveAllListeners();
            _buttonOpenPanelInformation.onClick.AddListener(() => onClickPanelInfo?.Invoke());
        }

        public bool AnyPanelOpenNow()
        {
            return m_currentActivePanel.Count > 0 && m_currentActivePanel != null;
        }

        public void SetupButtonAutoRotation(Action onClick)
        {
            _buttonAutoRotation.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}