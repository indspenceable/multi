using UnityEngine;
using System.Collections;

public abstract class PlayerAction {
	public abstract string name();
	public abstract void leftButtonClick(Vector3 location, PlayerManager me);
	public abstract void rightButtonClick(Vector3 location, PlayerManager me);
}
