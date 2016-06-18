using UnityEngine;
using System.Collections;

[System.Serializable]
public struct UnitCommand {
	public enum CommandType {
		NONE = 0,
		MOVE = 1,
		ATTACK_MOVE = 2,
	}
	public CommandType command;
	public Vector3 target;

	public UnitCommand(CommandType type) {
		this.command = type;
		this.target = Vector3.zero;
	}
}
