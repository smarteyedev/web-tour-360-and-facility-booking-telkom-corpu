using System;
using System.Collections.Generic;
using UnityEngine;


namespace Tour360TelkomCorpu.CanvasManager
{
    using UnityEngine.UI;
    public class PanelGalleryPhoto : PanelController<List<Sprite>, string>
    {
        [Header("Guidance Section")]

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private RectTransform _rectContentParent;
        [SerializeField] private Image _imagePrefab;
        private List<Image> m_ImagePooling = new List<Image>();
        [SerializeField] private Button buttonClose;

        protected override void ShowPanel(List<Sprite> spriteList, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null)
        {
            _panelContainer.gameObject.SetActive(true);

            if (m_ImagePooling.Count > 0)
            {
                foreach (var spawnedImage in m_ImagePooling)
                {
                    spawnedImage.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < spriteList.Count; i++)
            {
                Sprite sprite = spriteList[i];
                Image image;
                if (i < m_ImagePooling.Count)
                {
                    image = m_ImagePooling[i];
                    image.gameObject.SetActive(true);
                }
                else
                {
                    image = Instantiate(_imagePrefab, _rectContentParent, false);
                    m_ImagePooling.Add(image);
                }
                image.sprite = sprite;
            }

            buttonClose.onClick.RemoveAllListeners();
            buttonClose.onClick.AddListener(() => onClosePanel?.Invoke());
        }

        public override void HidePanel()
        {
            _panelContainer.gameObject.SetActive(false);
        }
    }
}
