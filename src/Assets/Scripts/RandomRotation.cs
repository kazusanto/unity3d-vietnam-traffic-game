using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomRotation : MonoBehaviour {

    [SerializeField] bool m_360 = false;
    [SerializeField] bool m_Front = false;
    [SerializeField] bool m_Back = false;
    [SerializeField] bool m_Right = false;
    [SerializeField] bool m_Left = false;

    // Use this for initialization
    void Start () {
        var angle = 0.0f;
        if (m_360) {
            angle = Random.Range(0.0f, 360.0f);
        } else {
            var angles = new List<float>();
            if (m_Front) {
                angles.Add(0.0f);
            }
            if (m_Back) {
                angles.Add(180.0f);
            }
            if (m_Right) {
                angles.Add(90.0f);
            }
            if (m_Left) {
                angles.Add(270.0f);
            }
            if (angles.Count > 0) {
                angle = angles[Random.Range(0, angles.Count)];
            }
        }
        var rot = transform.localEulerAngles;
        rot.y += angle;
        transform.localEulerAngles = rot;
    }

    // Update is called once per frame
    void Update () {

    }
}
