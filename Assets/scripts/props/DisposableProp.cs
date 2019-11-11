using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DisposableProp : MonoBehaviour {
    public AnimationCurve transparency;
    public float deathTime = 1.5f;
    public float force;
    public LayerMask carMask;

    Rigidbody rBody;
    Collider coll;
    public Renderer mat;
    bool die = false;
    float time;

    void Awake() {
        coll = GetComponent<Collider>();
        rBody = GetComponent<Rigidbody>();
    }

    void Update() {
        if (die) {
            mat.material.SetFloat("_Transparency", transparency.Evaluate(time / deathTime));
            if (time >= deathTime) Destroy(gameObject);
            else time += Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (carMask == (carMask | (1 << collision.transform.gameObject.layer))) {
            Physics.IgnoreCollision(coll, collision.collider);
            Vector3 dir = rBody.worldCenterOfMass - collision.contacts[0].point;
            dir.y = Mathf.Abs(dir.y) * 2;
            dir.Normalize();

            rBody.AddForceAtPosition(dir * force, collision.contacts[0].point, ForceMode.VelocityChange);
            die = true;
        }
    }

}
