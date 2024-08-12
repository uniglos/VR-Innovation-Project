using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;
using System;
using UnityEngine.AI;
using TMPro;

// Rotate the player XR rig if they look around
// 

public class CustomMovementBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster {
	[SerializeField] private GameObject handAnchor;
	//[SerializeField] private float deadzone = 0.1f;
	[SerializeField] private float moveModeThreshold = 0.04f;
	[SerializeField] private float movementSpeed = 20f;
	//[SerializeField] private TeleportArcGravity movementArc;
	//[SerializeField] private Transform pinchTarget;
	[SerializeField] private CharacterController controller;
	[SerializeField] private TextMeshProUGUI debugText;
	//[SerializeField] private NavMeshAgent agent;

	private PlayerLocomotor locomotor;
	private Hand hand;
	private Vector3 pinchPosition, handAnchorPosition, destination;

	private enum LocomotionMode {
		None,
		Walking,
		Turning,
		Locking
	}
	private LocomotionMode locomotionMode = LocomotionMode.None;

	#region Copied from TurnerEventBroadcaster.cs
	private UniqueIdentifier _identifier;
	public int Identifier => _identifier.ID;

	[SerializeField]
	[Tooltip("Degrees to continuously rotate during selection when in Smooth turn mode, it is remapped from the Axis value")]
	private AnimationCurve _smoothTurnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 100f);

	/// <summary>
	/// Degrees to continuously rotate during selection when in Smooth turn mode, it is remapped from the Axis value
	/// </summary>
	public AnimationCurve SmoothTurnCurve {
		get {
			return _smoothTurnCurve;
		}
		set {
			_smoothTurnCurve = value;
		}
	}

	public event Action<LocomotionEvent> _whenLocomotionEventRaised = delegate { };

	public event Action<LocomotionEvent> WhenLocomotionPerformed {
		add {
			_whenLocomotionEventRaised += value;
		}
		remove {
			_whenLocomotionEventRaised -= value;
		}
	}
	#endregion

	private void Awake() {
		_identifier = UniqueIdentifier.Generate();
		hand = GetComponent<Hand>();
		locomotor = FindObjectOfType<PlayerLocomotor>();
	}

	private void Start() {
		pinchPosition = Vector3.zero;
	}

	private void Update() {
		// rotate the debug text to always face the player
		debugText.transform.LookAt(Camera.main.transform);

		// rotate the text by 180 degrees on the y axis
		debugText.transform.Rotate(0, 180, 0);

		if(CheckPinch()) {
			Vector3 currentHandPosition = handAnchor.transform.localPosition;
			Vector3 handMovement = currentHandPosition - pinchPosition;


			//debugText.text = $"Hand Movement: {handMovement}\n" +
			//	$"Current Poisition of hand: {currentHandPosition}\n" +
			//	$"Pinch Position: {pinchPosition}";

			// Locking the user to one movement option at a time for each hand to avoid turning while walking
			if(locomotionMode == LocomotionMode.None) {
				if(Mathf.Abs(handMovement.z) > moveModeThreshold) {
					locomotionMode = LocomotionMode.Walking;
				} else if(Mathf.Abs(handMovement.x) > moveModeThreshold) {
					locomotionMode = LocomotionMode.Turning;
				} else { return; }
			}

			switch(locomotionMode) {
				case LocomotionMode.Walking:
					HandleMovement(handMovement);
					break;
				case LocomotionMode.Turning:
					//HandleRotation(handMovement.x);
					break;
				case LocomotionMode.Locking:
					break;
				case LocomotionMode.None:
					break;
				default:
					break;
			}
		} else {
			if(pinchPosition == Vector3.zero && destination == Vector3.zero) {
				return;
			}

			pinchPosition = destination = Vector3.zero;
			locomotionMode = LocomotionMode.None;
		}
	}

	private bool CheckPinch() {
		if(hand.GetFingerIsPinching(HandFinger.Index)) {
			handAnchorPosition = handAnchor.transform.localPosition;

			if(pinchPosition == Vector3.zero) {
				pinchPosition = handAnchorPosition;
			}
			return true;
		}
		return false;
	}

	private void HandleMovement(Vector3 handMovement) {
		// MOVEMENT
		#region Attempt at using ray-based movement
		//if(destination == Vector3.zero) {
		//	destination = Vector3.Normalize(pinchTarget.position - currentHandPosition);
		//}

		//Vector3 movement = Mathf.Clamp(handMovement.z, -0.25f, 0.25f) * movementSpeed * destination;

		//LocomotionEvent movementEvent = new LocomotionEvent(Identifier, movement,
		//	LocomotionEvent.TranslationType.Velocity);

		//locomotor.HandleLocomotionEvent(movementEvent);

		//// Broadcast the event
		//_whenLocomotionEventRaised.Invoke(movementEvent);
		#endregion

		float z = handMovement.z;
		Mathf.Clamp(z, -0.25f, 0.25f);

		// For some reason, the hands' z values are inverted for each hand, correcting that here
		//if(hand.Handedness == Handedness.Left) {
		//	handMovement *= -1;
		//}

		//agent.Move(handMovement * movementSpeed * Time.deltaTime * handAnchor.transform.right);
		float velocity = z * movementSpeed * Time.deltaTime;
		//Vector3 direction = Vector3.Normalize(transform.InverseTransformPoint(handAnchor.transform.position) - transform.InverseTransformPoint(Camera.main.transform.position));
		//debugText.text = $"Direction: {direction}";
		//controller.Move(new Vector3(velocity * Mathf.Abs(direction.x), 0, velocity * direction.z));

		controller.Move(handMovement.normalized * velocity);
	}

	private void HandleRotation(float handMovement) {
		// ROTATION
		float normalisedX = Mathf.Clamp(handMovement, -0.25f, 0.25f) / 0.25f;

		// Deadzone for hand movement (removed since curve solves the same problem)

		//if(handMovement.magnitude < deadzone) {
		//	return;
		//}

		// Make a new quaternion that rotates the player by the hand movement
		Quaternion rotation = Quaternion.Euler(0, _smoothTurnCurve.Evaluate(Mathf.Abs(normalisedX)) * Mathf.Sign(handMovement), 0);

		// Create a rotation event
		LocomotionEvent rotationEvent = new LocomotionEvent(Identifier, rotation,
			LocomotionEvent.RotationType.Velocity);

		locomotor.HandleLocomotionEvent(rotationEvent);

		// Broadcast the event
		_whenLocomotionEventRaised.Invoke(rotationEvent);
	}
}