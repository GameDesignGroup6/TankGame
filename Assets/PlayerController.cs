using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour {

	float speed = 10.0f;
	float rotationSpeed = 100.0f;
	bool ready = true;
	public Transform projectile;
	private Transform turret;

	public GameObject exclamationMark;
	public AudioClip shootSound,moveSound;
	private float moveSoundDelay;

	void Start(){
		foreach (Transform child in transform) {
			if(child.name == "Turret Mount"){
				turret = child.GetChild(0);
			}
		}
	}

	void Update () {
		float translation = Input.GetAxis ("Vertical") * speed;
		float rotation = Input.GetAxis ("Horizontal") * rotationSpeed;
		transform.Translate (0, 0, translation * Time.deltaTime);
		transform.Rotate (0, rotation * Time.deltaTime, 0);

		if ((Input.GetButton ("Horizontal") || Input.GetButton ("Vertical")))
			PlayMoveSound ();
		else
			audio.Stop ();

		if(Input.GetButtonDown("Fire1") && ready){
			ready = false;
			Transform clone = (Transform)Instantiate(projectile, turret.GetChild(0).position, turret.parent.rotation);
			Invoke ("Reload", 5f);
		}
	}

	void PlayMoveSound(){
		if (moveSoundDelay <= 0) {
			audio.PlayOneShot (moveSound);
			moveSoundDelay = 1.565f;
		}
		HearDetection (40f);
	}

	void PlayShootSound(){
		audio.PlayOneShot (shootSound);
		HearDetection (80f); //shooting have much larger hearRange than moving 
	}

	void HearDetection(float hearRange){
		Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, hearRange);
		foreach (Collider tank in InRangeTanks) {
			if (tank.tag == "BadGuy") {
				//idea: instantiate exclamationMark on those enemy tank to show their alert
				Destroy(Instantiate (exclamationMark, tank.transform.position + 5 * Vector3.up, Quaternion.identity), 3f);
				// i am not sure about this part
				if (tank.GetComponent<TankAI> ().state == "Idle") {
					tank.GetComponent<TankAI> ().state = "Follow";
					tank.GetComponent<TankAI> ().seekingPoint = transform.position;
				}
			}
		}
	}

	void Reload(){
		ready = true;
	}
}
