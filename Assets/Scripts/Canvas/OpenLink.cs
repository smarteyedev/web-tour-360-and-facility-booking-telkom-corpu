using UnityEngine;
using System.Runtime.InteropServices;

public class OpenLink : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void OpenUrlSameTab(string url);

    public void OpenURL(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenUrlSameTab(url);
#else
        Application.OpenURL(url);
#endif

        Debug.Log("Membuka URL: " + url);
    }
}
