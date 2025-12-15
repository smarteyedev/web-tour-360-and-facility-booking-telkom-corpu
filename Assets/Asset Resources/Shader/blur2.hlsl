// Asumsi fungsi ini berada di Custom Function Node atau di Code Block
float4 BlurColor(float4 UV, float BlurRadius, sampler2D Tex, float2 TexSize)
{
    float4 finalColor = 0;
    float totalWeight = 0;
    
    // Ukuran offset per piksel
    float2 pixelOffset = 1.0 / TexSize;
    
    // Jarak sampling (misalnya 5x5)
    for (int x = -2; x <= 2; x++)
    {
        for (int y = -2; y <= 2; y++)
        {
            // Menghitung bobot (weight). Untuk Box Blur, bobotnya sama (1).
            float weight = 1.0;
            
            // Menghitung posisi sampel
            float2 sampleUV = UV.xy + float2(x, y) * pixelOffset * BlurRadius;
            
            // Mengambil sampel warna
            float4 sampleColor = tex2D(Tex, sampleUV);
            
            finalColor += sampleColor * weight;
            totalWeight += weight;
        }
    }
    
    return finalColor / totalWeight; // Mengembalikan nilai rata-rata
}