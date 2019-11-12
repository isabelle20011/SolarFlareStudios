﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// This will be the navigation manager for each agent outside of the 
/// actual movement calculation which are controlled by NavMeshAgent
/// </summary>
public class Navigation : MonoBehaviour
{
	/// <summary>
	/// Should we be doing patrolling behavior
	/// </summary>
	public bool patrol = true;

	/// <summary>
	/// If we should be patrolling, which points should we patrol between
	/// </summary>
	public List<GameObject> patrolSpots = new List<GameObject>();
	public float multiplyBy;
	public int maxAI = 1;

	private int healthAI;

	/// <summary>
	/// The player we want to find and destroy
	/// </summary>
	private GameObject player;

	/// <summary>
	/// Used for distance calculations
	/// </summary>
	private Transform myTransform;

	/// <summary>
	/// Used to move us to places
	/// </summary>
	private NavMeshAgent agent;

	/// <summary>
	/// Which patrol spot is next
	/// </summary>
	private int patrolIndex = 0;

	/// <summary>
	/// Where'd you go cotton eye joe
	/// </summary>
	private Transform curDest = null;

	/// <summary>
	/// Are we currently seeking a patrol point
	/// </summary>
	private bool seekingPatrol = false;

	/// <summary>
	/// Are we currently seeking a player
	/// </summary>
	private bool seekingPlayer = false;

	private bool runningAway = false;

	//animator variables
	private Animator animator;
	private bool attack;
	private bool died;

	// Start is called before the first frame update
	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		myTransform = GetComponent<Transform>();
		animator = GetComponent<Animator>();
		player = GameObject.FindGameObjectWithTag("Player");
		healthAI = maxAI;

		if (!animator)
		{
			Debug.LogWarning("no animator in AI");
		}

	}

	// Update is called once per frame
	void Update()
	{
		if (player == null)
		{
			agent.enabled = false;
			attack = false;
		}

		if (CanMove())
		{
			//If the player gets too close, attack!
			if (maxAI > 1 && healthAI <= maxAI / 2 && Vector3.Distance(myTransform.position, player.transform.position) < 10)
			{
				runningAway = true;
				seekingPlayer = false;
				seekingPatrol = false;
				RunFrom(player.transform);
			}
			else if (Vector3.Distance(myTransform.position, player.transform.position) < 10)
			{
				seekingPlayer = true;
				seekingPatrol = false;
				runningAway = false;
				Seek(player.transform);
			}
			//if the player moved too far away, return to nearest patrol point
			else if (seekingPlayer && Vector3.Distance(myTransform.position, player.transform.position) >= 10)
			{
				seekingPlayer = false;
				runningAway = false;
				float closestDist = 999999999;
				for (int i = 0; i < patrolSpots.Count; i++)
				{
					float curDist = Vector3.Distance(myTransform.position, patrolSpots[i].transform.position);
					if (curDist < closestDist)
					{
						closestDist = curDist;
						patrolIndex = i;
					}
				}
			}
			if (patrol)
			{
				if (!seekingPatrol && !seekingPlayer && !runningAway)
				{
					curDest = patrolSpots[patrolIndex % patrolSpots.Count].GetComponent<Transform>();
					patrolIndex++;
					Seek(curDest);
					seekingPatrol = true;
				}
				else if (Vector3.Distance(myTransform.position, curDest.position) < 6)
				{
					seekingPatrol = false;
				}
			}
		}
		Debug.Log(seekingPlayer);
		animator.SetBool("Seeking", seekingPlayer);
		animator.SetBool("Patrol", seekingPatrol);
		animator.SetBool("RunningAway", runningAway);
		animator.SetBool("Attack", attack);
	}

	private bool CanMove()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Damage") || this.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")
			|| animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			agent.destination = this.transform.position;
			return false;
		}
		return true;
	}

	/// <summary>
	/// Seek the specified destination.
	/// </summary>
	/// <param name="destination">Where should we go next</param>
	private void Seek(Transform destination)
	{
		agent.destination = destination.position;
		if (Vector3.Distance(myTransform.position, player.transform.position) < 2)
		{
			attack = true;
		}
		else
		{
			attack = false;
		}
	}

	public void RunFrom(Transform playerPos)
	{
		transform.rotation = Quaternion.LookRotation(transform.position - playerPos.position);

		Vector3 runTo = transform.position + transform.forward * multiplyBy;

		NavMeshHit hit;
		NavMesh.SamplePosition(runTo, out hit, 5, 1 << NavMesh.GetAreaFromName("Default"));

		agent.SetDestination(hit.position);
	}

	private void OnTriggerEnter(Collider other)
	{
		// If the entering collider is the player OR in our case the sword
		/*if (other.gameObject == player)
		{
			PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
			if (playerMovement)
			{
				if (playerMovement.IsAttacking())
				{
					if (healthAI <= 1)
					{
						died = true;
						animator.SetBool("Died", died);
						agent.enabled = false;
						healthAI = 0;
						Destroy(gameObject, 4f);
					}
					else
					{
						animator.SetTrigger("Damaged");
						healthAI--;
					}
				}
			}
		}*/
	}

	/*public void ExecuteAttackSound()
	{
		EventManager.TriggerEvent<attackSoundEvent, Vector3>(transform.position);
	}

	public void ExecuteDamageSound()
	{
		EventManager.TriggerEvent<damageSoundEvent, Vector3>(transform.position);
	}*/
}
