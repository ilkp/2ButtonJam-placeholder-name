
using UnityEngine;

public class Pickup : MonoBehaviour
{
	[SerializeField] private GameObject m_sparkle;
	[SerializeField] private PickupType type;
	public PickupType Type
	{
		get { return type; }
		private set { type = value; }
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().ReceivePickup(type);
			Spawner.Instance.RemovePickup(type);
			if (type == PickupType.Score)
				Instantiate(m_sparkle, transform.position, Quaternion.identity);
			Destroy(gameObject);
		}
	}
}
