using UnityEngine;
using System.Collections;

public class camera : MonoBehaviour {
	public Transform player;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
	void FixedUpdate(){	
		transform.position = player.position + 30 * Vector3.up;
		transform.rotation =  Quaternion.Euler(new Vector3(90,player.rotation.eulerAngles.y,transform.rotation.z));
		Debug.Log(player.rotation.eulerAngles.y);
	}
}