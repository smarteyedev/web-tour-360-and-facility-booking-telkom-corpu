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
    [Header("Strapi Settings")]

    [Header("Data Master")]
    [SerializeField] private TelkomCorpuArea _telkomCorpuArea = new();
    [SerializeField] private List<Drone> _droneList = new();
    [SerializeField] private List<Building> _buildingList = new();
    [SerializeField] private List<Facility> _facilityList = new();

    [Header("Component References")]
    [SerializeField] private LoadingScreenHandler _loadingScreen;

    private void Start()
    {
      GetCorpuArea($"t3q960e8tpza3nu16hjmrdj5");
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
    documentId: $documentId, 
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
    drone_views_connection {
      nodes {
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
    }
    buildings_childs_connection {
      nodes {
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
            hotspot_image{
              url
            }
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
        _endpointTitle: "post",
        _isUsingToken: true,
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

          // Separate data
          _telkomCorpuArea = response.data.telkomCorpuArea;
          _droneList = response.data.telkomCorpuArea.drone_views_connection.nodes;
          _buildingList = response.data.telkomCorpuArea.buildings_childs_connection.nodes;
          _facilityList = new List<Facility>();

          // initial data
          foreach (var building in _buildingList)
          {
            if (building.facilities_childs != null)
              _facilityList.AddRange(building.facilities_childs);
          }

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
  }
}