using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Smarteye.RestAPI;
using Newtonsoft.Json.Linq;

public class DataManager : RestAPIHandler
{
  [Header("Strapi Settings")]
  [SerializeField] private string baseUrl = "http://localhost:1337";
  [SerializeField] private string jwtToken = "<PASTE_JWT_TOKEN_KAMU>";

  [Header("Data Master")]
  [SerializeField] private TelkomCorpuArea telkomCorpuArea = new();
  [SerializeField] private List<Drone> droneList = new();
  [SerializeField] private List<Building> buildingList = new();
  [SerializeField] private List<Facility> facilityList = new();

  private void Start()
  {
    StartCoroutine(FetchTelkomCorpuArea($"t3q960e8tpza3nu16hjmrdj5"));
  }

  public IEnumerator FetchTelkomCorpuArea(string documentId)
  {
    string endpoint = $"{baseUrl}/graphql";

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
      variables = new { documentId = documentId }
    };

    string jsonPayload = JsonConvert.SerializeObject(payload);
    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

    UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + jwtToken);

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      Debug.LogError($"❌ GraphQL Error: {request.error}\n{request.downloadHandler.text}");
      yield break;
    }

    string responseText = request.downloadHandler.text;
    Debug.Log($"✅ Response:\n{responseText}");

    var response = JsonConvert.DeserializeObject<GqlResponse<TelkomCorpuAreaDataMaster>>(responseText);

    // Pastikan ada data
    if (response.data == null || response.data.telkomCorpuArea == null)
    {
      Debug.LogWarning("⚠️ Data kosong dari Strapi");
      yield break;
    }

    // Pisahkan ke masing-masing list
    telkomCorpuArea = response.data.telkomCorpuArea;
    droneList = response.data.telkomCorpuArea.drone_views_connection.nodes;
    buildingList = response.data.telkomCorpuArea.buildings_childs_connection.nodes;
    facilityList = new List<Facility>();

    // Ambil semua fasilitas dari tiap building
    foreach (var building in buildingList)
    {
      if (building.facilities_childs != null)
        facilityList.AddRange(building.facilities_childs);
    }
  }

  public void GetCorpuArea(string documentId)
  {
    
  }

  public override void OnSuccessResult(JObject result)
  {

  }

  public override void OnProtocolErr(JObject result)
  {

  }
}
