using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour {

	void Start () {
		Cursor.visible = false;
	}
	
	void Update () {
		//Debug.DrawRay (GetComponent<Camera>().ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0.0f)).origin, GetComponent<Camera>().ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0.0f)).direction * GetComponent<Camera>().ViewportPointToRay (new Vector3 (0.0f, 0.0f, 0.0f)).direction, Color.red, 10);

		if (Input.GetMouseButtonUp(1)) {
			Cursor.visible = !Cursor.visible;
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)) {
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
	}
}
