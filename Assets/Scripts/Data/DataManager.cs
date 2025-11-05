using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Smarteye.RestAPI;
using System;
using UnityEngine.UI;

namespace WebTourCorpu.DataManager
{
  public class DataManager : RestAPIHandler
  {
    [Header("Data Manager | Data Asset")]
    //! [SerializeField] private TelkomCorpuArea _telkomCorpuArea = new();
    [SerializeField] private List<TelkomCorpuAreaCard> TelkomCorpuAreaOptionList = new();
    [SerializeField] private TelkomCorpuAreaCard TelkomCorpuAreaSelected = new();
    [SerializeField] private List<LocationDataModel> _locationDataList = new();

    [Header("Data Manager | Component References")]
    [SerializeField] private LoadingScreenHandler _loadingScreen;
    [SerializeField] private Image targetSprite;

    private void Start()
    {
      // GetTelkomCorpuDataMaster($"t3q960e8tpza3nu16hjmrdj5");
      GetTelkomCorpuAreaOptionList();
    }

    public void GetTelkomCorpuAreaOptionList()
    {
      StartCoroutine(_loadingScreen.LoadingScreenForApiProcess(
        _loadingProcess: TelkomCorpuAreaOption,
        _documentId: "",
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

    public void GetTelkomCorpuDataMaster(string documentId)
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

    public void GetLocationAsset()
    {
      TelkomCorpuAreaCard card = TelkomCorpuAreaOptionList[0];

      restAPI.GetAssetTexture(
        $"http://localhost:1337{card.thumbnail_image.url}",
        (tex) =>
        {
          card.thumbnail_image.textureImage = tex;
          targetSprite.sprite = card.thumbnail_image.GetSpriteImage();
        },
        (errMessage) =>
        {
          Debug.Log($"{errMessage}");
        }
      );
    }

    public IEnumerator TelkomCorpuAreaOption(Action<bool> _onResult, string _documentId)
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

          TelkomCorpuAreaOptionList = response.data.telkomCorpuAreas;

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

          //! _telkomCorpuArea = response.data.telkomCorpuArea;

          TelkomCorpuArea card = response.data.telkomCorpuArea;
          TelkomCorpuAreaSelected.name = card.name;
          TelkomCorpuAreaSelected.documentId = card.documentId;
          TelkomCorpuAreaSelected.address = card.address;
          TelkomCorpuAreaSelected.open_for_visitor = card.open_for_visitor;
          TelkomCorpuAreaSelected.thumbnail_name = card.thumbnail_name;

          if (response.data == null || response.data.telkomCorpuArea == null && response.data.telkomCorpuArea.open_for_visitor == true)
          {
            TelkomCorpuAreaSelected.open_for_visitor = false;
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