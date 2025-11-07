using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Smarteye.RestAPI;
using System;

namespace WebTourCorpu.DataManager
{
  public class DataManager : RestAPIHandler
  {
    [Header("Data Manager | Data Asset")]
    [SerializeField] private List<TelkomCorpuAreaCard> _telkomCorpuAreaOptionList = new();
    [SerializeField] private TelkomCorpuAreaCard _telkomCorpuAreaSelected = new();
    [SerializeField] private List<LocationDataModel> _locationDataList = new();

    public IEnumerator RequestCorpuAreaOptionsData(
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
              foreach (var f in fails) Debug.LogWarning($"Download fail: {f}");
            }
            finished = true;
          }
      );

      yield return new WaitUntil(() => finished);

      onProgress?.Invoke(1f);
      onDone?.Invoke(_telkomCorpuAreaOptionList);
    }

    public IEnumerator GetTelkomCorpuAreaOption(Action<bool> _onResult, string _documentId)
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

      _onResult?.Invoke(success);
    }

    public IEnumerator GetTelkomCorpuDataMaster(Action<bool> _onResult, string _documentId)
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

    # LANGSUNG LIST, BUKAN *_connection
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

    # JUGA LANGSUNG LIST
    buildings_childs {
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

      facilities_childs {
        documentId
        name
        category_functionality {
          functionality
          description
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
        variables = new { documentId = _documentId }
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

      _onResult?.Invoke(success);
    }

    private void BuildLocationList(TelkomCorpuArea selectionArea)
    {
      var result = new List<LocationDataModel>();

      // 1. Convert Drone Data
      if (selectionArea.drone_views != null)
      {
        foreach (var d in selectionArea.drone_views)
          result.Add(LocationDataModel.FromDrone(d));
      }

      // 2. Convert Building data
      if (selectionArea.buildings_childs != null)
      {
        foreach (var b in selectionArea.buildings_childs)
        {
          result.Add(LocationDataModel.FromBuilding(b));

          foreach (var f in b.facilities_childs)
            result.Add(LocationDataModel.FromFacility(f));
        }
      }

      // save result
      _locationDataList = result;

      Debug.Log($"Total location Data list: {_locationDataList.Count}");
    }
  }
}