using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidParticle : MonoBehaviour {
    public float cutoff;
    public Vector2 accelerationToRate;
    public ParticleSystem skidL, skidR;
    public CarPhysics car;

    float acceleration, min, max, grounded;
    Vector2 rate;
    Vector3 prevVelocity;
    Vector3 velocity;
    ParticleSystem.EmissionModule emission;

    void Update () {
        prevVelocity = velocity;
        velocity = car.horizontalVelocity;

        acceleration = Mathf.Max(0, Mathf.Abs(((velocity - prevVelocity) / Time.deltaTime).magnitude) - cutoff);

        min = accelerationToRate.x * acceleration;
        max = accelerationToRate.y * acceleration;

        grounded = car.BLWheelGrounded? 1 : 0;

        emission = skidL.emission;
        emission.rateOverDistance =  new ParticleSystem.MinMaxCurve(min * grounded, max * grounded);

        grounded = car.BRWheelGrounded ? 1 : 0;

        emission = skidR.emission;
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(min * grounded, max * grounded);
    }
}
