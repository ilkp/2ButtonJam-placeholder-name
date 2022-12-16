using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyRotatorStateMachine : EnemyStateMachine
{
	private enum State
	{
		Spawn,
		Rotate,
		Death
	}

	private const EnemyType m_type = EnemyType.Rotator;
	private readonly int RUN_ANIMATION = Animator.StringToHash("RotatorAnimation");
	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private State m_state;
	private int m_rotationDirection;
	private float m_angularVelocity = 20f;
	private bool m_dead = false;

	private void Start()
	{
		gameObject.tag = "Enemy";
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

	public override void TakeHit()
	{
		m_dead = true;
		GetComponent<BoxCollider2D>().enabled = false;
	}

	private IEnumerator SpawnState()
	{
		float angle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
		float randomDistance = UnityEngine.Random.Range(-0.2f * GlobalConstants.MAP_RADIUS, 0.2f * GlobalConstants.MAP_RADIUS);
		transform.position = (0.5f * GlobalConstants.MAP_RADIUS + randomDistance) * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
		m_rotationDirection = UnityEngine.Random.Range(0f, 1f) > 0.5f ? 1 : -1;
		yield return null;
		m_state = State.Rotate;
		NextState();
	}

	private IEnumerator RotateState()
	{
		// Play animation
		GetComponent<Animator>().Play(RUN_ANIMATION);

		do
		{
			yield return null;
			transform.RotateAround(GlobalConstants.ROTATION_POINT, GlobalConstants.ROTATION_AXIS, m_rotationDirection * m_angularVelocity * Time.deltaTime);
			transform.rotation = Quaternion.identity;

			// State transitions
			if (m_dead)
				m_state = State.Death;

		} while (m_state == State.Rotate);
		NextState();
	}

	private IEnumerator DeathState()
	{
		yield return null;
		Spawner.Instance.RemoveEnemy(m_type);
		Destroy(gameObject);
	}
}
