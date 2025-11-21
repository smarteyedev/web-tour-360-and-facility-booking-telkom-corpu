using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tour360TelkomCorpu.TourManager
{
    using Tour360TelkomCorpu.DataManager;
    using Tour360TelkomCorpu.CanvasManager;
    using Tour360TelkomCorpu.HotspotHandler;
    using Tour360TelkomCorpu.SphereController;
    using Unity.VisualScripting.Dependencies.NCalc;
    using Unity.VisualScripting;

    public class TourManager : MonoBehaviour
    {
        [SerializeField] private int _currentLocationIndex = 0;
        [SerializeField] private LocationDataModel _locationData = new LocationDataModel();
        [SerializeField] private List<int> _visitedLocationIndexList = new List<int>();

        [Header("Component References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private CanvasManager _canvasManager;
        [SerializeField] private List<HotspotHandler> _hotspotPrefab;
        [SerializeField] private List<HotspotHandler> m_hotspotPooling;
        [SerializeField] private SphereController _SphereController;

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            Debug.Log($"TourManager: Starting App...");

            StartCoroutine(_canvasManager.loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuAreaOptionData,
                _documentId: "",
                _onComplete: () =>
                {
                    // loading process complete
                    StartCoroutine(_dataManager.RequestTelkomCorpuAreaOptionContent(
                        (data) =>
                        {
                            _canvasManager.OpenPanel(
                                panelType: PanelType.WelcomingSection,
                                data: data,
                                callback: (msg) =>
                                    {
                                        _canvasManager.OpenPanel(PanelType.CorpuAreaSelection, data, (documentId) => GetTelkomCorpuDataMaster(documentId));
                                    }
                            );
                        },
                            (progress) => { /* Debug.Log($"{progress}") */ },
                            false
                        ));

                    Debug.Log($"TourManager: Started...");
                },
                _onError: () =>
                {
                    // loading process error when web request fail
                }
            ));

            _currentLocationIndex = 0;
        }

        public void GetTelkomCorpuDataMaster(string _documentId)
        {
            StartCoroutine(_canvasManager.loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuDataMaster,
                _documentId: _documentId,
                _onComplete: () =>
                {
                    // loading process complete
                    _canvasManager.CloseAllPanel();

                    SetupLocationAsset(_currentLocationIndex);
                },
                _onError: () =>
                {
                    // loading process error when web request fail
                }
            ));
        }

        public void OnChangeLocationByDocumentId(string documentId)
        {
            int targetIndex = _dataManager.GenerateLocationIndex(documentId);
            SetupLocationAsset(targetIndex);
        }

        public void SetupLocationAsset(int targetIndex)
        {
            StartCoroutine(_dataManager.RequestLocationDataContentByIndex(
                locationIndex: targetIndex,
                onDone: (data) =>
                {
                    if (data != null)
                    {
                        _locationData = data;
                        _currentLocationIndex = targetIndex;

                        if (_visitedLocationIndexList.Count == 0 || _currentLocationIndex != _visitedLocationIndexList[_visitedLocationIndexList.Count - 1])
                            _visitedLocationIndexList.Add(_currentLocationIndex);
#if UNITY_EDITOR
                        Debug.Log($"TourManager: Already Get Location {data.name} asset");
#endif

                        // START: SET ASSET FUNCTION ...
                        _canvasManager.SetLocationPlank(_locationData.name, () => Debug.Log("Info Panel Clicked"));

                        _SphereController.ChangeTextureWithFade(
                               _locationData.background_360_image.textureImage,
                               onStartTransition: null,
                               onFinishTransition: null
                        );

                        HideHotspot();

                        foreach (var nav in _locationData.navigations)
                        {
                           
                            if (nav.target_type != TargetHotspot.PANEL_GALLERY)
                            {
                                //hotspot.SetupHotspot(
                                //hotspotName: nav.hotspot_configuration.hotspot_title,
                                //iconSprite: nav.hotspot_configuration.hotspot_image.GetSpriteImage(),
                                //action: () =>
                                //{
                                //    switch (nav.target_type)
                                //    {
                                //        case TargetHotspot.BUILDING:
                                //            OnChangeLocationByDocumentId(nav.building_target.documentId);
                                //            break;
                                //        case TargetHotspot.FACILITY:
                                //            OnChangeLocationByDocumentId(nav.facility_target.documentId);
                                //            break;

                                //    }
                                //},
                                //position: new Vector3 (
                                //    nav.hotspot_configuration.coordinate_x,
                                //    nav.hotspot_configuration.coordinate_y,
                                //    0));
                            }
                           
                            InstantiateHotspot(nav);

                            // panggil fungsi instatiate
                        }
                        // END: SET ASSET FUNCTION ...
                    }
                    else
                    {
                        Debug.Log($"TourManager: target index is out of target");
                    }
                },
                (progress) => {/* Debug.Log($"{progress}") */},
                forceRedownload: false
            ));
        }

        public void NextLocation()
        {
            SetupLocationAsset(_currentLocationIndex + 1);
        }

        public void PreviousLocation()
        {
            if (_visitedLocationIndexList.Count > 1)
            {
                _visitedLocationIndexList.RemoveAt(_visitedLocationIndexList.Count - 1);
                SetupLocationAsset(_visitedLocationIndexList[_visitedLocationIndexList.Count - 1]);
            }
        }

        private void InstantiateHotspot(NavigationSetting nav)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Tidak ada Canvas di scene!");
                return;
            }

   
            HotspotHandler pooling = m_hotspotPooling
                .Find(h => h.hotspotType == nav.target_type && !h.gameObject.activeSelf);

          
            if (m_hotspotPooling.Count > 0 && m_hotspotPooling.Any(h => h.hotspotType == nav.target_type && !h.gameObject.activeSelf))
            {
                pooling.transform.localPosition = new Vector3(
                nav.hotspot_configuration.coordinate_x,
                nav.hotspot_configuration.coordinate_y,
                0);
                pooling.gameObject.SetActive(true);
            }
            else
            {
                HotspotHandler prefab = _hotspotPrefab.Find(p => p.hotspotType == nav.target_type);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab untuk hotspot type {nav.target_type} tidak ditemukan!");
                    return;
                }

                pooling = Instantiate(prefab, canvas.transform);
 
                pooling.transform.SetAsFirstSibling();
                m_hotspotPooling.Add(pooling); 
            }

          
            pooling.SetupHotspot(
                hotspotName: nav.hotspot_configuration.hotspot_title,
                iconSprite: nav.hotspot_configuration.hotspot_image.GetSpriteImage(),
                action: () =>
                {
                    switch (nav.target_type)
                    {
                        case TargetHotspot.BUILDING:
                            OnChangeLocationByDocumentId(nav.building_target.documentId);
                            break;

                        case TargetHotspot.FACILITY:
                            OnChangeLocationByDocumentId(nav.facility_target.documentId);
                            break;

                        case TargetHotspot.PANEL_GALLERY:
                            List<Sprite> spriteList = _locationData.gallery
                                .SelectMany(g => g.content_images)
                                .Select(img => img.GetSpriteImage())
                                .Where(s => s != null)
                                .ToList();
                            _canvasManager.OpenPanel(PanelType.GalleryPhoto, spriteList, null);
                            break;

                        case TargetHotspot.PANEL_NAVIGATION:
                            Debug.Log("Open Panel Navigation");
                            break;
                    }
                },
                position: new Vector3(
                    nav.hotspot_configuration.coordinate_x,
                    nav.hotspot_configuration.coordinate_y,
                    0
                )

            );
            pooling.Invoke("TrackingPosition", 0f);
            pooling.InvokeRepeating("TrackingPosition", 0.1f, 0.02f);
        }



        private void HideHotspot()
        {
            foreach (var hotspot in m_hotspotPooling)
            {
                if (hotspot != null)
                    hotspot.gameObject.SetActive(false);
            }
        }
    }
}