
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	public static Spawner Instance { get; private set; }

	private class DifficultyPreset
	{
		public int timeToExit;
		public int maxAlive;
		public float spawnTime;
	}

	private class SpawnData
	{
		public GameObject prefab;
		public DifficultyPreset[] diffPresets;
		public int diffIndex = 0;
		public int nAlive = 0;
		public float spawnTimer = 0f;
	}

	private Dictionary<PickupType, SpawnData> m_pickups = new Dictionary<PickupType, SpawnData>();
	private Dictionary<EnemyType, SpawnData> m_enemies = new Dictionary<EnemyType, SpawnData>();

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
	}

	private void Start()
	{
		m_pickups = new Dictionary<PickupType, SpawnData>()
		{
			{ PickupType.Score, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/ScorePickupPrefab"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 5, maxAlive = 1, spawnTime = 10f },
					new() { timeToExit = 15, maxAlive = 2, spawnTime = 10f },
					new() { timeToExit = 0, maxAlive = 3, spawnTime = 10f }
				}
			} } ,
			{ PickupType.Powerup, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/PowerupPickupPrefab"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 0, maxAlive = 1, spawnTime = 15 },
				}
			} },
			{ PickupType.Charge, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/ChargePickupPrefab"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 10, maxAlive = 1, spawnTime = 5f },
					new() { timeToExit = 0, maxAlive = 2, spawnTime = 5f },
				}
			} },
			{ PickupType.Bomb, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/BombPickupPrefab"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 15, maxAlive = 0, spawnTime = 30f },
					new() { timeToExit = 0, maxAlive = 1, spawnTime = 30f }
				}
			} }
		};
		m_enemies = new Dictionary<EnemyType, SpawnData>()
		{
			{ EnemyType.Chaser, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/EnemyChaser"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 5, maxAlive = 1, spawnTime = 5f },
					new() { timeToExit = 10, maxAlive = 1, spawnTime = 15f },
					new() { timeToExit = 15, maxAlive = 2, spawnTime = 13f },
					new() { timeToExit = 0, maxAlive = 3, spawnTime = 11f },
				}
			} },
			{ EnemyType.Rotator, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/EnemyRotator"),
				diffIndex = 0,
				diffPresets = new DifficultyPreset[]
				{
					new() { timeToExit = 6, maxAlive = 1, spawnTime = 10f },
					new() { timeToExit = 12, maxAlive = 2, spawnTime = 10f },
					new() { timeToExit = 0, maxAlive = 3, spawnTime = 8.5f },
				}
			} }
		};
		Restart();
	}

	private void Update()
	{
		int currentDifficulty = DifficultyTimer.Instance.DifficultyStage;
		// pickups
		foreach (PickupType type in m_pickups.Keys)
		{
			SpawnData pickup = m_pickups[type];
			DifficultyPreset diffPreset = pickup.diffPresets[pickup.diffIndex];
			if (pickup.nAlive < diffPreset.maxAlive && pickup.spawnTimer >= diffPreset.spawnTime)
			{
				pickup.spawnTimer = 0f;
				SpawnPickup(type);
				if (pickup.diffIndex < pickup.diffPresets.Length - 1 && currentDifficulty > diffPreset.timeToExit)
					++pickup.diffIndex;
			}
			else if (pickup.nAlive < diffPreset.maxAlive)
			{
				pickup.spawnTimer += Time.deltaTime;
			}
		}

		// enemies
		foreach (EnemyType type in m_enemies.Keys)
		{
			SpawnData enemy = m_enemies[type];
			DifficultyPreset diffPreset = enemy.diffPresets[enemy.diffIndex];
			if (enemy.nAlive < diffPreset.maxAlive && enemy.spawnTimer >= diffPreset.spawnTime)
			{
				enemy.spawnTimer = 0f;
				SpawnEnemy(type);
				if (enemy.diffIndex < enemy.diffPresets.Length - 1 && currentDifficulty > diffPreset.timeToExit)
					++enemy.diffIndex;
			}
			else if (enemy.nAlive < diffPreset.maxAlive)
			{
				enemy.spawnTimer += Time.deltaTime;
			}
		}
	}

	public void SpawnEnemy(EnemyType type)
	{
		Instantiate(m_enemies[type].prefab);
		++m_enemies[type].nAlive;
	}

	public void SpawnPickup(PickupType type)
	{
		GameObject go = Instantiate(m_pickups[type].prefab);
		float angle;
		float distance;
		Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
		int attempts = 0;
		int maxAttempts = 5;
		do
		{
			++attempts;
			angle = Random.Range(0f, 2 * Mathf.PI);
			distance = Random.Range(0f, GlobalConstants.MAP_RADIUS);
		} while (attempts < maxAttempts && (playerPos - distance * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f)).magnitude < 1.0f);
		go.transform.position = distance * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
		++m_pickups[type].nAlive;
	}

	public void Restart()
	{
		foreach (var pickup in m_pickups.Values)
		{
			pickup.diffIndex = 0;
			pickup.spawnTimer = 0f;
			pickup.nAlive = 0;
		}
		foreach (var enemy in m_enemies.Values)
		{
			enemy.diffIndex = 0;
			enemy.spawnTimer = 0f;
			enemy.nAlive = 0;
		}
	}

	public bool RemovePickup(PickupType type)
	{
		if (m_pickups[type].nAlive <= 0)
			return false;
		--m_pickups[type].nAlive;
		return true;
	}

	public bool RemoveEnemy(EnemyType type)
	{
		if (m_enemies[type].nAlive <= 0)
			return false;
		--m_enemies[type].nAlive;
		return true;
	}
}
