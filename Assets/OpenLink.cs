using UnityEngine;

public class OpenLink : MonoBehaviour
{
    // Fungsi ini akan dipanggil ketika tombol di-klik
    public void OpenURL(string url)
    {
        // Application.OpenURL akan membuka URL yang diberikan 
        // menggunakan browser default di perangkat pengguna.
        Application.OpenURL(url);
        Debug.Log("Membuka URL: " + url);
    }
}