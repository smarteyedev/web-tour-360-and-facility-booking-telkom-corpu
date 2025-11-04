using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
  [Header("Strapi Settings")]
  [SerializeField] private string baseUrl = "http://localhost:1337";
  [SerializeField] private string jwtToken = "<PASTE_JWT_TOKEN_KAMU>";

  [Header("Fetched Data (auto-filled)")]
  [SerializeField] private List<DroneNode> droneNodes = new();
  [SerializeField] private List<BuildingNode> buildingNodes = new();
  [SerializeField] private List<FacilityChild> facilityChildren = new();

  private void Start()
  {
    StartCoroutine(FetchTelkomCorpuArea($"t3q960e8tpza3nu16hjmrdj5"));
  }

  public IEnumerator FetchTelkomCorpuArea(string documentId)
  {
    string endpoint = $"{baseUrl}/graphql";

    string gqlQuery = @"
        query GetTelkomCorpuArea($documentId: ID!) {
          telkomCorpuArea(documentId: $documentId, status: PUBLISHED) {
            drone_views_connection {
              nodes {
                documentId
                name
                background_360_image { url }
                first_camera_pov
                maps_image { url }
                description_image { url }
              }
            }
            buildings_childs_connection {
              nodes {
                documentId
                name
                facilities_childs {
                  documentId
                  name
                  thumbnail_name
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

    var response = JsonConvert.DeserializeObject<GqlResponse<GetTelkomCorpuAreaData>>(responseText);

    // Pastikan ada data
    if (response.data == null || response.data.telkomCorpuArea == null)
    {
      Debug.LogWarning("⚠️ Data kosong dari Strapi");
      yield break;
    }

    // Pisahkan ke masing-masing list
    droneNodes = response.data.telkomCorpuArea.drone_views_connection.nodes;
    buildingNodes = response.data.telkomCorpuArea.buildings_childs_connection.nodes;
    facilityChildren = new List<FacilityChild>();

    // Ambil semua fasilitas dari tiap building
    foreach (var building in buildingNodes)
    {
      if (building.facilities_childs != null)
        facilityChildren.AddRange(building.facilities_childs);
    }

    Debug.Log($"📡 DroneNodes: {droneNodes.Count} | Buildings: {buildingNodes.Count} | Facilities: {facilityChildren.Count}");
  }

  // Public getter (opsional)
  public List<DroneNode> GetDroneNodes() => droneNodes;
  public List<BuildingNode> GetBuildingNodes() => buildingNodes;
  public List<FacilityChild> GetFacilityChildren() => facilityChildren;
}
