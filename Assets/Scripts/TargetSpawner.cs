using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.MorganHouston.Imprecision
{

	public class TargetSpawner : MonoBehaviour
	{

		public GameObject[] objects;

		public TextMeshProUGUI targetsLbl;

		private int maxSpawnCount = 10;
		private int spawnCount = 0;
		private int currentLevel = 0;

		private bool isGameOver;

		[Range(-20, 0f)]
		public float minX;
		[Range(3, 5)]
		public float minY;
		[Range(-20, 0f)]
		public float minZ;

		[Range(1, 20f)]
		public float maxX;
		[Range(6, 10)]
		public float maxY;
		[Range(1, 20f)]
		public float maxZ;

		public Transform player;

        private void Awake()
        {
			currentLevel = GameManager.Instance.LevelSelected;

		}


        void Start()
		{
            maxSpawnCount += (int)(currentLevel * 1.25f);
		}

		void Update()
		{
			CheckForTargets();
			UpdateTargetsLabel();
			CheckForGameOver();
		}

		private void UpdateTargetsLabel()
		{
			var prev = targetsLbl.text;
			string targetsLeft = $"Targets: {maxSpawnCount - spawnCount}";
			if (prev == targetsLeft)
				return;
			else
				targetsLbl.text = targetsLeft;
		}

		private void CheckForTargets()
		{
			if (GameObject.FindGameObjectsWithTag("target").Length <= 0 && spawnCount < maxSpawnCount)
			{
				Spawn();
				spawnCount++;

			}
		}

		void Spawn()
		{
			var x = Random.Range(minX, maxX);
			var y = Random.Range(minY, maxY);
			var z = Random.Range(minZ, maxZ);
			GameObject target = Instantiate(objects[Random.Range(0, objects.Length)], new Vector3(x, y, z), Quaternion.identity);
			target.transform.GetChild(0).transform.LookAt(player);
		}

		void CheckForGameOver()
        {
			if(spawnCount == maxSpawnCount && isGameOver == false && GameObject.FindGameObjectsWithTag("target").Length <= 0)
            {
				isGameOver = true;
				GameManager.Instance.GameOver();
            }
        }
	}

}