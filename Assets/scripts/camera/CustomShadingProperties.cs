using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CustomShadingProperties : MonoBehaviour {
    [SerializeField]
    int lightSteps = 4;
    [SerializeField]
    float stepDelta = 0.03f;
    [SerializeField]
    [Range(0,1)]
    float lowerBound = 0.8f;

    public bool adjustDarkness = true;

    [SerializeField]
    [Range(0, 1)]
    float satDarknessFactor = 0.1f;
    [SerializeField]
    [Range(0, 1)]
    float hueDarknessFactor = 0.1f;


    Camera cam;

    void OnValidate() {
        Shader.SetGlobalInt("lSteps", lightSteps);
        Shader.SetGlobalFloat("stepDelta", stepDelta);
        Shader.SetGlobalFloat("lowerBound", lowerBound);
        if(adjustDarkness) {
            Shader.SetGlobalFloat("darkSaturationFactor", satDarknessFactor);
            Shader.SetGlobalFloat("darkHueFactor", hueDarknessFactor);
        }
        else {
            Shader.SetGlobalFloat("darkSaturationFactor", 0);
            Shader.SetGlobalFloat("darkHueFactor", 0);
        }
    }

    void Awake() {
        cam = GetComponent<Camera>();
    }

    void OnPreRender() {
        Shader.SetGlobalVector("pixelDim", new Vector2(cam.pixelWidth, cam.pixelHeight));
    }
}
