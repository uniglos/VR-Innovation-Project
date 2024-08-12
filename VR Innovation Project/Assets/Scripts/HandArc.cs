using Oculus.Interaction.Input;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineHandler))]
public class HandArc : MonoBehaviour {
	[SerializeField] private Transform thumbTransform, indexTransform, wristTransform;
	[SerializeField] private PlayerControllerMovement movementController;
	[SerializeField] private float gravityModifier = 2.3f;
	[SerializeField] private Vector3 wristPositionAdjustment;
	[SerializeField] private float maxDistance = 10f;
	[SerializeField] private int arcPointsCount = 30;
	[SerializeField] private LayerMask navMeshLayerMask;
	private static readonly Vector3 gravityForce = new Vector3(0f, -9.81f, 0f);
	private LineHandler lineHandler;
	private Hand thisHand;
	private Vector3[] arcPoints;
	private Vector3 hitPosition = Vector3.zero;
	private Vector3 pinchStartPosition;

	public delegate void Destination(Vector3 position, Vector3 handMovement);
	public event Destination OnDestinationSet;

	public delegate void PinchReleased();
	public event PinchReleased OnPinchReleased;

	private void Start() {
		lineHandler = GetComponent<LineHandler>();
		thisHand = transform.parent.GetComponentsInChildren<Hand>()
			.First(hand => hand.name.Contains(transform.name.Contains("Left") ? "Left" : "Right"));
	}

	private void OnEnable() {
		OnDestinationSet += EnableLine;
	}

	private void OnDisable() {
		OnDestinationSet -= EnableLine;
	}

	private void EnableLine(Vector3 position, Vector3 handMovement) {
		lineHandler.UpdateLine(arcPoints);
	}

	private void Update() {
		if(CheckPinch()) {
			Vector3 startPoint = (thumbTransform.position + indexTransform.position) / 2;
			Vector3 wristPosition = wristTransform.position + wristTransform.TransformDirection(wristPositionAdjustment);
			Vector3 direction = (startPoint - wristPosition).normalized;
			Vector3 velocity = direction * maxDistance;

			arcPoints = CalculateArcPoints(startPoint, velocity);
			lineHandler.UpdateLine(arcPoints);

			float distance = Vector3.Distance(startPoint, wristPosition);
			Vector3 handMovement = startPoint - pinchStartPosition;
			OnDestinationSet?.Invoke(hitPosition, handMovement);
		} else if(pinchStartPosition != Vector3.zero) {
			pinchStartPosition = Vector3.zero;
			OnPinchReleased?.Invoke();
		}
	}

	private bool CheckPinch() {
		if(thisHand.GetFingerIsPinching(HandFinger.Index)) {
			if(pinchStartPosition == Vector3.zero) {
				pinchStartPosition = (thumbTransform.position + indexTransform.position) / 2;
			}
			return true;
		}
		return false;
	}

	private Vector3[] CalculateArcPoints(Vector3 startPoint, Vector3 velocity) {
		Vector3[] arcPoints = new Vector3[arcPointsCount];
		arcPoints[0] = startPoint;
		float t;
		bool objectHit = false;

		for(int i = 1 ; i < arcPointsCount ; i++) {
			t = i / (float)(arcPointsCount - 1);
			arcPoints[i] = startPoint + velocity * t + 0.5f * gravityModifier * t * t * gravityForce;

			if(!objectHit && IsObjectHit(arcPoints[i], arcPoints[i - 1])) {
				objectHit = true;
				arcPoints[i] = hitPosition;
				break;
			}
		}

		if(objectHit) {
			Vector3[] rearrangedArcPoints = new Vector3[arcPointsCount];
			rearrangedArcPoints[0] = startPoint;
			rearrangedArcPoints[arcPointsCount - 1] = hitPosition;

			for(int i = 1 ; i < arcPointsCount - 1 ; i++) {
				t = i / (float)(arcPointsCount - 2);
				rearrangedArcPoints[i] = startPoint + (hitPosition - startPoint) * t;
			}

			arcPoints = rearrangedArcPoints;
		}

		return arcPoints;
	}

	private bool IsObjectHit(Vector3 point, Vector3 prevPoint) {
		if(Physics.Linecast(prevPoint, point, out RaycastHit hit, navMeshLayerMask)) {
			hitPosition = hit.point;
			return true;
		}
		return false;
	}
}
