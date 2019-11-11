using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Entity {
    void Damage(float momentum, float relativeVelocity);
    void Damage(float damage);
    void Kill();
    float GetHealth();
    void SetHealth(float health);
}
