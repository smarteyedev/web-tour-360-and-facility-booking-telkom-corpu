using System.Collections.Generic;
using System;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace Tour360TelkomCorpu.DataManager
{
    [Serializable]
    public class LocationDataModel
    {
        public string name;
        public string documentId;
        public LocationType locationType = LocationType.NONE;
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

        [Header("Additional Property for Drone and Building")]
        public List<BuildingCategory> building_categories;
        public ImageField maps_image;
        public ImageField description_image;

        public bool IsHasCategory(string documentId)
        {
            return building_categories.Any((x) => x.documentId == documentId);
        }

        public static LocationDataModel FromDrone(Drone drone)
        {
            return new LocationDataModel
            {
                name = drone.name,
                documentId = drone.documentId,
                locationType = LocationType.DRONE,
                background_360_image = drone.background_360_image,
                first_camera_pov = drone.first_camera_pov,
                maps_image = drone.maps_image,
                description_image = drone.description_image,
                navigations = drone.navigations
            };
        }

        public static LocationDataModel FromBuilding(Building b)
        {
            return new LocationDataModel
            {
                name = b.name,
                documentId = b.documentId,
                locationType = LocationType.BUILDING,
                building_categories = b.building_categories,
                show_on_menu_panel = b.show_on_menu_panel,
                background_360_image = b.background_360_image,
                first_camera_pov = b.first_camera_pov,
                thumbnail_image = b.thumbnail_image,
                thumbnail_name = b.thumbnail_name,
                maps_image = b.maps_image,
                description_image = b.description_image,
                navigations = b.navigations
            };
        }

        public static LocationDataModel FromFacility(Facility f)
        {
            return new LocationDataModel
            {
                name = f.name,
                documentId = f.documentId,
                locationType = LocationType.FACILITY,
                building_parent = f.building_parent,
                bookable_status = f.bookable_status,
                show_on_menu_panel = f.show_on_menu_panel,
                background_360_image = f.background_360_image,
                first_camera_pov = f.first_camera_pov,
                thumbnail_image = f.thumbnail_image,
                thumbnail_name = f.thumbnail_name,
                facility_detail_image = f.facility_detail_image,
                description_text = f.description_text,
                gallery = f.gallery,
                navigations = f.navigations
            };
        }

        public bool IsThumbnailImageAssetDownloaded()
        {
            if (locationType != LocationType.FACILITY && locationType != LocationType.BUILDING) return true;

            return thumbnail_image.textureImage != null;
        }

        public Dictionary<Action<Texture2D>, string> GetDownloadableThumbnailImageAssetList(string baseUrl = null)
        {
            var targets = new Dictionary<Action<Texture2D>, string>();

            if (locationType == LocationType.FACILITY || locationType == LocationType.BUILDING)
            {
                if (ModelHandler.IsNeedToDownload(thumbnail_image))
                {
                    targets.Add(tex => thumbnail_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, thumbnail_image.url));
                }
            }

            return targets;
        }

        public bool IsInformationImageAssetDownloaded()
        {
            if (locationType == LocationType.FACILITY)
            {
                return background_360_image.textureImage != null && facility_detail_image.textureImage != null;
            }
            else
            {
                return background_360_image.textureImage != null && maps_image.textureImage != null && description_image.textureImage != null;
            }
        }

        public Dictionary<Action<Texture2D>, string> GetDownloadableInformationImageAssetList(string baseUrl = null)
        {
            var targets = new Dictionary<Action<Texture2D>, string>();

            if (ModelHandler.IsNeedToDownload(background_360_image))
            {
                targets.Add(tex => background_360_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, background_360_image.url));
            }

            if (navigations.Count > 0)
            {
                foreach (var nav in navigations)
                {
                    if (ModelHandler.IsNeedToDownload(nav.hotspot_configuration.hotspot_image))
                    {
                        targets.Add(tex => nav.hotspot_configuration.hotspot_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, nav.hotspot_configuration.hotspot_image.url));
                    }
                }
            }

            if (locationType == LocationType.FACILITY)
            {
                if (ModelHandler.IsNeedToDownload(facility_detail_image))
                {
                    targets.Add(tex => facility_detail_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, facility_detail_image.url));
                }

                if (gallery.Count > 0)
                {
                    foreach (var content in gallery)
                    {
                        foreach (var item in content.content_images)
                        {
                            if (ModelHandler.IsNeedToDownload(item))
                            {
                                targets.Add(tex => item.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, item.url));
                            }
                        }

                        if (ModelHandler.IsNeedToDownload(content.hotspot_configuration.hotspot_image))
                        {
                            targets.Add(tex => content.hotspot_configuration.hotspot_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, content.hotspot_configuration.hotspot_image.url));
                        }
                    }
                }
            }
            else
            {
                if (ModelHandler.IsNeedToDownload(maps_image))
                {
                    targets.Add(tex => maps_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, maps_image.url));
                }

                if (ModelHandler.IsNeedToDownload(description_image))
                {
                    targets.Add(tex => description_image.textureImage = tex, ModelHandler.BuildFullUrl(baseUrl, description_image.url));
                }
            }

            return targets;
        }
    }

    [Serializable]
    public enum LocationType
    {
        NONE, DRONE, BUILDING, FACILITY
    }
}