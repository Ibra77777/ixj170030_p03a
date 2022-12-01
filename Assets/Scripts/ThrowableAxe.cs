using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

[RequireComponent(typeof(Animator))]
public class ThrowableAxe : MonoBehaviour
{

    private Animator animator;
    private PlayerMovement input;
    private Rigidbody weaponRb;
    private Weapon weaponScript;
    private float returnTime;

    private Vector3 origLocPos;
    private Vector3 origLocRot;
    private Vector3 pullPosition;

    [Header("Public References")]
    public Transform weapon;
    public Transform hand;
    public Transform curvePoint;
    
    [Header("Parameters")]
    public float throwPower = 30;
    public float cameraZoomOffset = .3f;
    
    [Header("Bools")]
    public bool walking = true;
    public bool aiming = false;
    public bool hasWeapon = true;
    public bool pulling = false;
    
    void Start()
    {
        Cursor.visible = false;

        animator = GetComponent<Animator>();
        input = GetComponent<PlayerMovement>();
        weaponRb = weapon.GetComponent<Rigidbody>();
        weaponScript = weapon.GetComponent<Weapon>();
        origLocPos = weapon.localPosition;
        origLocRot = weapon.localEulerAngles;

    }

    void Update()
    {

        //If aiming rotate the player towards the camera foward, if not reset the camera rotation on the x axis
        if (aiming)
        {
            input.RotateToCamera(transform);
        }
        else
        {
            transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles.x, 0, .2f), transform.eulerAngles.y, transform.eulerAngles.z);
        }

        //Animation States
        animator.SetBool("pulling", pulling);
        walking = input.Speed > 0;
        animator.SetBool("walking", walking);


        if (Input.GetMouseButtonDown(1) && hasWeapon)
        {
            Aim(true, true, 0);
        }

        if (Input.GetMouseButtonUp(1) && hasWeapon)
        {
            Aim(false, true, 0);
        }

        if (hasWeapon)
        {

            if (aiming && Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("aim");
            }

        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                WeaponStartPull();
            }
        }

        if (pulling)
        {
            if (returnTime < 1)
            {
                weapon.position = GetQuadraticCurvePoint(returnTime, pullPosition, curvePoint.position, hand.position);
                returnTime += Time.deltaTime * 1.5f;
            }
            else
            {
                WeaponCatch();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }
    }

    void Aim(bool state, bool changeCamera, float delay)
    {

        if (walking)
            return;

        aiming = state;

        animator.SetBool("aim", aiming);

        if (!changeCamera)
            return;

        //Camera Offset
        float newAim = state ? cameraZoomOffset : 0;
        float originalAim = !state ? cameraZoomOffset : 0;

    }

    public void WeaponThrow()
    {
        Aim(false, true, 1f);

        hasWeapon = false;
        weaponScript.activated = true;
        weaponRb.isKinematic = false;
        weaponRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        weapon.parent = null;
        weapon.eulerAngles = new Vector3(0, -90 + transform.eulerAngles.y, 0);
        weapon.transform.position += transform.right / 5;
        weaponRb.AddForce(Camera.main.transform.forward * throwPower + transform.up * 2, ForceMode.Impulse);
    }

    public void WeaponStartPull()
    {
        pullPosition = weapon.position;
        weaponRb.Sleep();
        weaponRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        weaponRb.isKinematic = true;
        weaponScript.activated = true;
        pulling = true;
    }

    public void WeaponCatch()
    {
        returnTime = 0;
        pulling = false;
        weapon.parent = hand;
        weaponScript.activated = false;
        weapon.localEulerAngles = origLocRot;
        weapon.localPosition = origLocPos;
        hasWeapon = true;

    }

    public Vector3 GetQuadraticCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}