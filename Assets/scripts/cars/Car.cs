using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarPhysics))]
public class Car : MonoBehaviour {
    public Transform cameraFollow;


    [HideInInspector]
    public CarPhysics physics;

    public const uint levels = 11;

    [System.Serializable]
    public struct CarStatsInt {
        [Range(0, levels)]
        public uint acceleration;
        [Range(0, levels)]
        public uint maxSpeed;
        [Range(0, levels)]
        public uint control;
        [Range(0, levels)]
        public uint weight;
        [Range(0, levels)]
        public uint health;
    }

    public struct CarStats {
        public float acceleration;
        public float maxSpeed;
        public float control;
        public float weight;
        public float health;
    }


    public CarStatsInt carStats;
    public static readonly CarStats maxStats = new CarStats() { acceleration = 100f, maxSpeed = 40f, control = 2.5f, health = 100f, weight = 16 };
    public static readonly CarStats minStats = new CarStats() { acceleration = 35f, maxSpeed = 30f, control = 0.4f, health = 30f, weight = 6 };
    public CarStats stats;
    public CarStatManager statsManager = new CarStatManager();
    public CarHealthManager healthManager;

    public CarState driveState;

    public ParticleSystem driftBoomA, driftBoomB;
    public const float driftBoostTimeA = 1.65f, driftBoostTimeB = 4.5f;

    public void ResetStats() {
        stats.acceleration = Mathf.Lerp(minStats.acceleration, maxStats.acceleration, (float)carStats.acceleration / levels);
        stats.maxSpeed = Mathf.Lerp(minStats.maxSpeed, maxStats.maxSpeed, (float)carStats.maxSpeed / levels);
        stats.weight = Mathf.Lerp(minStats.weight, maxStats.weight, (float)carStats.weight / levels);
        stats.health = Mathf.Lerp(minStats.health, maxStats.health, (float)carStats.health / levels);
        stats.control = Mathf.Lerp(minStats.control, maxStats.control, (float)carStats.control / levels);
    }

	void Awake () {
        physics = GetComponent<CarPhysics>();

        ResetStats();

        driveState = new IdleMoveState();
        driveState = driveState.Init(this);
        healthManager.SetHealth(stats.health);
    }

	void Update () {
        statsManager.Update(this);
        driveState = driveState.HandleInput(this);
        driveState = driveState.Update(this);
	}

    void FixedUpdate() {
        driveState = driveState.FixedUpdate(this);
    }
}
