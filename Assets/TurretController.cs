using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {

	float horizontalSpeed = 2.0f;

	void Update () {
		float h = horizontalSpeed * Input.GetAxis ("Mouse X");
		transform.Rotate (0, h, 0);
	}
}
