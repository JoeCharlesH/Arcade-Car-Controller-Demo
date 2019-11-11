using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivingMoveState : CarState {
    protected float accelDir;
    protected float turnDir;

    public virtual CarState Init(Car car) {
        car.physics.friction = car.stats.control * 3;
        return this;
    }

    public virtual CarState HandleInput(Car car) {
        if (Input.GetButtonDown("DebugBoost1"))
            car.statsManager.BoostSpeed(car, 1.666666f);
        else if (Input.GetButtonDown("DebugBoost2"))
            car.statsManager.BoostSpeed(car, 2.333333f);

        if (Input.GetButton("Drift") && Input.GetAxis("HorizontalA") != 0 && CarStateUtility.carIsMoving(car, true)) {
            DriftMoveState newState = new DriftMoveState();
            return newState.Init(car);
        }
        accelDir = Input.GetAxisRaw("Move");
        turnDir = Input.GetAxis("HorizontalA");
        return this;
    }
    
    public CarState FixedUpdate(Car car) {
        if (!car.statsManager.boosting) car.physics.Accelerate(accelDir, car.stats.acceleration, car.stats.maxSpeed);
        else car.physics.Accelerate(1, car.stats.acceleration, car.stats.maxSpeed);
        car.physics.Turn(turnDir, 1.25f);
        return this;
    }

    public virtual CarState Update(Car car) {
        if (!CarStateUtility.carIsMoving(car)) {
            IdleMoveState newState = new IdleMoveState();
            return newState.Init(car);
        }
        else return this;
    }

    public CarState OnCarCollision(Car car, Car otherCar) {
        return this;
    }
}
