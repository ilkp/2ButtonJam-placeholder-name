
using UnityEngine;

public class Pickup : MonoBehaviour
{
	[SerializeField] private PickupType m_type;
	[SerializeField] private GameObject m_afterEffectPrefab;
	public PickupType Type
	{
		get { return m_type; }
		private set { m_type = value; }
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().ReceivePickup(m_type);
			Spawner.Instance.RemovePickup(m_type);
			if (m_afterEffectPrefab)
				Instantiate(m_afterEffectPrefab, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
}
