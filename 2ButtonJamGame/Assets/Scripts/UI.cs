
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class UI : MonoBehaviour
{
	public static UI Instance { get; private set; }
	[SerializeField] private GameObject m_gameOverMenu;
	[SerializeField] private PlayerStateMachine m_player;
	[SerializeField] private TMP_Text m_scoreText;
	[SerializeField] private Image[] m_powerupUI;
	[SerializeField] private Image[] m_hpUI;
	[SerializeField] private Sprite m_powerupEmpty;
    [SerializeField] private Sprite m_powerupFilled;
	[SerializeField] private Sprite m_hpEmpty;
	[SerializeField] private Sprite m_hpFilled;
	private bool m_buttonsActive = false;

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
		m_gameOverMenu.SetActive(false);
	}

	private void Update()
	{
		if (m_buttonsActive)
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				// Restart game
				GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
				GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
				for (int i = 0; i < enemies.Length; ++i)
					Destroy(enemies[i]);
				for (int i = 0; i < pickups.Length; ++i)
					Destroy(pickups[i]);
				FindObjectOfType<PlayerStateMachine>().Restart();
				FindObjectOfType<Spawner>().Restart();
				FindObjectOfType<DifficultyTimer>().Restart();
				m_gameOverMenu.SetActive(false);
				m_buttonsActive = false;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}
		}
	}

	public void ActivateButtons()
	{
		StartCoroutine(ActivateButtonsDelay());
	}

	private IEnumerator ActivateButtonsDelay()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		m_gameOverMenu.SetActive(true);
		m_buttonsActive = true;
	}

	private void OnEnable()
    {
		m_player.UIChanged += OnUIChanged;
	}

    private void OnDisable()
	{
		m_player.UIChanged -= OnUIChanged;
	}

    private void OnUIChanged()
	{
		m_scoreText.text = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().Score.ToString();
		int powerups = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().PowerupCharges;
		int i = 0;
		for (; i < powerups; ++i)
			m_powerupUI[i].sprite = m_powerupFilled;
		for (; i < PlayerStateMachine.MAX_POWERUPS; ++i)
			m_powerupUI[i].sprite = m_powerupEmpty;

		int hp = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().Hp;
		i = 0;
		for (; i < hp; ++i)
			m_hpUI[i].sprite = m_hpFilled;
		for (; i < PlayerStateMachine.MAX_HP; ++i)
			m_hpUI[i].sprite = m_hpEmpty;
	}
}
