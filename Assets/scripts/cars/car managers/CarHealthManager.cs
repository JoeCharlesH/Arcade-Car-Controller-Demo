using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Car))]
public class CarHealthManager : MonoBehaviour, Entity {
    [SerializeField]
    LayerMask destructables;
    [SerializeField]
    LayerMask obstacles;
    Car car;

    float health = 0;
    Rigidbody rBody;
    [SerializeField]
    Collider coll;

    public string cameraName = "CarCameraRenderer";

	void Start () {
        car = GetComponent<Car>();
        rBody = GetComponent<Rigidbody>();
	}

    public void Damage(float otherMomentum, float relativeVelocity) {
        float momentum = car.stats.weight * relativeVelocity;

        float damage = Mathf.Min(car.stats.health, Mathf.Abs(otherMomentum - momentum) / car.stats.weight);
        Debug.Log(gameObject.name + " took " + damage.ToString() + " damage.");
        CameraShake.Shake(cameraName, Mathf.Clamp01(0.1f + (damage / car.stats.health)), false);

        health -= damage;
        if (health <= 0) {
            health = 0;
            Kill();
        }
    }
    public void Damage(float damage) {
        Debug.Log(gameObject.name + " took " + damage.ToString() + " damage.");
        CameraShake.Shake(cameraName, Mathf.Clamp01(0.1f + (damage / car.stats.health)), false);

        health -= Mathf.Min(health, damage);
        if (health <= 0) {
            health = 0;
            Kill();
        }
    }

    public void Kill() {
        Destroy(car.gameObject);
    }

    public void SetHealth(float newHealth) {
        health = newHealth;
        if (health <= 0) {
            health = 0;
            Kill();
        }
    }

    public float GetHealth() {
        return health;
    }
    bool Valid(ContactPoint[] contacts) {
        foreach(ContactPoint contact in contacts) {
            if (coll.gameObject == contact.thisCollider.gameObject || coll.gameObject == contact.otherCollider.gameObject) return true;
        }
        return false;
    }

    public void OnCollisionEnter(Collision other) {
        if(Valid(other.contacts)) {
            if ((destructables & (1 << other.gameObject.layer)) != 0) {
                Entity e = other.gameObject.GetComponent<Entity>();
                e.Damage(car.stats.weight * Mathf.Abs((other.rigidbody.velocity - rBody.velocity).magnitude), Mathf.Abs(other.relativeVelocity.magnitude));
            }
            if ((obstacles & (1 << other.gameObject.layer)) != 0) {
                Vector3 velocity = transform.InverseTransformDirection(other.relativeVelocity);
                velocity.y = 0;
                Damage(Mathf.Pow(velocity.magnitude / car.stats.maxSpeed, 2) * car.stats.health);
            }
        }
    }
}
