using System.Collections.Generic;
using System;
using UnityEngine;

namespace Tour360TelkomCorpu.DataManager
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

        public bool IsImageAssetDownloaded()
        {
            return thumbnail_image.textureImage != null;
        }

        public Dictionary<Action<Texture2D>, string> DownloadAssetList(string baseUrl = null)
        {
            var targets = new Dictionary<Action<Texture2D>, string>();

            if (ModelHandler.IsNeedToDownload(thumbnail_image))
            {
                targets.Add(tex => thumbnail_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, thumbnail_image.url));
            }

            return targets;
        }
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
        public List<BuildingCategory> building_categories;
        public bool show_on_menu_panel;
        public ImageField background_360_image;
        public int first_camera_pov;
        public ImageField thumbnail_image;
        public string thumbnail_name;
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
        public BuildingTarget building_parent;
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
    public class Categories
    {
        public List<BuildingCategory> categories;
    }

    [Serializable]
    public class BuildingCategory
    {
        public string category_name;
        public string documentId;
        public List<BuildingChild> buildings;

        [Serializable]
        public struct BuildingChild
        {
            public string name;
            public string documentId;
        }
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
                Debug.LogWarning($"[StrapiDataModel.cs]: Asset texture from strapi is null");
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
        FACILITY = 1,
        BUILDING = 2,
        PANEL_NAVIGATION = 3,
        //PANEL_GALLERY = 4
    }

    public static class ModelHandler
    {
        public static string BuildFullUrl(string baseUrl, string path)
        {
            // Jika sudah absolute URL, langsung pakai
            if (Uri.TryCreate(path, UriKind.Absolute, out var abs)) return abs.ToString();

            // Kalau baseUrl tidak ada, kembalikan path apa adanya
            if (string.IsNullOrWhiteSpace(baseUrl)) return path;

            // Gabungkan baseUrl + path (tangani slash ganda)
            if (baseUrl.EndsWith("/")) baseUrl = baseUrl.TrimEnd('/');
            if (!path.StartsWith("/")) path = "/" + path;
            return baseUrl + path;
        }

        public static bool IsNeedToDownload(ImageField targetImage)
        {
            // Debug.Log($"try to check image: {targetImage.url}");
            return targetImage != null && targetImage.textureImage == null && !string.IsNullOrEmpty(targetImage.url);
        }
    }

    #endregion
}