
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	[SerializeField] private AudioClip m_selectAudioClip;
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			SceneManager.LoadScene(1);
			AudioManager.Instance.PlayClip(m_selectAudioClip);
		}
	}
}
