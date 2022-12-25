
using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }
	private GameObject m_gameMusicSource;

	public void PlayClip(AudioClip clip)
	{
		GameObject soundObject = new GameObject(clip.name);
		AudioSource audioSource = soundObject.AddComponent<AudioSource>();
		audioSource.PlayOneShot(clip, 0.25f);
		Destroy(soundObject, clip.length);
	}

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
		DontDestroyOnLoad(gameObject);
		m_gameMusicSource = new GameObject("Game music");
		DontDestroyOnLoad(m_gameMusicSource);
	}

	private void Start()
	{
		if (m_gameMusicSource.GetComponent<AudioSource>() == null)
		{
			AudioSource audioSource = m_gameMusicSource.AddComponent<AudioSource>();
			audioSource.clip = GameAssets.Instance.sound_gameMusic[0];
			audioSource.loop = true;
			audioSource.volume = 0.1f;
			m_gameMusicSource.GetComponent<AudioSource>().Play();
		}
	}

	public void SetMusicPitch(float pitch)
	{
		m_gameMusicSource.GetComponent<AudioSource>().pitch = pitch;
	}
}
