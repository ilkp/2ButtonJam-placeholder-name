using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyTimer : MonoBehaviour
{
	public static DifficultyTimer Instance;
	public int DifficultyStage = 1;
	private float m_timer = 0;
	private float m_step = 10f;

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
	}

	public void Restart()
	{
		m_timer = 0f;
		DifficultyStage = 1;
	}

	private void Start()
	{
		Restart();
	}

	private void Update()
	{
		m_timer += Time.deltaTime;
		if (m_timer > m_step)
		{
			++DifficultyStage;
			m_step *= 1.2f;
			m_timer = 0f;
		}
	}
}
