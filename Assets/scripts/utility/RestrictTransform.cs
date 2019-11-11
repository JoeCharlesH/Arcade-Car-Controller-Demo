using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictTransform : MonoBehaviour {
    [SerializeField]
    Transform reference;
    [SerializeField]
    float positionStep = 0.1f;
    [SerializeField]
    float rotationStep = 22.5f;
    
    float AdjustFloat(float val, float step) {
        return Mathf.Floor(val / step) * step;
    }

    void LateUpdate () {
        transform.position = new Vector3(
            AdjustFloat(reference.position.x, positionStep),
            AdjustFloat(reference.position.y, positionStep),
            AdjustFloat(reference.position.z, positionStep)
        );
        transform.eulerAngles = new Vector3(
            AdjustFloat(reference.eulerAngles.x, rotationStep),
            AdjustFloat(reference.eulerAngles.y, rotationStep),
            AdjustFloat(reference.eulerAngles.z, rotationStep)
        );
    }
}
