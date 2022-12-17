
using UnityEngine;

public class BurstAudio : MonoBehaviour
{
	[SerializeField] private AudioClip m_clip;

	public void PlayClip()
	{
		AudioManager.Instance.PlayClip(m_clip);
	}
}
