using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {
	public GameObject player;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 diff = player.transform.position - transform.position;
//		float scalar = Mathf.Min(5f * Time.deltaTime,1f);
		float scalar = 1f;
		transform.Translate(new Vector3(diff.x, diff.y, 0));
	}
}
