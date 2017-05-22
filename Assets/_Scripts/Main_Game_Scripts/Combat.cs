using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Combat : NetworkBehaviour 
{
	// 90 Second Timer | Set In Inspector
	public float timeLeft;

	// Number of Lives | Health Per Life
	// Player Three Lives
	public int numberOfLives = 3;
	public int currentHealth = maxHealth;
	public const int maxHealth = 100;

	// Prefabs and Scripts
	public GameObject explosionPrefab;
	private PlayerMove playerMoveScript;

	// Get Spawnpoints | Set Transform
	private Transform myTransform;
	private NetworkStartPosition[] spawnpoints;

	void Start () 
	{
		// Setup Generic Objects
		myTransform = transform;
		playerMoveScript = GetComponent<PlayerMove> ();
		spawnpoints = FindObjectsOfType<NetworkStartPosition> ();
	}

	void Update()
	{
		// Run Timer On Local Player | If Player Stays Alive For Longer Than 90 Seconds - Show Win Screen
		timeLeft -= Time.deltaTime;
		timeLeft = (float) System.Math.Round (timeLeft, 2);
		if(timeLeft < 0 && numberOfLives > 0)
		{
			RpcShowWinForClient();
		} 
	}

	// Method Called From Other Scripts | Inflict Damage On Local Player | Kill Player or Start Respawn Process
	public void TakeDamage(int amount)
	{
		if (!isServer) 
			return;

		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			if (numberOfLives != 1) {
				numberOfLives--;
				currentHealth = maxHealth;
				RpcStartRespawnProcess ();
			} else {
				RpcKillClient ();
			}
		}
	}

	[ClientRpc]
	void RpcStartRespawnProcess()
	{
		// Freeze Player
		playerMoveScript.playerIsFrozen = true;

		// Get Local Player
		if (isLocalPlayer) {
			// Deactivate All Of Ship Except Camera
			// Tells Server Player Object Has Changes
			foreach(Transform child in transform)
			{
				if (child.tag != "MainCamera") {
					child.gameObject.SetActive(false);
				}
			} 
		}

		// Trigger Explosion On Dead Player
		CmdPlayerExplode ();

		// Wait For Player Respawn
		StartCoroutine (HoldForRespawn ());
	}

	// Triggers Explosion On Dead Player
	[Command]
	void CmdPlayerExplode()
	{
		var explosion = (GameObject)Instantiate(
			explosionPrefab,
			myTransform.position, 
			Quaternion.identity);

		NetworkServer.Spawn(explosion);
		Destroy(explosion, 2.0f); 
	}

	// Holds For 1 Seconds | Freezes Movement Of Camera | Reactivates Player After 1 Second
	IEnumerator HoldForRespawn()
	{
		// Freeze Current Position
		playerMoveScript.playerIsFrozen = true;

		// Wait For Respawns
		yield return new WaitForSeconds (1);

		// Reactivate After Waiting
		if (isLocalPlayer) {
			foreach (Transform child in transform) {
				child.gameObject.SetActive (true);
			} 
		}

		// Respawn Player
		RpcRespawn ();
	}

	// Respawns Player | Resets All Forces/Velocity | Chooses Spawnpoint | Unfreezes Player
	[ClientRpc]
	void RpcRespawn() {
		// Get Local Player
		if (isLocalPlayer) {
			
			// Reset All Values
			gameObject.GetComponent<Rigidbody> ().velocity = new Vector3 (0f, 0f, 0f);
			gameObject.transform.rotation = new Quaternion (0, 0, 0, 0);
			gameObject.transform.Translate (0.0f, 0.0f, 0.0f);

			// Get Spawnpoints & Randomly Pick One
			Vector3 spawnPoint = Vector3.zero;
			int i = 0;
			if (spawnpoints.Length > 0) {
				i = Random.Range (0, spawnpoints.Length);
				spawnPoint = spawnpoints [i].transform.position;
			}

			// Spawn Player & Unfreeze
			gameObject.transform.position = spawnPoint;
			playerMoveScript.playerIsFrozen = false;
		}
	}

	[ClientRpc]
	void RpcKillClient()
	{
		// Gets Local Player | Tells Server Client Is Dead | Shows Player Dead Scene 
		if (isLocalPlayer) 
		{
			foreach (Transform child in transform) {
				child.gameObject.SetActive (false);
			}

			SceneManager.LoadScene("Dead_Player_Scene", LoadSceneMode.Single);
		}
	}

	[ClientRpc]
	void RpcShowWinForClient()
	{
		// Gets Local Player | Tells Server Client Is Dead | Shows Player Win Scene 
		if (isLocalPlayer) 
		{
			foreach (Transform child in transform) {
				child.gameObject.SetActive (false);
			}

			SceneManager.LoadScene("Player_Win_Scene", LoadSceneMode.Single);
		}
	}
}