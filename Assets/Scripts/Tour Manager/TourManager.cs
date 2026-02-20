using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tour360TelkomCorpu.TourManager
{
    using Tour360TelkomCorpu.DataManager;
    using Tour360TelkomCorpu.CanvasManager;
    using System.Collections;

    public class TourManager : MonoBehaviour
    {
        [Header("Datas")]
        private int m_currentLocationIndex = 0;
        [SerializeField] private LocationDataModel _locationData = new LocationDataModel();
        [SerializeField] private List<int> _visitedLocationIndexList = new List<int>();
        [SerializeField] private BuildingCategory _categorySelectedOnNavigationMenu = new BuildingCategory();

        [Header("Configuration")]

        public TargetEnvironment targetEnvironment = TargetEnvironment.Development;
        [Serializable]
        public enum TargetEnvironment
        {
            Development, Demo, Production
        }

        [SerializeField] private bool _isAlwaysShowLocationDescription = true;
        public float minVolumeMasterAudio = 0.3f;
        public float maxVolumeMasterAudio = 0.7f;

        [Space(10f)]

        private Coroutine _cleanupCoroutine;

        [Header("Component References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private CanvasManager _canvasManager;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private List<HotspotHandler> _hotspotPrefab;
        private Dictionary<HotspotHandler.HotspotType, List<HotspotHandler>> m_hotspotPooling = new Dictionary<HotspotHandler.HotspotType, List<HotspotHandler>>();
        [SerializeField] private SphereController _sphereController;

        private bool m_isTryToLoadingAsset = false;

        [Serializable]
        public enum TransitionAnimType
        {
            FadeBackground, CameraZoomInAndFall, CameraZoomIn
        }

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            Debug.Log($"TourManager: Starting App...");

            m_currentLocationIndex = 0;
            m_isTryToLoadingAsset = true;

            StartCoroutine(_canvasManager.loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuAreaOptionData,
                _documentId: "",
                _onComplete: () =>
                {
                    // loading process complete

                    MusicManager.Main.SetVolume(maxVolumeMasterAudio, 0);
                    MusicManager.Main.PlayFromLibrary("Backsound");

                    StartCoroutine(_dataManager.RequestTelkomCorpuAreaOptionContent(
                        (data) =>
                        {
                            _canvasManager.OpenPanel(
                                panelType: PanelType.WelcomingSection,
                                data: data,
                                callback: null,
                                onClosePanel: () =>
                                    {
                                        _canvasManager.OpenPanel(PanelType.CorpuAreaSelection, data, (object documentId) => GetTelkomCorpuDataMaster((string)documentId), null);
                                    }
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
        }

        public void OpenCorpuAreaSelectionPanel()
        {
            if (m_isTryToLoadingAsset == true) return;

            StartCoroutine(_dataManager.RequestTelkomCorpuAreaOptionContent((data) =>
            {
                _canvasManager.OpenPanel(PanelType.CorpuAreaSelection, data, (object documentId) => GetTelkomCorpuDataMaster((string)documentId), null);
                _visitedLocationIndexList.Clear();
                m_currentLocationIndex = 0;
            },
                (progress) => { /* Debug.Log($"{progress}") */ },
                false
            ));
        }

        public void GetTelkomCorpuDataMaster(string _documentId)
        {
            m_isTryToLoadingAsset = true;

            StartCoroutine(_canvasManager.loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuDataMaster,
                _documentId: _documentId,
                _onComplete: () =>
                {
                    m_isTryToLoadingAsset = false;

                    // loading process complete
                    _canvasManager.CloseAllPanel();
                    SetupLocationAsset(m_currentLocationIndex);
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
            if (m_isTryToLoadingAsset == true) return;

            StartCoroutine(_dataManager.RequestLocationDataContentByIndex(
                locationIndex: targetIndex,
                onValidStart: () =>
                {
                    if (_cleanupCoroutine != null)
                    {
                        StopCoroutine(_cleanupCoroutine);
                    }

                    _cleanupCoroutine = StartCoroutine(CleanupLocationAssetsCoroutine(
                        _locationData,
                        deepCleanup: true,
                        maxDestroyPerFrame: 50
                    ));

                    m_isTryToLoadingAsset = true;
                    _canvasManager.loadingScreen.ShowLoadingGif(false);

                    HideHotspot();

                    _canvasManager.CloseAllPanel();
                },
                onDone: (data) =>
                {
                    if (data != null)
                    {
                        _locationData = data;
                        m_currentLocationIndex = targetIndex;

                        if (_visitedLocationIndexList.Count == 0)
                        {
                            _canvasManager.SetActiveBarMenu(false);
                            _canvasManager.OpenPanel(PanelType.GuidanceSection, $"Buka Guidance", (object a) => { _canvasManager.SetActiveBarMenu(true); }, null);
                        }

                        if (_visitedLocationIndexList.Count == 0 || m_currentLocationIndex != _visitedLocationIndexList[_visitedLocationIndexList.Count - 1])
                            _visitedLocationIndexList.Add(m_currentLocationIndex);
#if UNITY_EDITOR
                        Debug.Log($"TourManager: Already Get Location {data.name} asset");
#endif

                        // START: SET ASSET FUNCTION ...

                        TransitionAnimType _transitionType = _locationData.locationType == LocationType.DRONE ? TransitionAnimType.CameraZoomInAndFall : TransitionAnimType.FadeBackground;

                        StartTransition(
                            type: _transitionType,
                            targetTexture: _locationData.background_360_image.textureImage,
                            camRotation: _locationData.first_camera_pov,
                            onStart: () =>
                            {

                            },
                            onFinish: () =>
                            {
                                _canvasManager.SetLocationDataUI(_locationData.name, GenerateShowLocationDescriptionAction(), _locationData.bookable_status, () =>
                                {
                                    _canvasManager.OpenPanel(PanelType.BookingSection, new FormatPanelBooking($"", $"https://facilitycorpu.id/booking/create?type=classroom&&classroom={5}"), (object link) => OpenLink((string)link), null);
                                });

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
                                                InstantiateHotspotGalleryHotspot(item.hotspot_configuration);
                                            }
                                        }
                                        else
                                        {
                                            InstantiateHotspotGalleryHotspot(item.hotspot_configuration);
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
                                                    action: GenerateHotspotActionByType(navigationData),
                                                    position: targetPosition,
                                                    canvas: _canvasManager.GetComponent<Canvas>(),
                                                    rct: _canvasManager.GetComponent<RectTransform>(),
                                                    cam: _cameraController.cam
                                                );

                                                tHotspot.gameObject.SetActive(true);
                                            }
                                            else
                                            {
                                                InstantiateHotspot(navigationData.hotspot_configuration, (HotspotHandler.HotspotType)tHotspotType, GenerateHotspotActionByType(navigationData));
                                            }
                                        }
                                        else
                                        {
                                            InstantiateHotspot(navigationData.hotspot_configuration, (HotspotHandler.HotspotType)tHotspotType, GenerateHotspotActionByType(navigationData));
                                        }
                                    }
                                }

                                _canvasManager.loadingScreen.HideLoadingGif();
                                m_isTryToLoadingAsset = false;

                                if (_isAlwaysShowLocationDescription && _locationData.locationType == LocationType.FACILITY)
                                {
                                    GenerateShowLocationDescriptionAction()?.Invoke();
                                }
                            }
                        );
                        // END: SET ASSET FUNCTION ...
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

        public void StartTransition(TransitionAnimType type, Texture targetTexture, float camRotation, Action onStart, Action onFinish)
        {
            switch (type)
            {
                case TransitionAnimType.FadeBackground:
                    _sphereController.ChangeTextureWithFade(
                            targetTexture: _locationData.background_360_image.textureImage,
                            duration: 1.0f,
                            onStartTransition: () =>
                            {
                                onStart?.Invoke();
                                _cameraController.SetHorizontalRotaion(camRotation);
                            },
                            onFinishTransition: onFinish
                        );

                    break;
                case TransitionAnimType.CameraZoomInAndFall:
                    _sphereController.ChangeTextureWithFade(
                                targetTexture: _locationData.background_360_image.textureImage,
                                duration: .1f,
                                onStartTransition: onStart,
                                onFinishTransition: () =>
                                {
                                    _cameraController.ZoomWithFallTransition(camRotation, onStart, onFinish);
                                }
                            );
                    break;
            }
        }


        #region Hotspot Functionality

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

        private void InstantiateHotspotGalleryHotspot(HotspotConfiguration config)
        {
            InstantiateHotspot(config, HotspotHandler.HotspotType.OpenPanelGallery, () =>
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

        private Action GenerateHotspotActionByType(NavigationSetting settings)
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
                    result = () =>
                    {
                        if (_locationData.locationType == LocationType.DRONE)
                        {
                            OpenPanelNavigationToBuilding();
                        }
                        else OpenPanelNavigationToFacility();
                    };
                    break;
            }

            return result;
        }

        private Action GenerateShowLocationDescriptionAction()
        {
            Action result = null;

            switch (_locationData.locationType)
            {
                case LocationType.DRONE:
                    result = () =>
                    {
                        if (m_isTryToLoadingAsset == true) return;
                        _canvasManager.OpenPanel(PanelType.DroneDescription, _locationData, null, null);
                    };
                    break;
                case LocationType.BUILDING:
                    result = () =>
                    {
                        if (m_isTryToLoadingAsset == true) return;
                        _canvasManager.OpenPanel(PanelType.BuildingDescription, _locationData, null, null);
                    };
                    break;
                case LocationType.FACILITY:
                    result = () =>
                    {
                        if (m_isTryToLoadingAsset == true) return;
                        FormatPanelDescriptionAsset dFacility = new FormatPanelDescriptionAsset();
                        dFacility.titleText = _locationData.name;
                        dFacility.descriptionText = _locationData.description_text;
                        dFacility.facilityDetailSprite = _locationData.facility_detail_image.GetSpriteImage();
                        dFacility.isCanBook = _locationData.bookable_status;
                        dFacility.onOpenPanelBooking = () => _canvasManager.OpenPanel(PanelType.BookingSection, new FormatPanelBooking($"", $"https://facilitycorpu.id/booking/create?type=classroom&&classroom={5}"), (object link) => OpenLink((string)link), null);
                        dFacility.isAutoShow = _isAlwaysShowLocationDescription;
                        _canvasManager.OpenPanel(PanelType.FacilityDescription, dFacility, (object newVal) => _isAlwaysShowLocationDescription = (bool)newVal, null);
                    };
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

        private void OpenPanelNavigationToFacility()
        {
            if (_locationData.locationType == LocationType.DRONE) return;

            string targetParent = _locationData.locationType == LocationType.FACILITY ? _locationData.building_parent.documentId : _locationData.documentId;

            StartCoroutine(_dataManager.RequestFacilityListByBuildingParent(
                parentDocumentId: targetParent,
                onValidStart: () =>
                {
                    // Debug.Log($"[{name}]: starting search for data panel navigation...");

                    m_isTryToLoadingAsset = true;
                },
                onDone: (List<LocationDataModel> locations) =>
                {
                    /* for (int i = 0; i < data.Count; i++)
                    {
                        Debug.Log($"[{name}]| navigation option {i + 1} to {data[i].thumbnail_name} & ...");
                    } */

                    FormatPaginationData data = new FormatPaginationData();
                    data.isUsingCategory = false;
                    data.currentCategorySelected = null;
                    data.categoryList = new List<BuildingCategory>();
                    data.locationDataList = locations;
                    data.onChangeCategoryAction = null;

                    _canvasManager.OpenPanel(PanelType.MenuNavigation, data, (object documentId) => OnChangeLocationByDocumentId((string)documentId), null);

                    m_isTryToLoadingAsset = false;
                },
                onProgress: (float progress) => { },
                forceRedownload: false
            ));
        }
        #endregion

        #region Button Bar Functionality

        public void NextLocation()
        {
            if (m_isTryToLoadingAsset == true) return;

            SetupLocationAsset(m_currentLocationIndex + 1);
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

        public void SetFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        public void OpenPanelNavigationToBuilding()
        {
            if (m_isTryToLoadingAsset == true) return;

            string target = String.IsNullOrEmpty(_categorySelectedOnNavigationMenu.documentId) ? _dataManager.GetFirstBuildingCategoryData().documentId : _categorySelectedOnNavigationMenu.documentId;

            // Debug.Log($"[TourManger.cs]: taget id {target}");

            StartCoroutine(_dataManager.RequestBuildingListByCategory(
                categoryDocumentId: target,
                onValidStart: () =>
                {
                    m_isTryToLoadingAsset = true;
                },
                result: (currentCategory, categoryList, locations) =>
                {
                    _categorySelectedOnNavigationMenu = currentCategory;

                    FormatPaginationData data = new FormatPaginationData();
                    data.isUsingCategory = true;
                    data.currentCategorySelected = currentCategory;
                    data.categoryList = categoryList;
                    data.locationDataList = locations;
                    data.onChangeCategoryAction = (BuildingCategory targetCategory) =>
                    {
                        _categorySelectedOnNavigationMenu = targetCategory;
                        OpenPanelNavigationToBuilding();
                    };

                    // Debug.Log($"[TourManager.cs]| jumlah data {data.locationDataList.Count}");

                    _canvasManager.OpenPanel(PanelType.MenuNavigation, data, (object documentId) => OnChangeLocationByDocumentId((string)documentId), null);

                    m_isTryToLoadingAsset = false;
                },
                onProgress: (float progress) => { },
                forceRedownload: false
            ));
        }

        [DllImport("__Internal")]
        private static extern void OpenInSameTab(string url);
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

        #endregion

        #region ===== MEMORY CLEANUP =====

        private IEnumerator CleanupLocationAssetsCoroutine(
            LocationDataModel loc,
            bool deepCleanup,
            int maxDestroyPerFrame = 10
        )
        {
            if (loc == null) yield break;

            int destroyedThisFrame = 0;

            // Helper lokal untuk destroy bertahap
            System.Action<Texture2D> destroyTex = (tex) =>
            {
                if (tex == null) return;
                Destroy(tex);
            };

            // 1. Background 360
            /* if (loc.background_360_image != null && loc.background_360_image.textureImage != null)
            {
                destroyTex(loc.background_360_image.textureImage);
                loc.background_360_image.textureImage = null;

                destroyedThisFrame++;
                if (destroyedThisFrame >= maxDestroyPerFrame)
                {
                    destroyedThisFrame = 0;
                    yield return null; // jeda 1 frame
                }
            } */

            // 2. Maps & description image (umumnya hanya di facility / building tertentu)
            if (loc.maps_image != null && loc.maps_image.textureImage != null)
            {
                destroyTex(loc.maps_image.textureImage);
                loc.maps_image.textureImage = null;

                destroyedThisFrame++;
                if (destroyedThisFrame >= maxDestroyPerFrame)
                {
                    destroyedThisFrame = 0;
                    yield return null;
                }
            }

            if (loc.description_image != null && loc.description_image.textureImage != null)
            {
                destroyTex(loc.description_image.textureImage);
                loc.description_image.textureImage = null;

                destroyedThisFrame++;
                if (destroyedThisFrame >= maxDestroyPerFrame)
                {
                    destroyedThisFrame = 0;
                    yield return null;
                }
            }

            // 3. Facility detail image
            if (loc.facility_detail_image != null && loc.facility_detail_image.textureImage != null)
            {
                destroyTex(loc.facility_detail_image.textureImage);
                loc.facility_detail_image.textureImage = null;

                destroyedThisFrame++;
                if (destroyedThisFrame >= maxDestroyPerFrame)
                {
                    destroyedThisFrame = 0;
                    yield return null;
                }
            }

            // 4. Gallery + gallery hotspot
            if (loc.gallery != null)
            {
                foreach (var g in loc.gallery)
                {
                    if (g == null) continue;

                    if (g.content_images != null)
                    {
                        foreach (var img in g.content_images)
                        {
                            if (img == null || img.textureImage == null) continue;

                            destroyTex(img.textureImage);
                            img.textureImage = null;

                            destroyedThisFrame++;
                            if (destroyedThisFrame >= maxDestroyPerFrame)
                            {
                                destroyedThisFrame = 0;
                                yield return null;
                            }
                        }
                    }

                    if (g.hotspot_configuration != null &&
                        g.hotspot_configuration.hotspot_image != null &&
                        g.hotspot_configuration.hotspot_image.textureImage != null)
                    {
                        destroyTex(g.hotspot_configuration.hotspot_image.textureImage);
                        g.hotspot_configuration.hotspot_image.textureImage = null;

                        destroyedThisFrame++;
                        if (destroyedThisFrame >= maxDestroyPerFrame)
                        {
                            destroyedThisFrame = 0;
                            yield return null;
                        }
                    }
                }
            }

            // 5. Navigations hotspot icon
            if (loc.navigations != null)
            {
                foreach (var nav in loc.navigations)
                {
                    if (nav == null ||
                        nav.hotspot_configuration == null ||
                        nav.hotspot_configuration.hotspot_image == null ||
                        nav.hotspot_configuration.hotspot_image.textureImage == null)
                        continue;

                    destroyTex(nav.hotspot_configuration.hotspot_image.textureImage);
                    nav.hotspot_configuration.hotspot_image.textureImage = null;

                    destroyedThisFrame++;
                    if (destroyedThisFrame >= maxDestroyPerFrame)
                    {
                        destroyedThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // 6. Deep cleanup opsional: bebaskan asset tidak terpakai + GC
            if (deepCleanup)
            {
                // UnloadUnusedAssets cukup berat → dijalankan saat layar sedang fade/loading
                yield return Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        #endregion

        #region Button_Settings

        public void ExitTour()
        {

        }

        #endregion

    }
}