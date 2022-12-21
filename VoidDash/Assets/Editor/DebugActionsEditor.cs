using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(DebugActions))]
public class DebugActionsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		PlayerStateMachine player = FindObjectOfType<PlayerStateMachine>();
		Spawner spawner = FindObjectOfType<Spawner>();

		DebugActions actions = (DebugActions)target;
		player.debugInvinsibility = GUILayout.Toggle(player.debugInvinsibility, "Player invinsibility");
		GUILayout.Label("Spawn enemies");
		foreach (EnemyType type in Enum.GetValues(typeof(EnemyType)))
			if (GUILayout.Button("Spawn " + type.ToString()))
				spawner.SpawnEnemy(type);
		GUILayout.Label("Spawn pickups");
		foreach (PickupType type in Enum.GetValues(typeof(PickupType)))
			if (GUILayout.Button("Spawn " + type.ToString()))
				spawner.SpawnPickup(type);
	}
}
