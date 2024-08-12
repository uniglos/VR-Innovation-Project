using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineHandler : MonoBehaviour {
	[NonSerialized] public Transform startPoint, endPoint;
	private LineRenderer lineRenderer;

	private void Start() {
		lineRenderer = GetComponent<LineRenderer>();
	}

	// When this object is instantiated, automatically call the InstantiateLine method
	private void OnEnable() {
		//UpdateLine(new Transform[] {startPoint, endPoint});
	}

	public void UpdateLine(Vector3[] linePositions) {
		lineRenderer.positionCount = linePositions.Length;
		lineRenderer.SetPositions(linePositions);
	}
}