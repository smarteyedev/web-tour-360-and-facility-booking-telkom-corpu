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
        [SerializeField] private GameObject _topbarMenu;
        [SerializeField] private GameObject _bottombarMenu;
        [SerializeField] private TextMeshProUGUI _textLocationName;
        [SerializeField] private Button _buttonOpenPanelInformation;
        [SerializeField] private GameObject _topbarDropDown;
        public ButtonToggle buttonAutoRotation;
        [SerializeField] private Button _buttonBooking;

        [Header("Component References")]
        public LoadingScreenHandler loadingScreen;
        [SerializeField] private List<MonoBehaviour> _panelComponentList;
        private Dictionary<PanelType, IPanel> m_panelControllerDictionary = new Dictionary<PanelType, IPanel>();
        private Dictionary<PanelType, IPanel> m_currentActivePanel = new Dictionary<PanelType, IPanel>();

        private void Awake()
        {
            SetupPanelDictionary();
        }

        private void SetupPanelDictionary()
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
                        Debug.LogWarning($"[CanvasManager.cs]: Duplicate registration for panel key '{key}'. Existing will be kept and this one ignored. Object: {p.name}", p);
#endif
                        // jika ingin overwrite, gunakan panelControllerDict[key] = ip;
                        continue;
                    }
                    m_panelControllerDictionary[key] = ip;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[CanvasManager.cs]: component '{p.name}' does not implement IPanel and will be ignored.", p);
#endif
                }
            }

#if UNITY_EDITOR
            Debug.Log($"[CanvasManager.cs]: Registered {m_panelControllerDictionary.Count} panels.");
#endif
        }


        /// <summary>
        /// Membuka panel dan mengirim data untuk ditampilkan
        /// </summary>
        /// <param name="panelType">Jenis panel yang akan ditampilkan</param>
        /// <param name="data">Data yang akan ditampilkan di dalam panel</param>
        /// <param name="callback">Aksi yang akan dieksekusi dalam fungsi yang ada dipanel. (ex: berpindah lokasi menggunakan documentId)</param>
        /// <param name="onClosePanel">Fungsi yang akan dipanggil ketika user menutup panel</param>
        public void OpenPanel(PanelType panelType, object data, Action<object> callback, Action onClosePanel)
        {
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
#if UNITY_EDITOR
                Debug.Log($"[CanvasManager.cs]: Panel {panelType.ToString()} is opened | Current active panel: {m_currentActivePanel.Count}");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"[CanvasManager.cs]: panel {panelType.ToString()} is not registered");
#endif
            }


            if (_topbarDropDown.activeSelf)
                _topbarDropDown.SetActive(false);
        }

        /// <summary>
        /// Menutup semua panel yang saat ini tampil
        /// </summary>
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
                Debug.Log($"[CanvasManager.cs]: All panel are closed | Current active panel: {m_currentActivePanel.Count}");
#endif
            }
        }

        public void SetActiveBarMenu(bool isActive)
        {
            _topbarMenu.SetActive(isActive);
            _bottombarMenu.SetActive(isActive);
        }

        public void SetLocationDataUI(string locationName, Action onClickPanelInfo, bool isCanBooking, Action onClickBookingPanel)
        {
            _textLocationName.text = locationName;
            _buttonOpenPanelInformation.onClick.RemoveAllListeners();
            _buttonOpenPanelInformation.onClick.AddListener(() => onClickPanelInfo?.Invoke());

            _buttonBooking.gameObject.SetActive(isCanBooking);

            if (isCanBooking)
            {
                _buttonBooking.onClick.RemoveAllListeners();
                _buttonBooking.onClick.AddListener(() => onClickBookingPanel?.Invoke());
            }

            if (_topbarDropDown.activeSelf) _topbarDropDown.SetActive(!_topbarDropDown.activeSelf);
        }

        public bool AnyPanelOpenNow()
        {
            return m_currentActivePanel.Count > 0 && m_currentActivePanel != null;
        }

        public void ToggleTopbarDropDown()
        {
            _topbarDropDown.SetActive(!_topbarDropDown.activeSelf);
        }
    }
}