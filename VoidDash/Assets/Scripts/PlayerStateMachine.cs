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

#if UNITY_EDITOR
	public bool debugInvinsibility = false;
#endif

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

	[SerializeField] private Animator m_burstAnimator;
	[SerializeField] private GameObject m_playerGraphics;
	[SerializeField] private GameObject m_lanceGraphics;
	[SerializeField] private float m_maxAngularSpeed = 20f;
	[SerializeField] private float m_angularAcceleration = 100f;
	[SerializeField] private float m_dashSpeed = 20f;

	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private KeyCode m_leftKey = KeyCode.RightArrow;
	private KeyCode m_rightKey = KeyCode.LeftArrow;
	private (float, Sprite[])[] m_playerSprites;
	private (float, Sprite[])[] m_lanceSprites;
	private bool m_lanceIsBreaking = false;
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

		m_playerSprites = new (float, Sprite[])[]
		{
			new (45f * 0.5f, new[]{GameAssets.Instance.sprite_player[2], GameAssets.Instance.sprite_player[2] }),
			new (45f * 1.5f, new[]{GameAssets.Instance.sprite_player[1], GameAssets.Instance.sprite_player[3] }),
			new (45f * 2.5f, new[]{GameAssets.Instance.sprite_player[0], GameAssets.Instance.sprite_player[4] }),
			new (45f * 3.5f, new[]{GameAssets.Instance.sprite_player[7], GameAssets.Instance.sprite_player[5] }),
			new (45f * 4.0f, new[]{GameAssets.Instance.sprite_player[6], GameAssets.Instance.sprite_player[6] })
		};
		m_lanceSprites = new (float, Sprite[])[]
		{
			new (45f * 0.5f, new[]{GameAssets.Instance.sprite_lance[2], GameAssets.Instance.sprite_lance[2] }),
			new (45f * 1.5f, new[]{GameAssets.Instance.sprite_lance[1], GameAssets.Instance.sprite_lance[3] }),
			new (45f * 2.5f, new[]{GameAssets.Instance.sprite_lance[0], GameAssets.Instance.sprite_lance[4] }),
			new (45f * 3.5f, new[]{GameAssets.Instance.sprite_lance[7], GameAssets.Instance.sprite_lance[5] }),
			new (45f * 4.0f, new[]{GameAssets.Instance.sprite_lance[6], GameAssets.Instance.sprite_lance[6] })
		};
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
			if (m_haveGodMode)
			{
				collision.GetComponent<EnemyStateMachine>().TakeHit();
			}
			else if (m_haveLance && m_state == State.Dash)
			{
				collision.GetComponent<EnemyStateMachine>().TakeHit();
				m_lanceIsBreaking = true;
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
		AudioManager.Instance.PlayClip(GameAssets.Instance.sound_playerHit[0]);
		StartCoroutine(HitCameraShake());
#if UNITY_EDITOR
		if (!debugInvinsibility)
		{
			--Hp;
			UIChanged?.Invoke();
			if (Hp <= 0)
			{
				m_dead = true;
				return;
			}
		}
#else
		--Hp;
		UIChanged?.Invoke();
		if (Hp <= 0)
		{
			m_dead = true;
			return;
		}
#endif
		StartCoroutine(InvinsibilityFrames());
	}

	private IEnumerator HitCameraShake()
	{
		float cameraSpeed = 40f;
		Vector3[] points = new Vector3[6];
		for (int i = 0; i < points.Length - 1; ++i)
			points[i] = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-0.25f, 0.25f), -10f);
		points[points.Length - 1] = new Vector3(0f, 0f, -10f);
		for (int i = 0; i < points.Length; ++i)
		{
			Vector3 direction = (points[i] - Camera.main.transform.position).normalized;
			while ((points[i] - Camera.main.transform.position).magnitude > 0.01f)
			{
				Vector3 translate = Time.deltaTime * cameraSpeed * direction;
				Vector3 cameraToPoint = points[i] - Camera.main.transform.position;
				if (cameraToPoint.magnitude < translate.magnitude)
					Camera.main.transform.position += cameraToPoint;
				else
					Camera.main.transform.position += translate;
				yield return null;
			}
		}
		Camera.main.transform.position = new Vector3(0f, 0f, -10f);
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
				AudioManager.Instance.PlayClip(GameAssets.Instance.sound_pickupScore[0]);
				AddScore(100);
				break;
			case PickupType.Charge:
				PowerupCharges = Mathf.Clamp(++PowerupCharges, 0, MAX_POWERUPS);
				if (AudioManager.Instance)
					AudioManager.Instance.PlayClip(GameAssets.Instance.sound_pickupPowerup[0]);
				break;
			case PickupType.Powerup:
				Hp = Mathf.Clamp(++Hp, 0, MAX_HP);
				if (PowerupCharges > 0)
					AudioManager.Instance.PlayClip(GameAssets.Instance.sound_getLance[0]);
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
		if (m_lanceIsBreaking)
		{
			SetLance(false);
			m_lanceIsBreaking = false;
		}
		do
		{
			yield return null;

			// Handle animation
			float playerAngle = Mathf.Rad2Deg * Mathf.Acos(transform.position.x / transform.position.magnitude);
			for (int i = 0; i < m_playerSprites.Length; ++i)
			{
				if (playerAngle <= m_playerSprites[i].Item1)
				{
					m_playerGraphics.GetComponent<SpriteRenderer>().sprite = m_playerSprites[i].Item2[transform.position.y > 0f ? 0 : 1];
					m_lanceGraphics.GetComponent<SpriteRenderer>().sprite = m_lanceSprites[i].Item2[transform.position.y > 0f ? 0 : 1];
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
		AudioManager.Instance.PlayClip(GameAssets.Instance.sound_dash[0]);
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
		AudioManager.Instance.PlayClip(GameAssets.Instance.sound_playerDeath[0]);
		m_burstAnimator.SetBool("PlayerIsDead", true);
		m_burstAnimator.SetTrigger("Stop");
		m_playerGraphics.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.sprite_player[0];
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
