using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTwos : MonoBehaviour
{
    int frames = 0;
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
            if (frames == 0) {
                rotation = reference.rotation;
                position = reference.position;
            }

            frames = (frames + 1) % 2;
            yield return new WaitForSecondsRealtime(0.04167f);
        }
    }
}
