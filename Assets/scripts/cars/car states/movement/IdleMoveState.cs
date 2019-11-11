using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleMoveState : CarState {

    public CarState HandleInput(Car car) {
        if (Input.GetAxisRaw("Move") != 0 && car.physics.canMove) {
            DrivingMoveState newState = new DrivingMoveState();
            newState.Init(car);
            return newState;
        }
        else return this;
    }

    public CarState Init(Car car) {
        return this;
    }

    public CarState FixedUpdate(Car car) {
        return this;
    }

    public CarState Update(Car car) {
        return this;
    }

    public CarState OnCarCollision(Car car, Car otherCar) {
        return this;
    }

}
