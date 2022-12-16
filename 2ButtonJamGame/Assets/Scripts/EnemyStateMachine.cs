
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyStateMachine : MonoBehaviour
{
	protected bool m_dead = false;
	public void TakeHit()
	{
		m_dead = true;
		GetComponent<BoxCollider2D>().enabled = false;
	}
}
