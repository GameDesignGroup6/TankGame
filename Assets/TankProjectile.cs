using UnityEngine;
using System.Collections;

public class TankProjectile : MonoBehaviour {

	private float velocity;
	private Vector3 direction;
	// Use this for initialization
	void Start () {

		//Destroy (gameObject,1.5f);
	}
	
	// Update is called once per frame
	void Update () {
		//TankProjectile clone;
			//clone = Instantiate(bullet, turret.position,turret.rotation) as TankProjectile;
			//transform.position += direction*velocity;
		//if ((rigidbody.position-target.position).sqrMagnitude< target.position.sqrMagnitude*.01)
		//rigidbody.AddForce(direction*velocity);
			
	}
	void OnCollisionEnter(Collision hit){
		TankAI target;
		GameObject hitObject = hit.gameObject;
		//if (hit.gameObject.tag != "Untagged"){
			//TankAI target = hit.gameObject.GetComponent<TankAI>();
		while (hit.gameObject.CompareTag("TankParts")){
			hitObject = hitObject.transform.parent.gameObject;
		}
		target = hitObject.GetComponent<TankAI>();
			if (target==null){
				Debug.Log ("didn't hit a tank");
			}
			else{
				target.gotHit();
				Debug.Log ("hit a tank tagged and " + target.tankHealth);
			}
		//}
		Destroy (gameObject);
		Debug.Log (hit.gameObject.tag + "and" + hit.gameObject.name + "and" +  hit.gameObject.transform.position);
	}
}
