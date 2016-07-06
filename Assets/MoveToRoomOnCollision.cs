using UnityEngine;
using System.Collections;

public class MoveToRoomOnCollision : MonoBehaviour {
	BoxCollider2D myCollider;
	public GameObject myRoom;
	public GameObject targetRoom;
	// Use this for initialization
	void Start () {
		myCollider = this.GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if (myCollider.IsTouchingLayers(LayerMask.NameToLayer("Player"))) {
			Debug.Log("hi");
			StartCoroutine(doScroll(5f));
		}
	}

	public IEnumerator doScroll(float time) {
		float dt = 0f;
		Vector3 myRoomStart = Vector3.zero;
		Vector3 myRoomEnd = new Vector3(-26,0);
		Vector3 targetRoomStart = new Vector3(26,0);
		Vector3 targetRoomEnd = Vector3.zero;
		while (dt/time < 1f) {
			dt += Time.deltaTime;
			myRoom.transform.position = Vector3.Lerp(myRoomStart, myRoomEnd, dt/time);
			targetRoom.transform.position = Vector3.Lerp(targetRoomStart, targetRoomEnd, dt/time);
			yield return 0;
		}
		myRoom.transform.position = myRoomEnd;
		targetRoom.transform.position = targetRoomEnd;
	}
}
