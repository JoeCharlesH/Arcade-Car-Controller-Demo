using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStateUtility {
    public static bool carIsMoving(Car car, bool movingForwardOnly = false) {
        if(movingForwardOnly) return car.physics.horizontalVelocity.magnitude > 0.05f || (car.physics.canMove && Input.GetAxisRaw("Move") > 0);
        else return car.physics.horizontalVelocity.magnitude > 0.05f || (car.physics.canMove && Input.GetAxisRaw("Move") != 0);
    }
}
