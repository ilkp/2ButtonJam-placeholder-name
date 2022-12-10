using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
	public CollectableSpawner spawner;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			spawner.RemoveCollectable(this);
			Destroy(gameObject);
		}
	}
}
