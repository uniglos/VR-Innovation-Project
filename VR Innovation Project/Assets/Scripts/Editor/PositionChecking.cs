using TMPro;
using UnityEngine;

public class PositionChecking : MonoBehaviour {
	[SerializeField] private GameObject[] gameObjects;
	private TextMeshProUGUI debugText;

	// Start is called before the first frame update
	void Start() {
		debugText = GetComponent<TextMeshProUGUI>();
	}

	// Update is called once per frame
	void Update() {
		//clear previous text
		debugText.text = "";

		// for each gameobject in the array, add its name, local and world position to the debug text if it changed from it's most recent position
		foreach(GameObject go in gameObjects) {
			//if(go.transform.hasChanged) {
				debugText.text += $"{go.name}:\nLocal Position: {go.transform.localRotation}\nWorld Position: {go.transform.rotation}\n\n";
				go.transform.hasChanged = false;
			//}
		}
	}
}
