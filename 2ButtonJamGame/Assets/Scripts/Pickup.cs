
using UnityEngine;

public class Pickup : MonoBehaviour
{
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
			PickupSpawner.Instance.RemovePickup(type);
			Destroy(gameObject);
		}
	}
}
