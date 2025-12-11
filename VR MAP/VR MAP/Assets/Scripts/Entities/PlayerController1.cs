using System.ComponentModel.Design;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")] [SerializeField] private Camera cam;
    [Header("Movement")]
    [SerializeField] private float camSensitivity = 20;
    [SerializeField] private float moveSensitivity = 3;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 5f;


    [Header("Inputs")]
    [SerializeField] private InputActionReference zqsd;
    [SerializeField] private InputActionReference powerUp;
    [SerializeField] private InputActionReference mouseMovement;
    [SerializeField] private InputActionReference fire;
    [SerializeField] private InputActionReference jump;
    
    [Header("GroundCheck")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundCheckMask;

    [Header("Weapons")]
    [SerializeField] private GunShooter gun;


    [SerializeField]  public PlayerStats stats;


    private CharacterController controller;
    private float rotationX = 0.0f;
    private bool isGrounded = false;
    private Vector3 velocity = Vector3.zero;

    private bool isSpeedBoostActive = false;

    private bool stunEnable = false;
    private bool speedBoostEnable = false;
    private bool shockwaveEnable = false;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stats = GetComponent<PlayerStats>();
        if (zqsd)
        {
            zqsd.action.Enable();
        }

        if (powerUp)
        {
            powerUp.action.performed += PowerUpPressed;
            powerUp.action.Enable();
        }

        if (mouseMovement)
        {
            mouseMovement.action.Enable();
        }

        if (jump)
        {
            jump.action.performed += JumpPressed;
            jump.action.Enable();
        }


        if (fire)
        {
            fire.action.performed += FirePressed;
            fire.action.Enable();
        }

        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ApplyPowerUp(string powerName)
    {
        if (powerName == "Stun")
        {
            if(!stunEnable)
            {
                stunEnable = true;
            }
            else
            {
                stats.stunDuration += 1f;
            }
        }
        else if (powerName == "SpeedBoost")
        {

            if (!speedBoostEnable)
            {
                speedBoostEnable = true;
            }
            else
            {
                stats.speedBoostMultiplier += 0.5f; 
                stats.speedBoostDuration += 1f; 
            }

        }else if (powerName == "Shockwave")
        {
            if (!shockwaveEnable)
            {
                shockwaveEnable = true;
            }
            else
            {
                stats.shockwaveDamage += 10f;
                stats.shockwaveRadius += 1f;
            }
        }
    }


    private void PowerUpPressed(InputAction.CallbackContext obj)
    {
        var control = obj.control;

        if (stunEnable && control.name == "q")
        {
            StunAround();
        }
        else if (speedBoostEnable && control.name == "e")
        {
            SpeedBoost();
        }
        PowerSelectionManager tmp = FindObjectOfType<PowerSelectionManager>();
        if(tmp != null)
        {
            tmp.ShowSelection();
        }

    }

    private void StunAround()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.shockwaveRadius);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Stun(stats.stunDuration);
            }
        }
    }

    private void SpeedBoost()
    {
        if (!isSpeedBoostActive)
        {
            StartCoroutine(SpeedBoostRoutine());
            isSpeedBoostActive = true;
        }
    }


    private System.Collections.IEnumerator SpeedBoostRoutine()
    {

        float baseSpeed = stats.moveSpeed;
        stats.moveSpeed *= stats.speedBoostMultiplier;  

        yield return new WaitForSeconds(stats.speedBoostDuration);

        stats.moveSpeed = baseSpeed; 
        isSpeedBoostActive = false;

    }


    private void DoShockwave()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.shockwaveRadius);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(stats.shockwaveDamage);

                Vector3 dir = (enemy.transform.position - transform.position).normalized;

                enemy.Knockback(dir, 10f, 1f); 
            }
        }
    }



    private void JumpPressed(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            if (shockwaveEnable)
            {
                DoShockwave();
            }
        }
    }

    



    private void FirePressed(InputAction.CallbackContext obj)
    {
        gun.Shoot(stats.attackDamage);
    }

    // Update is called once per frame
    void Update()
    {

        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundCheckMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float mouseX = mouseMovement.action.ReadValue<Vector2>().x * camSensitivity * Time.deltaTime;
        float mouseY = mouseMovement.action.ReadValue<Vector2>().y * camSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
        
        Vector2 zqsdValue = zqsd.action.ReadValue<Vector2>();
        controller.Move(transform.TransformDirection(new Vector3(zqsdValue.x, 0, zqsdValue.y)).normalized * moveSensitivity * stats.moveSpeed * Time.deltaTime);
        
        velocity.y += gravity * Time.deltaTime *2f;
        controller.Move(velocity * Time.deltaTime);
    }
}