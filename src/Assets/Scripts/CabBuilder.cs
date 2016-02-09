using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class CabBuilder : MonoBehaviour {

    [SerializeField] private GameObject m_Prefab = null;
    [SerializeField] private string m_StageTag = "Stage";
    [SerializeField] private float m_Interval = 1.0f;

    private float m_time = 0;
    private StageBuilder m_stage = null;
    private bool m_preparing = true;

    // Use this for initialization
	void Start () {
        m_stage = GameObject.FindGameObjectWithTag(m_StageTag).GetComponent<StageBuilder>();
        m_stage.PrepareStage();
        m_time = 0.0f;
        m_preparing = true;
        int num = (int)(3.0f / m_Interval);
        for (int i = 0; i < num; i++) {
            buildVehicle();
        }
        m_preparing = false;
	}
	
	// Update is called once per frame
	void Update () {
        m_time += Time.deltaTime;
        if (m_time >= m_Interval) {
            m_time = 0.0f;
            buildVehicle();
        }
	}

    private void buildVehicle() {
        var pos = m_stage.GetNewVehiclePosition(m_preparing);
        var vehicle = GameObject.Instantiate(m_Prefab);
        vehicle.transform.SetParent(transform);
        vehicle.transform.position = pos;
    }
}
