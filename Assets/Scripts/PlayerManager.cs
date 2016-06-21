using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;


public class PlayerManager : NetworkBehaviour {
	public GameObject UnitPrefab;
	[HideInInspector]
	private UnitSelection _selection;
	public UnitSelection selection {
		get {
			if (isLocalPlayer) {
				if (_selection == null) {
					_selection = gameObject.AddComponent<UnitSelection>();
				}
				return _selection;
			}
			return null;
		}
	}
	[HideInInspector]
	public PlayerAction currentAction;

	// UI related bullshit
	Text hpUiText;
	Text currentCommandText;
	Text currentActionText;


	// ----------
	// Setup code
	// ----------
	public override void OnStartServer() {
		initState();
	}

	public override void OnStartLocalPlayer() {
		hpUiText = GameObject.Find("HpText").GetComponent<Text>();
		currentCommandText = GameObject.Find("CurrentCommand").GetComponent<Text>();
		currentActionText = GameObject.Find("CurrentAction").GetComponent<Text>();
		CmdSpawnHero();
	}

	[Command]
	private void CmdSpawnHero() {
		GameObject newUnit = Instantiate(UnitPrefab) as GameObject;
		NetworkServer.Spawn(newUnit);
		RpcSetHeroAtStart(newUnit);
	}

	[ClientRpc]
	private void RpcSetHeroAtStart(GameObject g) {
		if (isLocalPlayer) {
			selection.unit = g.GetComponent<Hero>();
		}
	}

	[Command]
	public void CmdIssueUnitCommand(GameObject unit, UnitCommand command) {
		// Running on server! so we are clear to issue commands to the units
		unit.GetComponent<Hero>().currentCommand = command;
	}

	// -------------------------------------------------
	// update logic
	// 
	// this class is largely responsible for local UI so
	// a large majority of this runs on the client.
	// -------------------------------------------------
	void Update() {
		if (isLocalPlayer) {
			UpdateUI();
			HandleMouseButtonPresses();

			if (currentAction == null) {
				this.SetCurrentActionBasedOnKeyPress();
			}
		}

		SyncState();
	}

	void HandleMouseButtonPresses()
	{
		if (Input.GetMouseButtonDown (0)) {
			// Left click
			if (currentAction == null) {
				// OK, maybe try re-selecting.
			}
			else {
				Vector3 point = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				currentAction.leftButtonClick (new Vector3 (point.x, point.y), this);
			}
		}
		if (Input.GetMouseButtonDown (1)) {
			// Right Click
			if (currentAction == null) {
				currentAction = new MoveAction ();
				Vector3 point = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				currentAction.leftButtonClick (new Vector3 (point.x, point.y), this);
			}
			else {
				Vector3 point = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				currentAction.rightButtonClick (new Vector3 (point.x, point.y), this);
			}
		}
	}

	void SetCurrentActionBasedOnKeyPress() {
		if (Input.GetKeyDown(KeyCode.A)) {
			this.currentAction = new AttackMoveAction();
		}
	}

	void UpdateUI()
	{
		if (selection.unit) {
			hpUiText.text = selection.unit.hp + " hp";
			currentCommandText.text = selection.unit.currentCommand.command.ToString();
		}
		if (currentAction != null) {
			currentActionText.text = currentAction.name();
		} else {
			currentActionText.text = "N/A";
		}
	}

	private void SyncState () {
	}

	[Server]
	private void initState() {
	}
}
