using System;
using System.Collections.Generic;
using Tour360TelkomCorpu.DataManager;
using UnityEngine;

namespace Tour360TelkomCorpu.CanvasManager
{
    public enum PanelType
    {
        None, WelcomingSection, CorpuAreaSelection, GuidanceSection, BuildingDescription, FacilityDescription, GalleryPhoto, MenuNavigation, BookingSection, DroneDescription
    }

    public interface IPanel
    {
        void ShowPanel(object data, Action<object> callback = null, Action onClosePanel = null);
        void HidePanel();
        PanelType panelIdentity();
    }

    [Serializable]
    public class FormatPanelBooking
    {
        public string facilityName;
        public string urlBooking;

        public FormatPanelBooking(string name, string url)
        {
            facilityName = name;
            urlBooking = url;
        }
    }

    [Serializable]
    public class FormatPanelDescriptionAsset
    {
        public string titleText;
        public string descriptionText;
        public Sprite facilityDetailSprite;
        public bool isCanBook;
        public Action onOpenPanelBooking;
        public bool isAutoShow;
    }

    [Serializable]
    public class FormatPaginationData
    {
        public bool isUsingCategory;
        public BuildingCategory currentCategorySelected;
        public List<BuildingCategory> categoryList;
        public List<LocationDataModel> locationDataList;
        public Action<BuildingCategory> onChangeCategoryAction;
    }

    public abstract class PanelController<TData, TAction> : MonoBehaviour, IPanel
    {
        [Header("PanelController Base")]
        [SerializeField] protected PanelType _panelIndentity;

        protected virtual void Awake() { }
        protected virtual void Start() { }

        PanelType IPanel.panelIdentity()
        {
            return _panelIndentity;
        }

        void IPanel.ShowPanel(object data, Action<object> callback, Action onClosePanel)
        {
            // Jika data null, panggil ShowPanel dengan default data
            if (data == null)
            {
                ShowPanel(default, null, onClosePanel);
                return;
            }

            if (!(data is TData d))
            {
#if UNITY_EDITOR
                Debug.LogError($"{name}: ShowPanel expected data of type {typeof(TData)}, but received {data.GetType()}.", this);
#endif
                return;
            }

            Action<TAction> typedCallback = null;
            if (callback is Action<TAction> cb)
            {
                typedCallback = cb;
            }
            else if (callback != null)
            {
                typedCallback = (TAction tValue) =>
                        {
                            try
                            {
                                callback.Invoke((object)tValue); // boxing if TAction is value type
                            }
                            catch (InvalidCastException icex)
                            {
#if UNITY_EDITOR
                                Debug.LogWarning($"{name}: Failed to forward callback due to invalid cast: {icex}. Callback ignored.", this);
#endif
                            }
                            catch (Exception ex)
                            {
#if UNITY_EDITOR
                                Debug.LogError($"{name}: Exception when invoking forwarded callback: {ex}", this);
#endif
                            }
                        };
            }

            ShowPanel(d, typedCallback, onClosePanel);
        }
        protected abstract void ShowPanel(TData contentData, Action<TAction> callback = null, Action onClosePanel = null);

        void IPanel.HidePanel()
        {
            HidePanel();
        }
        public abstract void HidePanel();
    }
}