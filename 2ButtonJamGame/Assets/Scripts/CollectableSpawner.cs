using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
	private const float SPAWN_TIME = 5f;
	[SerializeField] private GameObject m_collectablePrefab;
	private List<GameObject> m_activeCollectables = new List<GameObject>();
	private float m_timer = 0f;
	private int max_collectables = 1;

	public void RemoveCollectable(Collectable collectable)
	{
		m_activeCollectables.Remove(collectable.gameObject);
	}

	private void Update()
	{
		if (m_timer >= SPAWN_TIME)
		{
			m_timer = 0f;
			GameObject go = Instantiate(m_collectablePrefab);
			float randomAngle = Mathf.Deg2Rad * Random.Range(0f, 360f);
			float randomDistance = Random.Range(0f, GlobalConstants.MAP_RADIUS);
			go.transform.position = randomDistance * new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f);
			m_activeCollectables.Add(go);
			go.GetComponent<Collectable>().spawner = this;
		}
		if (m_activeCollectables.Count < max_collectables)
		{
			m_timer += Time.deltaTime;
		}
	}
}
