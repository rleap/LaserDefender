using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // Configuration parameters
    [Header("Player Stats")]
    [SerializeField] int health = 1000;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float xPadding = 1f;
    [SerializeField] float yPadding = 1f;

    [Header("Player Projectile")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFirePeriod = 0.2f;

    [Header("Player VFX")]
    [SerializeField] GameObject deathVFX;
    [SerializeField] float durationOfExplosion = 1f;

    [Header("Player SFX")]
    [SerializeField] AudioClip deathSound;
    [SerializeField] [Range(0, 1)] float deathSoundVolume = 0.75f;
    [SerializeField] AudioClip projectileSound;
    [SerializeField] [Range(0,1)] float projectileSoundVolume = 0.25f;

    Coroutine firingCoroutine;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    // Cached references


	// Use this for initialization  
	void Start () {
        SetMoveBoundries();

	}

    // Update is called once per frame
    void Update () {
        Move();
        Fire();

	}

    // Move player
    private void Move()
    {
        // Setup deltas on both axis
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        // Set new movement point and restrict to viewport boundaries
        var newXPos = Mathf.Clamp(transform.position.x + deltaX,xMin,xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY,yMin,yMax);

        // Apply movement to player
        transform.position = new Vector2(newXPos, newYPos);

    }

    // Set player move boundaries
    private void SetMoveBoundries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.transform.position.x - 5.5f + xPadding;
        xMax = gameCamera.transform.position.x + 5.5f - xPadding;
        yMin = gameCamera.transform.position.y - 10f + yPadding;
        yMax = gameCamera.transform.position.y + 10f - yPadding;
        //xMin = gameCamera.ScreenToViewportPoint(new Vector3(0, 0, 0)).x + xPadding;
        //xMax = gameCamera.ScreenToViewportPoint(new Vector3(1, 0, 0)).x - xPadding;
        //yMin = gameCamera.ScreenToViewportPoint(new Vector3(0, 0, 0)).y + yPadding;
        //yMax = gameCamera.ScreenToViewportPoint(new Vector3(0, 1, 0)).y - yPadding;
    }

    // Allow player to fire laser
    private void Fire()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }
        if(Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            ) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(
                projectileSound,
                Camera.main.transform.position,
                projectileSoundVolume
            );
            yield return new WaitForSeconds(projectileFirePeriod);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        GameObject explosion = Instantiate(deathVFX, transform.position, transform.rotation);
        Destroy(explosion, durationOfExplosion);
        AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position, deathSoundVolume);
        FindObjectOfType<Level>().LoadGameOver();
    }

    public int GetHealth()
    {
        return health;
    }
}

