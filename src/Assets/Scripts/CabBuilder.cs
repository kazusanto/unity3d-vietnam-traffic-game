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
    private GameObject m_target = null;
    private StageBuilder m_stage = null;
    private Dictionary<float, Range<Vector2>[]> m_points = new Dictionary<float, Range<Vector2>[]>();
    private float m_next = 0.0f;
    private bool m_preparing = true;

    // Use this for initialization
	void Start () {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_stage = GameObject.FindGameObjectWithTag(m_StageTag).GetComponent<StageBuilder>();
        m_stage.PrepareStage();
        m_next = m_Backword;
        m_time = 0.0f;
        m_preparing = true;
        while (m_next < m_stage.Constructed) {
            updatePoints();
        }
        for (int i = 0; i < 20; i++) {
            buildOne();
        }
        m_preparing = false;
	}
	
	// Update is called once per frame
	void Update () {
        m_time += Time.deltaTime;
        if (m_time >= m_Interval) {
            m_time = 0.0f;
            if (m_next < m_stage.Constructed) {
                updatePoints();
            }
            buildOne();
        }
	}

    private void buildOne() {
        var point = choosePoint();
        var vehicle = GameObject.Instantiate(m_Prefab);
        vehicle.transform.SetParent(transform);
        vehicle.transform.position = point;
    }

    private void updatePoints() {
        while (m_next < m_stage.Constructed) {
            var areas = m_stage.GetRoadArea(m_next);
            if (areas != null) {
                m_points[m_next] = areas;
            }
            m_next += m_stage.UnitSize;
        }
        var list = new List<float>();
        foreach (var x in m_points.Keys) {
            if (x < m_target.transform.position.x + m_Backword) {
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
        float key_x = m_target.transform.position.x;
        Range<Vector2> area = null;
        if (keys.Count > 0) {
            key_x = keys[Random.Range(0, keys.Count)];
            var points = m_points[key_x];
            if (points.Length > 0) {
                area = points[Random.Range(0, points.Length)];
                if (!m_preparing) {
                    area = adjustArea(area);
                }
            }
        }
        if (area != null) {
            var x = Random.Range(area.Min.x, area.Max.x);
            var z = Random.Range(area.Min.y, area.Max.y);
            return new Vector3(x, 0.0f, z);
        } else {
            var x = key_x + Random.Range(0.0f, m_stage.UnitSize);
            var z = m_target.transform.position.z + m_Far;
            var rule = m_stage.GetTrafficRule(x, z);
            if ((rule & StageBuilder.TrafficRule.Up) == StageBuilder.TrafficRule.Up) {
                z = m_target.transform.position.z + m_Near;
            }
            return new Vector3(x, 0.0f, z);
        }
    }

    private Range<Vector2> adjustArea(Range<Vector2> area) {
        float x = (area.Min.x + area.Max.x) * 0.5f;
        Vector3 min = new Vector3(x, 0.0f, area.Min.y);
        Vector3 max = new Vector3(x, 0.0f, area.Max.y);
        Vector3 view_min = Camera.main.WorldToViewportPoint(min);
        Vector3 view_max = Camera.main.WorldToViewportPoint(max);
        bool in_min = (view_min.x > -0.0f && view_min.x < 1.0f && view_min.z > -0.0f);
        bool in_max = (view_max.x > -0.0f && view_max.x < 1.0f && view_max.z > -0.0f);
        if (in_min || in_max) {
            return null;
        }
        return area;
    }
}
