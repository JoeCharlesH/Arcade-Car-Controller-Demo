using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Dither : MonoBehaviour {
    Material mat;

    public List<Color> palette = new List<Color>();
    [Range(1, 20)]
    public int lightStep = 4;
    [Range(0, 1)]
    public float lightStepDelta = 0.125f;

    void UpdateMaterial() {
        if (!mat) mat = new Material(Shader.Find("Custom/Dither"));
        mat.SetInt("lStep", lightStep);
        mat.SetFloat("lStepDelta", lightStepDelta);

        List<Vector4> hslColors = new List<Vector4>();
        float h, s, v, l;
        foreach(Color color in palette) {
            Color.RGBToHSV(color, out h, out s, out v);
            l = (2 - s) * v / 2;
            s = (l != 0 && l < 1) ? (s * v / (l < 0.5f ? (l * 2) : (2 - l * 2))) : s;
            hslColors.Add(new Vector4(h, s, l, 1));
        }

        mat.SetVectorArray("palette", hslColors);
        mat.SetInt("paletteSize", hslColors.Count);
    }

    void OnValidate() {
        if(palette.Count == 0) {
            Debug.LogWarning("Palette must contain at least one color, adding white to palette");
            palette.Add(Color.white);
        }
        if (palette.Count > 16) {
            Debug.LogWarning("Palette can't be larger than material's palette array, resizing array...");
            palette.RemoveRange(16, palette.Count - 16);
        }
        UpdateMaterial();
    }

    void Start() {
        if (!SystemInfo.supportsImageEffects)
            enabled = false;
	}

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if (!mat) OnValidate();
        mat.mainTexture = src;
        Graphics.Blit(src, dst, mat);
    }
}
