using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;
    CharacterController controller;
    Animator animator;
    AudioSource audioSource;
    

    [Header("Controller")]
    public float moveSpeed = 5;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;
    public int maxHealth = 100;
    public int currentHealth;


    Vector3 _PlayerVelocity;
    bool isGrounded;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;
    float xRotation = 0f;

    [Header("Audio")]
    public AudioClip walkSound;
    bool isWalking = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        // Repeat Inputs
        if (input.Attack.IsPressed()) { Attack(); }
        if (input.Cast.IsPressed()) { Cast(); }

        SetAnimations();
        HandleWalkSound();

        // Test TakeDamage on Spacebar
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(5);
            Debug.Log("Health: " + currentHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        //audioSource.PlayOneShot(hurtSound);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        // audioSource.PlayOneShot(deathSound);
        // ChangeAnimationState("Death");
        // Disable player controls or trigger game over logic
        Debug.Log($"You are dead");
        enabled = false;
    }

    void FixedUpdate() { MoveInput(input.Movement.ReadValue<Vector2>()); }
    void LateUpdate() { LookInput(input.Look.ReadValue<Vector2>()); }

    // Player Movement
    void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        controller.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;

        if (isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;

        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    // Player Look
    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable() { input.Enable(); }
    void OnDisable() { input.Disable(); }

    // Jump Action
    void Jump()
    {
        if (isGrounded)
            _PlayerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }

    void AssignInputs()
    {
        input.Jump.performed += ctx => Jump();
        input.Attack.started += ctx => Attack();
        input.Cast.started += ctx => Cast();
    }

    // ANIMATION
    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";
    public const string SPELL = "spellCast";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        if (!attacking && !Casting)
        {
            if (_PlayerVelocity.x == 0 && _PlayerVelocity.z == 0)
            { ChangeAnimationState(IDLE); }
            else
            { ChangeAnimationState(WALK); }
        }
    }

    void HandleWalkSound()
    {
        bool isMoving = input.Movement.ReadValue<Vector2>().magnitude > 0;

        if (isMoving && isGrounded && !isWalking)
        {
            isWalking = true;
            audioSource.loop = true;
            audioSource.clip = walkSound;
            audioSource.Play();
        }
        else if ((!isMoving || !isGrounded) && isWalking)
        {
            isWalking = false;
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    // ATTACK
    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    public GameObject hitEffect;
    public GameObject SpellBall;
    public AudioClip swordSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    // CAST
    [Header("Casting")]
    public float castDistance = 20f;
    public float castDelay = 1f;
    public float castSpeed = 1f;
    public int castDamage = 2;

    bool readyToCast = true;
    bool Casting = false;

    public void Cast()
    {
        if (!readyToCast || Casting) return;

        readyToCast = false;
        Casting = true;

        Invoke(nameof(ResetCast), castSpeed);
        Invoke(nameof(CastRaycast), castDelay);

        ChangeAnimationState(SPELL);
    }

    void ResetCast()
    {
        Casting = false;
        readyToCast = true;
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            if (hit.transform.TryGetComponent<EnemyStats>(out EnemyStats target))
            {
                target.TakeDamage(attackDamage);
            }
        }
    }

    void CastRaycast()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, castDistance, attackLayer))
        {
            SpellHit(hit.point);

            if (hit.transform.TryGetComponent<EnemyStats>(out EnemyStats target))
            {
                target.TakeDamage(castDamage);
            }
        }
    }

    void SpellHit(Vector3 pos)
    {
        GameObject GO = Instantiate(SpellBall, pos, Quaternion.identity);
        Destroy(GO, 20);
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }
}
