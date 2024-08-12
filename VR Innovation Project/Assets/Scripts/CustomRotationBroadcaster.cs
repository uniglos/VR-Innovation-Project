using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;
using System;

public class CustomRotationBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster {
	[SerializeField] private PlayerLocomotor locomotor;

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
	}

	public void HandleRotation(Vector3 localHandMovement) {
		float normalisedX = Mathf.Clamp(localHandMovement.x, -0.25f, 0.25f) / 0.25f;

		// Make a new quaternion that rotates the player by the hand movement
		Quaternion rotation = Quaternion.Euler(0, _smoothTurnCurve.Evaluate(Mathf.Abs(normalisedX)) * Mathf.Sign(localHandMovement.x), 0);

		// Create a rotation event
		LocomotionEvent rotationEvent = new LocomotionEvent(Identifier, rotation, LocomotionEvent.RotationType.Velocity);

		locomotor.HandleLocomotionEvent(rotationEvent);

		// Broadcast the event
		_whenLocomotionEventRaised.Invoke(rotationEvent);
	}
}
