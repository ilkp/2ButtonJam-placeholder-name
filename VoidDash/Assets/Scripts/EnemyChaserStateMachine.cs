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

	private const EnemyType m_type = EnemyType.Chaser;
	private readonly int RUN_ANIMATION = Animator.StringToHash("ChaserAnimationRun");

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

	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		m_dead = false;
		IsSpawning = true;
		Vector3 direction = GameObject.FindGameObjectWithTag("Player").transform.position.normalized;
		direction = new Vector3(direction.y, -direction.x, 0f);
		direction.x *= UnityEngine.Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
		transform.position = GlobalConstants.MAP_RADIUS * direction;
		StartCoroutine(SpawnFlashing());
		yield return null;
		m_state = State.Run;
		NextState();
	}

	private IEnumerator RunState()
	{
		// Play animation
		m_animator.Play(RUN_ANIMATION);

		do
		{
			yield return null;

			// Handle movement
			float moveDirection = Vector3.Cross(transform.position, m_playerTransform.position).z;
			if (moveDirection == 0f)
				moveDirection = 1f;
			else
				moveDirection /= Mathf.Abs(moveDirection);
			if (m_playerTransform.GetComponent<PlayerStateMachine>().HasGodMode)
				moveDirection *= -1f;
			m_currentAngularSpeed += Time.deltaTime * m_angularAcceleration * moveDirection;
			m_currentAngularSpeed = Mathf.Clamp(m_currentAngularSpeed, -m_maxAngularSpeed, m_maxAngularSpeed);
			m_animator.speed = Mathf.Clamp(Mathf.Abs(m_currentAngularSpeed) / m_maxAngularSpeed, 0.2f, 1f);
			transform.RotateAround(GlobalConstants.ROTATION_POINT, GlobalConstants.ROTATION_AXIS, m_currentAngularSpeed * Time.deltaTime);
			transform.rotation = Quaternion.identity;

			// State transitions
			if (m_dead)
				m_state = State.Death;

		} while (m_state == State.Run);
		NextState();
	}

	private IEnumerator DeathState()
	{
		GetComponent<BoxCollider2D>().enabled = false;
		GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().AddScore(75);
		Spawner.Instance.RemoveEnemy(m_type);
		yield return null;
		Destroy(gameObject);
	}
}
