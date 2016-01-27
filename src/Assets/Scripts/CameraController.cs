using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    [SerializeField] private float m_Distance = 5.0f;
    [SerializeField] private float m_Shift = 1.0f;
    [SerializeField] private float m_Tilt = 12.0f;

    private GameObject m_target;

    // Use this for initialization
	void Start () {
        m_target = GameObject.FindGameObjectWithTag("Player");
        transform.rotation = transform.rotation * Quaternion.AngleAxis(m_Tilt - transform.localEulerAngles.x, Vector3.right);
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(
            m_target.transform.position.x + m_Shift, 
            transform.position.y,
            m_target.transform.transform.position.z - m_Distance);
	}
}
