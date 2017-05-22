using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMove : NetworkBehaviour
{
	// Declare Yaw/Speed
	private float yaw;
	private float speed;

	public bool playerIsSpedUp;

	// Declare Public Variables | AudioSources
	public bool playerIsFrozen;
	public GameObject bulletPrefab;
	private AudioSource bulletShotSound;

	// Declare Scripts/MyTransform
	private Combat combatScript;
	private Transform myTransform;

	// Used For Time Remaining
	private Text timeRemainingText;
	private GameObject timeRemainingTextObject;

	// Use For Player Lives
	private Text text;
	private GameObject textObject;

	void Start()
	{
		// Setup Generic Variables
		speed = .2f;
		playerIsFrozen = false;
		playerIsSpedUp = false;
		myTransform = transform;
		combatScript = GetComponent<Combat> ();
		bulletShotSound = GetComponent<AudioSource>();

		if (isLocalPlayer) {
			// Set Local Player's Camera To Follow
			Camera.main.transform.position = (new Vector3(myTransform.position.x, myTransform.position.y, myTransform.position.z) - (myTransform.forward * -1) * 2.5f + myTransform.up * 2);
			Camera.main.transform.LookAt (new Vector3(myTransform.position.x, myTransform.position.y + 1, myTransform.position.z));
			Camera.main.transform.parent = myTransform;

			// Setup Text For Player's Life
			textObject = GameObject.Find ("PlayerLives");
			text = textObject.GetComponent<Text>();
		
			// Setup Text For Player's Time Remaining
			timeRemainingTextObject = GameObject.Find ("TimeRemaining");
			timeRemainingText = timeRemainingTextObject.GetComponent<Text>();
		}
	}

	// Send Command To Server When Player Fires A Bullet
	[Command]
	void CmdFire()
	{
		// Play Bullet Sound
		bulletShotSound.Play();

		var bullet = (GameObject)Instantiate(
			bulletPrefab,
			myTransform.position - myTransform.forward * 1.8f, 
			Quaternion.identity);

		bullet.GetComponent<Rigidbody>().velocity = - myTransform.forward * 75;
		NetworkServer.Spawn(bullet);
		Destroy(bullet, 2.0f);
	}

	void Update()
	{
		// Set Time Remaining For All Players
		timeRemainingText.text = "Time Remaining: " + combatScript.timeLeft;

		if (!isLocalPlayer)
			return;

		// If The Player Is Frozen, Stop Movement
		else if (playerIsFrozen == false) {

			yaw = 0.0f;
			if (Input.GetAxis ("Yaw") > 0.1 || Input.GetAxis ("Yaw") < -0.1) {
				yaw += Input.GetAxis ("Yaw") * 2.0f;
			}
				
			// If Player Grabbed Speedup Cube
			if (playerIsSpedUp == false) {
				myTransform.Rotate (-Input.GetAxis ("Pitch"), Input.GetAxis ("Roll") + yaw / 2.0f, yaw);
				myTransform.Translate (0.0f, 0.0f, speed * -1);
			} 
			else {
				myTransform.Rotate (-Input.GetAxis ("Pitch") * 2, (Input.GetAxis ("Roll") * 2) + yaw / 2.0f, yaw);
				myTransform.Translate (0.0f, 0.0f, (speed * 2) * -1);
			}

			if (Input.GetKeyDown (KeyCode.Space)) {
				// Command function is called from the client, but invoked on the server
				CmdFire ();
			}
		}

		// Constantly Update Player Lives
		text.text = "Lives: " + combatScript.numberOfLives;
	}

	void OnTriggerEnter(Collider col)
	{
		// Player Collides With Obstacle, Kill Them
		if (col.tag == "Obstacle") {
			combatScript.TakeDamage (100);
		}

		// Temp Speed Up Player
		if (col.tag == "SpeedupCube") {

			// Set Player To Speed Up
			playerIsSpedUp = true;

			// Play Speedup Sound
			var hitObject = col.gameObject;
			var audioSource = hitObject.GetComponent<AudioSource> ();
			audioSource.Play ();

			// Start Cooldown 
			StartCoroutine(startPlayerSpeedUpCooldown ());
		}
	}

	IEnumerator startPlayerSpeedUpCooldown() {
		yield return new WaitForSeconds (5);
		playerIsSpedUp = false;
	}
}