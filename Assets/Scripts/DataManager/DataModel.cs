using System.Collections.Generic;

#region ===== JSON MODEL =====
[System.Serializable]
public class GqlResponse<T>
{
    public T data;
}

[System.Serializable]
public class GetTelkomCorpuAreaData
{
    public TelkomCorpuArea telkomCorpuArea;
}

[System.Serializable]
public class TelkomCorpuArea
{
    public DroneViewConnection drone_views_connection;
    public BuildingConnection buildings_childs_connection;
}

[System.Serializable]
public class DroneViewConnection
{
    public List<DroneNode> nodes;
}

[System.Serializable]
public class DroneNode
{
    public string name;
    public string documentId;
    public ImageField background_360_image;
    public ImageField maps_image;
    public ImageField description_image;
    public float first_camera_pov;
}

[System.Serializable]
public class BuildingConnection
{
    public List<BuildingNode> nodes;
}

[System.Serializable]
public class BuildingNode
{
    public string name;
    public string documentId;
    public List<FacilityChild> facilities_childs;
}

[System.Serializable]
public class FacilityChild
{
    public string name;
    public string documentId;
    public string thumbnail_name;
}

[System.Serializable]
public class ImageField
{
    public string url;
}
#endregion