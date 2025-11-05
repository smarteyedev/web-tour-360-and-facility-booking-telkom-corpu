using System.Collections.Generic;
using System;
using UnityEngine;

namespace WebTourCorpu.DataManager
{
    [Serializable]
    public class LocationDataModel
    {
        public string name;
        public string documentId;
        public LocationType locationType = LocationType.NONE;
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

        [Header("Additional Property for Drone and Building")]
        public ImageField maps_image;
        public ImageField description_image;

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
                background_360_image = b.background_360_image,
                first_camera_pov = b.first_camera_pov,
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
                category_functionality = f.category_functionality,
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
    }

    [Serializable]
    public enum LocationType
    {
        NONE, DRONE, BUILDING, FACILITY
    }
}