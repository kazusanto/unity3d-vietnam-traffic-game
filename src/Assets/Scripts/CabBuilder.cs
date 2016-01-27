using UnityEngine;
using System.Collections;

public class CabBuilder : MonoBehaviour {

    [SerializeField] private GameObject m_Prefab = null;
    [SerializeField] private float m_Interval = 1.0f;

    private float m_time = 0;
    private GameObject m_target = null;

    // Use this for initialization
	void Start () {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_time = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        m_time += Time.deltaTime;
        if (m_time >= m_Interval) {
            m_time = 0.0f;
            var vehicle = GameObject.Instantiate(m_Prefab);
            var x = m_target.transform.position.x;
            var z = m_target.transform.position.z;
            var rx = Random.Range(-3.0f, 30.0f);
            var vx = x + rx;
            var vz = z + 50.0f + Random.Range(0.0f, 10.0f);
            vehicle.transform.position = new Vector3(vx, 0.0f, vz);
        }
	}
}
