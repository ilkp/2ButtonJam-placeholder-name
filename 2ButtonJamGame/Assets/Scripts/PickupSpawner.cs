
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
	public static PickupSpawner Instance { get; private set; }

	private class PickupMetaData
	{
		public GameObject prefab;
		public int nAlive = 0;
		public int maxAlive;
		public float spawnTimer = 0f;
		public float spawnTime;
	}

	private Dictionary<PickupType, PickupMetaData> m_pickups;

	public bool RemovePickup(PickupType pickupType)
	{
		if (m_pickups[pickupType].nAlive <= 0)
			return false;
		--m_pickups[pickupType].nAlive;
		return true;
	}

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
	}

	private void Start()
	{
		m_pickups = new Dictionary<PickupType, PickupMetaData>
		{
			{ PickupType.Score, new() { maxAlive = 1, spawnTime = 5f, prefab = (GameObject)Resources.Load("Prefabs/ScorePickupPrefab") } },
			{ PickupType.Powerup, new() { maxAlive = 1, spawnTime = 1f, prefab = (GameObject)Resources.Load("Prefabs/PowerupPickupPrefab") } },
			{ PickupType.PowerupCharge, new() { maxAlive = 1, spawnTime = 5f, prefab = (GameObject)Resources.Load("Prefabs/PowerupChargePickupPrefab") } }
		};
	}

	private void Update()
	{
		foreach (var pickup in m_pickups.Values)
		{
			if (pickup.nAlive < pickup.maxAlive && pickup.spawnTimer >= pickup.spawnTime)
			{
				pickup.spawnTimer = 0f;
				GameObject go = Instantiate(pickup.prefab);
				float angle = Random.Range(0f, 2 * Mathf.PI);
				float distance = Random.Range(0f, GlobalConstants.MAP_RADIUS);
				go.transform.position = distance * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
				++pickup.nAlive;
			}
			else if (pickup.nAlive < pickup.maxAlive)
			{
				pickup.spawnTimer += Time.deltaTime;
			}
		}
	}
}
