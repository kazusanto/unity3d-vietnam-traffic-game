using UnityEngine;
using System.Collections;
using Game;

public class CameraController : MonoBehaviour {

    enum Status {
        Undefine,
        Normal,
        Reverse,
        Transition
    };

    [SerializeField] float m_Distance = 5.0f;
    [SerializeField] float m_Tilt = 10.0f;
    [SerializeField] float m_Rise = 1f;
    [SerializeField] float m_Shift = 1.5f;
    [SerializeField] float m_TransitDuration = 1.5f;

    GameObject m_target = null;
    PlayerController m_player = null;
    Vector3 m_normalPosition;
    Vector3 m_reversePosition;
    Vector3 m_transitionPosition;
    Vector3 m_normalAngle;
    Vector3 m_reverseAngle;
    Vector3 m_transitionAngle;
    Vector3 m_currentPosition;
    Vector3 m_currentAngle;
    Status m_status = Status.Normal;
    Status m_status_to = Status.Undefine;
    float m_TransitBegin = 0.0f;

    // Use this for initialization
	void Start () {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_player = m_target.GetComponent<PlayerController>();

        var x = m_Shift;
        var y = m_Distance * Mathf.Sin(m_Tilt * Mathf.Deg2Rad) + m_Rise;
        var z = -m_Distance * Mathf.Cos(m_Tilt * Mathf.Deg2Rad);
        m_normalPosition = new Vector3(x, y, z);
        m_normalAngle = new Vector3(m_Tilt, 0.0f, 0.0f);
        m_reversePosition = new Vector3(x, y, -z);
        m_reverseAngle = new Vector3(m_Tilt, 180.0f, 0.0f);
        var tx = -m_Distance * 1.5f * Mathf.Cos(m_Tilt * 3.0f * Mathf.Deg2Rad) - m_Shift;
        var ty = m_Distance * 1.5f  * Mathf.Sin(m_Tilt * 3.0f * Mathf.Deg2Rad) + m_Rise;
        var tz = 0.0f;
        m_transitionPosition = new Vector3(tx, ty, tz);
        m_transitionAngle = new Vector3(m_Tilt * 3.0f, 90.0f, 0.0f);

        m_currentPosition = m_normalPosition;
        m_currentAngle = m_normalAngle;
        m_status = Status.Normal;
        m_status_to = Status.Undefine;
        m_TransitBegin = 0.0f;
	}
	
	// LateUpdate is called once per frame after other Updates
	void LateUpdate () {
        var pos = m_currentPosition;
        var angle = m_currentAngle;
        if (m_status_to == Status.Undefine) {
            if (m_player.GetLookingFor() <= 0.0f) {
                m_status_to = Status.Normal;
            } else {
                m_status_to = Status.Reverse;
            }
            if (m_status != m_status_to) {
                if (m_status != Status.Transition) {
                    m_status_to = Status.Transition;
                }
                m_TransitBegin = Time.time;
            }
        }
        switch (m_status_to) {
        case Status.Normal:
            pos = m_normalPosition;
            angle = m_normalAngle;
            break;
        case Status.Reverse:
            pos = m_reversePosition;
            angle = m_reverseAngle;
            break;
        case Status.Transition:
            pos = m_transitionPosition;
            angle = m_transitionAngle;
            break;
        }
        if (m_TransitBegin > 0.0f) {
            float t = (Time.time - m_TransitBegin) / m_TransitDuration;
            if (t > 1.0f) {
                t = 1.0f;
            }
            t = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t));
            pos = Vector3.Lerp(m_currentPosition, pos, t);
            angle = Vector3.Lerp(m_currentAngle, angle, t);
            if (t == 1.0f) {
                m_TransitBegin = 0.0f;
                m_status = m_status_to;
                m_status_to = Status.Undefine;
                m_currentPosition = pos;
                m_currentAngle = angle;
            }
        } else {
            m_status_to = Status.Undefine;
        }
        pos.x += m_player.transform.position.x;
        pos.z += m_player.transform.position.z;
        transform.position = pos;
        transform.eulerAngles = angle;
    }
}
