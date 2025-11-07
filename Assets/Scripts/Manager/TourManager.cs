using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebTourCorpu.DataManager;

namespace Tour360TelkomCorpu
{
    public class TourManager : MonoBehaviour
    {
        [SerializeField] private int _currentLocationIndex = 0;
        [SerializeField] private LocationDataModel _locationData = new LocationDataModel();

        [Header("Component References")]
        [SerializeField] private LoadingScreenHandler _loadingScreen;
        [SerializeField] private DataManager _dataManager;

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
                _loadingProcess: _dataManager.GetTelkomCorpuAreaOptionData,
                _documentId: "",
                _onComplete: () =>
                {
                    // loading process complete
                    ShowCorpuAreaOption();
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
            StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
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

        public void ShowCorpuAreaOption()
        {
            StartCoroutine(_dataManager.RequestTelkomCorpuAreaOptionContent(
                (data) => Debug.Log($"content downloaded: {data.Count}"),
                (progress) => { /* Debug.Log($"{progress}") */ },
                false
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