
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent (typeof(SpriteRenderer))]
public class EnemyStateMachine : MonoBehaviour
{
	[SerializeField] private AudioClip m_deathSound;
	public bool IsSpawning { get; protected set; }
	protected float SPAWN_FLASH_TIME = 1f;
	protected bool m_dead = false;

	public void TakeHit()
	{
		AudioManager.Instance.PlayClip(m_deathSound);
		m_dead = true;
		GetComponent<BoxCollider2D>().enabled = false;
	}

	protected IEnumerator SpawnFlashing()
	{
		IsSpawning = true;
		Color flashColor = new Color(255f, 255f, 255f, 255f);
		Color originalColor = GetComponent<SpriteRenderer>().color;
		Color[] colors = new Color[] { flashColor, originalColor };
		int colorIndex = 0;
		float timer = 0f;
		float timerB = 0f;
		GetComponent<SpriteRenderer>().material.color = flashColor;
		while (timer < SPAWN_FLASH_TIME)
		{
			timer += Time.deltaTime;
			timerB += Time.deltaTime;
			if (timerB > 0.2f)
			{
				GetComponent<SpriteRenderer>().material.color = colors[(++colorIndex) % colors.Length];
				timerB = 0f;
			}
			yield return null;
		}
		IsSpawning = false;
		GetComponent<SpriteRenderer>().material.color = originalColor;
	}
}
