using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tour360TelkomCorpu.CanvasManager
{
    public interface IPanel
    {
        public enum PanelType
        {
            None, WelcomingSection, CorpuAreaSelection, GuidanceSection, BuildingDescription, FacilityDescription, GalleryPhoto, MenuNavigationCategory, MenuNavigationToFacility, BookingSection
        }

        void ShowPanel(object data);
        void HidePanel();
        PanelType panelIdentity();
    }

    public abstract class PanelController<T> : MonoBehaviour, IPanel
    {
        [Header("PanelController Base")]
        [SerializeField] protected IPanel.PanelType panelIndentity;

        protected virtual void Start() { }

        IPanel.PanelType IPanel.panelIdentity()
        {
            return panelIndentity;
        }

        void IPanel.ShowPanel(object data)
        {
            if (data == null)
            {
                ShowPanel(default);
                return;
            }

            if (data is T d)
            {
                ShowPanel(d);
                // Debug.Log($"{name}: data cast success ({typeof(T)})");
            }
            else
            {
                // Debug.LogError($"{name}: ShowPanel expected data of type {typeof(T)}, but received {data.GetType()}.", this);
            }
        }
        protected abstract void ShowPanel(T contentData);

        void IPanel.HidePanel()
        {
            HidePanel();
        }
        protected abstract void HidePanel();
    }
}