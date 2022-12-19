
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI : MonoBehaviour
{
	private const float SCORE_HIGHLIGHT_TIME = 0.5f;
	private float m_scoreHighlightTimer = 0f;
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
	private Vector3 m_scoreDefaultScale;
	private Coroutine m_scoreHighlightCoroutine;

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
		m_gameOverMenu.SetActive(false);
	}

	private void Start()
	{
		m_scoreDefaultScale = m_scoreText.transform.localScale;
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
				AudioManager.Instance.PlayClip(GameAssets.Instance.sound_select[0]);
				m_gameOverMenu.SetActive(false);
				m_buttonsActive = false;
			}
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				if (Application.platform == RuntimePlatform.WebGLPlayer)
					SceneManager.LoadScene(0);
				else
					Application.Quit();
#endif
			}
		}
	}

	public void HighlightScore()
	{
		m_scoreHighlightTimer = 0f;
		if (m_scoreHighlightCoroutine == null)
			StartCoroutine(HighlightScoreCoroutine());
	}

	private IEnumerator HighlightScoreCoroutine()
	{
		while (m_scoreHighlightTimer < SCORE_HIGHLIGHT_TIME)
		{
			m_scoreHighlightTimer += Time.deltaTime;
			m_scoreText.transform.localScale = Vector3.Lerp(1.5f * m_scoreDefaultScale, m_scoreDefaultScale, m_scoreHighlightTimer / SCORE_HIGHLIGHT_TIME);
			yield return null;
		}
		m_scoreText.transform.localScale = m_scoreDefaultScale;
	}

	public void ActivateButtons()
	{
		StartCoroutine(ActivateButtonsDelay());
	}

	private IEnumerator ActivateButtonsDelay()
	{
		float timer = 0f;
		while (timer < 0.5f)
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
