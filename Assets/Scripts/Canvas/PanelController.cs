using System;
using UnityEngine;

namespace Tour360TelkomCorpu.CanvasManager
{
    public enum PanelType
    {
        None, WelcomingSection, CorpuAreaSelection, GuidanceSection, BuildingDescription, FacilityDescription, GalleryPhoto, MenuNavigationCategory, MenuNavigationToFacility, BookingSection, DroneDescription
    }

    public interface IPanel
    {
        void ShowPanel(object data, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null);
        void HidePanel();
        PanelType panelIdentity();
    }

    [Serializable]
    public class FormatPanelLocationMapsAsset
    {
        public Sprite mapsSprite;
        public Sprite DescriptionSprite;
    }

    [Serializable]
    public class FormatPanelDescriptionAsset
    {
        public string descriptionText;
        public Sprite facilityDetailSprite;
    }

    public abstract class PanelController<TData, Taction> : MonoBehaviour, IPanel
    {
        [Header("PanelController Base")]
        [SerializeField] protected PanelType _panelIndentity;

        protected virtual void Awake() { }
        protected virtual void Start() { }

        PanelType IPanel.panelIdentity()
        {
            return _panelIndentity;
        }

        void IPanel.ShowPanel(object data, Action<string> callbackUsingDocumentId, Action onClosePanel)
        {
            if (data == null)
            {
                ShowPanel(default);
                return;
            }

            if (data is TData d)
            {
                ShowPanel(d, callbackUsingDocumentId, onClosePanel);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"{name}: ShowPanel expected data of type {typeof(TData)}, but received {data.GetType()}.", this);
#endif
            }
        }
        protected abstract void ShowPanel(TData contentData, Action<string> callbackUsingDocumentId = null, Action onClosePanel = null);

        void IPanel.HidePanel()
        {
            HidePanel();
        }
        public abstract void HidePanel();
    }
}