using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {
	void Update () {
		if(Input.GetKeyDown(KeyCode.M)) CameraShake.Shake("CarCameraRenderer", 1, false);
	}
}
