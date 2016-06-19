using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Monster : NetworkUnit {
	public override void takeDamage(int damage) {
		base.takeDamage(damage);
		if (hp <= 0) {
			NetworkServer.Destroy(gameObject);
		}
	}
		
	public override void FaceTowards(Vector3 point) {
		Debug.Log(point - this.transform.position);
		float x = (point - this.transform.position).x;
		GetComponent<SpriteRenderer>().flipX = (x > 0);
	}
}
