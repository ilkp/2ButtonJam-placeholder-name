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
		Dash
	}

	private readonly Vector3 ROTATION_POINT = Vector3.zero;
	private readonly Vector3 ROTATION_AXIS = new Vector3(0f, 0f, 1f);

	[SerializeField] private Transform m_playerGraphics;
	[SerializeField] private float m_maxAngularSpeed = 20f;
	[SerializeField] private float m_angularAcceleration = 100f;
	[SerializeField] private float m_dashSpeed = 20f;
	[SerializeField] private float m_dashSteeringStregth = 20f;
	private float m_angularSpeed = 0f;
	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private State m_state;

	private KeyCode m_leftKey = KeyCode.LeftArrow;
	private KeyCode m_rightKey = KeyCode.RightArrow;

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
		m_state = State.Spawn;
		NextState();
	}

	private void SetGraphicsDirection()
	{

	}

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		transform.position = new Vector3(-GlobalConstants.MAP_RADIUS, 0f, 0f);
		transform.LookAt(transform.position + new Vector3(0f, 0f, 1f), -transform.position);
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		float timer = 0f;
		m_angularSpeed = 0f;
		do
		{
			yield return null;
			float direction = 0f;
			if (Input.GetKey(m_leftKey))
			{
				direction -= 1f;
				m_playerGraphics.transform.LookAt(transform.position - new Vector3(0f, 0f, 1f), transform.up);
			}
			if (Input.GetKey(m_rightKey))
			{
				direction += 1f;
				m_playerGraphics.transform.LookAt(transform.position + new Vector3(0f, 0f, 1f), transform.up);
			}

			m_angularSpeed += Time.deltaTime * m_angularAcceleration * direction;
			m_angularSpeed = Mathf.Clamp(m_angularSpeed, -m_maxAngularSpeed, m_maxAngularSpeed);
			transform.RotateAround(ROTATION_POINT, ROTATION_AXIS, m_angularSpeed * Time.deltaTime);

			if (Input.GetKey(m_leftKey) && Input.GetKey(m_rightKey) && m_state == State.Run)
			{
				timer += Time.deltaTime;
				if (timer >= 0.1f)
					m_state = State.Dash;
			}
			else
			{
				timer = 0f;
			}
		} while (m_state == State.Run);
		NextState();
	}

	private IEnumerator DashState()
	{
		do
		{
			yield return null;
			transform.Translate(m_dashSpeed * Time.deltaTime * transform.up, Space.World);
			if (transform.position.magnitude > GlobalConstants.MAP_RADIUS)
				m_state = State.Run;
			if (Input.GetKey(m_leftKey))
				m_playerGraphics.transform.LookAt(transform.position + new Vector3(0f, 0f, 1f), transform.up);
			else if (Input.GetKey(m_rightKey))
				m_playerGraphics.transform.LookAt(transform.position - new Vector3(0f, 0f, 1f), transform.up);

		} while (m_state == State.Dash);

		transform.position = transform.position.normalized * GlobalConstants.MAP_RADIUS;
		transform.LookAt(transform.position + new Vector3(0f, 0f, 1f), -transform.position);
		NextState();
	}
}
