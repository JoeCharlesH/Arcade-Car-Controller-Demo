using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ToonRamp : MonoBehaviour {
    [SerializeField]
    Texture2D ramp;

    private void Awake() {
        Shader.SetGlobalTexture("_ToonRamp", ramp);
    }

    void OnValidate() {
        Shader.SetGlobalTexture("_ToonRamp", ramp);
    }
}
