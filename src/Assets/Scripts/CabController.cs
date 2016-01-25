using UnityEngine;
using System.Collections;

public class CabController : MonoBehaviour
{
    public bool isPlayer = false;
    public float MaxSpeed = 5.0f;
    public float MaxSteering = 20.0f;
    public float Brake = 0.1f;


    private Animator animator = null;
    private float steering = 0.0f;
    private float speed = 0.0f;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update () {
        if (!isPlayer) {
            return;
        }
        var steered = false;
        var accel = 0.0f;
        var brake = 0.0f;
        if (Input.GetKey(KeyCode.UpArrow)) {
            accel = 1.0f;
            if (Input.GetKey(KeyCode.RightArrow)) {
                steering += 0.1f;
                if (steering > 1.0f) {
                    steering = 1.0f;
                }
                steered = true;
            } else if (Input.GetKey(KeyCode.LeftArrow)) {
                steering -= 0.1f;
                if (steering < -1.0f) {
                    steering = -1.0f;
                }
                steered = true;
            }
        } else {
            brake = 1.0f;
        }
        if (steered == false) {
            if (steering > 0.1f) {
                steering -= 0.1f;
            } else if (steering < -0.1f) {
                steering += 0.1f;
            } else {
                steering = 0.0f;
            }
        }
        Move(steering, accel, brake, brake);
    }

    void FixedUpdate() {
//        print(Time.deltaTime);
//        Vector3 eulerAngleVelocity = new Vector3(0.0f, steering * MaxSteering, 0.0f);
//        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
//        rigid.MoveRotation(rigid.rotation * deltaRotation);

//        var vec = new Vector3(0.0f, -100.0f, 10000.0f);
//        rigid.AddForce(vec * Time.deltaTime, ForceMode.Force);
        //animator.SetFloat("speed", 1.0f);
    }

    public void Move(float steering, float accel, float footbrake, float handbrake) {
        if (steering > 1.0f) {
            steering = 1.0f;
        }
        if (steering < -1.0f) {
            steering = -1.0f;
        }
        speed += accel;
        if (handbrake > 0.0f || footbrake > 0.0f) {
            speed -= (handbrake + footbrake) * Brake;
        }
        if (speed > 1.0f) {
            speed = 1.0f;
        }
        if (speed < 0.0f) {
            speed = 0.0f;
        }
        transform.Rotate(new Vector3(0, steering * MaxSteering * speed, 0));
        var forward = transform.forward.normalized;
        transform.Translate(forward * speed * MaxSpeed * Time.deltaTime, Space.World);
/*
        // Vector3 eulerAngleVelocity = new Vector3(0.0f, steering * MaxSteering, 0.0f);
        // Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
        //rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
*/
        animator.SetFloat("direction", steering);
        animator.SetFloat("speed", speed);
    }
}
