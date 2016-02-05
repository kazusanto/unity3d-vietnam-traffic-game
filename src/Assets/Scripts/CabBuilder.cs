using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;

public class CabBuilder : MonoBehaviour {

    [SerializeField] private GameObject m_Prefab = null;
    [SerializeField] private string m_StageTag = "Stage";
    [SerializeField] private float m_Interval = 1.0f;
    [SerializeField] private float m_Far = 60.0f;
    [SerializeField] private float m_Near = -30.0f;
    [SerializeField] private float m_Forward = 30.0f;
    [SerializeField] private float m_Backword = -20.0f;

    private float m_time = 0;
    private GameObject m_player = null;
    private StageBuilder m_stage = null;
    private Dictionary<float, Range<Vector2>[]> m_points = new Dictionary<float, Range<Vector2>[]>();
    private float m_next = 0.0f;
    private bool m_preparing = true;

    // Use this for initialization
	void Start () {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_stage = GameObject.FindGameObjectWithTag(m_StageTag).GetComponent<StageBuilder>();
        m_stage.PrepareStage();
        m_next = m_Backword;
        m_time = 0.0f;
        m_preparing = true;
        updatePoints();
        for (int i = 0; i < 20; i++) {
            buildVehicle();
        }
        m_preparing = false;
	}
	
	// Update is called once per frame
	void Update () {
        m_time += Time.deltaTime;
        if (m_time >= m_Interval) {
            m_time = 0.0f;
            updatePoints();
            buildVehicle();
        }
	}

    private void buildVehicle() {
        var point = choosePoint();
        var vehicle = GameObject.Instantiate(m_Prefab);
        vehicle.transform.SetParent(transform);
        vehicle.transform.position = point;
    }

    private void updatePoints() {
        var forward = m_player.transform.position.x + m_Forward;
        while (m_next < m_stage.Constructed && m_next < forward) {
            var areas = m_stage.GetRoadArea(m_next);
            if (areas != null) {
                m_points[m_next] = areas;
            }
            m_next += m_stage.UnitSize;
        }
        var list = new List<float>();
        foreach (var x in m_points.Keys) {
            if (x < m_player.transform.position.x + m_Backword) {
                list.Add(x);
            }
        }
        foreach (var x in list) {
            m_points.Remove(x);
        }
    }

    private Vector3 choosePoint() {
        var keys = new List<float>();
        foreach (var k in m_points.Keys) {
            keys.Add(k);
        }
        float key_x = m_player.transform.position.x;
        Range<Vector2> area = null;
        if (keys.Count > 0) {
            key_x = keys[Random.Range(0, keys.Count)];
            var points = m_points[key_x];
            if (points.Length > 0) {
                area = points[Random.Range(0, points.Length)];
            }
        }
        if (area != null) {
            var point = new Vector3(Random.Range(area.Min.x, area.Max.x), 0.0f, Random.Range(area.Min.y, area.Max.y));
            return adjustPoint(point);
        }
        return Vector3.zero;
    }

    private Vector3 adjustPoint(Vector3 point) {
        bool illegal = false;
        if (point.z < 10.0f && point.z > -10.0f) {
            illegal = true;
        } else if (!m_preparing) {
            Vector3 view_pos = Camera.main.WorldToViewportPoint(point);
            if (view_pos.x > -0.0f && view_pos.x < 1.0f && view_pos.z > -0.0f) {
                illegal = true;
            }
        }
        if (illegal) {
            var z = m_player.transform.position.z + m_Far;
            var rule = m_stage.GetTrafficRule(point.x, z);
            if ((rule & StageBuilder.TrafficRule.Up) == StageBuilder.TrafficRule.Up) {
                z = m_player.transform.position.z + m_Near;
            }
            point.z = z;
        }
        return point;
    }
}
