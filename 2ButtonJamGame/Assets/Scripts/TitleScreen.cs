
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
			SceneManager.LoadScene(1);
	}
}
