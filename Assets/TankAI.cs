using UnityEngine;
using System.Collections;
using System.Linq;

public class TankAI : MonoBehaviour {

	/*
	 * idle
	 * attack
	 * flee
	 * caution
	 * follow
	 */
	public string state;
	public int tankHealth;
	public int attackDamage = 1;

	private Collider[] enemyTanks;
	private Collider[] allyTanks;
	public Vector3 seekingPoint;

	public int sightRange = 15;
	public int coneBaseRadius = 30;
	
	void Start () {
		state = "Idle";
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
		Collider[] Enemys = new Collider[InRangeTanks.Length];
		Collider[] Allys = new Collider[InRangeTanks.Length];
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
			if(tank.tag == transform.tag){
				int allyLength = Allys.Length;
				Allys[allyLength] = tank;
			}
			else{
				int enemyLength = Enemys.Length;
				Enemys[enemyLength] = tank;
			}
		}
		//raycast
		if(Enemys.Length != 0 && !state.Equals("Flee")){
			state = "Attack";
		}
		if(Allys.Length != 0 && state.Equals("Idle")){
			state = "Follow";
		//	seekingPoint = Ally position
		}
		//if(sound origin visible && state == "Caution)
		//	state = "Idle";
		if(state.Equals("Attack") && Enemys.Length == 0){
			state = "Follow";
		//	seekingPoint = last point of enemy;
		}
		//else{
		//	possibly state to "Idle" if in state "Follow"
	}
	void Move(){
		/*
		 * if (state == 0){
		 * 		Wander();
		 * if (state == 1){
		 * 		AttackMove();
		 * if (state == 2){
		 * 		Flee();
		 * if (state == 4 or 5){
		 * 		Seek();
		 */
	}

	void Wander(){
	}

	void AttackMove(){

		Aim ();
	}

	void Flee(){
	}

	void Seek(){
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

