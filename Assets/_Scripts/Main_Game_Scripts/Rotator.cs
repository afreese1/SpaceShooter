using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
	private float speed = 10f;

	private int selectedIndex = 0;
	private Transform myTransform;
	private Vector3 selectionRotationVector;
	private int [] directions = new int[4] {0, 1, 2, 3};

	void Start ()
	{
		myTransform = transform;
		selectedIndex = Random.Range (0, directions.Length);

		switch(selectedIndex)       
		{         
		case 0:
			selectionRotationVector = Vector3.left;
			break;                  
		case 1:  
			selectionRotationVector = Vector3.right;
			break;           
		case 2:       
			selectionRotationVector = Vector3.up;
			break; 
		case 3:  
			selectionRotationVector = Vector3.down;
			break;
		default:
			print("Reached Default");
			break;      
		}
	}

	void Update ()
	{
		myTransform.Rotate(selectionRotationVector, speed * Time.deltaTime);
	}
}