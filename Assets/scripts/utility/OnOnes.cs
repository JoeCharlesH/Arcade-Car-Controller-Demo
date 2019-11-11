using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOnes : MonoBehaviour
{
    public Transform reference;
    Quaternion rotation;
    Vector3 position;

    void Start() {
        StartCoroutine(Animate());    
    }

    void LateUpdate() {
        transform.position = position;
        transform.rotation = rotation;
    }

    IEnumerator Animate() {
        while (true) {
            rotation = reference.rotation;
            position = reference.position;
            yield return new WaitForSecondsRealtime(0.04167f);
        }
    }
}
