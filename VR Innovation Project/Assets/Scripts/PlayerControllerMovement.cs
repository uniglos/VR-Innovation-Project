using UnityEngine;
using UnityEngine.AI;

public class PlayerControllerMovement : MonoBehaviour {
	private OVRPlayerController playerController;
	private CharacterController characterController;
	private CustomRotationBroadcaster rotationBroadcaster;
	[SerializeField] private GameObject spherePrefab;
	[SerializeField] private float movementSpeed = 1.0f;
	[SerializeField] private HandArc handArc;

	private enum MovementType {
		None,
		Walking,
		Turning
	}

	private MovementType movementType = MovementType.None;
	private float movementThreshold = 0.25f;

	private void Start() {
		playerController = GetComponent<OVRPlayerController>();
		characterController = GetComponent<CharacterController>();
		rotationBroadcaster = GetComponent<CustomRotationBroadcaster>();
		handArc.OnDestinationSet += HandleHandMovement;
		handArc.OnPinchReleased += ResetMovementType;
	}

	private void OnDestroy() {
		handArc.OnDestinationSet -= HandleHandMovement;
		handArc.OnPinchReleased -= ResetMovementType;
	}

	private void HandleHandMovement(Vector3 destination, Vector3 handMovement) {
		// Convert hand movement to local space relative to the player's orientation
		Vector3 localHandMovement = transform.InverseTransformDirection(handMovement);

		if(localHandMovement.z < -movementThreshold || movementType == MovementType.Walking) {
			MovePlayer(destination);
			movementType = MovementType.Walking;
		} else if(Mathf.Abs(localHandMovement.x) > movementThreshold || movementType == MovementType.Turning) {
			rotationBroadcaster.HandleRotation(localHandMovement);
			movementType = MovementType.Turning;
		}
	}

	private void MovePlayer(Vector3 destination) {
		if(IsOnNavMesh(ref destination)) {
			Vector3 direction = (destination - transform.position).normalized;
			direction.y = 0f;
			characterController.Move(movementSpeed * Time.deltaTime * direction);
		}
	}

	private bool IsOnNavMesh(ref Vector3 destination) {
		NavMeshHit hit;
		if(NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas)) {
			destination = hit.position;
			return true;
		}
		return false;
	}

	private void ResetMovementType() {
		movementType = MovementType.None;
	}
}
