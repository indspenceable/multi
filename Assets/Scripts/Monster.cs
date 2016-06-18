using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Monster : NetworkBehaviour {
	public UnitCommand? currentCommand;
	public float speed = 1;

	[SyncVar] public int hp;
	public override void OnStartServer() {
		hp = 100;
	}

	void Update() {
		if (currentCommand.HasValue && currentCommand.Value.command == UnitCommand.CommandType.MOVE) {
			Vector3 dist = currentCommand.Value.target - transform.position;
			if (dist.magnitude > speed*Time.deltaTime) {
				transform.position += dist.normalized * speed *Time.deltaTime;
			} else {
				transform.position = currentCommand.Value.target;
				currentCommand = null;
			}
		}
	}
}
