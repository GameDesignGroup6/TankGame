using UnityEngine;
using System.Collections;
using System.Linq;

public class TankAI : MonoBehaviour {
	
	/*
	 * idle
	 * attack
	 * flee
	 * follow
	 */
	public string state;
	public int tankHealth;
	public int attackDamage = 1;
	
	public Collider[] enemyTanks;
	public Collider[] allyTanks;	
	public Vector3 seekingPoint;
	
	public int sightRange = 30;
	public int coneBaseRadius = 30;
	
	public int mass;
	public Vector3 velocity;
	public float maxVelocity;
	public float acceleration;
	public float slowingRadius;
	public float haltingRadius;
	private float wanderAngle;
	
	void Start () {
		state = "Idle";
		velocity = Vector3.zero;
		wanderAngle = Vector3.Angle (velocity, new Vector3(0,0,1));
	}
	
	void Update () {
		Look ();
		Move ();
	}
	
	void Look(){
		//check radius
		Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, sightRange);
		//var Enemys = from tank in InRangeTanks where tank.tag != transform.tag select tank;
		//var Allys = from tank in InRangeTanks where tank.tag == transform.tag select tank;
		ArrayList Enemys = new ArrayList();//Collider[InRangeTanks.Length];
		ArrayList Allys = new ArrayList();//[InRangeTanks.Length];
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
		//raycast
		if(Enemys.Count != 0 && (!state.Equals("Flee") || !state.Equals ("Attack"))){
			state = "Attack";
			Collider enemy = (Collider)Enemys[0];
			seekingPoint = enemy.transform.position;
		}
		if(Allys.Count != 0 && state.Equals("Idle")){
			state = "Follow";
			Collider ally = (Collider)Allys[0];
			seekingPoint = ally.transform.position;
		}
		if(seekingPoint == transform.position && state == "Follow")
			state = "Idle";
		if(state.Equals("Attack") && Enemys.Count == 0){
			state = "Follow";
		}
		//else{
		//	possibly state to "Idle" if in state "Follow"
	}
	void Move(){
		
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
		wanderAngle += Random.Range(0f, 1f) * 1f - .5f * .5f;
		wanderForce = circleCenter + displacement;
		
		wanderForce = Vector3.ClampMagnitude(wanderForce, acceleration);
		wanderForce = wanderForce / mass;
		velocity = Vector3.ClampMagnitude((velocity + wanderForce), maxVelocity);
		if (velocity.magnitude >= .002f || velocity.magnitude <= -.002f) {
			Quaternion direction = Quaternion.LookRotation(velocity);
			transform.rotation = direction;
		}
		transform.Translate (Vector3.forward*velocity.magnitude);
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
		Vector3 steering = (desiredVelocity - velocity);
		
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		if (velocity.magnitude >= .002f || velocity.magnitude <= -.002f) {
			Quaternion direction = Quaternion.LookRotation(velocity);
			transform.rotation = direction;
		}
		transform.Translate(velocity.magnitude*Vector3.forward);

		Aim ();
	}
	
	void Flee(){
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-flee-and-arrival--gamedev-1303
		Vector3 desiredVelocity = (transform.position-seekingPoint).normalized;
		desiredVelocity = new Vector3 (desiredVelocity.x*maxVelocity, 0f, desiredVelocity.z*maxVelocity);
		Vector3 steering = desiredVelocity - velocity;
		
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		if (velocity.magnitude >= .2f || velocity.magnitude <= -.2f) {
			Quaternion direction = Quaternion.LookRotation(velocity);
			transform.rotation = direction;
		}
		transform.Translate(velocity.magnitude*Vector3.forward);
	}
	
	void Seek(){
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-seek--gamedev-849
		Vector3 desiredVelocity = (seekingPoint - transform.position).normalized;
		desiredVelocity = new Vector3 (desiredVelocity.x*maxVelocity, 0f, desiredVelocity.z*maxVelocity);
		Vector3 steering = desiredVelocity - velocity;
		
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		if (velocity.magnitude >= .002f || velocity.magnitude <= -.002f) {
			Quaternion direction = Quaternion.LookRotation(velocity);
			transform.rotation = direction;
		}
		transform.Translate(velocity.magnitude*Vector3.forward);
	}
	
	void Aim(){
		/*
		 * move barrel to point to enemy/ahead of enemy
		 * if (barrel direction correct){
		 * 		Shoot();
		 */
	}
	
	void Shoot(){
		//choose random point around enemy tank
		//check if tank is on the point
		//damage tank if hit
	}
}
