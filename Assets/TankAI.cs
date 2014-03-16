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
	public int state;
	public int tankHealth;
	public int attackDamage;
	public int rotateSpeed;
	public Transform turret;
	public Transform target;
	public Transform bullet;

	// Use this for initialization
	void Start () {
		state = 0;
		tankHealth=10;
		attackDamage=1;
		rotateSpeed=60;
	}
	
	// Update is called once per frame
	void Update () {
		Look ();
		Move ();
		Aim ();
	}

	void Look(){
		//check radius
		//check cone
		//raycast
		//if(enemy visisble && state != 2){
		//	state = 1;}
		//if(ally visible && state == 0){
		//	state = 5;}
		//if(sound origin visible && state == 4)
		//	state = 0;
		//else{
		//	possibly state to 0 if in state 5
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
		turret.LookAt (target);
		//target.Translate(Vector3.right); //just testing lookat
		Shoot ();

	}

	void Shoot(){
		//choose random point around enemy tank
		//check if tank is on the point
		//damage tank if hit
		Rigidbody clone;
		clone = Instantiate(bullet, turret.GetChild (1).position,turret.rotation) as Rigidbody;
		clone.velocity=turret.GetChild (1).forward*120;

		}
}

