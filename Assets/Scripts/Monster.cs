using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Monster : NetworkUnit {
	public override IEnumerator Attack() {
		Debug.Log("Monster attack is happening!");
		yield return new WaitForSeconds(1f);
	}
}
