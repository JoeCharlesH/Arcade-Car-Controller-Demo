using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CarState {
    CarState HandleInput(Car car);

    CarState Init(Car car);

    CarState Update(Car car);

    CarState FixedUpdate(Car car);

    CarState OnCarCollision(Car car, Car otherCar);
}
