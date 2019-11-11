using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : Singleton<CameraShake> {
	protected CameraShake () {}

	const float maxRoll = 10f;
	const float maxPitch = 10f;
	const float maxYaw = 10f;
	const float maxDist = 0.2f;
	const float maxDuration = 1f;

	public AnimationCurve shakeOverTime = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1), new Keyframe(0.6f, 1), new Keyframe(1, 0));
	SortedDictionary<string, Transform> cameraCache = new SortedDictionary<string, Transform>();

	public static void Shake(string cameraName, float shakeIntensity, bool is2D) {
		shakeIntensity = Mathf.Clamp01(shakeIntensity + 0.1f);
		float time = shakeIntensity * maxDuration;

		if (instance.cameraCache.ContainsKey(cameraName)) {
			instance.StartCoroutine(instance.ShakeCamera(instance.cameraCache[cameraName], shakeIntensity, time, is2D));
		}
		else {
			Transform cam = GameObject.Find(cameraName).transform;
			if (cam) {
				instance.cameraCache.Add(cameraName, cam);
				instance.StartCoroutine(instance.ShakeCamera(cam, shakeIntensity, time, is2D));
			}
		}
	}

	public static float noise (float seed, float val) {
		return (Mathf.PerlinNoise(seed, val) - 0.5f) * 2;
	}

	IEnumerator ShakeCamera(Transform cam, float intensity, float duration, bool is2D) {
		float time = 0;
		float timeWeight;
		float seed = Random.Range(0f, 512f);
		float percent;
		

		if (is2D) {
			Vector3 pos = cam.localPosition;
			Vector3 rot = cam.localRotation.eulerAngles;
			Vector3 offset = new Vector3();
			Vector3 rotOffset = new Vector3();

			while (time <= duration) {
				percent = time / duration;
				timeWeight = shakeOverTime.Evaluate(percent);

				offset.Set(
					maxDist * intensity * noise(seed, percent),
					maxDist * intensity * noise(seed + 1, percent),
					0
				);
				rotOffset.Set(0, 0, maxRoll * intensity * noise(seed + 2, percent));

				cam.localPosition = pos + (offset * timeWeight);
				cam.localRotation = Quaternion.Euler(rot + (rotOffset * timeWeight));

				yield return null;
				time += Time.deltaTime;
			}
			cam.localPosition = pos;
			cam.localRotation = Quaternion.Euler(rot);
		}
		else {
			Vector3 rot = cam.localRotation.eulerAngles;
			Vector3 rotOffset = new Vector3();

			while (time <= duration) {
				percent = time / duration;
				timeWeight = shakeOverTime.Evaluate(percent);

				rotOffset.Set(
					maxPitch * intensity * noise(seed, percent * (12 * intensity)),
					maxYaw * intensity * noise(seed + 1, percent * (12 * intensity)),
					maxRoll * intensity * noise(seed + 2, percent * (12 * intensity))
				);

				cam.localRotation = Quaternion.Euler(rot + (rotOffset * timeWeight));

				yield return null;
				time += Time.deltaTime;
			}
			cam.localRotation = Quaternion.Euler(rot);
		}
	}

	public override void OnDestroy() {
		cameraCache.Clear();
		base.OnDestroy();
	}
}
