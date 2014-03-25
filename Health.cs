using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	int maxHealth = 100;
	int health = 100;
	Texture2D front;
	Texture2D back;
	
	void Start () {
		back = new Texture2D(1, 1, TextureFormat.RGB24, false);
		front = new Texture2D(1, 1, TextureFormat.RGB24, false);
		back.SetPixel(0, 0, Color.red);
		front.SetPixel(0, 0, Color.green);
		back.Apply();
		front.Apply();
	}

	void Update() {
		if(Input.GetButtonDown("Fire1"))
		   health--;
		}

	void OnGUI() {
		GUI.DrawTexture(new Rect(5, Screen.height - 25, 200, 20), back, ScaleMode.StretchToFill);
		GUI.DrawTexture(new Rect(5, Screen.height - 25, 200 * health / maxHealth, 20), front, ScaleMode.StretchToFill);
	}
}
