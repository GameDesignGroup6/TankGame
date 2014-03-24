using UnityEngine;
using System.Collections;

public class TankAI : MonoBehaviour {

	/*
	 * 0 = idle
	 * 1 = attack
	 * 2 = flee
	 * 4 = caution
	 * 5 = banded
	 */
	public string state;
	public int tankHealth;
	public int attackDamage;

	public int rotateSpeed;
	private Transform turret;
	public Transform target;
	//public Transform bullet;
	public Rigidbody projectile;
	public float projectileVelocity;

	public ArrayList Enemys;
	public ArrayList Allys;
	public Vector3 seekingPoint;
	
	public int sightRange = 15;
	public int coneBaseRadius = 30;
	
	public int mass;
	public Vector3 velocity;
	public float maxVelocity;
	public float acceleration;
	public float slowingRadius;
	public float haltingRadius;
	private float wanderAngle;

	private int testBullet;// a test variable to limit the number of bullet instances.


	// Use this for initialization
	void Start () {
		//tankHealth=10;
		attackDamage=1;
		rotateSpeed=60;
		state = "Idle";
		velocity = Vector3.zero;
		wanderAngle = Vector3.Angle (velocity, new Vector3(0,1,0));
		turret= transform.GetChild (0).GetChild(0);

		testBullet = 5;
		projectileVelocity = 20;

	}
	
	// Update is called once per frame
	void Update () {
		Look ();
		Move ();
		Aim ();


	}

	void Look(){
		//check radius
		Collider[] InRangeTanks = Physics.OverlapSphere (transform.position, sightRange);
		//var Enemys = from tank in InRangeTanks where tank.tag != transform.tag select tank;
		//var Allys = from tank in InRangeTanks where tank.tag == transform.tag select tank;
		Enemys = new ArrayList();//Collider[InRangeTanks.Length];
		Allys = new ArrayList();//[InRangeTanks.Length];
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
		if(Enemys.Count != 0 && !state.Equals("Flee")){
			state = "Attack";
			Collider enemy = (Collider)Enemys[0];
			seekingPoint = enemy.transform.position;
			target= enemy.transform; //for aiming

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
		
		if (state == "idle")
			Wander();
		if (state == "Attack")
			AttackMove();
		if (state == "Flee")
			Flee();
		if (state == "Follow")
			Seek();
	}

	void Wander(){
		
		// Calculate the circle center
		Vector3 circleCenter;
		circleCenter = velocity;
		Vector3.Normalize(circleCenter);
		Vector3.Scale(circleCenter, new Vector3(5,0,5));
		// Calculate the displacement force
		Vector3 displacement;
		displacement = new Vector3(0, 0, 1);
		
		// Randomly change the vector direction
		// by making it change its current angle
		displacement = SetAngle(displacement, wanderAngle);
		
		// Change wanderAngle just a bit, so it
		// won't have the same value in the
		// next game frame.
		//
		// Finally calculate and return the wander force
		Vector3 wanderForce;
		wanderAngle += Random.Range(0f, 1f) * .05f - .05f * .5f;
		wanderForce = circleCenter + displacement;
		
		wanderForce = Vector3.	ClampMagnitude(wanderForce, acceleration);
		wanderForce = wanderForce / mass;
		velocity = Vector3.ClampMagnitude((velocity + wanderForce), maxVelocity);
		transform.Translate(velocity);
		if (velocity.magnitude >= .2f || velocity.magnitude <= -.2f) {
			transform.forward = velocity;
		}
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
		if (distance <= slowingRadius && distance > haltingRadius) {
			Vector3.Scale(desiredVelocity, new Vector3(maxVelocity * distance/ slowingRadius, 0f, maxVelocity * distance/ slowingRadius));
		}
		if (distance <= haltingRadius) {
			desiredVelocity = new Vector3(0,0,0);
		}
		Vector3 steering = desiredVelocity - velocity;
		
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		transform.Translate(velocity);
		if (velocity.magnitude >= .2f || velocity.magnitude <= -.2f) {
			transform.forward = velocity;
		}
		
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
		transform.Translate(velocity);
		if (velocity.magnitude >= .2f || velocity.magnitude <= -.2f) {
			transform.forward = velocity;
		}
	}
	
	void Seek(){
		//courtesy of http://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-seek--gamedev-849
		Vector3 desiredVelocity = (seekingPoint - transform.position).normalized;
		desiredVelocity = new Vector3 (desiredVelocity.x*maxVelocity, 0f, desiredVelocity.z*maxVelocity);
		Vector3 steering = desiredVelocity - velocity;
		
		steering = Vector3.	ClampMagnitude(steering, acceleration);
		steering = steering / mass;
		
		velocity = Vector3.ClampMagnitude((velocity + steering), maxVelocity);
		transform.Translate(velocity);
		if (velocity.magnitude >= .2f || velocity.magnitude <= -.2f) {
			transform.forward = velocity;
		}
	}

	void Aim(){
		/*
		 * move barrel to point to enemy/ahead of enemy
		 * if (barrel direction correct){
		 * 		Shoot();
		 */

		turret.LookAt (target);
		//target.Translate(Vector3.right); //just testing lookat
		Shoot ();

	}

	/**
	 * instantiates a TankProjectile object that goes towards a target
	 */
	void Shoot(){
		if (testBullet>0){
		Rigidbody clone = Instantiate(projectile, turret.GetChild (1).position,turret.rotation) as Rigidbody;
			clone.velocity = turret.forward*projectileVelocity;
			testBullet--;
		}
	}

	/**
	 * a public method for decrementing tankHealth
	 * This is so that the bullet can decrease the tank's health upon impact
	 */
	public void gotHit(){
		tankHealth= tankHealth-1;
	}
}

