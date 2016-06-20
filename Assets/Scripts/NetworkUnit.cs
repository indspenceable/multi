using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkUnit : NetworkBehaviour {
	[SyncVar] public UnitCommand currentCommand = new UnitCommand(UnitCommand.CommandType.STOP);
	[SyncVar] public float speed = 1;
	[SyncVar] public int hp;
	public int maxHp;

	[SyncVar] public float attackRange = 1f;
	[SyncVar] public float sightRange = 3f;

	// Server Only
	public NetworkUnit currentHealTarget = null;
	public NetworkUnit currentAttackTarget = null;
	public LayerMask attackTargetLayers;
	public LayerMask healTargetLayers;
	public bool canHeal = false;
	public bool canAttack = true;

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
			hp = maxHp;
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
				currentAttackTarget = IdentifyTargetToAttack();

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
				if (canHeal) {
					currentHealTarget = IdentifyTargetToHeal();
					if (currentHealTarget != null) {
						// If I have a target to attack: attack it
						yield return HealOrApproachTarget();
					}
				} else if (canAttack) {
					currentAttackTarget = IdentifyTargetToAttack();
					if (currentAttackTarget != null) {
						// If I have a target to attack: attack it
						yield return AttackOrApproachTarget();
					}
				}
				// Otherwise, do nothing!
			}

			// Only do this once a frame.
			yield return new WaitForEndOfFrame();
		}
	}
		
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

	private IEnumerator HealOrApproachTarget() {
		float dist = Vector3.Distance(currentHealTarget.transform.position, transform.position) -
			currentHealTarget.GetComponent<CircleCollider2D>().radius;
		if (dist <= attackRange) {
			// TODO attack here.
			FaceTowards(currentHealTarget.transform.position);
			yield return Heal(currentHealTarget);
		} else if (dist <= sightRange) {
			MoveToward(currentHealTarget.transform.position);
		} else {
			currentAttackTarget = null;
		}
	}

	public virtual void takeDamage(int damage) {
		hp = Mathf.Clamp(hp-damage, 0, maxHp);
	}
		
	public virtual IEnumerator Attack(NetworkUnit target) {
		GetComponent<Animator>().SetBool("IsAttacking", true);
		target.takeDamage(1);
		yield return new WaitForSeconds(1f);
		GetComponent<Animator>().SetBool("IsAttacking", false);
	}

	public virtual IEnumerator Heal(NetworkUnit target) {
		GetComponent<Animator>().SetBool("IsHealing", true);
		target.takeDamage(-1);
		yield return new WaitForSeconds(5f);
		GetComponent<Animator>().SetBool("IsHealing", false);
	}

	public virtual void FaceTowards(Vector3 point) {
		float x = (point - transform.position).x;
		RpcSetFlipX(x > 0);
	}

	[ClientRpc]
	public void RpcSetFlipX(bool flip) {
		GetComponent<SpriteRenderer>().flipX = flip;
	}

	private NetworkUnit IdentifyTargetToAttack() {
		if (currentAttackTarget) {
			if (Vector3.Distance(currentAttackTarget.transform.position, transform.position) <= sightRange) {
				return currentAttackTarget;
			}
		}
		// Look for a nearby unit.
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, sightRange, Vector3.right, 0f, attackTargetLayers);
		if (hits.Length > 0) {
			return hits[0].transform.gameObject.GetComponent<NetworkUnit>();
		} else {
			return null;
		}
	}

	private NetworkUnit IdentifyTargetToHeal() {
		if (currentHealTarget) {
			if (Vector3.Distance(currentHealTarget.transform.position, transform.position) <= sightRange) {
				return currentAttackTarget;
			}
		}
		// Look for a nearby unit.
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, sightRange, Vector3.right, 0f, healTargetLayers);
		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].transform.gameObject != gameObject) {
				return hits[i].transform.gameObject.GetComponent<NetworkUnit>();
			}
		}
		return null;
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
