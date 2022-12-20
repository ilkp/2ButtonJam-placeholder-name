using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
		if (GUILayout.Button("Spawn Chaser"))
			spawner.SpawnEnemy(EnemyType.Chaser);
		if (GUILayout.Button("Spawn Rotator"))
			spawner.SpawnEnemy(EnemyType.Rotator);
		if (GUILayout.Button("Spawn Score Pickup"))
			spawner.SpawnPickup(PickupType.Score);
		if (GUILayout.Button("Spawn Charge Pickup"))
			spawner.SpawnPickup(PickupType.PowerupCharge);
		if (GUILayout.Button("Spawn PowerUp Pickup"))
			spawner.SpawnPickup(PickupType.Powerup);
	}
}
