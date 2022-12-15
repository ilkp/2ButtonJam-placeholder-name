using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EnemyChaserStateMachine : EnemyStateMachine
{
	private enum State
	{
		Spawn,
		Run,
		Death
	}

	private readonly int RUN_ANIMATION = Animator.StringToHash("ChaserAnimationRun");
	private readonly Vector3 ROTATION_POINT = Vector3.zero;
	private readonly Vector3 ROTATION_AXIS = new Vector3(0f, 0f, 1f);

	[SerializeField] private float m_maxAngularSpeed = 5f;
	[SerializeField] private float m_angularAcceleration = 10f;
	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private Transform m_playerTransform;
	private Animator m_animator;
	private float m_currentAngularSpeed = 0f;
	private State m_state;

	private void Start()
	{
		gameObject.tag = "Enemy";
		m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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

	public override void TakeHit()
	{
		m_state = State.Death;
	}

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		float randomAngle = Mathf.Deg2Rad * UnityEngine.Random.Range(-45f, 45f);
		transform.position = GlobalConstants.MAP_RADIUS * new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f);
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		do
		{
			yield return null;

			// Play animation
			m_animator.Play(RUN_ANIMATION);

			// Handle movement
			float moveDirection = Vector3.Cross(transform.position, m_playerTransform.position).z;
			if (moveDirection == 0f)
				moveDirection = 1f;
			else
				moveDirection /= Mathf.Abs(moveDirection);
			m_currentAngularSpeed += Time.deltaTime * m_angularAcceleration * moveDirection;
			m_currentAngularSpeed = Mathf.Clamp(m_currentAngularSpeed, -m_maxAngularSpeed, m_maxAngularSpeed);
			transform.RotateAround(ROTATION_POINT, ROTATION_AXIS, m_currentAngularSpeed * Time.deltaTime);
			transform.rotation = Quaternion.identity;

		} while (m_state == State.Run);
		NextState();
	}

	private IEnumerator DeathState()
	{
		gameObject.SetActive(false);
		yield return null;
		Destroy(gameObject);
	}
}
