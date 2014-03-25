﻿using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
public class EventManager : MonoBehaviour {
	public GameObject allyTankPrefab,playerPrefab,enemyTankPrefab;
	public GameObject exclamationMark;
	public AudioClip shootSound,moveSound;
	private float moveSoundDelay;

	//Instantiate tanks, i think player tank better be manually placed on scene
	void Start () {
		GameObject temp;
	    //	temp = (GameObject)Instantiate (playerPrefab, new Vector3 (12, 5, 164), Quaternion.identity);
		//temp.tag="Player";
		temp=(GameObject)Instantiate (allyTankPrefab, new Vector3 (12, 3, 60), Quaternion.identity);
		temp.tag = "Ally";
		temp=(GameObject)Instantiate (allyTankPrefab, new Vector3 (12, 3, 16), Quaternion.identity);
		temp.tag="Ally";
		temp = (GameObject)Instantiate (allyTankPrefab, new Vector3 (12, 3, 88), Quaternion.identity);
		temp.tag="Ally";
		temp = (GameObject)Instantiate (allyTankPrefab, new Vector3 (12, 3, 124), Quaternion.identity);
		temp.tag="Ally";
		temp = (GameObject)Instantiate (enemyTankPrefab, new Vector3 (188, 3, 56), Quaternion.identity);
		temp.tag="Enemy";
		temp = (GameObject)Instantiate (enemyTankPrefab, new Vector3 (188, 3, 16), Quaternion.identity);
		temp.tag="Enemy";
		temp = (GameObject)Instantiate (enemyTankPrefab, new Vector3 (188, 3, 88), Quaternion.identity);
		temp.tag="Enemy";
		temp = (GameObject)Instantiate (enemyTankPrefab, new Vector3 (188, 3, 124), Quaternion.identity);
		temp.tag="Enemy";
		temp = (GameObject)Instantiate (enemyTankPrefab, new Vector3 (188, 3, 164), Quaternion.identity);
		temp.tag="Enemy";
	}

	void Update () {
		moveSoundDelay -= Time.deltaTime;
		//player sound play
		if ((Input.GetButton ("Horizontal") || Input.GetButton ("Vertical")))
						PlayMoveSound ();
				else
						audio.Stop ();
	}

	//call under "move" method
	void PlayMoveSound(){
		if (moveSoundDelay <= 0) {
			audio.PlayOneShot (moveSound);
			moveSoundDelay = 1.565f;
		}
		HearDetection (40f);
	}

	//call under shoot method
	void PlayShootSound(){
			audio.PlayOneShot (shootSound);
		HearDetection (80f); //shooting have much larger hearRange than moving 
	}


	//detect enemy tank within certain range and tell them that "i am here" when playing any sound, and enemy tank will come to position last time they hear sound 
	void HearDetection(float hearRange){
		Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, hearRange);
		foreach (Collider tank in InRangeTanks)
						if (tank.tag == "Ally") {
								//idea: instantiate exclamationMark on those enemy tank to show their alert
								Destroy (Instantiate (exclamationMark, tank.transform.position + 5*Vector3.up, Quaternion.identity), 3f);
								// i am not sure about this part
								if (tank.GetComponent<TankAI> ().state == "Idle") {
										tank.GetComponent<TankAI> ().state = "follow";
										tank.GetComponent<TankAI> ().seekingObject = transform;
								}
		}
						}
	}


