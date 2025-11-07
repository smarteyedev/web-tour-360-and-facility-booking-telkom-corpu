using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebTourCorpu.DataManager;

namespace Tour360TelkomCorpu
{
    public class TourManager : MonoBehaviour
    {
        // [SerializeField] private List<TelkomCorpuAreaCard> _telkomCorpuAreaOptionList = new();
        [SerializeField] private LoadingScreenHandler _loadingScreen;

        [Header("Component References")]
        [SerializeField] private DataManager _dataManager;

        private void Start()
        {
            StartApplication();
        }

        public void StartApplication()
        {
            StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
              _loadingProcess: _dataManager.GetTelkomCorpuAreaOption,
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
        }

        public void GetTelkomCorpuDataMaster(string documentId)
        {
            StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
              _loadingProcess: _dataManager.GetTelkomCorpuDataMaster,
              _documentId: documentId,
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
            StartCoroutine(_dataManager.RequestCorpuAreaOptionsData(
                      (data) => Debug.Log($"content downloaded: {data.Count}"),
                      (progress) => { /* Debug.Log($"{progress}") */ },
                      false
                  ));
            ;
        }
    }
}