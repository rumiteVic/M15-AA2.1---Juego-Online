using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundDetector))]
public class CharacterMover : NetworkBehaviour
{
    public Camera cam;
    public float movementAcceleration;
    public float movementDeceleration;
    Vector3 currentMov;
    public float speedMovement;
    public float speedTurn;
    public float jumpForce;
    Rigidbody rb;
    GroundDetector gd;
    public float airSpeedFollowup = 1f;
    float airSpeedFollowupCurrent;
    public Vector3 velocity { get; private set; }
    public float velocityAngular { get; private set; }
    public Vector3 velocityAxis { get; private set; }
    Quaternion velocityRotation;
    Vector3 lastPos;
    Quaternion lastRot;

    public Transform followStuff;
    // Start is called before the first frame update
     void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gd = GetComponent<GroundDetector>();
        gd.groundedUp.AddListener(DroppedOff);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.enabled = false;
            return;
            //Destroy(GetComponent<Rigidbody>());
        }
        cam = Camera.main;
        //Hacer que la camara lo siga pasandole sus cosas seleccionando la camara
        //Más en concreto al padre de la camara que tiene esos scripts
        CameraController camCtrl = cam.GetComponentInParent<CameraController>();
        ObjectFollower follower = cam.GetComponentInParent<ObjectFollower>();
        //Hacemos que lo siga
        follower.follow = followStuff;
        //Y hacemos que se coloque bien
        follower.transform.position = followStuff.position + follower.offset;
        //La camara pone de target al gameObject vacio dentro del player para que lo siga
        camCtrl.SetTarget(followStuff);

        RelayManager.players.Add(this);
    }
    private void Update()
    {
        if(!IsOwner) return;
        if (gd.grounded && InputManager.actions.Player.Jump.WasPressedThisFrame())
        {
            rb.linearVelocity = transform.up * jumpForce;
        }
    }
    void FixedUpdate()
    {
        if(!IsOwner) return;
        Velocity();
        Movement();

        lastPos = transform.position;
        lastRot = transform.rotation;
    }
    void Velocity()
    {
        velocity = transform.InverseTransformDirection((transform.position - lastPos) / Time.fixedDeltaTime);
        velocityRotation = Quaternion.Inverse(lastRot) * transform.rotation;
        float _velocityAngular;
        Vector3 _velocityAxis;
        velocityRotation.ToAngleAxis(out _velocityAngular, out _velocityAxis);
        velocityAngular = _velocityAngular / Time.fixedDeltaTime;
        velocityAxis = _velocityAxis;
        if (Vector3.Dot(velocityAxis, transform.up) < 0)
        {
            velocityAngular *= -1;
        }

        airSpeedFollowupCurrent = Mathf.Clamp(airSpeedFollowupCurrent + Time.fixedDeltaTime, 0, airSpeedFollowup);
    }
    void Movement()
    {
        if (gd.grounded)
        {
            Vector3 mov = InputManager.actions.Player.Move.ReadValue<Vector2>();
            float magnitude = Mathf.Clamp01(mov.magnitude);
            if (magnitude > 0)
            {
                Vector3 movForward = cam.transform.forward * mov.y;
                Vector3 movRight = cam.transform.right * mov.x;

                mov = movForward + movRight;

                mov.y = 0;

                mov = mov.normalized * magnitude;
            }

            Debug.DrawRay(transform.position, mov, Color.yellow);
            if (magnitude > currentMov.magnitude)
            {
                currentMov = Vector3.Lerp(currentMov, mov, movementAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentMov = Vector3.Lerp(currentMov, mov, movementDeceleration * Time.fixedDeltaTime);
            }

            Debug.DrawRay(transform.position, currentMov, Color.green);
            Quaternion rot = currentMov.magnitude > 0.01f ? Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentMov), speedTurn * Time.fixedDeltaTime) : transform.rotation;
            rb.Move(rb.position + currentMov * speedMovement * Time.fixedDeltaTime, rot);
        }
    }
    void DroppedOff()
    {
        if (airSpeedFollowupCurrent > 0)
        {
            rb.linearVelocity += transform.TransformDirection(velocity * airSpeedFollowupCurrent - rb.linearVelocity);
        }
        airSpeedFollowupCurrent = 0;
    }
}
