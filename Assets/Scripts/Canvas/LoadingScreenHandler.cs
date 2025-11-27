using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class LoadingScreenHandler : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] public GameObject _screenPanel;
        [SerializeField] private Slider _loadingBar;

        [Header("GIF Animation")]
        [SerializeField] private Image _gifImage;           // tempat tampil GIF
        [SerializeField] private List<Sprite> _gifFrames;   // frame-frame GIF
        [SerializeField] private float _gifSpeed = 0.08f;   // kecepatan animasi

        public IEnumerator LoadingScreenForApiProcess(Func<Action<bool>, string, IEnumerator> _loadingProcess, String _documentId, Action _onComplete = null, Action _onError = null)
        {


            _screenPanel.SetActive(true);
            _loadingBar.value = 0f;
            float _visualProgress = 0f;
            float _speed = 0.5f;

            // Simulasi progres visual hingga mendekati 100%
            while (_visualProgress < 0.20f)
            {
                _visualProgress += Time.deltaTime * _speed;
                _loadingBar.value = Mathf.Clamp01(_visualProgress);
                yield return null;
            }

            // Jalankan loading proses (ambil data) dan tunggu hasil sukses/gagal
            bool isSuccess = false;
            yield return StartCoroutine(_loadingProcess.Invoke(success => isSuccess = success, _documentId));

            _onComplete?.Invoke();

            // Simulasi progres visual hingga mendekati 100%
            while (_visualProgress < 0.95f)
            {
                _visualProgress += Time.deltaTime * _speed;
                _loadingBar.value = Mathf.Clamp01(_visualProgress);
                yield return null;
            }

            if (isSuccess)
            {
                _loadingBar.value = 1f;
                // yield return new WaitForSeconds(0.5f); // jeda

                _screenPanel.SetActive(false);
                _loadingBar.value = 0f;
            }
            else
            {
                _loadingBar.value = 1f;
                yield return new WaitForSeconds(.8f);

                _onError?.Invoke();
            }
        }

        public void Start()
        {
            // Mulai animasi GIF ketika object aktif
            StartCoroutine(LoadingAnimation());
        }

        public IEnumerator LoadingAnimation()
        {
            if (_gifFrames == null || _gifFrames.Count == 0)
                yield break;

            int index = 0;

            while (true)
            {
                _gifImage.sprite = _gifFrames[index];
                index = (index + 1) % _gifFrames.Count;
                yield return new WaitForSeconds(_gifSpeed);
            }
        }
    }
}