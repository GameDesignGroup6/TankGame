using UnityEngine;
using System.Collections;

public class TankProjectile : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy (gameObject, 5f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * .8f);
	}

	void OnTriggerStay(Collider hit){
		if(hit.transform.name == "Wall"){
			Destroy (gameObject);
		}
		TankAI target = hit.transform.root.GetComponent<TankAI>();
		if (target==null){
			return;
		}
		else{
			target.gotHit();
		}
		Destroy (gameObject);
	}
}