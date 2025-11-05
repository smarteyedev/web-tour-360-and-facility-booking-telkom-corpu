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
    //! [SerializeField] private TelkomCorpuArea _telkomCorpuArea = new();
    [SerializeField] private TelkomCorpuAreaCard TelkomCorpuAreaSelected = new();
    [SerializeField] private List<LocationDataModel> _locationDataList = new();

    [Header("Data Manager | Component References")]
    [SerializeField] private LoadingScreenHandler _loadingScreen;

    private void Start()
    {
      GetCorpuArea($"f3jcumigzcg986pz2s39di0y");
    }

    public void GetCorpuArea(string documentId)
    {
      StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
        _loadingProcess: GetTelkomCorpuDataMaster,
        _documentId: documentId,
        _onComplete: () =>
        {
          // loading process complete
        },
        _onError: () =>
        {
          // loading process error when web request fail
        }
      ));
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

      var payload = new
      {
        query = gqlQuery,
        variables = new { documentId = _documentId }
      };

      string jsonPayload = JsonConvert.SerializeObject(payload);

      restAPI.PostWithHeaderAndBody(
        _endpointTitle: "HitStrapi",
        _body: jsonPayload,
        _success: (result) =>
        {
          var response = JsonConvert.DeserializeObject<GqlResponse<TelkomCorpuAreaDataMaster>>(result.ToString());

          //! Pastikan ada data
          /* if (response.data == null || response.data.telkomCorpuArea == null)
          {
            Debug.LogWarning("⚠️ Data kosong dari Strapi");
            yield break;
          } */

          //! _telkomCorpuArea = response.data.telkomCorpuArea;

          TelkomCorpuArea card = response.data.telkomCorpuArea;
          TelkomCorpuAreaSelected.name = card.name;
          TelkomCorpuAreaSelected.documentId = card.documentId;
          TelkomCorpuAreaSelected.address = card.address;
          TelkomCorpuAreaSelected.open_for_visitor = card.open_for_visitor;
          TelkomCorpuAreaSelected.thumbnail_name = card.thumbnail_name;

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