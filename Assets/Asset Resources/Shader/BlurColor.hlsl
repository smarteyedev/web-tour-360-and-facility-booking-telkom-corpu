void BlurColor_float(float4 Seed, float Min, float Max, float BlurX, float BlurY, out float4 Out)
{
    float randomno = frac(sin(dot(Seed.xy, float2(12.9898, 78233))) * 43758.54553);
    
    float noise = lerp(Min, Max, randomno);
    
    float uvx = float(sin(noise)) * BlurX;
    float uvy = float(sin(noise)) * BlurY;
    
    float4 uvpos = float4(Seed.x + uvx, Seed.y + uvy, Seed.zw);
    
    Out = uvpos;

}