using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRotatorStateMachine : MonoBehaviour
{
	private enum State
	{
		Spawn,
		Rotate
	}

	private Dictionary<State, string> m_stateFunctionNames = new Dictionary<State, string>();
	private State m_state;

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
	private void NextState()
	{
		StartCoroutine(m_stateFunctionNames[m_state]);
	}

	private IEnumerator SpawnState()
	{
		float randomAngle = Mathf.Deg2Rad * UnityEngine.Random.Range(-45f, 45f);
		transform.position = GlobalConstants.MAP_RADIUS * new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0f);
		yield return null;
		m_state = State.Rotate;
		NextState();
	}

	private IEnumerator RotateState()
	{
		yield return null;
	}
}
