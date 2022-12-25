
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	public static Spawner Instance { get; private set; }

	private class DifficultyPreset
	{
		public int levelToExit;
		public int maxAlive;
		public float spawnTime;
	}

	private class SpawnData
	{
		public GameObject prefab;
		public DifficultyPreset[] difficultyPresets;
		public int difficultyIndex = 0;
		public int nAlive = 0;
		public float spawnTimer = 0f;
		public float overSpawnTimer = 0f;
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
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 5, maxAlive = 1, spawnTime = 7.5f },
					new() { levelToExit = 15, maxAlive = 2, spawnTime = 7.5f }
				}
			} } ,
			{ PickupType.Powerup, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/PowerupPickupPrefab"),
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 0, maxAlive = 1, spawnTime = 15f },
				}
			} },
			{ PickupType.Charge, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/ChargePickupPrefab"),
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 10, maxAlive = 1, spawnTime = 12f },
					new() { levelToExit = 0, maxAlive = 1, spawnTime = 10f },
				}
			} },
			{ PickupType.Bomb, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/BombPickupPrefab"),
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 12, maxAlive = 0, spawnTime = 30f },
					new() { levelToExit = 0, maxAlive = 1, spawnTime = 30f }
				}
			} }
		};
		m_enemies = new Dictionary<EnemyType, SpawnData>()
		{
			{ EnemyType.Chaser, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/EnemyChaser"),
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 1, maxAlive = 1, spawnTime = 5f },
					new() { levelToExit = 10, maxAlive = 1, spawnTime = 15f },
					new() { levelToExit = 15, maxAlive = 2, spawnTime = 13f },
					new() { levelToExit = 0, maxAlive = 3, spawnTime = 11f },
				}
			} },
			{ EnemyType.Rotator, new SpawnData() {
				prefab = (GameObject)Resources.Load("Prefabs/EnemyRotator"),
				difficultyPresets = new DifficultyPreset[]
				{
					new() { levelToExit = 6, maxAlive = 1, spawnTime = 10f },
					new() { levelToExit = 12, maxAlive = 2, spawnTime = 10f },
					new() { levelToExit = 0, maxAlive = 3, spawnTime = 10f },
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
			DifficultyPreset diffPreset = pickup.difficultyPresets[pickup.difficultyIndex];
			if (pickup.nAlive < diffPreset.maxAlive)
			{
				pickup.spawnTimer += Time.deltaTime;
				if (pickup.spawnTimer >= diffPreset.spawnTime)
					SpawnPickup(type);
			}
			else if (diffPreset.maxAlive > 0)
			{
				pickup.overSpawnTimer += Time.deltaTime;
				if (pickup.overSpawnTimer >= 5f * diffPreset.spawnTime)
					SpawnPickup(type);
			}
			if (pickup.difficultyIndex < pickup.difficultyPresets.Length - 1 && currentDifficulty >= diffPreset.levelToExit)
				++pickup.difficultyIndex;
		}

		// enemies
		foreach (EnemyType type in m_enemies.Keys)
		{
			SpawnData enemy = m_enemies[type];
			DifficultyPreset diffPreset = enemy.difficultyPresets[enemy.difficultyIndex];
			if (enemy.nAlive < diffPreset.maxAlive)
			{
				enemy.spawnTimer += Time.deltaTime;
				if (enemy.spawnTimer >= diffPreset.spawnTime)
					SpawnEnemy(type);
			}
			else if (diffPreset.maxAlive > 0)
			{
				enemy.overSpawnTimer += Time.deltaTime;
				if (enemy.overSpawnTimer >= 3f * diffPreset.spawnTime)
					SpawnEnemy(type);
			}
			if (enemy.difficultyIndex < enemy.difficultyPresets.Length - 1 && currentDifficulty >= diffPreset.levelToExit)
				++enemy.difficultyIndex;
		}
	}

	public void SpawnEnemy(EnemyType type)
	{
		Instantiate(m_enemies[type].prefab);
		++m_enemies[type].nAlive;
		m_enemies[type].spawnTimer = 0f;
		m_enemies[type].overSpawnTimer = 0f;
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
		m_pickups[type].spawnTimer = 0f;
		m_pickups[type].overSpawnTimer = 0f;
	}

	public void Restart()
	{
		foreach (var pickup in m_pickups.Values)
		{
			pickup.difficultyIndex = 0;
			pickup.spawnTimer = 0f;
			pickup.overSpawnTimer = 0f;
			pickup.nAlive = 0;
		}
		foreach (var enemy in m_enemies.Values)
		{
			enemy.difficultyIndex = 0;
			enemy.spawnTimer = 0f;
			enemy.overSpawnTimer = 0f;
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
