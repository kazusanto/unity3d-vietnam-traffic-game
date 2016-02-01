using UnityEngine;
using System.Collections;

public class CabController : MonoBehaviour
{
    [SerializeField] private bool m_isPlayer = false;
    [SerializeField] private float m_MaxSpeed = 5.0f;
    [SerializeField] private float m_MaxSteering = 1.5f;
    [SerializeField] private float m_Accel = 0.1f;
    [SerializeField] private float m_Brake = 0.1f;
    [SerializeField] private GameObject[] m_Baggages = null;
    [SerializeField] private string m_BaggageOriginName = "CabBody";
    [SerializeField] private Vector3 m_BaggageOrigin = Vector3.zero;

    private Animator m_animator = null;
    private float m_steering = 0.0f;
    private float m_speed = 0.0f;

    // Use this for initialization
    void Start () {
        m_animator = GetComponent<Animator>();
        GameObject baggage_obj = null;
        if (m_Baggages.Length > 0) {
            var i = Random.Range(0, m_Baggages.Length + 1);
            if (i < m_Baggages.Length) {
                baggage_obj = GameObject.Instantiate(m_Baggages[i]);
                var baggage = baggage_obj.transform;
                var seat = transform.Find(m_BaggageOriginName);
                var joint = new GameObject("Joint").transform;
                joint.localPosition = new Vector3(
                    m_BaggageOrigin.x / seat.localScale.x,
                    m_BaggageOrigin.z / seat.localScale.z,
                    m_BaggageOrigin.y / seat.localScale.y);
                joint.localScale = new Vector3(
                    1.0f / seat.localScale.x,
                    1.0f / seat.localScale.z,
                    1.0f / seat.localScale.y);
                joint.SetParent(seat, false);
                baggage.SetParent(joint, false);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (!m_isPlayer) {
            return;
        }
        var steered = false;
        var accel = 0.0f;
        var brake = 0.0f;
        if (Input.GetKey(KeyCode.UpArrow)) {
            accel = 1.0f;
            if (Input.GetKey(KeyCode.RightArrow)) {
                m_steering += 0.1f;
                if (m_steering > 1.0f) {
                    m_steering = 1.0f;
                }
                steered = true;
            } else if (Input.GetKey(KeyCode.LeftArrow)) {
                m_steering -= 0.1f;
                if (m_steering < -1.0f) {
                    m_steering = -1.0f;
                }
                steered = true;
            }
        } else {
            brake = 1.0f;
        }
        if (steered == false) {
            if (m_steering > 0.1f) {
                m_steering -= 0.1f;
            } else if (m_steering < -0.1f) {
                m_steering += 0.1f;
            } else {
                m_steering = 0.0f;
            }
        }
        Move(m_steering, accel, brake, brake);
    }

    public void Move(float steering, float accel, float footbrake, float handbrake) {
        if (steering > 1.0f) {
            steering = 1.0f;
        }
        if (steering < -1.0f) {
            steering = -1.0f;
        }
        m_speed += accel * m_Accel;
        if (handbrake > 0.0f || footbrake > 0.0f) {
            m_speed -= (handbrake + footbrake) / 2.0f * m_Brake;
        }
        var max = 1.0f - Mathf.Abs(steering) * 0.3f;
        if (m_speed > max) {
            m_speed = max;
        }
        if (m_speed < 0.0f) {
            m_speed = 0.0f;
        }
        transform.Rotate(new Vector3(0, steering * m_MaxSteering * m_speed, 0));
        var forward = transform.forward.normalized;
        transform.Translate(forward * m_speed * m_MaxSpeed * Time.deltaTime, Space.World);
        if (transform.position.y < 0.0f || transform.position.y > 0.15f) {
            transform.Translate(Vector3.down * transform.position.y);
        }
        m_animator.SetFloat("direction", steering);
        m_animator.SetFloat("speed", m_speed);
    }
}
