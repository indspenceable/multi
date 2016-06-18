using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BuildLevelOnServerStart : NetworkManager {
	public GameObject baddie;

	public override void OnStartServer() {
		StartCoroutine(CreateLevel());
	}
	IEnumerator CreateLevel() {
		while (! NetworkServer.active) {
			yield return null;
		}
//		GetComponent<LevelBuilder>()
		GameObject go = Instantiate(baddie, new Vector3(2f,2f), Quaternion.identity) as GameObject;
		NetworkServer.Spawn(go);
	}
}
