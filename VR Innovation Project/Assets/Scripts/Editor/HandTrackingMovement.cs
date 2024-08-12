using UnityEngine;
using UnityEngine.AI;
using Oculus.Interaction.Input;
using TMPro;

// TODO: Swap the rotation to CentreEyeAnchor instead of the CameraRig // DONE
// TODO: Movement should be based on the forward vector of the pinching hand // DONE
// TODO: Lock/unlock movement after moving the pinched hand up
// TODO: UI

public class HandTrackingMovement : MonoBehaviour {

	[SerializeField] private GameObject leftHandAnchor, rightHandAnchor, centreEyeAnchor;
	[SerializeField] private float deadzone = 0.1f;
	[SerializeField] private float movementSpeed = 10f;
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private float maxDistance = 1.0f; // Max distance to sample for NavMesh
	[SerializeField] private TextMeshProUGUI debugText;
	[SerializeField] private Object spherePrefab;

	private enum PinchingHand { Left, Right, None }
	private PinchingHand pinchingHand = PinchingHand.None;
	private Hand leftHand, rightHand;
	private Vector3 pinchPosition;
	private Vector3 leftHandAnchorPosition, rightHandAnchorPosition;

	private void Start() {
		// Initialise previousHandPosition to avoid null reference
		pinchPosition = Vector3.zero;

		// Get the children with the Hand component and assign them to the correct variable
		//Hand[] hands = GetComponentsInChildren<Hand>();
		//foreach(Hand hand in hands) {
		//	if(hand.Handedness == Handedness.Left) {
		//		leftHand = hand;
		//		Get the child that has the work "Anchor" in its name and assign it to the leftHandAnchor variable
		//		foreach(Transform child in hand.GetComponentInChildren<Transform>()) {
		//			if(child.name.ToLower().Contains("anchor")) {
		//				leftHandAnchor = child.gameObject;
		//			}
		//		}
		//	} else {
		//		rightHand = hand;
		//		foreach(Transform child in hand.GetComponentInChildren<Transform>()) {
		//			if(child.name.ToLower().Contains("anchor")) {
		//				leftHandAnchor = child.gameObject;
		//			}
		//		}
		//	}
		//}
	}

	private void Update() {
		if(CheckPinch(rightHand) || CheckPinch(leftHand)) {
			HandleHandMovement(pinchingHand == PinchingHand.Left ? PinchingHand.Left : PinchingHand.Right);
		} else {
			if(pinchingHand != PinchingHand.None) {
				pinchingHand = PinchingHand.None;
				pinchPosition = Vector3.zero;
			}
		}

		//debugText.text = $"Left Hand Anchor: {leftHandAnchorPosition}\nRight Hand Anchor: {rightHandAnchorPosition}";
	}

	private bool CheckPinch(Hand hand) {
		if(hand.GetFingerIsPinching(HandFinger.Index)) {
			switch(pinchingHand) {
				case PinchingHand.Left:
					leftHandAnchorPosition = leftHandAnchor.transform.localPosition;
					break;
				case PinchingHand.Right:
					rightHandAnchorPosition = rightHandAnchor.transform.localPosition;
					break;
				case PinchingHand.None: //pinched for the first time
				default:
					pinchingHand = hand.Handedness == Handedness.Left ? PinchingHand.Left : PinchingHand.Right;

					//Update this hand's anchor position

					if(pinchPosition == Vector3.zero) {
						pinchPosition = pinchingHand == PinchingHand.Left ? leftHandAnchorPosition : rightHandAnchorPosition;

						//Instantiate the Sphere prefab at the previous hand position 
						//Instantiate(spherePrefab, pinchPosition, Quaternion.identity);
					}
					break;
			}
			return true;
		}
		return false;
	}

	private void HandleHandMovement(PinchingHand hand) {
		Vector3 currentHandPosition = hand == PinchingHand.Left ? leftHandAnchorPosition : rightHandAnchorPosition;
		Vector3 handMovement = currentHandPosition - pinchPosition;

		debugText.text = $"Hand Movement: {handMovement}" +
			$"\nCurrent Position: {currentHandPosition}" +
			$"\nPrevious Position: {pinchPosition}";

		// If the hand movement is within the deadzone, don't move
		if(handMovement.magnitude < deadzone) {
			return;
		}

		// Calculate the potential new position
		Vector3 potentialNewPosition = transform.position + transform.forward * handMovement.x * movementSpeed * Time.deltaTime;

		// Check if the new position is on a walkable surface
		//if(IsWalkable(potentialNewPosition)) {
		//	transform.position = potentialNewPosition;
		//}

		// Add rotation to the centreEyeAchoer's rotation based on x-axis movement of the hand
		transform.Rotate(transform.up, handMovement.x * rotationSpeed * Time.deltaTime);

		//centreEyeAnchor.transform.Rotate(Vector3.up, handMovement.x * rotationSpeed * Time.deltaTime);
	}

	private bool IsWalkable(Vector3 position) {
		NavMeshHit hit;
		if(NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas)) {
			return hit.hit;
		}
		return false;
	}
}