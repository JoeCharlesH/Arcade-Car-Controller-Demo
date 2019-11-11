using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftMoveState : CarState {
    float timeWeight;
    float mag;
    float dir;
    float time = 0;
    bool playedA = false, playedB = false;

    bool boostEvaluate(Car car, float maxTime, float boostAmount, ref bool particleTriggered, ParticleSystem driftBoom) {
        if(time >= maxTime) {
            if (!particleTriggered) {
                driftBoom.Play(true);
                particleTriggered = true;
            }
            if(!Input.GetButton("Drift") && CarStateUtility.carIsMoving(car, true)) {
                car.statsManager.BoostSpeed(car, boostAmount);
                return true;
            }
        }
        return false;
    }

    public CarState HandleInput(Car car) {
        mag = (dir * Input.GetAxis("HorizontalA") / 2) + 0.5f;
        if (boostEvaluate(car, Car.driftBoostTimeB * timeWeight, 2.333333f, ref playedB, car.driftBoomB) || boostEvaluate(car, Car.driftBoostTimeA * timeWeight, 1.666666f, ref playedA, car.driftBoomA)) {
            DrivingMoveState newState = new DrivingMoveState();
            return newState.Init(car);
        }
        else if (!Input.GetButton("Drift") || !CarStateUtility.carIsMoving(car, true)) {
            DrivingMoveState newState = new DrivingMoveState();
            return newState.Init(car);
        }
        return this;
    }

    public CarState Init(Car car) {
        car.physics.friction = car.stats.control * 1.5f;
        dir = Mathf.Sign(Input.GetAxisRaw("HorizontalA"));
        timeWeight = ((1 - 0.65f) * Mathf.Pow((float)car.carStats.control / Car.levels, 2)) + 0.65f;
        return this;
    }

    public CarState FixedUpdate(Car car) {
        car.physics.Accelerate(1, car.stats.acceleration, car.stats.maxSpeed);
        car.physics.Turn(dir, 2f * (Mathf.Pow(mag + 1.2f, 2) / 4.84f));
        return this;
    }

    public CarState Update(Car car) {
        time += Time.deltaTime;
        return this;
    }

    public CarState OnCarCollision(Car car, Car otherCar) {
        return this;
    }
}
