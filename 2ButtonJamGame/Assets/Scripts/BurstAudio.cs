
using UnityEngine;

public class BurstAudio : MonoBehaviour
{
	[SerializeField] private AudioClip m_clip;
	private GameObject m_soundObject = null;

	public void PlayClip()
	{
		if (m_soundObject != null)
			return;
		m_soundObject = AudioManager.Instance.PlayClip(m_clip);
	}
}
