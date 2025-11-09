using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tour360TelkomCorpu.TourManager
{
    using Tour360TelkomCorpu.DataManager;
    using Tour360TelkomCorpu.CanvasManager;

    public class TourManager : MonoBehaviour
    {
        [SerializeField] private int _currentLocationIndex = 0;
        [SerializeField] private LocationDataModel _locationData = new LocationDataModel();

        [Header("Component References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private CanvasManager _canvasManager;

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            Debug.Log($"TourManager: Starting...");

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
                },
                _onError: () =>
                {
                    // loading process error when web request fail
                }
            ));
        }

        public void SetupLocationAsset(int targetIndex)
        {
            StartCoroutine(_dataManager.RequestLocationDataContentByIndex(
                locationIndex: targetIndex,
                onDone: (data) =>
                {
                    _locationData = data;
                    _currentLocationIndex = targetIndex;
                },
                (progress) => {/* Debug.Log($"{progress}") */},
                forceRedownload: false
            ));
        }
    }
}