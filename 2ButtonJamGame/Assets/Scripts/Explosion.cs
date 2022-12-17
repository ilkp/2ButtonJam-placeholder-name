
using UnityEngine;

public class Explosion : MonoBehaviour
{
	[SerializeField] private AudioClip m_audioClip;

	private void Start()
	{
		AudioManager.Instance.PlayClip(m_audioClip);
	}

	private void Remove()
	{
		Destroy(gameObject);
	}
}
