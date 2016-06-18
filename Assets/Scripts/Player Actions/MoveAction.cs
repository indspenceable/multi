using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MoveAction : PlayerAction {
	public override string name() {
		return "move";
	}

	public override void leftButtonClick(Vector3 location, PlayerManager me) {
		me.currentAction = null;
		UnitCommand command = new UnitCommand(UnitCommand.CommandType.MOVE);
		command.target = location;
		me.CmdIssueUnitCommand(me.selection.unit.gameObject, command);
	}

	public override void rightButtonClick(Vector3 location, PlayerManager me) {
		me.currentAction = null;
	}
}
