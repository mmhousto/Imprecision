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

		[Range(-30, -6f)]
		public float minX;
		[Range(3, 6)]
		public float minY;
		[Range(-30, -6f)]
		public float minZ;

		[Range(6, 30f)]
		public float maxX;
		[Range(6, 10)]
		public float maxY;
		[Range(6, 30f)]
		public float maxZ;

		public Transform player;

        private void Awake()
        {
			if (GameManager.tutorialFinished)
			{
				currentLevel = GameManager.Instance.LevelSelected;
				maxSpawnCount += (int)(currentLevel * 1.25f);
				minX = -1f * ((currentLevel * 1.25f) / 2) + 8;
				maxX = 1f * ((currentLevel * 1.25f) / 2) + 8;
				minZ = -1f * ((currentLevel * 1.25f) / 2) + 8;
				maxZ = 1f * ((currentLevel * 1.25f) / 2) + 8;
				minY = currentLevel >= 15 ? 6 : Random.Range(3, 6);
				maxY = currentLevel >= 15 ? 10 : Random.Range(6, 10);
			}
			else
			{
                currentLevel = 1;
                maxSpawnCount = 999;
                minX = -1f * ((currentLevel * 1.25f) / 2) + 6;
                maxX = 1f * ((currentLevel * 1.25f) / 2) + 6;
                minZ = -1f * ((currentLevel * 1.25f) / 2) + 6;
                maxZ = 1f * ((currentLevel * 1.25f) / 2) + 6;
                minY = 3;
				maxY = 6;
            }
		}

		void Update()
		{
			if (GameManager.tutorialFinished)
			{
				CheckForTargets();
				UpdateTargetsLabel();
				CheckForGameOver();
			}
			else
			{
                CheckForTargets();
                UpdateTargetsLabel();
            }
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