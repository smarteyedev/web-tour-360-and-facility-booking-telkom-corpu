using System;
using System.Collections.Generic;
using UnityEngine;


namespace Tour360TelkomCorpu.CanvasManager
{
    using Tour360TelkomCorpu.DataManager;
    using UnityEngine.UI;
    public class PanelGalleryPhoto : PanelController<List<Sprite>, Action>
    {
        [Header("Guidance Section")]

        [Header("Component References")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private RectTransform _rectContentParent;
        [SerializeField] private Image _imagePrefab;
        [SerializeField] private List<Image> m_ImagePooling;


        protected override void ShowPanel(List<Sprite> spriteList, Action<string> callbackUsingDocumentId = null)
        {
            Debug.Log("📌 [Gallery] ShowPanel DIPANGGIL!");
            _panelContainer.gameObject.SetActive(true);

            for(int i = 0; i < spriteList.Count; i++)
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
     
        }


        public override void HidePanel() 
        {
            _panelContainer.gameObject.SetActive(false);
        }
    }
}
