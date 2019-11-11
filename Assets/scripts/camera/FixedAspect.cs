using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class FixedAspect : MonoBehaviour
{
    Camera cam;
    const float targetAspect = 2F;
    float width;
    float height;

    void Awake()
    {
        cam = GetComponent<Camera>();
        SetAspect();
        width = Screen.width;
        height = Screen.width;
    }

    void Update()
    {
        if (width != Screen.width || height != Screen.height)
        {
            SetAspect();
            width = Screen.width;
            height = Screen.height;
        }
    }

    void SetAspect()
    {
        float scalar = ((float)Screen.width / (float)Screen.height) / targetAspect;

        if (scalar < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scalar;
            rect.x = 0;
            rect.y = (1.0f - scalar) / 2.0f;

            cam.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scalar;

            Rect rect = cam.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}
