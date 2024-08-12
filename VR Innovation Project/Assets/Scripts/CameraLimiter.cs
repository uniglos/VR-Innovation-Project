using UnityEngine;

public class CameraLimiter : MonoBehaviour {

	private CharacterController controller;
	private OVRCameraRig cameraRig;

	private void Start() {
		controller = GetComponent<CharacterController>();
		cameraRig = GetComponentInChildren<OVRCameraRig>();
	}

	private void FixedUpdate() {
		var centrePoint = transform.InverseTransformPoint(cameraRig.centerEyeAnchor.position);
		controller.center = new Vector3(centrePoint.x, controller.height / 2 + controller.skinWidth, centrePoint.z);

		controller.Move(new Vector3(0.001f, -0.001f, 0.001f));
		controller.Move(new Vector3(-0.001f, -0.001f, -0.001f));
	}
}