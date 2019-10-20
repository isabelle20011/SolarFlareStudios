﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameManager;

public class PlayerHealth : MonoBehaviour
{
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    [SerializeField] private int startingHealth = 1;                              // The amount of health the player starts the game with.
    [SerializeField] private int currentHealth;                                   // The current health the player has.
    [SerializeField] private int maxHealth = 4;
    public AudioClip deathClip;
    public AudioClip damageClip;
    public AudioClip healingClip;

    private Animator anim;                                              // Reference to the Animator component.
    private AudioSource playerAudio;                                    // Reference to the AudioSource component.
    private CharacterMovement playerMovement;                              // Reference to the player's movement.
    private Rigidbody rigidbody;
    private bool isDead;
    private int playerLives;

    public float Health { get { return currentHealth; } }
    public float MaxHealth { get { return maxHealth; } }

    #region Sigleton
    private static PlayerHealth instance;
    public static PlayerHealth Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PlayerHealth>();
            return instance;
        }
    }
    #endregion

    private void Awake()
    {
        // Setting up the references.
        anim = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<AudioSource>();
        playerMovement = GetComponent<CharacterMovement>();
        rigidbody = GetComponent<Rigidbody>();
        // Set the initial health of the player.
        currentHealth = startingHealth;
    }

    private void Start()
    {
        playerLives = GameManager_Master.Instance.playerLives;
    }

    public void AddHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += 1;

            if (healingClip != null)
            {
                playerAudio.clip = healingClip;
                playerAudio.Play();
            }

            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }
    }


    private void Update()
    {
    }


    public void TakeDamage()
    {
        currentHealth--;

        ClampHealth();
        if (currentHealth <= 0 && !isDead)
        {
            // ... it should die.
            Death();
        }

        if (damageClip != null && !isDead)
        {
            playerAudio.clip = damageClip;
            playerAudio.Play();
        }
    }


    public void Death()
    {
        isDead = true;

        anim.SetTrigger("Die");
        currentHealth = 0;
        ClampHealth();

        if (deathClip != null)
        {
            playerAudio.clip = deathClip;
            playerAudio.Play();
        }

        Destroy(gameObject, 4f);
        rigidbody.isKinematic = true;
        //playerMovement.enabled = false;
        playerLives--;
        if (playerLives >= 0)
        {
            GameManager_Master.Instance.CallPlayerDied();
			print("called");
        }
        else
        {
            GameManager_Master.Instance.CallEventGameOver();
        }
    }


    private void ClampHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);

        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }

    public bool PlayerAlive()
    {
        return !isDead;
    }
}
