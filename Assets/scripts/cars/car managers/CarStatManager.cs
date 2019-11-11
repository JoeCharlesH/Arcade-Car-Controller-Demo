using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarStatManager {
    public const float maxBoostTime = 1.5f;
    float currentBoost = 0;
    public float boostTime;
    public bool boosting = false;
    public System.Action OnBoost;
    public System.Action OnBoostEnd;

    public void BoostSpeed(Car car, float multiplier) {
        if (!boosting || currentBoost < multiplier) {
            boostTime = 0;
            currentBoost = multiplier;
            car.stats.maxSpeed = Car.maxStats.maxSpeed * ((float)car.carStats.maxSpeed / Car.levels) * multiplier;
            boosting = true;
            if(OnBoost != null) OnBoost();
        }
    }

    void UpdateBoost(Car car) {
        if (boosting) {
            if (boostTime >= maxBoostTime) {
                boosting = false;
                currentBoost = 0;
                car.stats.maxSpeed = Car.maxStats.maxSpeed * ((float)car.carStats.maxSpeed / Car.levels);
                if (OnBoostEnd != null) OnBoostEnd();
            }
            else boostTime += Time.deltaTime;
        }
    }

    public void Update(Car car) {
        UpdateBoost(car);
    }
}
