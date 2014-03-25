using UnityEngine;
using System.Collections;
using System.Linq;
using System;
[RequireComponent(typeof(AudioSource))]
public class TankAI : MonoBehaviour {
	
	public GameObject exclamationMark;
	public AudioClip shootSound,moveSound;
	private float moveSoundDelay;
	
	/*
	 * idle
	 * attack
	 * flee
	 * follow
	 * visit
	 */
	public string state;
	public int tankHealth = 5;
	
	private Transform turret;
	public Transform target;
	public Transform projectile;
	private bool ready = true;
	
	public ArrayList enemyTanks;
	public ArrayList allyTanks;	
	public Vector3 seekingPoint;
	public Transform seekingObject;
	private Transform[] followers;
	
	public int sightRange = 30;
	public int coneBaseRadius = 30;
	
	public int mass;
	public Vector3 velocity;
	public float maxVelocity;
	public float acceleration;
	public float slowingRadius;
	public float haltingRadius;
	private float wanderAngle;
	private bool avoiding = false;
	private Transform obstacle;
	private bool side;
	
	public Transform deadTank;
	private GameObject exclamation;
	
	void Start () {
		state = "Visit";
		velocity = Vector3.zero;
		wanderAngle = Vector3.Angle (velocity, new Vector3(0,0,1));
		seekingPoint = transform.position + 2 * transform.forward;
		foreach (Transform child in transform) {
			if(child.name == "Turret Mount"){
				turret = child.GetChild(0);
			}
		}
		followers = new Transform[6];
	}
	
	void Update () {
		moveSoundDelay -= Time.deltaTime;
		if (!avoiding) {
			Look ();
			ColCheck ();
			if(avoiding){
				return;
			}
			Move ();
		}
		else{
			Step(moveAround(Vector3.zero));
		}
		if (state == "Attack")
			if (exclamation != null)
				Destroy (exclamation);
	}
	
	void Look(){
		//check radius
		Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, sightRange);
		ArrayList Enemys = new ArrayList();
		ArrayList Allys = new ArrayList();
		
		//check cone
		foreach (Collider tank in InRangeTanks) {
			var targetLocalPosition = transform.InverseTransformPoint(tank.transform.position);		//this may not be right
			if (targetLocalPosition.z < 0){
				continue;
			}
			var coneRadius = (targetLocalPosition.z / sightRange) * coneBaseRadius;
			Vector2 TargetXY = new Vector2(targetLocalPosition.x, targetLocalPosition.y);
			if (TargetXY.magnitude > coneRadius){
				continue;
			}
			if(tank.tag == "Untagged"){
				continue;
			}
			if(tank.tag == transform.tag){
				Allys.Add(tank);
			}
			else{
				Enemys.Add (tank);
			}
		}
		//check for blocked vision
		
