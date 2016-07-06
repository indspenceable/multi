using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	Animator animator;
	
	public bool startFacingLeft = true;
	private bool facingLeft = true;
	
	public float walkSpeed = 2f;
	
	public float vy = 0f;
	public float jumpStrength = 10f;
	public float highJumpStrength = 20f;
	public bool highJumpEnabled = false;

	public float gravityStrength = 30f;
	public GameObject currentRoom;
	
	void Flip(){
		// Switch the way the player is labelled as facing
		facingLeft = !facingLeft;
		
		// Multiply the player's x local scale by -1
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
	
	
	// Use this for initialization
	void Start () {
		animator = gameObject.GetComponent<Animator>();
		if (startFacingLeft != facingLeft)
			Flip();
	}
	
	// Woo not fixedupdate
	void Update () {
		if (!moving) {
			moveUpDown();
			moveLeftRight();
			checkForExits();
			interact();	
		}
	}
	
	private bool moving = false;
	public LayerMask exitMask;
	void checkForExits() {
		if (moving) return;
		Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.4f, exitMask);
		if (collider != null) {
			RoomExit exit = collider.gameObject.GetComponent<RoomExit>();
			GameObject newRoom = Instantiate(exit.destination) as GameObject;
			newRoom.transform.position = exit.offset;
			StartCoroutine(AnimateRoomMove(-exit.offset, newRoom, exit.time));
		}
	}
	
	IEnumerator AnimateRoomMove(Vector3 offset, GameObject newRoom, float time) {
		moving = true;
		GameObject room1 = currentRoom;
		GameObject room2 = newRoom;
		
		Vector3 r1s, r1e, r2s, r2e, mts, mte;
		r1s = room1.transform.position;
		r2s = room2.transform.position;
		r1e = r1s + offset;
		r2e = r2s + offset;
		mts = transform.position;
		
		float ddx = System.Math.Sign(offset.x);
		float ddy = System.Math.Sign(offset.y);
		mte = transform.position + offset - new Vector3(ddx, ddy);
		float t = 0f;
		
		while (t < 1f){
			t += Time.deltaTime / time; // sweeps from 0 to 1 in time seconds
			transform.position = Vector3.Lerp(mts, mte, t); // set position proportional to t
			room2.transform.position = Vector3.Lerp(r2s, r2e, t);
			room1.transform.position = Vector3.Lerp(r1s, r1e, t);
			yield return 0; // leave the routine and return here in the next frame
		}
		
		Destroy(currentRoom);
		currentRoom = newRoom;
		moving = false;
	}
	
	
	public LayerMask interactableMask;
	public void interact() {
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Collider2D collider = Physics2D.OverlapCircle(transform.position, 0.4f, interactableMask);
			if (collider != null) {
//				Door d = collider.gameObject.GetComponent<Door>();
//				StartCoroutine(AnimateTeleportRoom(Instantiate(d.destination) as GameObject, 1f, d.x, d.y));
			}
		}
	}
	
	
	public IEnumerator AnimateTeleportRoom(GameObject newRoom, float time, int x, int y) {
		moving = true;
		
		//Fade out
		
		Destroy(currentRoom);
		currentRoom = newRoom;
		transform.position = new Vector3(x - 19.5f, y - 14.5f, transform.position.z);
		yield return 0;
		
		// Fade in
		
		moving = false;
	}
	
	// TODO move this into a "move horizontally" script
	public bool airControlAllowed = true;
	public float vx = 0f;
	
	void moveLeftRight() {
		grounded = vertCheck(-yAxisWallCollisionDistance) && vy <= 0f;
		if (grounded || airControlAllowed || (jumpVx == 0f && vy < 0f && initiatedJump)) {
			vx = 0f;
			if (Input.GetKey(KeyCode.A))
				vx -= walkSpeed;
			if (Input.GetKey (KeyCode.D))
				vx += walkSpeed;
		} else {
			vx = jumpVx;
		}
		
		animator.SetBool("horiz", vx!=0f);
		if ((vx > 0f && facingLeft && grounded) || (vx < 0f && !facingLeft && grounded))
			Flip();
		if (vx > 0) {
			moveRight(vx*Time.deltaTime);
		} else if (vx < 0) {
			moveLeft(vx*Time.deltaTime);
		}
	}
	
	
	//TODO move this into a "move vertically" script
	private bool grounded;
	public LayerMask groundMask;
	public float maxGravity = 100f;
	
	
	
	void OnDrawGizmos() {
		Vector3 origin;
		origin = new Vector2(transform.position.x,transform.position.y+horizCheckOffset);
		Gizmos.DrawLine(origin, origin + Vector3.right*xAxisWallCollisionDistance);
		Gizmos.DrawLine(origin, origin - Vector3.right*xAxisWallCollisionDistance);
		
		origin = new Vector2(transform.position.x,transform.position.y-horizCheckOffset);
		Gizmos.DrawLine(origin, origin + Vector3.right*xAxisWallCollisionDistance);
		Gizmos.DrawLine(origin, origin - Vector3.right*xAxisWallCollisionDistance);
		
		origin = new Vector2(transform.position.x+(vertCheckOffset),transform.position.y);
		Gizmos.DrawLine(origin, origin + Vector3.up*yAxisWallCollisionDistance);
		Gizmos.DrawLine(origin, origin - Vector3.up*yAxisWallCollisionDistance);
		
		origin = new Vector2(transform.position.x-(vertCheckOffset),transform.position.y);
		Gizmos.DrawLine(origin, origin + Vector3.up*yAxisWallCollisionDistance);
		Gizmos.DrawLine(origin, origin - Vector3.up*yAxisWallCollisionDistance);
	}
	
	public float horizCheckOffset = 0.4f;
	bool horizCheck(float dv) {
		return (Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y-(horizCheckOffset)), Vector2.right, dv, groundMask) ||
		        Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y+(horizCheckOffset)), Vector2.right, dv, groundMask));
	}
	
	public float vertCheckOffset = 0.1f;
	bool vertCheck(float dv) {
		return (Physics2D.Raycast(new Vector2(transform.position.x-(vertCheckOffset),transform.position.y), Vector2.up, dv, groundMask) ||
		        Physics2D.Raycast(new Vector2(transform.position.x+(vertCheckOffset),transform.position.y), Vector2.up, dv, groundMask));
	}
	
	
	public float xAxisWallCollisionDistance = 0.3f;
	void moveRight(float amt) {
		float i = 0f;
		Vector3 step = new Vector3(0.001f, 0f);
		while (i < amt && !horizCheck(xAxisWallCollisionDistance)) {
			transform.Translate(step);
			i += 0.001f;
		}
	}
	void moveLeft(float amt) {
		float i = 0f;
		Vector3 step = new Vector3(-0.001f, 0f);
		while (i > amt && !horizCheck(-xAxisWallCollisionDistance)) {
			transform.Translate(step);
			i -= 0.001f;
		}
	}
	
	public float yAxisWallCollisionDistance = 0.5f;
	void rise(float amt) {
		float i = 0f;
		Vector3 step = new Vector3(0f, 0.001f);
		while (i < amt && !vertCheck(yAxisWallCollisionDistance)) {
			transform.Translate(step);
			i += 0.001f;
		}
	}
	void fall(float amt) {
		float i = 0f;
		Vector3 step = new Vector3(0f, -0.001f);
		while (i > amt && !vertCheck(-yAxisWallCollisionDistance)) {
			transform.Translate(step);
			i -= 0.001f;
		}
	}
	
	void restOnGround() {
		Vector3 step = new Vector3(0f, 0.001f);
		while (vertCheck(-yAxisWallCollisionDistance))
			transform.Translate(step);
		transform.Translate(-step);
	}
	
	bool floating;
	bool didFloat;
	public float floatSpeed = 1f;
	float jumpVx;
	bool initiatedJump;
	bool hasDoubleJump = false;
	
	void moveUpDown() {
		grounded = vertCheck(-yAxisWallCollisionDistance) && vy <= 0f;
		if (grounded) {
			restOnGround();
			vy = 0f;
			hasDoubleJump = true;
			// We can jump, here.
			if (Input.GetKeyDown(KeyCode.M)) {
				vy = highJumpEnabled ? highJumpStrength :jumpStrength;
				jumpVx = vx;
				initiatedJump = true;
			} else {
				jumpVx = 0f;
				initiatedJump = false;
			}
			didFloat = false;
			floating = false;
		} else {
			if (floating) {
				vy = -floatSpeed * Time.deltaTime;
			} else {
				vy -= gravityStrength*Time.deltaTime;
			}
			if (vy < -maxGravity)
				vy = -maxGravity;
			
			if (Input.GetKeyDown(KeyCode.M)) {
				if (hasDoubleJump && vy > 0f) {
					vy = jumpStrength*2/3f;
					hasDoubleJump = false;
				} else {
					if (!didFloat || floating) {
						didFloat = true;
						floating = !floating;
					}
				}
			} else if (Input.GetKeyUp(KeyCode.M) && vy > 0) {
				vy /= 2f;
			}
			
		}
		
		animator.SetBool("rising", vy > 0);
		animator.SetBool("falling", vy < 0);
		if (vy > 0) {
			rise(vy*Time.deltaTime);
		} else if (vy < 0) {
			fall(vy*Time.deltaTime);
			//			transform.Translate(new Vector3(0, vy*Time.deltaTime));
		}
	}
}