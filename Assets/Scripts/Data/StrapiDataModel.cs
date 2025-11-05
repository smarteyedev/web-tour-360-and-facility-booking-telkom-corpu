using System.Collections.Generic;
using System;
using UnityEngine;

namespace WebTourCorpu.DataManager
{
    #region ===== Strapi Data Model =====

    [Serializable]
    public class GqlResponse<T>
    {
        public T data;
    }

    public class TelkomCorpuAreas
    {
        public List<TelkomCorpuAreaCard> telkomCorpuAreas;
    }

    [Serializable]
    public class TelkomCorpuAreaCard
    {
        public string name;
        public string documentId;
        public string address;
        public bool open_for_visitor;
        public string thumbnail_name;
        public ImageField thumbnail_image;
    }

    [Serializable]
    public class TelkomCorpuAreaDataMaster
    {
        public TelkomCorpuArea telkomCorpuArea;
    }

    [Serializable]
    public class TelkomCorpuArea
    {
        public string name;
        public string documentId;
        public string address;
        public bool open_for_visitor;
        public string thumbnail_name;
        public ImageField thumbnail_image;
        public List<Drone> drone_views;
        public List<Building> buildings_childs;
    }

    [Serializable]
    public class Drone
    {
        public string name;
        public string documentId;
        public ImageField background_360_image;
        public float first_camera_pov;
        public ImageField maps_image;
        public ImageField description_image;
        public List<NavigationSetting> navigations;
    }

    [Serializable]
    public class Building
    {
        public string name;
        public string documentId;
        public ImageField background_360_image;
        public int first_camera_pov;
        public ImageField maps_image;
        public ImageField description_image;
        public List<Facility> facilities_childs;
        public List<NavigationSetting> navigations;
    }

    [Serializable]
    public class Facility
    {
        public string name;
        public string documentId;
        public CategoryFunctionality category_functionality;
        public bool bookable_status;
        public bool show_on_menu_panel;
        public ImageField background_360_image;
        public float first_camera_pov;
        public ImageField thumbnail_image;
        public string thumbnail_name;
        public ImageField facility_detail_image;
        public string description_text;
        public List<FacilityGallery> gallery;
        public List<NavigationSetting> navigations;
    }

    [Serializable]
    public class CategoryFunctionality
    {
        public string functionality;
        public string description;
    }

    [Serializable]
    public class FacilityGallery
    {
        public List<ImageField> content_images;
        public HotspotConfiguration hotspot_configuration;
    }

    [Serializable]
    public class ImageField
    {
        public string url;
        public Texture2D textureImage;

        public Sprite GetSpriteImage()
        {
            if (textureImage == null)
            {
                Debug.Log($"asset is null");
                return null;
            }

            var rect = new Rect(0, 0, textureImage.width, textureImage.height);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(textureImage, rect, pivot, 100f, 0, SpriteMeshType.Tight, Vector4.zero, false);
        }
    }

    [Serializable]
    public class NavigationSetting
    {
        public TargetHotspot target_type;
        public BuildingTarget building_target;
        public FacilityTarget facility_target;
        public HotspotConfiguration hotspot_configuration;
    }

    [Serializable]
    public class BuildingTarget
    {
        public string documentId;
        public string name;
    }

    [Serializable]
    public class FacilityTarget
    {
        public string documentId;
        public string name;
    }

    [Serializable]
    public class HotspotConfiguration
    {
        public string hotspot_title;
        public float coordinate_x;
        public float coordinate_y;
        public ImageField hotspot_image;
    }

    [Serializable]
    public enum TargetHotspot
    {
        FACILITY,
        BUILDING,
        PANEL_GALLERY,
        PANEL_NAVIGATION
    }
    #endregion
}