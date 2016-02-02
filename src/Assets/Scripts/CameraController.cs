﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    enum Status {
        Undefine,
        Normal,
        Reverse,
        Transition
    };

    [SerializeField] private float m_Distance = 5.0f;
    [SerializeField] private float m_Tilt = 12.0f;
    [SerializeField] private float m_Rise = 1.5f;
    [SerializeField] private float m_Shift = 1.0f;
    [SerializeField] private int m_TransitionFrames = 80;

    private GameObject m_target = null;
    private PlayerController m_player = null;
    private Vector3 m_normalPosition;
    private Vector3 m_reversePosition;
    private Vector3 m_transitionPosition;
    private Vector3 m_normalAngle;
    private Vector3 m_reverseAngle;
    private Vector3 m_transitionAngle;
    private Vector3 m_currentPosition;
    private Vector3 m_currentAngle;
    private Status m_status = Status.Normal;
    private Status m_status_to = Status.Undefine;
    private int m_count = 0;

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
        m_count = 0;
	}
	
	// Update is called once per frame
	void Update () {
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
                m_count = m_TransitionFrames;
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
        if (m_count > 0) {
            float t = (float)(m_TransitionFrames - m_count) / (float)m_TransitionFrames;
            pos = Vector3.Lerp(m_currentPosition, pos, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            angle = Vector3.Lerp(m_currentAngle, angle, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t)));
            if (--m_count <= 0) {
                m_count = 0;
                m_status = m_status_to;
                m_status_to = Status.Undefine;
                m_currentPosition = pos;
                m_currentAngle = angle;
            }
        } else {
            m_status_to = Status.Undefine;
        }
        transform.position = m_player.transform.position + pos;
        transform.eulerAngles = angle;
    }
}
