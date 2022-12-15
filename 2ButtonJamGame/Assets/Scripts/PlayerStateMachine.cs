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
		Dash
	}

	private const float LANCE_DISTANCE = 0.75f;
	private const float LANCE_DAMAGE_ARC = 45f;
	private const float INVINSIBILITY_FRAMES = 2f;
	private const float DRAG = 1.2f;
	private readonly Vector3 ROTATION_POINT = Vector3.zero;
	private readonly Vector3 ROTATION_AXIS = new Vector3(0f, 0f, 1f);
	private readonly int RUN_ANIMATION_EAST = Animator.StringToHash("PlayerAnimationRunEast");
	private readonly int RUN_ANIMATION_WEST = Animator.StringToHash("PlayerAnimationRunWest");
	private readonly int RUN_ANIMATION_NORTH = Animator.StringToHash("PlayerAnimationRunNorth");
	private readonly int RUN_ANIMATION_SOUTH = Animator.StringToHash("PlayerAnimationRunSouth");

	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private KeyCode m_leftKey = KeyCode.LeftArrow;
	private KeyCode m_rightKey = KeyCode.RightArrow;

	[SerializeField] GameObject m_playerGraphics;
	[SerializeField] GameObject m_lanceGraphics;
	[SerializeField] private float m_maxAngularSpeed = 20f;
	[SerializeField] private float m_angularAcceleration = 100f;
	[SerializeField] private float m_dashSpeed = 20f;
	[SerializeField] private float m_dashSteeringStregth = 20f;
	private Animator m_animator;
	private float m_angularVelocity = 0f;
	private State m_state;
	private bool m_isInvinsible = false;

	private Vector3 m_previousFramePosition;
	private bool m_haveLance = true;


	private void Awake()
	{
		gameObject.tag = "Player";
	}

	private void Start()
	{
		SetLance(false);
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
		m_state = State.Spawn;
		NextState();
	}

	private void Update()
	{
		m_previousFramePosition = transform.position;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy") && !m_isInvinsible)
			TakeHit();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy") && m_haveLance)
		{
			Vector3 prevPositionToCurrent = transform.position - m_previousFramePosition;
			if (prevPositionToCurrent.magnitude > 0f && Vector3.Angle(prevPositionToCurrent, collision.transform.position - transform.position) <= LANCE_DAMAGE_ARC)
			{
				collision.GetComponent<EnemyStateMachine>().TakeHit();
				SetLance(false);
			}
		}
	}

	private void TakeHit()
	{
		StartCoroutine(InvinsibilityFrames());
	}

	private IEnumerator InvinsibilityFrames()
	{
		m_isInvinsible = true;
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
		m_isInvinsible = false;
	}
	
	private void SetLance(bool set)
	{
		m_haveLance = set;
		m_lanceGraphics.SetActive(set);
	}

	public void ReceivePickup(PickupType type)
	{
		switch (type)
		{
			case PickupType.Score:
				break;
			case PickupType.PowerupCharge:
				break;
			case PickupType.Powerup:
				SetLance(true);
				break;
		}
	}

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		transform.position = new Vector3(-GlobalConstants.MAP_RADIUS, 0f, 0f);
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		float timer = 0f;
		m_angularVelocity = 0f;
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
			transform.RotateAround(ROTATION_POINT, ROTATION_AXIS, m_angularVelocity * Time.deltaTime);
			transform.rotation = Quaternion.identity;

			// Handle going into dashing
			if (Input.GetKey(m_leftKey) && Input.GetKey(m_rightKey))
				timer += Time.deltaTime;
			else
				timer = 0f;
			if (timer >= 0.1f)
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
			if (transform.position.magnitude > GlobalConstants.MAP_RADIUS)
				m_state = State.Run;

		} while (m_state == State.Dash);

		transform.position = transform.position.normalized * GlobalConstants.MAP_RADIUS;
		NextState();
	}
}
