using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
	private enum State
	{
		Spawn,
		Run,
		Dash
	}

	private readonly Vector3 ROTATION_POINT = Vector3.zero;
	private readonly Vector3 ROTATION_AXIS = new Vector3(0f, 0f, 1f);

	[SerializeField] private float m_angularSpeed = 10f;
	[SerializeField] private float m_dashSpeed = 20f;
	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private State m_state;
	private Transform m_playerTransform;

	private void Start()
	{
		m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		transform.position = new Vector3(GlobalConstants.MAP_RADIUS, 0f, 0f);
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		do
		{
			yield return null;
			float direction = Vector3.Cross(transform.position, m_playerTransform.position).z;
			if (direction == 0f)
				direction = 1f;
			else
				direction /= Mathf.Abs(direction);
			transform.RotateAround(ROTATION_POINT, ROTATION_AXIS, m_angularSpeed * Time.deltaTime * direction);
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
			{
				transform.position = transform.position.normalized * GlobalConstants.MAP_RADIUS;
				m_state = State.Run;
			}
		} while (m_state == State.Dash);
		NextState();
	}
}
