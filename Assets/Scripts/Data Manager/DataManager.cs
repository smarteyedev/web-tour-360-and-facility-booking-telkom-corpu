using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Smarteye.RestAPI;
using System;
using System.Linq;

namespace Tour360TelkomCorpu.DataManager
{
  public class DataManager : RestAPIHandler
  {
    [Header("Data Manager | Cache Data Asset")]
    [SerializeField] private List<TelkomCorpuAreaCard> _telkomCorpuAreaOptionList = new();
    [SerializeField] private List<BuildingCategory> _buildingCategoryList = new();
    [SerializeField] private TelkomCorpuAreaCard _telkomCorpuAreaSelected = new();
    [SerializeField] private List<LocationDataModel> _locationDataList = new();

    public IEnumerator GetTelkomCorpuAreaOptionData(Action<bool> onResult, string documentId)
    {
      bool areaOptionProcess = false;
      bool areaOptionResult = false;

      bool categoryProcess = false;

      string gqlQueryCorpuSelection = @"
      query CorpuAreaSelection{
        telkomCorpuAreas {
          documentId
          name
          address
          open_for_visitor
          thumbnail_name
          thumbnail_image {
            url
            name
          }
        }
      }";

      var bodyCorpuSelection = new
      {
        query = gqlQueryCorpuSelection,
      };

      string jsonBody = JsonConvert.SerializeObject(bodyCorpuSelection);

      restAPI.PostWithHeaderAndBody(
        _endpointTitle: "HitStrapi",
        _body: jsonBody,
        _success: (result) =>
        {
          var response = JsonConvert.DeserializeObject<GqlResponse<TelkomCorpuAreas>>(result.ToString());

          if (_telkomCorpuAreaOptionList.Count > 0) _telkomCorpuAreaOptionList.Clear();
          _telkomCorpuAreaOptionList = response.data.telkomCorpuAreas;



          areaOptionProcess = true;
          areaOptionResult = true;
        },
        _err: (errResult) =>
        {

          areaOptionProcess = true;
          areaOptionResult = false;
        });

      string gqlQueryBuildingCategory = @"
              query Categories {
                categories {
                  category_name
                  documentId
                  buildings {
                    name
                    documentId
                  }
                }
              }";

      var bodyCorpuBuildingCategory = new
      {
        query = gqlQueryBuildingCategory,
      };

      string jsonBodyBuildingCategory = JsonConvert.SerializeObject(bodyCorpuBuildingCategory);

      restAPI.PostWithHeaderAndBody(
        _endpointTitle: "HitStrapi",
      _body: jsonBodyBuildingCategory,
      _success: (result) =>
      {
        var response = JsonConvert.DeserializeObject<GqlResponse<Categories>>(result.ToString());

        if (_buildingCategoryList.Count > 0) _buildingCategoryList.Clear();
        _buildingCategoryList = response.data.categories;

        categoryProcess = true;
      },
      _err: (errResult) =>
      {
        categoryProcess = true;
      });

      yield return new WaitUntil(() => categoryProcess && areaOptionProcess);

      while (!areaOptionProcess)
        yield return null;

      onResult?.Invoke(areaOptionResult);
    }

    // download request texture
    public IEnumerator RequestTelkomCorpuAreaOptionContent(
        Action<List<TelkomCorpuAreaCard>> onDone,
        Action<float> onProgress = null,
        bool forceRedownload = false
    )
    {
      if (_telkomCorpuAreaOptionList == null)
      {
        _telkomCorpuAreaOptionList = new List<TelkomCorpuAreaCard>();
        onProgress?.Invoke(1f);
        onDone?.Invoke(_telkomCorpuAreaOptionList);
        yield break;
      }

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();

      foreach (var card in _telkomCorpuAreaOptionList)
      {
        if (card == null) continue;

        bool needDownload = forceRedownload ? true : !card.IsImageAssetDownloaded();

        if (needDownload)
        {
          var pairs = card.DownloadAssetList(restAPI.targetAPIConfig.baseUrl);
          foreach (var kv in pairs)
          {
            downloadTargets[kv.Key] = kv.Value;
          }
        }
      }

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(_telkomCorpuAreaOptionList);
        yield break;
      }

      bool finished = false;

