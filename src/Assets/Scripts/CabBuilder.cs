using UnityEngine;
using System.Collections;

public class CabBuilder : MonoBehaviour {

    [SerializeField] private GameObject m_Prefab = null;
    [SerializeField] private string m_StageTag = "Stage";
    [SerializeField] private float m_Interval = 1.0f;
    [SerializeField] private float m_Far = 60.0f;
    [SerializeField] private float m_Near = -30.0f;
    [SerializeField] private float m_Forward = 30.0f;
    [SerializeField] private float m_Backword = -20.0f;

    private float m_time = 0;
    private GameObject m_target = null;
    private StageBuilder m_stage = null;

    // Use this for initialization
	void Start () {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_stage = GameObject.FindGameObjectWithTag(m_StageTag).GetComponent<StageBuilder>();
        m_time = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        m_time += Time.deltaTime;
        if (m_time >= m_Interval) {
            m_time = 0.0f;
            var vehicle = GameObject.Instantiate(m_Prefab);
            vehicle.transform.SetParent(transform);
            var x = m_target.transform.position.x;
            var z = m_target.transform.position.z;
            var rx = Random.Range(m_Backword, m_Forward);
            var vx = x + rx;
            var vz = z + m_Far;
            var rule = m_stage.GetTrafficRule(vx, vz);
            if ((rule & StageBuilder.TrafficRule.Up) != 0) {
                vz = z + m_Near;
                vehicle.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
            }
            vehicle.transform.position = new Vector3(vx, 0.0f, vz);
        }
	}
}
