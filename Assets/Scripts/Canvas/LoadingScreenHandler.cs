using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Tour360TelkomCorpu.CanvasManager
{
    public class LoadingScreenHandler : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private GameObject _screenPanel;
        [SerializeField] private Image _imageBackground;
        [SerializeField] private Image _gifImage;
        [SerializeField] private TextMeshProUGUI _textLoadingMassage;
        [SerializeField] private Slider _loadingBar;

        [Header("GIF Animation")]
        [SerializeField] private List<Sprite> _gifFrames;   // frame-frame GIF
        [SerializeField] private float _gifSpeed = 0.08f;   // kecepatan animasi
        private Coroutine m_currentAnimation = null;
        private bool m_isStillHasApiProcess = false;

        public IEnumerator LoadingScreenForApiProcess(Func<Action<bool>, string, IEnumerator> _loadingProcess, String _documentId, Action _onComplete = null, Action _onError = null)
        {
            _screenPanel.SetActive(true);
            ShowLoadingGif(true);
            _textLoadingMassage.gameObject.SetActive(true);
            _loadingBar.gameObject.SetActive(true);

            m_isStillHasApiProcess = true;

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

                m_isStillHasApiProcess = false;

                _screenPanel.SetActive(false);
                HideLoadingGif();
                _textLoadingMassage.gameObject.SetActive(false);
                _loadingBar.gameObject.SetActive(false);
            }
            else
            {
                _loadingBar.value = 1f;
                yield return new WaitForSeconds(.8f);

                _onError?.Invoke();
            }
        }

        public void ShowLoadingGif(bool isUsingBackground)
        {
            if (m_currentAnimation != null) return;

            if (!_screenPanel.activeSelf) _screenPanel.SetActive(true);
            _imageBackground.enabled = isUsingBackground;

            _gifImage.gameObject.SetActive(true);
            m_currentAnimation = StartCoroutine(LoadingAnimation());
        }

        public void HideLoadingGif()
        {
            if (m_currentAnimation == null || m_isStillHasApiProcess) return;

            StopCoroutine(m_currentAnimation);
            m_currentAnimation = null;
            _gifImage.gameObject.SetActive(false);
            if (_screenPanel.activeSelf) _screenPanel.SetActive(false);
        }

        private IEnumerator LoadingAnimation()
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