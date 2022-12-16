using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
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
	private readonly int RUN_ANIMATION_EAST = Animator.StringToHash("PlayerAnimationRunEast");
	private readonly int RUN_ANIMATION_WEST = Animator.StringToHash("PlayerAnimationRunWest");
	private readonly int RUN_ANIMATION_NORTH = Animator.StringToHash("PlayerAnimationRunNorth");
	private readonly int RUN_ANIMATION_SOUTH = Animator.StringToHash("PlayerAnimationRunSouth");

	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private KeyCode m_leftKey = KeyCode.LeftArrow;
	private KeyCode m_rightKey = KeyCode.RightArrow;

	[SerializeField] private GameObject m_playerGraphics;
	[SerializeField] private GameObject m_lanceGraphics;
	[SerializeField] private float m_maxAngularSpeed = 20f;
	[SerializeField] private float m_angularAcceleration = 100f;
	[SerializeField] private float m_dashSpeed = 20f;
	private float m_godModeTimer = 0f;
	private bool m_dead = false;
	private Animator m_animator;
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
		m_animator = GetComponent<Animator>();
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
		Restart();
	}

	private void Update()
	{
		m_previousFramePosition = transform.position;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
			TakeHit();
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
				TakeHit();
			}
		}
	}

	public void Restart()
	{
		m_state = State.Spawn;
		NextState();
	}

	private void TakeHit()
	{
		if (m_haveInvinsibilityFrames || m_haveGodMode)
			return;
		--Hp;
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
		m_playerGraphics.SetActive(false);
		while (timer < INVINSIBILITY_FRAMES)
		{
			timer += Time.deltaTime;
			timerB += Time.deltaTime;
			if (timerB > 0.2f)
			{
				m_playerGraphics.SetActive(!m_playerGraphics.activeSelf);
				timerB = 0f;
			}
			yield return null;
		}
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
	}

	public void ReceivePickup(PickupType type)
	{
		switch (type)
		{
			case PickupType.Life:
				Hp = Mathf.Clamp(++Hp, 0, MAX_HP);
				break;
			case PickupType.Score:
				Score += 100;
				break;
			case PickupType.PowerupCharge:
				PowerupCharges = Mathf.Clamp(++PowerupCharges, 0, MAX_POWERUPS);
				break;
			case PickupType.Powerup:
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
		transform.position = new Vector3(-GlobalConstants.MAP_RADIUS, 0f, 0f);
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
			if (playerAngle < 45f || playerAngle > 135f)
			{
				if (transform.position.x >= 0)
				{
					m_animator.Play(RUN_ANIMATION_EAST);
					m_lanceGraphics.transform.localPosition = new Vector3(0f, LANCE_DISTANCE, 0f);
					m_lanceGraphics.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
				}
				else
				{
					m_animator.Play(RUN_ANIMATION_WEST);
					m_lanceGraphics.transform.localPosition = new Vector3(0f, -LANCE_DISTANCE, 0f);
					m_lanceGraphics.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
				}
			}
			else
			{
				if (transform.position.y >= 0)
				{
					m_animator.Play(RUN_ANIMATION_NORTH);
					m_lanceGraphics.transform.localPosition = new Vector3(-LANCE_DISTANCE, 0f, 0f);
					m_lanceGraphics.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				}
				else
				{
					m_animator.Play(RUN_ANIMATION_SOUTH);
					m_lanceGraphics.transform.localPosition = new Vector3(LANCE_DISTANCE, 0f, 0f);
					m_lanceGraphics.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				}
			}

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
		Vector3 dashDirection = -transform.position.normalized;
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
		m_playerGraphics.SetActive(false);
		yield return null;
		UI.Instance.ActivateButtons();
	}
}
