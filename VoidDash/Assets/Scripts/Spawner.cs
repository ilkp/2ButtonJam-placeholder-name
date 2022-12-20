
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	public static Spawner Instance { get; private set; }

	private class SpawnableData
	{
		public GameObject prefab;
		public int nAlive = 0;
		public int maxAlive;
		public float spawnTimer = 0f;
		public float spawnTime;
	}

	private Dictionary<PickupType, SpawnableData> m_pickups;
	private Dictionary<EnemyType, SpawnableData> m_enemies;

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
	}

	private void Start()
	{
		Restart();
	}

	private void Update()
	{
		m_enemies[EnemyType.Rotator].maxAlive = Mathf.Max(DifficultyTimer.Instance.DifficultyStage / 3, 1);
		m_enemies[EnemyType.Chaser].maxAlive = Mathf.Max(DifficultyTimer.Instance.DifficultyStage / 5, 1);
		if (DifficultyTimer.Instance.DifficultyStage > 5)
		{
			m_enemies[EnemyType.Rotator].spawnTime = 8f;
			m_enemies[EnemyType.Chaser].spawnTime = 13f;
			m_pickups[PickupType.Score].maxAlive = 2;
		}
		if (DifficultyTimer.Instance.DifficultyStage > 10)
		{
			m_enemies[EnemyType.Rotator].spawnTime = 7f;
			m_enemies[EnemyType.Chaser].spawnTime = 12f;
			m_pickups[PickupType.Score].maxAlive = 3;
			m_pickups[PickupType.Score].maxAlive = 2;
		}

		// pickups
		foreach (PickupType type in m_pickups.Keys)
		{
			SpawnableData pickup = m_pickups[type];
			if (pickup.nAlive < pickup.maxAlive && pickup.spawnTimer >= pickup.spawnTime)
			{
				pickup.spawnTimer = 0f;
				SpawnPickup(type);
			}
			else if (pickup.nAlive < pickup.maxAlive)
			{
				pickup.spawnTimer += Time.deltaTime;
			}
		}

		// enemies
		foreach (EnemyType type in m_enemies.Keys)
		{
			SpawnableData enemy = m_enemies[type];
			if (enemy.nAlive < enemy.maxAlive && enemy.spawnTimer >= enemy.spawnTime)
			{
				enemy.spawnTimer = 0f;
				SpawnEnemy(type);
			}
			else if (enemy.nAlive < enemy.maxAlive)
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
		float angle = Random.Range(0f, 2 * Mathf.PI);
		float distance = Random.Range(0f, GlobalConstants.MAP_RADIUS);
		go.transform.position = distance * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
		++m_pickups[type].nAlive;
	}

	public void Restart()
	{
		m_pickups = new Dictionary<PickupType, SpawnableData>
		{
			{ PickupType.Score, new() { maxAlive = 2, spawnTime = 10f, prefab = (GameObject)Resources.Load("Prefabs/ScorePickupPrefab") } },
			{ PickupType.Powerup, new() { maxAlive = 1, spawnTime = 15f, prefab = (GameObject)Resources.Load("Prefabs/PowerupPickupPrefab") } },
			{ PickupType.PowerupCharge, new() { maxAlive = 1, spawnTime = 10f, prefab = (GameObject)Resources.Load("Prefabs/PowerupChargePickupPrefab") } }
		};

		m_enemies = new Dictionary<EnemyType, SpawnableData>
		{
			{ EnemyType.Chaser, new() { maxAlive = 1, spawnTime = 7.5f, prefab = (GameObject)Resources.Load("Prefabs/EnemyChaser") } },
			{ EnemyType.Rotator, new() { maxAlive = 2, spawnTime = 10f, prefab = (GameObject)Resources.Load("Prefabs/EnemyRotator") } }
		};
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
