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

    public class TourManager : MonoBehaviour
    {
        [SerializeField] private int _currentLocationIndex = 0;
        [SerializeField] private LocationDataModel _locationData = new LocationDataModel();
        [SerializeField] private List<int> _visitedLocationIndexList = new List<int>();

        [Header("Component References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private CanvasManager _canvasManager;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private List<HotspotHandler> _hotspotPrefab;
        private Dictionary<HotspotHandler.HotspotType, List<HotspotHandler>> m_hotspotPooling = new Dictionary<HotspotHandler.HotspotType, List<HotspotHandler>>();
        [SerializeField] private SphereController _sphereController;

        private bool m_isTryToLoadingAsset = false;

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            Debug.Log($"TourManager: Starting App...");

            m_isTryToLoadingAsset = true;

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
                                        _canvasManager.OpenPanel(PanelType.CorpuAreaSelection, data, (documentId) => GetTelkomCorpuDataMaster(documentId), null);
                                    },
                                onClosePanel: null
                            );
                        },
                            (progress) => { /* Debug.Log($"{progress}") */ },
                            false
                        ));

                    m_isTryToLoadingAsset = false;
                    Debug.Log($"TourManager: Started...");
                },
                _onError: () =>
                {
                    m_isTryToLoadingAsset = false;
                    // loading process error when web request fail
                }
            ));

            _currentLocationIndex = 0;
        }

        public void GetTelkomCorpuDataMaster(string _documentId)
        {
            m_isTryToLoadingAsset = true;
            StartCoroutine(_canvasManager.loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuDataMaster,
                _documentId: _documentId,
                _onComplete: () =>
                {
                    // loading process complete
                    _canvasManager.CloseAllPanel();

                    SetupLocationAsset(_currentLocationIndex);
                    m_isTryToLoadingAsset = false;
                },
                _onError: () =>
                {
                    m_isTryToLoadingAsset = false;
                    // loading process error when web request fail
                }
            ));
        }

        public void OnChangeLocationByDocumentId(string documentId)
        {
            if (m_isTryToLoadingAsset == true) return;

            int targetIndex = _dataManager.GenerateLocationIndex(documentId);
            SetupLocationAsset(targetIndex);
        }

        public void SetupLocationAsset(int targetIndex)
        {
            StartCoroutine(_dataManager.RequestLocationDataContentByIndex(
                locationIndex: targetIndex,
                onValidStart: () =>
                {
                    HideHotspot();
                    m_isTryToLoadingAsset = true;
                },
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
                        _canvasManager.SetLocationPlank(_locationData.name, () =>
                        {
                            switch (_locationData.locationType)
                            {
                                case LocationType.DRONE:
                                    FormatPanelLocationMapsAsset dMapsDrone = new FormatPanelLocationMapsAsset();
                                    dMapsDrone.mapsSprite = _locationData.maps_image.GetSpriteImage();
                                    dMapsDrone.DescriptionSprite = _locationData.description_image.GetSpriteImage();
                                    _canvasManager.OpenPanel(PanelType.DroneDescription, dMapsDrone, null, null);
                                    break;
                                case LocationType.BUILDING:
                                    FormatPanelLocationMapsAsset dMapsBuilding = new FormatPanelLocationMapsAsset();
                                    dMapsBuilding.mapsSprite = _locationData.maps_image.GetSpriteImage();
                                    dMapsBuilding.DescriptionSprite = _locationData.description_image.GetSpriteImage();
                                    _canvasManager.OpenPanel(PanelType.BuildingDescription, dMapsBuilding, null, null);
                                    break;
                                case LocationType.FACILITY:
                                    FormatPanelDescriptionAsset dFacility = new FormatPanelDescriptionAsset();
                                    dFacility.descriptionText = _locationData.description_text;
                                    dFacility.facilityDetailSprite = _locationData.facility_detail_image.GetSpriteImage();
                                    _canvasManager.OpenPanel(PanelType.FacilityDescription, dFacility, null, null);
                                    break;
                            }
                        });

                        _sphereController.ChangeTextureWithFade(
                            _locationData.background_360_image.textureImage,
                            onStartTransition: null,
                            onFinishTransition: null
                        );

                        if (_locationData.locationType == LocationType.FACILITY && _locationData.gallery.Count > 0)
                        {
                            foreach (var item in _locationData.gallery)
                            {
                                if (m_hotspotPooling.TryGetValue(HotspotHandler.HotspotType.OpenPanelGallery, out var list) && list != null)
                                {
                                    var tHotspot = list.FirstOrDefault(h => !h.gameObject.activeSelf);

                                    if (tHotspot != null)
                                    {
                                        Vector3 targetPosition = _sphereController.UVToWorldPosition(item.hotspot_configuration.coordinate_x, item.hotspot_configuration.coordinate_y);

                                        tHotspot.SetupHotspot(
                                            hotspotName: item.hotspot_configuration.hotspot_title,
                                            iconSprite: item.hotspot_configuration.hotspot_image.GetSpriteImage(),
                                            action: () =>
                                            {

                                                List<Sprite> spriteList = _locationData.gallery
                                                    .SelectMany(g => g.content_images)
                                                    .Select(img => img.GetSpriteImage())
                                                    .Where(s => s != null)
                                                    .ToList();

                                                _canvasManager.OpenPanel(
                                                panelType: PanelType.GalleryPhoto,
                                                data: spriteList,
                                                callback: null,
                                                onClosePanel: null
                                            );
                                            },
                                            position: targetPosition,
                                            canvas: _canvasManager.GetComponent<Canvas>(),
                                            rct: _canvasManager.GetComponent<RectTransform>(),
                                            cam: _cameraController.cam
                                        );

                                        tHotspot.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        InstantiateHotspot(item.hotspot_configuration, HotspotHandler.HotspotType.OpenPanelGallery, () =>
                                        {
                                            List<Sprite> spriteList = _locationData.gallery
                                                                        .SelectMany(g => g.content_images)
                                                                        .Select(img => img.GetSpriteImage())
                                                                        .Where(s => s != null)
                                                                        .ToList();

                                            _canvasManager.OpenPanel(
                                                panelType: PanelType.GalleryPhoto,
                                                data: spriteList,
                                                callback: null,
                                                onClosePanel: null
                                            );
                                        });
                                    }
                                }
                                else
                                {
                                    InstantiateHotspot(item.hotspot_configuration, HotspotHandler.HotspotType.OpenPanelGallery, () =>
                                    {
                                        List<Sprite> spriteList = _locationData.gallery
                                                                    .SelectMany(g => g.content_images)
                                                                    .Select(img => img.GetSpriteImage())
                                                                    .Where(s => s != null)
                                                                    .ToList();

                                        _canvasManager.OpenPanel(
                                            panelType: PanelType.GalleryPhoto,
                                            data: spriteList,
                                            callback: null,
                                            onClosePanel: null
                                        );
                                    });
                                }

                            }
                        }

                        if (_locationData.navigations.Count > 0 && _locationData.navigations != null)
                        {
                            foreach (var navigationData in _locationData.navigations)
                            {
                                int tHotspotType = 0;

                                if (_locationData.locationType == LocationType.DRONE)
                                {
                                    tHotspotType = 2;
                                }
                                else
                                {
                                    tHotspotType = navigationData.target_type == TargetHotspot.PANEL_NAVIGATION ? 3 : 1;
                                }

                                // cek hotspot typenya udah dispawn atau belum
                                if (m_hotspotPooling.TryGetValue((HotspotHandler.HotspotType)tHotspotType, out var list) && list != null)
                                {

                                    var tHotspot = list.FirstOrDefault(h => !h.gameObject.activeSelf);

                                    if (tHotspot != null)
                                    {
                                        Vector3 targetPosition = _sphereController.UVToWorldPosition(navigationData.hotspot_configuration.coordinate_x, navigationData.hotspot_configuration.coordinate_y);

                                        tHotspot.SetupHotspot(
                                            hotspotName: navigationData.hotspot_configuration.hotspot_title,
                                            iconSprite: navigationData.hotspot_configuration.hotspot_image.GetSpriteImage(),
                                            action: GenerateNavigationActionByType(navigationData),
                                            position: targetPosition,
                                            canvas: _canvasManager.GetComponent<Canvas>(),
                                            rct: _canvasManager.GetComponent<RectTransform>(),
                                            cam: _cameraController.cam
                                        );

                                        tHotspot.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        InstantiateHotspot(navigationData.hotspot_configuration, (HotspotHandler.HotspotType)tHotspotType, GenerateNavigationActionByType(navigationData));
                                    }
                                }
                                else
                                {
                                    InstantiateHotspot(navigationData.hotspot_configuration, (HotspotHandler.HotspotType)tHotspotType, GenerateNavigationActionByType(navigationData));
                                }
                            }
                        }
                        // END: SET ASSET FUNCTION ...

                        m_isTryToLoadingAsset = false;
                    }
                    else
                    {
                        m_isTryToLoadingAsset = false;
                        Debug.Log($"TourManager: target index is out of target");
                    }
                },
                (progress) => {/* Debug.Log($"{progress}") */},
                forceRedownload: false
            ));
        }

        public void NextLocation()
        {
            if (m_isTryToLoadingAsset == true) return;

            SetupLocationAsset(_currentLocationIndex + 1);
        }

        public void PreviousLocation()
        {
            if (m_isTryToLoadingAsset == true) return;

            if (_visitedLocationIndexList.Count > 1)
            {
                _visitedLocationIndexList.RemoveAt(_visitedLocationIndexList.Count - 1);
                SetupLocationAsset(_visitedLocationIndexList[_visitedLocationIndexList.Count - 1]);
            }
        }

        private void InstantiateHotspot(HotspotConfiguration hotspotConfig, HotspotHandler.HotspotType hotspotType, Action onClickAction)
        {
            HotspotHandler newPrefab = _hotspotPrefab.Find(p => p.hotspotType == hotspotType);
            if (newPrefab == null)
            {
                Debug.LogError($"[TourManager]: Prefab untuk hotspot type {hotspotType} tidak ditemukan di list hotspotPrefab!");
                return;
            }

            var nav = Instantiate(newPrefab, _canvasManager.transform);
            nav.transform.SetAsFirstSibling();

            Vector3 targetPosition = _sphereController.UVToWorldPosition(hotspotConfig.coordinate_x, hotspotConfig.coordinate_y);

            nav.SetupHotspot(
                hotspotName: hotspotConfig.hotspot_title,
                iconSprite: hotspotConfig.hotspot_image.GetSpriteImage(),
                action: onClickAction,
                position: targetPosition,
                canvas: _canvasManager.GetComponent<Canvas>(),
                rct: _canvasManager.GetComponent<RectTransform>(),
                cam: _cameraController.cam
            );

            if (!m_hotspotPooling.TryGetValue(hotspotType, out var list))
            {
                list = new List<HotspotHandler>();
                m_hotspotPooling[hotspotType] = list;
                list.Add(nav);
            }
            else
            {
                m_hotspotPooling[hotspotType].Add(nav);
            }
        }

        private Action GenerateNavigationActionByType(NavigationSetting settings)
        {
            Action result = null;

            switch (settings.target_type)
            {
                case TargetHotspot.BUILDING:
                    result = () =>
                    {
                        OnChangeLocationByDocumentId(settings.building_target.documentId);
                    };
                    break;

                case TargetHotspot.FACILITY:
                    result = () =>
                    {
                        OnChangeLocationByDocumentId(settings.facility_target.documentId);
                    };
                    break;

                case TargetHotspot.PANEL_NAVIGATION:
                    result = () => Debug.Log($"[TourManager]: Open Panel Navigation");
                    break;
            }

            return result;
        }

        private void HideHotspot()
        {
            if (m_hotspotPooling == null) return;

            foreach (var item in m_hotspotPooling)
            {
                foreach (var hotspot in item.Value)
                {
                    hotspot.gameObject.SetActive(false);
                }
            }
        }
    }
}