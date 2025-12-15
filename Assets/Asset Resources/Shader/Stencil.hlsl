void ApplyStencil_float(float4 color, float stencil, float stencilComp, float stencilRef, out float4 Out)
{
    bool shouldRender = false;

    if (stencilComp == 3) // Equal
    {
        shouldRender = (stencil == stencilRef);
    }
    else if (stencilComp == 6) // Always
    {
        shouldRender = true;
    }

    if (!shouldRender)
    {
        clip(-1);
    }

    Out = color;
}