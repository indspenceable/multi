using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AttackMoveAction : PlayerAction {
	public override string name() {
		return "Attack-Move";
	}

	public override void leftButtonClick(Vector3 location, PlayerManager me) {
		me.currentAction = null;
		UnitCommand command = new UnitCommand(UnitCommand.CommandType.ATTACK_MOVE);
		command.target = location;
		me.CmdIssueUnitCommand(me.selection.unit.gameObject, command);
	}

	public override void rightButtonClick(Vector3 location, PlayerManager me) {
		me.currentAction = null;
	}
}