		if (Enemys.Count != 0 && (!state.Equals ("Flee") || !state.Equals ("Attack"))) {
			state = "Attack";
			Collider enemy = (Collider)Enemys[0];
			seekingPoint = enemy.transform.position;
			seekingObject = enemy.transform;
		}
		if(Allys.Count != 0 && state.Equals("Idle")){
			state = "Follow";
			Collider ally = (Collider)Allys[0];
			for(int i = 0; i < 6; i++){
				if(ally.transform.GetComponent<TankAI>().followers[i] == null){
					ally.transform.GetComponent<TankAI>().followers[i] = transform;
					break;
				}
			}
			seekingPoint = ally.transform.position;
			seekingObject = ally.transform;
		}
		if(state.Equals("Follow") && (seekingObject == transform || seekingObject.GetComponent<TankAI>().state.Equals("Follow"))){
			seekingObject.GetComponent<TankAI>().followers[Array.IndexOf(seekingObject.GetComponent<TankAI>().followers, transform)] = null;
			state = "Idle";
		}
		if(state.Equals("Follow") && seekingObject.GetComponent<TankAI>().state == "Attack"){
			seekingObject.GetComponent<TankAI>().followers[Array.IndexOf(seekingObject.GetComponent<TankAI>().followers, transform)] = null;
			state = "Attack";
			seekingObject = seekingObject.GetComponent<TankAI>().seekingObject;
		}
		if (Allys.Count == 0 && state.Equals ("Follow")) {
			seekingObject.GetComponent<TankAI>().followers[Array.IndexOf(seekingObject.GetComponent<TankAI>().followers, transform)] = null;
			state = "Visit";
			seekingObject = null;
		}
		Collider[] seekingArea = Physics.OverlapSphere (seekingPoint, .5f);
		if(seekingArea.Contains(transform.collider) && state == "Visit")
			state = "Idle";
		if(state.Equals("Attack") && Enemys.Count == 0){
			state = "Visit";
			seekingObject = null;
		}
	}
	
	void ColCheck(){
		RaycastHit hit;
		if (Physics.Raycast (transform.position, velocity, out hit, 4f)) {
			obstacle = hit.transform;
			if(hit.transform.name == "Pool"){
				avoiding = true;
			}
			if(hit.transform.name == "Ally Left L"){
				avoiding = true;
			}
			if(hit.transform.name == "Ally Right L"){
				avoiding = true;
			}
			if(hit.transform.name == "Enemy Left L"){
				avoiding = true;
			}
			if(hit.transform.name == "Enemy Right L"){
				avoiding = true;
			}
			if(hit.transform.name == "Wall"){
				avoiding = true;
			}
			if(hit.transform.name == "BackWall" || hit.transform.name == "FrontWall" || hit.transform.name == "SideWall"){
				moveAround(Vector3.zero);
			}
		}
	}
	
	void Move(){
		PlayMoveSound ();
		if (state == "Idle") {
			Wander ();
			return;
		}
		if (state == "Attack") {
			AttackMove ();
			return;
		}
		if (state == "Flee") {
			Flee ();
			return;
		}
		if (state == "Follow") {
			Seek ();
			return;
		}
		if (state == "Visit") {
			Seek ();
			return;
		}
	}
	
	void Wander(){
		
		// Calculate the circle center
		Vector3 circleCenter;
		circleCenter = velocity;
		Vector3.Normalize(circleCenter);
		Vector3.Scale(circleCenter, new Vector3(5,0,5));
		// Calculate the displacement force
		Vector3 displacement;
		displacement = transform.forward;
		
		// Randomly change the vector direction
		// by making it change its current angle
		displacement = SetAngle(displacement, wanderAngle);
		
		// Change wanderAngle just a bit, so it
		// won't have the same value in the
		// next game frame.
		//
		// Finally calculate and return the wander force
		Vector3 wanderForce;
		wanderAngle += UnityEngine.Random.Range(0f, 1f) * 1f - .5f * .5f;
		wanderForce = circleCenter + displacement;
		/*
		Vector3 ahead = transform.position + velocity.normalized * sightRange;
		Vector3 ahead2 = transform.position + velocity.normalized * sightRange * 0.5f;
		Vector3 avoidance = new Vector3(0,0,0);
		RaycastHit hit;
		if (Physics.Raycast (transform.position, velocity, out hit, 50f)) {
			avoidance.x = ahead.x - hit.transform.position.x;
			avoidance.z = ahead.z - hit.transform.position.z;
			avoidance = avoidance.normalized * 10f;
			Debug.DrawRay(transform.position, avoidance);
		}
		wanderForce += avoidance;
		*/
		Step (wanderForce);
	}
	
	Vector3 SetAngle(Vector3 vector, float value){
		float len = vector.magnitude;
		vector.x = Mathf.Cos(value) * len;
		vector.z = Mathf.Sin(value) * len;
		return vector;
	}
	
	void AttackMove(){
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-flee-and-arrival--gamedev-1303
		Vector3 desiredVelocity = (seekingPoint - transform.position).normalized;
		float distance = Vector3.Distance (seekingPoint, transform.position);
		if (distance > slowingRadius) {
			desiredVelocity = new Vector3 (desiredVelocity.x * maxVelocity, 0f, desiredVelocity.z * maxVelocity);
		} 
		if (distance <= slowingRadius) {
			desiredVelocity = new Vector3(0,0,0);
		}
		Step (desiredVelocity);
		
		Aim ();
	}
	
	void Flee(){
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-flee-and-arrival--gamedev-1303
		Vector3 desiredVelocity = (transform.position-seekingPoint).normalized;
		desiredVelocity = new Vector3 (desiredVelocity.x*maxVelocity, 0f, desiredVelocity.z*maxVelocity);
		Step (desiredVelocity);
	}
	
	void Seek(){
		if (state.Equals ("Follow")) {
			seekingPoint = seekingObject.position - seekingObject.GetComponent<TankAI>().velocity.normalized * 5 * (Array.IndexOf(seekingObject.GetComponent<TankAI>().followers, transform)+ 1);
		}
		
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-seek--gamedev-849
		Vector3 desiredVelocity = (seekingPoint - transform.position).normalized;
		desiredVelocity = new Vector3 (desiredVelocity.x*maxVelocity, 0f, desiredVelocity.z*maxVelocity);
		Step (desiredVelocity);
	}
	
	void Step(Vector3 desiredVelocity){
		
		Vector3 steering = desiredVelocity - velocity;
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		if (velocity.magnitude >= .002f || velocity.magnitude <= -.002f) {
			Quaternion direction = Quaternion.LookRotation(velocity);
			transform.rotation = direction;
		}
		
		transform.Translate(velocity.magnitude*Vector3.forward);
		if (avoiding) {
			if(side && transform.position.x < obstacle.position.x)
				avoiding = false;
			if(!side && transform.position.x < obstacle.position.x)
				avoiding = false;
		}
	}
	
	void Aim(){
		
		turret.parent.LookAt (seekingObject);
		if(ready)
			Shoot ();
	}
	
	void Shoot(){
		//choose random point around enemy tank
		//check if tank is on the point
		//damage tank if hit
		ready = false;
		PlayShootSound ();
		Transform clone = (Transform)Instantiate(projectile, turret.GetChild(0).position, turret.parent.rotation);
		Invoke ("Reload", 5f);
	}
	
	void Reload(){
		ready = true;
	}
	
	public void gotHit(){
		tankHealth = tankHealth - 1;
		if (tankHealth == 0) {
			Transform corpse = (Transform)Instantiate(deadTank, transform.position, transform.rotation);
			Destroy(gameObject);
		}
		if (!state.Equals ("Attack")) {
			state = "Attack";
			Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, sightRange);
			foreach(Collider tank in InRangeTanks){
				if(tank.tag != transform.tag && tank.tag != "Untagged"){
					seekingObject = tank.transform;
				}
			}
		}
	}
	
	public Vector3 moveAround(Vector3 desiredVelocity){
		
		if (obstacle.name == "SideWall" || obstacle.name == "FrontWall" || obstacle.name == "BackWall") {
			if(obstacle.name == "SideWall"){
				desiredVelocity.z = -1f*desiredVelocity.z;
				velocity.z = -1f*velocity.z;
			}
			else{
				desiredVelocity.x = -1f*desiredVelocity.x;
				velocity.x = -1f*velocity.x;
			}
			return desiredVelocity;
		}
		
		float obstacleLength = obstacle.childCount*4;
		if (obstacle.name == "Pool") {
			obstacleLength = obstacleLength/3;
		}
		if(obstacle.name.EndsWith("L")){
			obstacleLength = 11;
		}
		if(transform.position.x < obstacle.position.x){
			side = true;
			if(transform.position.z < obstacle.position.z + obstacleLength/2f + 4f){
				velocity = new Vector3(0,0,1) * maxVelocity;
				desiredVelocity = new Vector3(0,0,1) * maxVelocity;
			}
			else{
				desiredVelocity = Vector3.right * maxVelocity;
				velocity = Vector3.right * maxVelocity;
			}
		}
		if(transform.position.x > obstacle.position.x){
			side = false;
			if(transform.position.z < obstacle.position.z + obstacleLength/2f + 4f){
				velocity = new Vector3(0,0,1) * maxVelocity;
				desiredVelocity = new Vector3(0,0,1) * maxVelocity;
			}
			else{
				velocity = Vector3.left * maxVelocity;
				desiredVelocity = Vector3.left * maxVelocity;
			}
		}
		return desiredVelocity;
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
		foreach (Collider tank in InRangeTanks) {
				if ((this.tag=="GoodGuy" && tank.tag=="BadGuy") ||(this.tag=="BadGuy" && tank.tag=="GoodGuy")){
					//idea: instantiate exclamationMark on those enemy tank to show their alert
					if (exclamation==null &&state!="Attatck"){
						exclamation=(GameObject)Instantiate (exclamationMark, tank.transform.position + 5*Vector3.up, Quaternion.identity);
						exclamation.transform.parent=transform;
				}
			}
				// i am not sure about this part
				if (state == "Idle") {
					state = "Follow";
					seekingPoint = transform.position;
				}
			}
		}
	}