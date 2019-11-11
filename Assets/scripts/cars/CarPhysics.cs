using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarPhysics : MonoBehaviour {
    public float hopPower;
    [Range(0f, 1f)]
    public float drag;
    [Range(0f, 1f)]
    public float angularDrag;

    public LayerMask mask;
    public Collider carCollider;

    public Transform[] springPoints = new Transform[4];
    public float springLength;
    public float springForce;
    [Range(0,1)]
    public float antiBarFactor;
    public Vector3 COMOffset;
    public Transform centerOffset;
    public float friction;
    public bool isGrounded = false;

    public struct SpringCollisionInfo {
        public float compressionRatio;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 responseForce;
    }
    public SpringCollisionInfo[] springCollisions = new SpringCollisionInfo[4];

    public float speed {get{ return rBody.velocity.magnitude; }}
    public float forwardSpeed {get{ return transform.InverseTransformDirection(rBody.velocity).z; }}

    public Vector3 localHorizontalVelocity { get {
            lHV = transform.InverseTransformDirection(rBody.velocity);
            lHV.y = 0;
            return lHV;
    }}
    Vector3 lHV;

    public Vector3 horizontalVelocity {get{ return transform.TransformDirection(localHorizontalVelocity); }}

    public float turnSpeed {get{ return transform.InverseTransformDirection(rBody.angularVelocity).y; }}

    public Vector3 velocity {get{ return rBody.velocity; }}
    public Vector3 angularVelocity {get{ return rBody.angularVelocity; }}

    public bool canMove {get{ return isGrounded; }}

    public bool triedMoving = false;

    public bool FLWheelGrounded {get{ return springCollisions[0].normal != Vector3.zero; }}
    public bool FRWheelGrounded {get{ return springCollisions[1].normal != Vector3.zero; } }
    public bool BLWheelGrounded {get{ return springCollisions[2].normal != Vector3.zero; } }
    public bool BRWheelGrounded {get{ return springCollisions[3].normal != Vector3.zero; } }

    Rigidbody rBody;
    RaycastHit hit;
    float slideSpeed;
    uint groundCount;
    bool accelerating = false;
    bool turning = false;
    Vector3 groundNormal;
    float averageRatio;
    float weight;
    float turnVel;

    void Start () {
        if (springPoints.Length != 4) Debug.LogError("CarPhysics: improper amount of wheels");

        springPoints[3].position = carCollider.bounds.center + (new Vector3( carCollider.bounds.size.x / 2, -carCollider.bounds.size.y / 2, -carCollider.bounds.size.z / 2) * 0.95f);  //BR
        springPoints[1].position = carCollider.bounds.center + (new Vector3( carCollider.bounds.size.x / 2, -carCollider.bounds.size.y / 2,  carCollider.bounds.size.z / 2) * 0.95f);   //FR
        springPoints[2].position = carCollider.bounds.center + (new Vector3(-carCollider.bounds.size.x / 2, -carCollider.bounds.size.y / 2, -carCollider.bounds.size.z / 2) * 0.95f); //BL
        springPoints[0].position = carCollider.bounds.center + (new Vector3(-carCollider.bounds.size.x / 2, -carCollider.bounds.size.y / 2,  carCollider.bounds.size.z / 2) * 0.95f);  //FL

        rBody = GetComponent<Rigidbody>();
        rBody.centerOfMass = rBody.centerOfMass + COMOffset;
    }

    void FixedUpdate () {
        UpdateSuspension();
        AddFrictionForce();
        AddHorizontalDrag();

        accelerating = false;
        turning = false;
    }

    void UpdateSuspension() {
        //main suspension calculation for all four corners
        isGrounded = true;
        groundCount = 0;
        for (int i = 0; i < 4; i++) {
            if (Physics.Raycast(springPoints[i].position, -transform.up, out hit, springLength, mask)) {
                springCollisions[i].compressionRatio = 1 - Mathf.Clamp01(hit.distance / springLength);
                springCollisions[i].point = hit.point;
                springCollisions[i].normal = hit.normal;
                springCollisions[i].responseForce = (transform.up * springForce * springCollisions[i].compressionRatio) - Vector3.Project(rBody.GetPointVelocity(springPoints[i].position), transform.up);

                rBody.AddForceAtPosition(
                    springCollisions[i].responseForce,
                    springPoints[i].position,
                    ForceMode.Acceleration
                );
                Debug.DrawRay(springPoints[i].position, -transform.up * hit.distance, Color.red);
                groundCount++;
            }
            else {
                springCollisions[i].compressionRatio = 1;
                springCollisions[i].point = springPoints[i].position + (transform.up * springLength);
                springCollisions[i].normal = Vector3.zero;
                springCollisions[i].responseForce = Vector3.zero;
                Debug.DrawRay(springPoints[i].position, -transform.up * springLength, Color.green);
            }
        }

        if (groundCount < 2)
            isGrounded = false;

        float antibarForce;
        //balance bar calculation
        for (int i = 0; i < 4; i += 2) {
            antibarForce = (springCollisions[i].responseForce.magnitude - springCollisions[i + 1].responseForce.magnitude) * antiBarFactor / 2;
            rBody.AddForceAtPosition(
                -springCollisions[i].responseForce.normalized * antibarForce,
                springPoints[i].position,
                ForceMode.Acceleration
            );
            rBody.AddForceAtPosition(
                springCollisions[i + 1].responseForce.normalized * antibarForce,
                springPoints[i + 1].position,
                ForceMode.Acceleration
            );
        }
    }

    void AddHorizontalDrag() {
        if (isGrounded) {
            if (!accelerating)
                rBody.AddRelativeForce(-localHorizontalVelocity * drag, ForceMode.VelocityChange);
            if (!turning)
                rBody.AddRelativeTorque(Vector3.up * -turnSpeed * angularDrag, ForceMode.VelocityChange);
        }
        else {
            rBody.AddRelativeForce(-localHorizontalVelocity * (drag / 2), ForceMode.VelocityChange);
            rBody.AddTorque(-rBody.angularVelocity * (angularDrag / 5), ForceMode.VelocityChange);
        }
    }

    void AddFrictionForce() {
        if (isGrounded) {
            slideSpeed = Vector3.Dot(transform.right, rBody.velocity);
            rBody.AddForce(transform.right * -slideSpeed * friction, ForceMode.Acceleration);
        }
    }

    public void Accelerate(float dir, float accel, float max) {
        triedMoving = (dir != 0);
        if (isGrounded && triedMoving) {
            weight = Mathf.Sign(dir) == Mathf.Sign(forwardSpeed)? (1 - (Mathf.Abs(forwardSpeed / max))) : 1;

            Vector3 dirAdjust = transform.TransformPoint(
                centerOffset.localPosition.x,
                centerOffset.localPosition.y,
                centerOffset.localPosition.z * Mathf.Sign(dir)
            );
            rBody.AddForceAtPosition(
                transform.forward * accel * weight * dir,
                dirAdjust,
                ForceMode.Acceleration
            );
            accelerating = true;
        }
        
    }

    public void Turn(float dir, float max) {
        if (dir != 0 && isGrounded && Mathf.Abs(forwardSpeed) > 3) {
            turnVel = transform.InverseTransformDirection(rBody.angularVelocity).y;
            rBody.AddRelativeTorque(Vector3.up * dir * max * (1 - Mathf.Clamp01(Mathf.Abs(turnVel / max))), ForceMode.VelocityChange);
        }
    }
}
