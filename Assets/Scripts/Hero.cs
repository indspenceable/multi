using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hero : NetworkUnit {
	public override IEnumerator Attack() {
		Debug.Log("Hero attack is happening!");
		yield return new WaitForSeconds(1f);
	}
}
