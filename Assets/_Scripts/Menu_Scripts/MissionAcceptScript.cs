using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MissionAcceptScript : MonoBehaviour
{
	// Load Multiplayer Gameplay Scene
	public void loadLevel()
	{
		SceneManager.LoadScene("Shooter_Online_Scene", LoadSceneMode.Single);
	}

	// Load Menu Scene
	public void loadMenu()
	{
		SceneManager.LoadScene ("Menu_Scene", LoadSceneMode.Single);
	}

	// Exit Application
	public void Exit()
	{
		Application.Quit ();
	}
}
