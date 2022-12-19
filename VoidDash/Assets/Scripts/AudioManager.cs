
using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	public void PlayClip(AudioClip clip)
	{
		GameObject soundObject = new GameObject(clip.name);
		AudioSource audioSource = soundObject.AddComponent<AudioSource>();
		audioSource.PlayOneShot(clip, 0.5f);
		Destroy(soundObject, clip.length);
	}

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
	}
}
