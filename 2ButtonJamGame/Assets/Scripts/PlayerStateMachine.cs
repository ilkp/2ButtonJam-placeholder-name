using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateMachine : MonoBehaviour
{
	private enum State
	{
		Spawn,
		Run,
		Dash,
		Death
	}

	public Action UIChanged;
	public const int MAX_POWERUPS = 3;
	public const int MAX_HP = 3;
	public int PowerupCharges { get; private set; } = 0;
	public int Hp { get; private set; } = MAX_HP;
	public int Score { get; private set; } = 0;

	private const float GOD_MODE_TIME = 10f;
	private const float LANCE_DISTANCE = 0.75f;
	private const float LANCE_DAMAGE_ARC = 45f;
	private const float INVINSIBILITY_FRAMES = 2f;
	private const float DRAG = 1.2f;

	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private KeyCode m_leftKey = KeyCode.RightArrow;
	private KeyCode m_rightKey = KeyCode.LeftArrow;

	[SerializeField] private AudioClip m_powerupAudioClip;
	[SerializeField] private AudioClip m_deathAudioClip;
	[SerializeField] private AudioClip m_dashAudioClip;
	[SerializeField] private AudioClip m_getLanceAudioClip;
	[SerializeField] private AudioClip m_hitHurtAudioClip;
	[SerializeField] private AudioClip m_pickupScoreAudioClip;

	[SerializeField] private Sprite m_playerSprite_n;
	[SerializeField] private Sprite m_playerSprite_ne;
	[SerializeField] private Sprite m_playerSprite_e;
	[SerializeField] private Sprite m_playerSprite_se;
	[SerializeField] private Sprite m_playerSprite_s;
	[SerializeField] private Sprite m_playerSprite_sw;
	[SerializeField] private Sprite m_playerSprite_w;
	[SerializeField] private Sprite m_playerSprite_nw;
	[SerializeField] private Sprite m_lanceSprite_n;
	[SerializeField] private Sprite m_lanceSprite_ne;
	[SerializeField] private Sprite m_lanceSprite_e;
	[SerializeField] private Sprite m_lanceSprite_se;
	[SerializeField] private Sprite m_lanceSprite_s;
	[SerializeField] private Sprite m_lanceSprite_sw;
	[SerializeField] private Sprite m_lanceSprite_w;
	[SerializeField] private Sprite m_lanceSprite_nw;

	[SerializeField] private Animator m_burstAnimator;
	[SerializeField] private GameObject m_playerGraphics;
	[SerializeField] private GameObject m_lanceGraphics;
	[SerializeField] private float m_maxAngularSpeed = 20f;
	[SerializeField] private float m_angularAcceleration = 100f;
	[SerializeField] private float m_dashSpeed = 20f;
	private readonly float[] m_playerSpriteAngles = new float[]
	{
		45f * 0.5f,
		45f * 1.5f,
		45f * 2.5f,
		45f * 3.5f,
		45f * 4f
	};
	private Sprite[] m_playerSprites;
	private Sprite[] m_lanceSprites;
	private int m_burstDirection;
	private float m_godModeTimer = 0f;
	private bool m_dead = false;
	private float m_angularVelocity = 0f;
	private State m_state;
	private bool m_haveInvinsibilityFrames = false;

	private Vector3 m_previousFramePosition;
	private bool m_haveLance = false;
	private bool m_haveGodMode = false;


	private void Awake()
	{
		gameObject.tag = "Player";
	}

	private void Start()
	{
		m_playerSprites = new Sprite[]
		{
			m_playerSprite_e,
			m_playerSprite_ne,
			m_playerSprite_n,
			m_playerSprite_nw,
			m_playerSprite_w,
			m_playerSprite_e,
			m_playerSprite_se,
			m_playerSprite_s,
			m_playerSprite_sw,
			m_playerSprite_w
		};
		m_lanceSprites = new Sprite[]
		{
			m_lanceSprite_e,
			m_lanceSprite_ne,
			m_lanceSprite_n,
			m_lanceSprite_nw,
			m_lanceSprite_w,
			m_lanceSprite_e,
			m_lanceSprite_se,
			m_lanceSprite_s,
			m_lanceSprite_sw,
			m_lanceSprite_w
		};
		// We add states and corresponding function names into dictionary for easy access
		string[] methodNames = GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Select(x => x.Name).ToArray();
		foreach (State state in Enum.GetValues(typeof(State)))
		{
			string methodName = state.ToString() + "State";
			if (!methodNames.Contains(methodName))
			{
				Debug.LogError(GetType().Name + ": " + methodName + " is missing");
				continue;
			}
			m_stateFunctionNames.Add(state, methodName);
		}
		m_state = State.Spawn;
		NextState();
	}

	private void Update()
	{
		m_previousFramePosition = transform.position;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
			TakeHit(collision.GetComponent<EnemyStateMachine>());
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			//Vector3 prevPositionToCurrent = transform.position - m_previousFramePosition;
			if (m_haveLance || m_haveGodMode)// && prevPositionToCurrent.magnitude > 0f && Vector3.Angle(prevPositionToCurrent, collision.transform.position - transform.position) <= LANCE_DAMAGE_ARC)
			{
				collision.GetComponent<EnemyStateMachine>().TakeHit();
				if (!m_haveGodMode)
					SetLance(false);
			}
			else
			{
				TakeHit(collision.GetComponent<EnemyStateMachine>());
			}
		}
	}

	public void Restart()
	{
		m_state = State.Spawn;
	}

	private void TakeHit(EnemyStateMachine enemy)
	{
		if (m_dead || m_haveInvinsibilityFrames || m_haveGodMode || enemy.IsSpawning)
			return;
		--Hp;
		AudioManager.Instance.PlayClip(m_hitHurtAudioClip);
		UIChanged?.Invoke();
		if (Hp <= 0)
		{
			m_dead = true;
			return;
		}
		StartCoroutine(InvinsibilityFrames());
	}

	private IEnumerator InvinsibilityFrames()
	{
		m_haveInvinsibilityFrames = true;
		float timer = 0f;
		float timerB = 0f;
		m_playerGraphics.GetComponent<SpriteRenderer>().enabled = false;
		while (timer < INVINSIBILITY_FRAMES)
		{
			timer += Time.deltaTime;
			timerB += Time.deltaTime;
			if (timerB > 0.2f)
			{
				m_playerGraphics.GetComponent<SpriteRenderer>().enabled = !m_playerGraphics.GetComponent<SpriteRenderer>().enabled;
				//m_playerGraphics.SetActive(!m_playerGraphics.activeSelf);
				timerB = 0f;
			}
			yield return null;
		}
		m_playerGraphics.GetComponent<SpriteRenderer>().enabled = true;
		m_playerGraphics.SetActive(true);
		m_haveInvinsibilityFrames = false;
	}

	private void StartGodMode()
	{
		m_godModeTimer = GOD_MODE_TIME;
		if (!m_haveGodMode)
			StartCoroutine(GodMode());
	}

	private IEnumerator GodMode()
	{
		m_haveGodMode = true;
		m_playerGraphics.GetComponent<SpriteRenderer>().color = new Color(250f, 0f, 250f, 250f);
		while (m_godModeTimer > 0f)
		{
			m_godModeTimer -= Time.deltaTime;
			yield return null;
		}
		m_playerGraphics.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 250f);
		m_haveGodMode = false;
		StartCoroutine(InvinsibilityFrames());
	}
	
	private void SetLance(bool set)
	{
		m_haveLance = set;
		m_lanceGraphics.SetActive(set);
	}

	public void AddScore(int score)
	{
		Score += score;
		UIChanged?.Invoke();
		FindObjectOfType<UI>().HighlightScore();
	}

	public void ReceivePickup(PickupType type)
	{
		switch (type)
		{
			case PickupType.Life:
				Hp = Mathf.Clamp(++Hp, 0, MAX_HP);
				break;
			case PickupType.Score:
				AudioManager.Instance.PlayClip(m_pickupScoreAudioClip);
				AddScore(100);
				break;
			case PickupType.PowerupCharge:
				PowerupCharges = Mathf.Clamp(++PowerupCharges, 0, MAX_POWERUPS);
				if (AudioManager.Instance)
					AudioManager.Instance.PlayClip(m_powerupAudioClip);
				break;
			case PickupType.Powerup:
				Hp = Mathf.Clamp(++Hp, 0, MAX_HP);
				if (PowerupCharges > 0)
					AudioManager.Instance.PlayClip(m_getLanceAudioClip);
				if (PowerupCharges == 3)
				{
					PowerupCharges = 0;
					StartGodMode();
					SetLance(true);
				}
				else if (PowerupCharges == 2)
				{
					PowerupCharges = 0;
					StartGodMode();
				}
				else if (PowerupCharges == 1)
				{
					PowerupCharges = 0;
					SetLance(true);
				}
				break;
		}
		UIChanged?.Invoke();
	}

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		m_playerGraphics.SetActive(true);
		SetLance(false);
		transform.position = new Vector3(0f, GlobalConstants.MAP_RADIUS, 0f);
		m_dead = false;
		m_angularVelocity = 0;
		Hp = MAX_HP;
		PowerupCharges = 0;
		Score = 0;
		UIChanged?.Invoke();
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		float dashTimer = 0f;
		m_angularVelocity = 0f;
		transform.position = transform.position.normalized * GlobalConstants.MAP_RADIUS;
		do
		{
			yield return null;

			// Handle animation
			float playerAngle = Mathf.Rad2Deg * Mathf.Acos(transform.position.x / transform.position.magnitude);
			for (int i = 0; i < m_playerSpriteAngles.Length; ++i)
			{
				if (playerAngle <= m_playerSpriteAngles[i])
				{
					m_playerGraphics.GetComponent<SpriteRenderer>().sprite = m_playerSprites[i + (transform.position.y > 0f ? 0 : 5)];
					m_lanceGraphics.GetComponent<SpriteRenderer>().sprite = m_lanceSprites[i + (transform.position.y > 0f ? 0 : 5)];
					break;
				}
			}
			m_lanceGraphics.transform.position = transform.position + new Vector3(transform.position.y, -transform.position.x, 0f).normalized;

			// Handle movement
			int moveDirection = 0;
			if (Input.GetKey(m_leftKey))
				moveDirection -= 1;
			if (Input.GetKey(m_rightKey))
				moveDirection += 1;
			m_angularVelocity += Time.deltaTime * (m_angularAcceleration * moveDirection - DRAG * m_angularVelocity);
			m_angularVelocity = Mathf.Clamp(m_angularVelocity, -m_maxAngularSpeed, m_maxAngularSpeed);
			transform.RotateAround(GlobalConstants.ROTATION_POINT, GlobalConstants.ROTATION_AXIS, m_angularVelocity * Time.deltaTime);
			transform.rotation = Quaternion.identity;
			if (Input.GetKey(m_leftKey) && Input.GetKey(m_rightKey))
				dashTimer += Time.deltaTime;
			else
				dashTimer = 0f;

			// Handle burst animation
			if (moveDirection != 0)
				m_burstDirection = moveDirection;
			m_burstAnimator.transform.position = transform.position + m_burstDirection * new Vector3(transform.position.y, -transform.position.x, 0f).normalized;
			m_burstAnimator.transform.LookAt(m_burstAnimator.transform.position + new Vector3(0f, 0f, 1f), transform.position.normalized);
			if (m_burstDirection == 1)
				m_burstAnimator.gameObject.GetComponent<SpriteRenderer>().flipX = true;
			else
				m_burstAnimator.gameObject.GetComponent<SpriteRenderer>().flipX = false;

			if (moveDirection != 0)
				m_burstAnimator.SetBool("PressingMove", true);
			else
				m_burstAnimator.SetBool("PressingMove", false);

			// State transitions
			if (m_dead)
				m_state = State.Death;
			else if (dashTimer >= 0.1f)
				m_state = State.Dash;

		} while (m_state == State.Run);
		NextState();
	}

	private IEnumerator DashState()
	{
		AudioManager.Instance.PlayClip(m_deathAudioClip);
		Vector3 dashDirection = -transform.position.normalized;
		m_burstAnimator.SetTrigger("Stop");
		do
		{
			yield return null;
			transform.Translate(m_dashSpeed * Time.deltaTime * dashDirection, Space.World);

			// State transitions
			if (m_dead)
				m_state = State.Death;
			else if (transform.position.magnitude > GlobalConstants.MAP_RADIUS)
				m_state = State.Run;

		} while (m_state == State.Dash);
		NextState();
	}

	private IEnumerator DeathState()
	{
		UI.Instance.ActivateButtons();
		AudioManager.Instance.PlayClip(m_dashAudioClip);
		m_burstAnimator.SetBool("PlayerIsDead", true);
		m_burstAnimator.SetTrigger("Stop");
		m_playerGraphics.GetComponent<SpriteRenderer>().sprite = m_playerSprite_n;
		Vector3 rotate = new Vector3(0f, 0f, 120f);
		float timer = 0f;
		float maxTime = 2.5f;
		do
		{
			if (timer < maxTime)
			{
				m_playerGraphics.transform.localPosition = Vector3.Lerp(Vector3.one, -transform.position, timer / maxTime);
				m_playerGraphics.transform.Rotate(rotate * Time.deltaTime);
				m_playerGraphics.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / maxTime);
				timer += Time.deltaTime;
			}
			else
			{
				m_playerGraphics.SetActive(false);
			}
			yield return null;

		} while (m_state == State.Death);
		m_burstAnimator.SetBool("PlayerIsDead", false);
		m_playerGraphics.transform.localPosition = Vector3.zero;
		m_playerGraphics.transform.localScale = Vector3.one;
		m_playerGraphics.transform.localRotation = Quaternion.identity;
		m_playerGraphics.SetActive(true);
		NextState();
	}
}
