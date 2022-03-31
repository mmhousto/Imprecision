using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
	
	public GameObject[] objects;

	private int maxSpawnCount = 10;
	private int spawnCount = 0;
	public Transform player;


	void Start()
	{
		
	}

	void Update()
	{
		if (GameObject.FindGameObjectsWithTag("target").Length <= 0 && spawnCount < maxSpawnCount)
		{
			Spawn();
			spawnCount++;

		}
	}

	void Spawn()
	{
		var x = Random.Range(18.00f, 28.15f);
		var y = Random.Range(4, 7);
		var z = Random.Range(24.00f, 35.00f);
		GameObject target = Instantiate(objects[Random.Range(0, objects.Length)], new Vector3(x, y, z), Quaternion.identity);
		target.transform.GetChild(0).transform.LookAt(player);
	}
}