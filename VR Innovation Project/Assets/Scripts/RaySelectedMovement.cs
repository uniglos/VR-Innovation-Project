using UnityEngine;
using UnityEngine.AI;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class RayTrackingMovement : MonoBehaviour {

	#region UNUSED CODE
	//[SerializeField] private IndexPinchSelector leftHandPinch, rightHandRay;
	//[SerializeField] private NavMeshAgent navMeshAgent;
	//[SerializeField] private float pinchThreshold = 0.7f;

	//private bool isPinching = false;
	//private Vector3 targetPosition;

	//void Update() {
	//	CheckPinch(leftHandPinch);
	//	CheckPinch(rightHandRay);

	//	if(isPinching) {
	//		//UpdateTargetPosition();
	//		MoveTowardsTarget();
	//	}
	//}

	//private void CheckPinch(IndexPinchSelector handRay) {
	//	if(handRay.Hand.GetFingerIsPinching(HandFinger.Index)) {
	//		if(!isPinching) {
	//			isPinching = true;
	//			UpdateTargetPosition(handRay.Hand);
	//		}
	//	} else {
	//		isPinching = false;
	//		navMeshAgent.isStopped = true;
	//	}
	//}

	//private void UpdateTargetPosition(IHand hand) {
	//	Ray ray = new Ray(hand. , hand.transform.forward);
	//	if(Physics.Raycast(ray, out RaycastHit hit)) {
	//		if(hit.collider.CompareTag("Floor")) {
	//			targetPosition = hit.point;
	//		}
	//	}
	//}
	#endregion	
	
	[SerializeField] private Hand leftHand, rightHand;
	[SerializeField] private NavMeshAgent navMeshAgent;
	[SerializeField] private float tagetPointDistanceFromHand = 5f;
	//[SerializeField] private float pinchThreshold = 0.7f;

	private enum PinchingHand { Left, Right, None }
	private PinchingHand pinchingHand = PinchingHand.None;
	private Vector3 targetPosition, pinchingPosition;

	private void Start() {
		navMeshAgent = GetComponent<NavMeshAgent>();
		pinchingPosition = Vector3.zero;
	}

	private void Update() {
		if(CheckPinch(rightHand) || CheckPinch(leftHand)) {
			UpdateTargetPosition(pinchingHand == PinchingHand.Left ? leftHand : rightHand);
		} else {
			if(pinchingHand != PinchingHand.None) {
				pinchingHand = PinchingHand.None;
				navMeshAgent.isStopped = true;
			}
		}
	}

	private bool CheckPinch(Hand hand) {
		if(hand.GetFingerIsPinching(HandFinger.Index) && pinchingHand == PinchingHand.None) {
			if(hand.Handedness == Handedness.Left) {
				pinchingHand = PinchingHand.Left;
			} else {
				pinchingHand = PinchingHand.Right;
			}

			if(pinchingPosition == Vector3.zero) {
				pinchingPosition = hand.transform.localPosition;
			}
			
			return true;
			
		} else { return false; }
	}

	private void UpdateTargetPosition(Hand hand) {
		// get the ray interactor from the hand from the [BuildingBlock] Left/Right Ray GameObject
		RayInteractor rayInteractor = hand.GetComponentInChildren<RayInteractor>();

		// make a raycast from the ray interactor and update the target position if the ray hits the floor
		Ray ray = new(rayInteractor.Origin, rayInteractor.Forward);
		if(Physics.Raycast(ray, out RaycastHit hit)) {
			targetPosition = ray.GetPoint(tagetPointDistanceFromHand); // Move only 1 unit in the forward direction
			MoveTowardsTarget();
		}
	}

	private void MoveTowardsTarget() {
		navMeshAgent.isStopped = false;
		navMeshAgent.SetDestination(targetPosition);
	}
}