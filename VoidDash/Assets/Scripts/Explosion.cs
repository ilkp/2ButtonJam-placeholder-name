
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Explosion : MonoBehaviour
{
	[SerializeField] private float m_animationLength;
	private float m_timer = 0f;
	private int m_frame = 0;
	private int m_nFrames;
	private float m_frameTime;

	private void Start()
	{
		m_nFrames = GameAssets.Instance.sprite_explosion.Length;
		m_frameTime = m_animationLength / m_nFrames;
		GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.sprite_explosion[m_frame];
		AudioManager.Instance.PlayClip(GameAssets.Instance.sound_explosion[0]);
	}

	private void Update()
	{
		m_timer += Time.deltaTime;
		if (m_timer >= m_frameTime)
		{
			++m_frame;
			if (m_frame < m_nFrames)
			{
				m_timer = 0f;
				GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.sprite_explosion[m_frame];
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}
