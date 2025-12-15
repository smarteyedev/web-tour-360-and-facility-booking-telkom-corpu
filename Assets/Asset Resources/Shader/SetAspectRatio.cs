using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAspectRatio : MonoBehaviour
{
    private static readonly int AspectRatioID = Shader.PropertyToID("_Aspect_Ratio");

    void Start()
    {
        // Mendapatkan referensi RectTransform dan Material
        RectTransform rect = GetComponent<RectTransform>();
        Material material = GetComponent<UnityEngine.UI.Image>().material;

        // Hitung Rasio Aspek: Lebar dibagi Tinggi
        float aspectRatio = rect.rect.width / rect.rect.height;

        // Kirim nilai Aspect Ratio ke Shader
        material.SetFloat(AspectRatioID, aspectRatio);
    }
}
