using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CarCamera : MonoBehaviour {
    public Car car;
    public float pitchOffset;
    public float speed;
    public float rotationSpeed;
    public float fovSpeed;
    public float speedToFOV;

    float currentFOV;
    Vector3 currentPosVelocity;
    Vector3 currentRotVelocity;
    Vector3 rotation;
    Vector3 target;

    [SerializeField]
    Camera cam;

    bool CheckRefrenceState() {
        if (!car || !cam) {
            enabled = false;
            return false;
        }
        return true;
    }

	void Start () {
        if (!CheckRefrenceState()) return;
        cam.name = transform.name + "Renderer";
        transform.rotation = Quaternion.LookRotation((car.transform.position + (car.transform.forward * pitchOffset)) - car.cameraFollow.position);
        rotation = transform.eulerAngles;
        transform.position = car.cameraFollow.position;
    }

	void LateUpdate () {
        if (!CheckRefrenceState()) return;
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, FOV(Mathf.Abs(car.physics.forwardSpeed)), ref currentFOV, 1 / fovSpeed);
        AdjustPosition();
        AdjustRotation();
    }

    float FOV(float speed) {
        return 70 + (speed * speedToFOV);
    }

    void AdjustPosition() {
        transform.position = Vector3.SmoothDamp(transform.position, car.cameraFollow.position, ref currentPosVelocity, 1 / speed);
    }

    void AdjustRotation() {
        target = Quaternion.LookRotation(((car.transform.position + (car.transform.forward * pitchOffset)) - transform.position).normalized).eulerAngles;

        rotation.x = Mathf.SmoothDampAngle(rotation.x, target.x, ref currentRotVelocity.x, 1 / rotationSpeed);
        rotation.y = Mathf.SmoothDampAngle(rotation.y, target.y, ref currentRotVelocity.y, 1 / rotationSpeed);
        rotation.z = Mathf.SmoothDampAngle(rotation.z, target.z, ref currentRotVelocity.z, 1 / rotationSpeed);

        transform.rotation = Quaternion.Euler(rotation);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (!Application.isPlaying && car && car.cameraFollow) {
            transform.position = car.cameraFollow.position;
            transform.rotation = Quaternion.LookRotation((car.transform.position + (car.transform.forward * pitchOffset)) - car.cameraFollow.position);
        }
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, FOV(0), 3, 0, 1);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
