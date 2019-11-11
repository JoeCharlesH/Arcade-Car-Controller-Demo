using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class RetroEffect : MonoBehaviour {
    public int pixelWidth = 720;
    int pixelHeight;

    public bool useDither;

    [System.Serializable]
    public class DitherProperties {
        public List<Color> palette = new List<Color>();
        [Range(1, 20)]
        public int lightStep = 4;
        [Range(0, 1)]
        public float lightStepDelta = 0.125f;
    }
    public DitherProperties ditherSettings;
    Material ditherMat;

    void UpdateDitherMaterial() {
        if (!ditherMat) ditherMat = new Material(Shader.Find("Custom/Dither"));
        ditherMat.SetInt("lStep", ditherSettings.lightStep);
        ditherMat.SetFloat("lStepDelta", ditherSettings.lightStepDelta);

        List<Vector4> hslColors = new List<Vector4>();
        float h, s, v, l;
        foreach (Color color in ditherSettings.palette) {
            Color.RGBToHSV(color, out h, out s, out v);
            l = (2 - s) * v / 2;
            s = (l != 0 && l < 1) ? (s * v / (l < 0.5f ? (l * 2) : (2 - l * 2))) : s;
            hslColors.Add(new Vector4(h, s, l, 1));
        }

        ditherMat.SetVectorArray("palette", hslColors);
        ditherMat.SetInt("paletteSize", hslColors.Count);
    }

    void OnValidate() {
        if(ditherSettings.palette.Count == 0) {
            Debug.LogWarning("Palette must contain at least one color, adding white to palette");
            ditherSettings.palette.Add(Color.white);
        }
        if (ditherSettings.palette.Count > 16) {
            Debug.LogWarning("Palette can't be larger than material's palette array, resizing array...");
            ditherSettings.palette.RemoveRange(16, ditherSettings.palette.Count - 16);
        }
        UpdateDitherMaterial();
    }

    void Start() {
        if (!SystemInfo.supportsImageEffects)
            enabled = false;
	}

	void Update () {
        pixelHeight = (int)(pixelWidth * (Screen.height / (float)Screen.width));
	}

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        src.filterMode = FilterMode.Point;
        RenderTexture buffer = RenderTexture.GetTemporary(pixelWidth, pixelHeight, -1);
        buffer.filterMode = FilterMode.Point;
        Graphics.Blit(src, buffer);
        if (useDither) {
            if (!ditherMat) OnValidate();
            ditherMat.mainTexture = buffer;
            Graphics.Blit(buffer, dst, ditherMat);
        }
        else Graphics.Blit(buffer, dst);
        RenderTexture.ReleaseTemporary(buffer);
    }
}
