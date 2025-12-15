using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class UIShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        // Force shader to behave like UI shader
        foreach (var obj in materialEditor.targets)
        {
            Material mat = (Material)obj;
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 100;
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_ZWrite", 0);
        }
    }
}