      restAPI.GetAssetTextures(
          downloadTargets,
          onProgress: p =>
          {
            onProgress?.Invoke(p);
          },
          onDone: fails =>
          {
            if (fails != null && fails.Count > 0)
            {
              foreach (var f in fails) Debug.LogWarning($"[DataManager.cs]: Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      onDone?.Invoke(_telkomCorpuAreaOptionList);
    }

    public IEnumerator GetTelkomCorpuDataMaster(Action<bool> onResult, string documentId)
    {
      bool isDone = false;
      bool success = false;

      string gqlQuery = @"
      query GetTelkomCorpuArea($documentId: ID!) {
        telkomCorpuArea(
          documentId: $documentId
          status: PUBLISHED
        ) {
          documentId
          name
          address
          open_for_visitor
          thumbnail_name
          thumbnail_image {
            url
          }

          # Drone Asset
          drone_views {
            documentId
            name
            background_360_image { 
              url 
            }
            first_camera_pov
            maps_image { 
              url 
            }
            description_image { 
              url 
            }
            navigations {
              target_type
              building_target {
                documentId
                name
              }
              facility_target {
                documentId
                name
              }
              hotspot_configuration {
                hotspot_title
                coordinate_x
                coordinate_y
                hotspot_image {
                  url
                }
              }
            }
          }

          # Building Asset
          buildings_childs {
            documentId
            name
            building_categories {
              documentId
              category_name
            }
            show_on_menu_panel
            background_360_image {
              url
            }
            first_camera_pov
            thumbnail_image {
              url
            }
            thumbnail_name
            maps_image {
              url
            }
            description_image {
              url
            }

            # Facility Asset
            facilities_childs {
              documentId
              name
              building_parent {
                documentId
                name
              }
              bookable_status
              show_on_menu_panel
              background_360_image {
                url
              }
              first_camera_pov
              thumbnail_image {
                url
              }
              thumbnail_name
              facility_detail_image {
                url
              }
              description_text
              gallery {
                content_images {
                  url
                }
                hotspot_configuration {
                  hotspot_title
                  coordinate_x
                  coordinate_y
                  hotspot_image {
                    url
                  }
                }
              }
              navigations {
                target_type
                building_target {
                  documentId
                  name
                }
                facility_target {
                  documentId
                  name
                }
                hotspot_configuration {
                  hotspot_title
                  coordinate_x
                  coordinate_y
                  hotspot_image {
                    url
                  }
                }
              }
            }

            # navigation in building
            navigations {
              target_type
              building_target {
                documentId
              }
              facility_target {
                documentId
              }
              hotspot_configuration {
                hotspot_title
                coordinate_x
                coordinate_y
                hotspot_image {
                  url
                }
              }
            }
          }
        }
      }";

      var body = new
      {
        query = gqlQuery,
        variables = new { documentId = documentId }
      };

      string jsonBody = JsonConvert.SerializeObject(body);

      restAPI.PostWithHeaderAndBody(
        _endpointTitle: "HitStrapi",
        _body: jsonBody,
        _success: (result) =>
        {
          var response = JsonConvert.DeserializeObject<GqlResponse<TelkomCorpuAreaDataMaster>>(result.ToString());

          TelkomCorpuArea card = response.data.telkomCorpuArea;
          _telkomCorpuAreaSelected.name = card.name;
          _telkomCorpuAreaSelected.documentId = card.documentId;
          _telkomCorpuAreaSelected.address = card.address;
          _telkomCorpuAreaSelected.open_for_visitor = card.open_for_visitor;
          _telkomCorpuAreaSelected.thumbnail_name = card.thumbnail_name;

          if (response.data == null || response.data.telkomCorpuArea == null && response.data.telkomCorpuArea.open_for_visitor == true)
          {
            _telkomCorpuAreaSelected.open_for_visitor = false;
          }

          BuildLocationList(response.data.telkomCorpuArea);

          success = true;
          isDone = true;
        },
        _err: (errResult) =>
        {
          success = false;
          isDone = true;
        }
      );

      while (!isDone)
        yield return null;

      onResult?.Invoke(success);
    }

    private void BuildLocationList(TelkomCorpuArea _selectionArea)
    {
      var result = new List<LocationDataModel>();

      // 1. Convert Drone Data
      if (_selectionArea.drone_views != null)
      {
        foreach (var d in _selectionArea.drone_views)
          result.Add(LocationDataModel.FromDrone(d));
      }

      // 2. Convert Building data
      if (_selectionArea.buildings_childs != null)
      {
        foreach (var b in _selectionArea.buildings_childs)
        {
          result.Add(LocationDataModel.FromBuilding(b));

          foreach (var f in b.facilities_childs)
            result.Add(LocationDataModel.FromFacility(f));
        }
      }

      // save result
      if (_locationDataList.Count > 0) _locationDataList.Clear();
      _locationDataList = result;

#if UNITY_EDITOR
      Debug.Log($"[DataManager.cs]: Total location Data list: {_locationDataList.Count}");
#endif
    }

    public IEnumerator RequestLocationDataContentByIndex(
      int locationIndex,
      Action onValidStart,
      Action<LocationDataModel> onDone,
      Action<float> onProgress = null,
      bool forceRedownload = false
    )
    {
      if (_locationDataList == null || locationIndex > _locationDataList.Count - 1)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(null);
        yield break;
      }

      onValidStart?.Invoke();

      var sourceLocation = _locationDataList[locationIndex];
#if UNITY_EDITOR
      Debug.Log($"[DataManager.cs]: Checking location {sourceLocation.name} asset...");
#endif
      LocationDataModel locationTarget = CloneLocationWithoutTextures(sourceLocation);

      /* var downloadTargets = new Dictionary<Action<Texture2D>, string>();
      bool needDownload = forceRedownload ? true : !locationTarget.IsInformationImageAssetDownloaded();

      if (needDownload)
      {
        var pairs = locationTarget.GetDownloadableInformationImageAssetList(restAPI.targetAPIConfig.baseUrl);
        foreach (var kv in pairs)
        {
          downloadTargets[kv.Key] = kv.Value;
        }
      }

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(locationTarget);
        yield break;
      } */

      var downloadTargets = locationTarget.GetDownloadableInformationImageAssetList(restAPI.targetAPIConfig.baseUrl);

      if (downloadTargets == null || downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(locationTarget);
        yield break;
      }

      bool finished = false;

      restAPI.GetAssetTextures(
          downloadTargets,
          onProgress: p =>
          {
            onProgress?.Invoke(p);
          },
          onDone: fails =>
          {
            if (fails != null && fails.Count > 0)
            {
              foreach (var f in fails) Debug.LogWarning($"[DataManager.cs]: Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      onDone?.Invoke(locationTarget);
    }

    public int GenerateLocationIndex(string documentId)
    {
      var l = _locationDataList.First((x) => x.documentId == documentId);
      return _locationDataList.IndexOf(l);
    }

    public IEnumerator RequestFacilityListByBuildingParent(
      string parentDocumentId,
      Action onValidStart,
      Action<List<LocationDataModel>> onDone,
      Action<float> onProgress = null,
      bool forceRedownload = false
    )
    {
      if (_locationDataList == null || string.IsNullOrEmpty(parentDocumentId))
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(null);
        yield break;
      }

      onValidStart?.Invoke();

      // List<LocationDataModel> locationTarget = new List<LocationDataModel>();
      var sourceLocations = _locationDataList
        .Where(loc =>
            loc.locationType == LocationType.FACILITY &&
            loc.building_parent != null &&
            loc.building_parent.documentId == parentDocumentId &&
            loc.show_on_menu_panel == true)
        .ToList();

      /* var downloadTargets = new Dictionary<Action<Texture2D>, string>();
      bool needDownload = forceRedownload ? true : locationTarget.Any((x) => x.IsThumbnailImageAssetDownloaded() == false);

      // Debug.Log($"[DataManager.cs]| need download thumbnail asset?? {needDownload}...");

      if (needDownload)
      {
        foreach (var loc in locationTarget)
        {
          var pairs = loc.GetDownloadableThumbnailImageAssetList(restAPI.targetAPIConfig.baseUrl);
          foreach (var kv in pairs)
          {
            downloadTargets[kv.Key] = kv.Value;
          }
        }

      }

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(locationTarget);
        yield break;
      } */

      // Clone supaya texture tidak nyangkut di master list
      List<LocationDataModel> locationTarget = CloneLocationListWithoutTextures(sourceLocations);

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();

      // Untuk panel navigasi facility kita cuma pakai thumbnail
      foreach (var loc in locationTarget)
      {
        var pairs = loc.GetDownloadableThumbnailImageAssetList(restAPI.targetAPIConfig.baseUrl);
        foreach (var kv in pairs)
        {
          downloadTargets[kv.Key] = kv.Value;
        }
      }

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        onDone?.Invoke(locationTarget);
        yield break;
      }

      bool finished = false;

      restAPI.GetAssetTextures(
          downloadTargets,
          onProgress: p =>
          {
            onProgress?.Invoke(p);
          },
          onDone: fails =>
          {
            if (fails != null && fails.Count > 0)
            {
              foreach (var f in fails) Debug.LogWarning($"[DataManager.cs]: Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      onDone?.Invoke(locationTarget);
    }

    public IEnumerator RequestBuildingListByCategory(
      string categoryDocumentId,
      Action onValidStart,
      Action<BuildingCategory, List<BuildingCategory>, List<LocationDataModel>> result,
      Action<float> onProgress = null,
      bool forceRedownload = false
    )
    {
      if (!_buildingCategoryList.Any((x) => x.documentId == categoryDocumentId) || _locationDataList == null || string.IsNullOrEmpty(categoryDocumentId))
      {
        // Debug.Log($"[DataManager.cs]: dokumen null {_buildingCategoryList.FirstOrDefault((x) => x.documentId == categoryDocumentId) == null} | location data list {_locationDataList == null} | target id {string.IsNullOrEmpty(categoryDocumentId)}");
        onProgress?.Invoke(1f);
        result?.Invoke(null, new List<BuildingCategory>(), new List<LocationDataModel>());
        yield break;
      }

      onValidStart?.Invoke();

      // Debug.Log($"[DataManager.cs]: try to search for location with category '{categoryDocumentId}'");

      // List<LocationDataModel> locationTarget = new List<LocationDataModel>();
      /* locationTarget = _locationDataList
        .Where(loc =>
            loc.locationType == LocationType.BUILDING &&
            loc.IsHasCategory(categoryDocumentId) == true &&
            loc.show_on_menu_panel == true)
        .ToList();

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();
      bool needDownload = forceRedownload ? true : locationTarget.Any((x) => x.IsThumbnailImageAssetDownloaded() == false);

      // Debug.Log($"[DataManager.cs]| need download thumbnail asset?? {needDownload}...");

      if (needDownload)
      {
        foreach (var loc in locationTarget)
        {
          var pairs = loc.GetDownloadableThumbnailImageAssetList(restAPI.targetAPIConfig.baseUrl);
          foreach (var kv in pairs)
          {
            downloadTargets[kv.Key] = kv.Value;
          }
        }

      }

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        result?.Invoke(_buildingCategoryList.FirstOrDefault((x) => x.documentId == categoryDocumentId), _buildingCategoryList.Where((c) => c.buildings.Count > 0).ToList(), locationTarget);
        yield break;
      } */

      var sourceLocations = _locationDataList
        .Where(loc =>
            loc.locationType == LocationType.BUILDING &&
            loc.IsHasCategory(categoryDocumentId) == true &&
            loc.show_on_menu_panel == true)
        .ToList();

      // Clone supaya texture tidak disimpan di _locationDataList
      List<LocationDataModel> locationTarget = CloneLocationListWithoutTextures(sourceLocations);

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();

      // Semua thumbnail building untuk kategori ini
      foreach (var loc in locationTarget)
      {
        var pairs = loc.GetDownloadableThumbnailImageAssetList(restAPI.targetAPIConfig.baseUrl);
        foreach (var kv in pairs)
        {
          downloadTargets[kv.Key] = kv.Value;
        }
      }

      var selectedCategory = _buildingCategoryList.FirstOrDefault((x) => x.documentId == categoryDocumentId);
      var nonEmptyCategories = _buildingCategoryList.Where((c) => c.buildings.Count > 0).ToList();

      if (downloadTargets.Count == 0)
      {
        onProgress?.Invoke(1f);
        result?.Invoke(selectedCategory, nonEmptyCategories, locationTarget);
        yield break;
      }

      bool finished = false;

      restAPI.GetAssetTextures(
          downloadTargets,
          onProgress: p =>
          {
            onProgress?.Invoke(p);
          },
          onDone: fails =>
          {
            if (fails != null && fails.Count > 0)
            {
              foreach (var f in fails) Debug.LogWarning($"[DataManager.cs]: Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      result?.Invoke(_buildingCategoryList.FirstOrDefault((x) => x.documentId == categoryDocumentId),
                                            _buildingCategoryList.Where((c) => c.buildings.Count > 0).ToList(),
                                            locationTarget);
    }

    public BuildingCategory GetFirstBuildingCategoryData()
    {
      BuildingCategory result = _buildingCategoryList[0];
      if (result == null) return null;
      return _buildingCategoryList[0];
    }

    #region ===== Location Clone Helper =====

    private LocationDataModel CloneLocationWithoutTextures(LocationDataModel source)
    {
      if (source == null) return null;

      var clone = new LocationDataModel
      {
        name = source.name,
        documentId = source.documentId,
        locationType = source.locationType,
        building_parent = source.building_parent,
        bookable_status = source.bookable_status,
        show_on_menu_panel = source.show_on_menu_panel,
        first_camera_pov = source.first_camera_pov,
        thumbnail_name = source.thumbnail_name,
        description_text = source.description_text,
        building_categories = source.building_categories != null
              ? new List<BuildingCategory>(source.building_categories)
              : new List<BuildingCategory>()
      };

      // ===== ImageField: buat instance baru, copy hanya URL =====
      clone.background_360_image = source.background_360_image != null
          ? new ImageField { url = source.background_360_image.url }
          : null;

      clone.thumbnail_image = source.thumbnail_image != null
          ? new ImageField { url = source.thumbnail_image.url }
          : null;

      clone.facility_detail_image = source.facility_detail_image != null
          ? new ImageField { url = source.facility_detail_image.url }
          : null;

      clone.maps_image = source.maps_image != null
          ? new ImageField { url = source.maps_image.url }
          : null;

      clone.description_image = source.description_image != null
          ? new ImageField { url = source.description_image.url }
          : null;

      // ===== Gallery =====
      if (source.gallery != null)
      {
        clone.gallery = new List<FacilityGallery>(source.gallery.Count);
        foreach (var g in source.gallery)
        {
          if (g == null)
          {
            clone.gallery.Add(null);
            continue;
          }

          var newGallery = new FacilityGallery
          {
            content_images = g.content_images != null
                  ? g.content_images.Select(img =>
                      img != null ? new ImageField { url = img.url } : null
                  ).ToList()
                  : new List<ImageField>(),
            hotspot_configuration = g.hotspot_configuration != null
                  ? new HotspotConfiguration
                  {
                    hotspot_title = g.hotspot_configuration.hotspot_title,
                    coordinate_x = g.hotspot_configuration.coordinate_x,
                    coordinate_y = g.hotspot_configuration.coordinate_y,
                    hotspot_image = g.hotspot_configuration.hotspot_image != null
                          ? new ImageField
                          {
                            url = g.hotspot_configuration.hotspot_image.url
                          }
                          : null
                  }
                  : null
          };

          clone.gallery.Add(newGallery);
        }
      }
      else
      {
        clone.gallery = new List<FacilityGallery>();
      }

      // ===== Navigations =====
      if (source.navigations != null)
      {
        clone.navigations = new List<NavigationSetting>(source.navigations.Count);
        foreach (var nav in source.navigations)
        {
          if (nav == null)
          {
            clone.navigations.Add(null);
            continue;
          }

          var newNav = new NavigationSetting
          {
            target_type = nav.target_type,
            building_target = nav.building_target != null
                  ? new BuildingTarget
                  {
                    documentId = nav.building_target.documentId,
                    name = nav.building_target.name
                  }
                  : null,
            facility_target = nav.facility_target != null
                  ? new FacilityTarget
                  {
                    documentId = nav.facility_target.documentId,
                    name = nav.facility_target.name
                  }
                  : null,
            hotspot_configuration = nav.hotspot_configuration != null
                  ? new HotspotConfiguration
                  {
                    hotspot_title = nav.hotspot_configuration.hotspot_title,
                    coordinate_x = nav.hotspot_configuration.coordinate_x,
                    coordinate_y = nav.hotspot_configuration.coordinate_y,
                    hotspot_image = nav.hotspot_configuration.hotspot_image != null
                          ? new ImageField
                          {
                            url = nav.hotspot_configuration.hotspot_image.url
                          }
                          : null
                  }
                  : null
          };

          clone.navigations.Add(newNav);
        }
      }
      else
      {
        clone.navigations = new List<NavigationSetting>();
      }

      return clone;
    }

    private List<LocationDataModel> CloneLocationListWithoutTextures(IEnumerable<LocationDataModel> sources)
    {
      if (sources == null) return new List<LocationDataModel>();
      return sources.Select(CloneLocationWithoutTextures).ToList();
    }

    #endregion

  }
}