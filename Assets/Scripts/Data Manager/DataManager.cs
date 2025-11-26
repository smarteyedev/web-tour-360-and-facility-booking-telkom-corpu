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
    [SerializeField] private TelkomCorpuAreaCard _telkomCorpuAreaSelected = new();
    [SerializeField] private List<LocationDataModel> _locationDataList = new();

    public IEnumerator GetTelkomCorpuAreaOptionData(Action<bool> onResult, string documentId)
    {
      bool isDone = false;
      bool success = false;

      string gqlQuery = @"
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

      var body = new
      {
        query = gqlQuery,
      };

      string jsonBody = JsonConvert.SerializeObject(body);

      restAPI.PostWithHeaderAndBody(
        _endpointTitle: "HitStrapi",
        _body: jsonBody,
        _success: (result) =>
        {
          var response = JsonConvert.DeserializeObject<GqlResponse<TelkomCorpuAreas>>(result.ToString());

          if (_telkomCorpuAreaOptionList.Count > 0) _telkomCorpuAreaOptionList.Clear();
          _telkomCorpuAreaOptionList = response.data.telkomCorpuAreas;

          success = true;
          isDone = true;
        },
        _err: (errResult) =>
        {

          success = false;
          isDone = true;
        });

      while (!isDone)
        yield return null;

      onResult?.Invoke(success);
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
              foreach (var f in fails) Debug.LogWarning($"DataManager: Download fail: {f}");
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
            name
          }

          # Drone Asset
          drone_views {
            documentId
            name
            background_360_image { 
              url 
              name
            }
            first_camera_pov
            maps_image { 
              url 
              name
            }
            description_image { 
              url 
              name
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
                  name
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
              name
            }
            first_camera_pov
            thumbnail_image {
              url
              name
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
                name
              }
              first_camera_pov
              thumbnail_image {
                url
                name
              }
              thumbnail_name
              facility_detail_image {
                url
                name
              }
              description_text
              gallery {
                content_images {
                  url
                  name
                }
                hotspot_configuration {
                  hotspot_title
                  coordinate_x
                  coordinate_y
                  hotspot_image {
                    url
                    name
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
                    name
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
                  name
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
      Debug.Log($"DataManager: Total location Data list: {_locationDataList.Count}");
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

      LocationDataModel locationTarget = _locationDataList[locationIndex];
#if UNITY_EDITOR
      Debug.Log($"[{name}]: Checking location {locationTarget.name} asset...");
#endif

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();
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
              foreach (var f in fails) Debug.LogWarning($"DataManager: Download fail: {f}");
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

      List<LocationDataModel> locationTarget = new List<LocationDataModel>();
      locationTarget = _locationDataList
        .Where(loc =>
            loc.locationType == LocationType.FACILITY &&
            loc.building_parent != null &&
            loc.building_parent.documentId == parentDocumentId &&
            loc.show_on_menu_panel == true)
        .ToList();

      var downloadTargets = new Dictionary<Action<Texture2D>, string>();
      bool needDownload = forceRedownload ? true : locationTarget.Any((x) => x.IsThumbnailImageAssetDownloaded() == false);

      Debug.Log($"[DataManager.cs]| need download thumbnail asset?? {needDownload}...");

      if (needDownload)
      {
        foreach (var loc in locationTarget)
        {
          var pairs = loc.GetDownloadableThumbnailImageAssetList(restAPI.targetAPIConfig.baseUrl);
          foreach (var kv in pairs)
          {
            // downloadTargets[kv.Key] = kv.Value;
            Debug.Log($"[DataManager.cs]: item will be downloaded {kv.Value}...");
          }
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
              foreach (var f in fails) Debug.LogWarning($"DataManager: Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      onDone?.Invoke(locationTarget);
    }
  }
}