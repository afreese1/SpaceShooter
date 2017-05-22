using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
	// Setup Public GameObjects
	public GameObject explosionPrefab;

	// Setup Transform
	private Transform myTransform;

	// Set Transform For Efficiency
	void Start() {
		myTransform = transform;
	}

	void OnTriggerEnter(Collider col)
	{
		// If Bullet Collides With Obstacle | Destroy Bullet | Show Explosion
		if (col.tag == "Obstacle" || col.tag == "SpeedupCube") {
			Destroy (gameObject);
			CmdTriggerExplosion ();
		}

		// If Bullet Hits Another Player | Destroy Bullet | Hurt Player | Show Explosion
		if (col.tag == "Player") {
			var hit = col.gameObject;
			var hitPlayer = hit.GetComponent<PlayerMove> ();
			if (hitPlayer != null) {
				Destroy (gameObject);

				Combat combat = hit.GetComponent<Combat> ();
				combat.TakeDamage (50);
			}
		}
	}

	// Secondary Check If Bullet Leaves An Obstacle Collider | Destroy Bullet | Show Explosion
	void onTriggerExit(Collider col)
	{
		if (col.tag == "Obstacle") {
			Destroy (gameObject);
			CmdTriggerExplosion ();
		}
	}

	// Send Command To Server To Spawn Explosion At The Bullet's Current Position
	[Command]
	void CmdTriggerExplosion()
	{
		var explosion = (GameObject)Instantiate(
			explosionPrefab,
			myTransform.position, 
			Quaternion.identity);

		NetworkServer.Spawn(explosion);
		Destroy(explosion, 2.0f); 
	}
}