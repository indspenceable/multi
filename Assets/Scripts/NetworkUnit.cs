﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkUnit : NetworkBehaviour {
	[SyncVar] public UnitCommand currentCommand = new UnitCommand(UnitCommand.CommandType.STOP);
	[SyncVar] public float speed = 1;
	[SyncVar] public int hp;

	[SyncVar] public float attackRange = 1f;
	[SyncVar] public float sightRange = 3f;

	// Server Only
	public NetworkUnit currentAttackTarget = null;
	public LayerMask targetLayer;

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, sightRange);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, attackRange);
		if (currentAttackTarget != null) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(currentAttackTarget.transform.position, 1f);
		}
		if (currentCommand.command != UnitCommand.CommandType.STOP) {
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(currentCommand.target, 0.5f);
		}
	}

	void Start() {
		if (isServer) {
			StartCoroutine(ActionLoop());
		}
	}

	IEnumerator ActionLoop() {
		while (true) {
			if (currentCommand.command == UnitCommand.CommandType.MOVE) {
				// Move command ensures that we have no attack target.
				currentAttackTarget = null;

				// If we finish moving, transition to stop.
				if (MoveToward(currentCommand.target)) {
					currentCommand = new UnitCommand(UnitCommand.CommandType.STOP);
				}
			} else if (currentCommand.command == UnitCommand.CommandType.ATTACK_MOVE) {
				// Ensure I can still see my target, and try to find me a new one if not
				// or I didn't have one to begin with.
				currentAttackTarget = IdentifyTarget();

				if (currentAttackTarget != null) {
					// If I have a target to attack: attack it
					yield return AttackOrApproachTarget();
				} else {
					// Otherwise, move!
					if (MoveToward(currentCommand.target)) {
						currentCommand = new UnitCommand(UnitCommand.CommandType.STOP);
					}
				}
			} else {
				// STOP command.
				// Is there something nearby?
				currentAttackTarget = IdentifyTarget();

				if (currentAttackTarget != null) {
					// If I have a target to attack: attack it
					yield return AttackOrApproachTarget();
				}
				// Otherwise, do nothing!
			}

			// Only do this once a frame.
			yield return new WaitForEndOfFrame();
		}
	}

	// return false if we can't see, or no target.
	private IEnumerator AttackOrApproachTarget() {
		float dist = Vector3.Distance(currentAttackTarget.transform.position, transform.position) -
			currentAttackTarget.GetComponent<CircleCollider2D>().radius;
		if (dist <= attackRange) {
			// TODO attack here.
			FaceTowards(currentAttackTarget.transform.position);
			yield return Attack(currentAttackTarget);
		} else if (dist <= sightRange) {
			MoveToward(currentAttackTarget.transform.position);
		} else {
			currentAttackTarget = null;
		}
	}

	public virtual void takeDamage(int damage) {
		this.hp -= damage;
	}



	public virtual IEnumerator Attack(NetworkUnit target) {
		GetComponent<Animator>().SetBool("IsAttacking", true);
		target.takeDamage(1);
		yield return new WaitForSeconds(1f);
		GetComponent<Animator>().SetBool("IsAttacking", false);
	}


	public virtual void FaceTowards(Vector3 point) {
		float x = (point - transform.position).x;
		RpcSetFlipX(x > 0);
	}

	[ClientRpc]
	public void RpcSetFlipX(bool flip) {
		GetComponent<SpriteRenderer>().flipX = flip;
	}

	private NetworkUnit IdentifyTarget() {
		if (currentAttackTarget) {
			if (Vector3.Distance(currentAttackTarget.transform.position, transform.position) <= sightRange) {
				return currentAttackTarget;
			}
		}
		// Look for a nearby unit.
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, sightRange, Vector3.right, 0f, targetLayer);
		if (hits.Length > 0) {
			return hits[0].transform.gameObject.GetComponent<NetworkUnit>();
		} else {
			return null;
		}
	}

	private bool MoveToward(Vector3 target) {
		Vector3 dist = target - transform.position;
		FaceTowards(target);
		if (dist.magnitude > speed*Time.deltaTime) {
			transform.position += dist.normalized * speed *Time.deltaTime;
			return false;
		} else {
			transform.position = currentCommand.target;
			return true;
		}
	}
}
