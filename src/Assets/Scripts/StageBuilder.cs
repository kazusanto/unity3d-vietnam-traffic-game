using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageBuilder : MonoBehaviour {

    private struct LandRange {
        public LandRange(int begin, int end) {
            this.begin = begin; 
            this.end = end; 
        }
        public int begin;
        public int end;
    }

    public enum TrafficRule {
        Stop          = 0,
        Left          = 1,
        Right         = 2,
        Up            = 4,
        UpLeft        = 5,
        UpRight       = 6,
        Down          = 8,
        DownLeft      = 9,
        DownRight     = 10,
    };

    [SerializeField] private GameObject m_Land = null;
    [SerializeField] private GameObject m_Sidewalk = null;
    [SerializeField] private GameObject m_SidewalkCorner = null;
    [SerializeField] private float m_UnitSize = 2.0f;
    [SerializeField] private int m_Backward = -20;
    [SerializeField] private int m_Forward = 20;
    [SerializeField] private int m_Near = -20;
    [SerializeField] private int m_Far = 30;
    [SerializeField] private int m_MaxLands = 5;
    [SerializeField] private float m_Straightness = 0.5f;
    [SerializeField] private float m_Blindness = 0.5f;
    [SerializeField] private int[] m_LandWidths = { 3, 3, 3 };
    [SerializeField] private int[] m_RoadWidths = { 4, 4, 4 };
    [SerializeField] private bool m_isLoop = true;

    private GameObject m_player = null;
    private float m_min = 0;
    private float m_max = 0;
    private int m_next = 0;
    private int m_index = 0;
    private List<int> m_prevLands = null;
    private List<LandRange> m_reservedLands = null;
    private Dictionary<int, List<TrafficRule>> m_trafficRules = new Dictionary<int, List<TrafficRule>>();

    public TrafficRule GetTrafficRule(float x, float z) {
        bool beyond = false;
        TrafficRule rule = TrafficRule.Stop;
        float unit_harf = (float)m_UnitSize / 2.0f;
        int ux = (int)((x + unit_harf) / m_UnitSize) - m_Backward;
        int uz = (int)((z + unit_harf) / m_UnitSize) - m_Near;
        if (uz >= m_Far - m_Near) {
            beyond = true;
            uz = m_Far - m_Near - 1;
        }
        if (uz < 0 || ux < 0) {
            return TrafficRule.Stop;
        }
        if (m_trafficRules.ContainsKey(ux)) {
            var rules = m_trafficRules[ux];
            rule = uz < rules.Count ? rules[uz] : TrafficRule.Stop;
        }
        if (beyond && rule == TrafficRule.Stop) { 
            rule = TrafficRule.Right;
        }
        return rule;
    }

    private void SetTrafficRule(int ux, int uz, TrafficRule rule) {
        ux -= m_Backward;
        uz -= m_Near;
        if (uz < 0 || ux < 0) {
            return;
        }
        if (m_trafficRules.ContainsKey(ux)) {
            var rules = m_trafficRules[ux];
            if (uz < rules.Count) {
                rules[uz] = rule;
            } else {
                rules.AddRange(new TrafficRule[uz - rules.Count + 1]);
                rules[uz] = rule;
            }
            m_trafficRules[ux] = rules;
        } else {
            var rules = new List<TrafficRule>();
            rules.AddRange(new TrafficRule[m_Far - m_Near]);
            rules[uz] = rule;
            m_trafficRules.Add(ux, rules);
        }
    }

    private void removePassedTrafficRules() {
        var minux = (int)(m_player.transform.position.x / m_UnitSize) + m_Backward;
        var list = new List<int>();
        foreach (var ux in m_trafficRules.Keys) {
            if (ux < minux) {
                list.Add(ux);
            }
        }
        foreach (var ux in list) {
            m_trafficRules.Remove(ux);
        }
    }

    private int LandWidth(int index) { 
        if (m_isLoop) {
            return GetLoopedValue<int>(m_LandWidths, index);
        } else {
            return GetExpandedValue<int>(m_LandWidths, index);
        }
    }

    private int RoadWidth(int index) { 
        if (m_isLoop) {
            return GetLoopedValue<int>(m_RoadWidths, index);
        } else {
            return GetExpandedValue<int>(m_RoadWidths, index);
        }
    }

    private T GetExpandedValue<T>(T[] array, int index) { 
        if (index < 0) {
            return array[0];
        } else {
            var len = array.Length;
            return array[index < len ? index : len - 1];
        }
    }

    private T GetLoopedValue<T>(T[] array, int index) { 
        var len = array.Length;
        while (index < 0) {
            index += len;
        }
        while (index >= len) {
            index -= len;
        }
        return array[index];
    }

    private void AddUnique<T>(List<T> list, T value) {
        if (list.IndexOf(value) == -1) {
            list.Add(value);
        }
    }

    // Use this for initialization
	void Start () {
        m_min = m_max = m_next = m_Backward;
        m_player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        var ux = (int)(m_player.transform.position.x / m_UnitSize);
        var built = false;
        while (m_next < ux + m_Forward) {
            buildNext();
            built = true;
        }
        if (built) {
            removePassedLands();
            removePassedTrafficRules();
        }
	}

    private void buildNext() {
        var landWidth = LandWidth(m_index);
        var roadWidth = RoadWidth(m_index);
        var ux = m_next;
        var dz = landWidth + roadWidth; // minimum space
        List<int> lands = new List<int>();
        lands.Add(0);
        if (m_prevLands != null) {
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    AddUnique(lands, range.begin);
                    AddUnique(lands, range.end + RoadWidth(m_index - 1));
                }
            }
            foreach (var puz in m_prevLands) {
                bool repeat = true;
                if (RoadWidth(m_index - 1) != roadWidth) {
                    repeat = false;
                }
                if (Random.Range(0.0f, 1.0f) > m_Straightness) {
                    repeat = false;
                }
                if (repeat) {
                    AddUnique(lands, puz);
                }
            }
        }
        for (var i = lands.Count; i < m_MaxLands; i++) {
            var tmp = Random.Range(m_Near + dz, m_Far - landWidth + 1);
            var safe = true;
            foreach (var yet in lands) {
                if (tmp + dz > yet && tmp < yet + dz) {
                    safe = false;
                    break;
                }
            }
            if (safe && m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (tmp + dz > range.begin && tmp < range.end + roadWidth) {
                        safe = false;
                        break;
                    }
                }
            }
            if (safe) {
                AddUnique(lands, tmp);
            }
        }
        lands.Sort();

        List<LandRange> reserved = new List<LandRange>();
        var uz = m_Near;
        var landWidth2 = landWidth + roadWidth + LandWidth(m_index + 1);
        var leftwidth = RoadWidth(m_index - 1);
        for (var i = 0; i < lands.Count; i++) {
            var nz = lands[i];
            if (uz == nz) {
                continue;
            }
            var skip = false;
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (uz == range.begin) {
                        createTrafficRules(ux + landWidth, uz, roadWidth, nz - uz - roadWidth, TrafficRule.Down);
                        createTrafficRules(ux + landWidth, nz - roadWidth, roadWidth, roadWidth, TrafficRule.DownRight);
                        skip = true;
                        break;
                    }
                }
            }
            if (skip) {
                uz = nz;
                continue;
            }
            bool isRight = Random.Range(0, 2) == 0 ? true : false;
            var depth = nz - uz - roadWidth;
            var width = landWidth;
            if (uz != 0 && Random.Range(0.0f, 1.0f) < m_Blindness) {
                width = landWidth2;
                var range = new LandRange(uz, uz + depth);
                reserved.Add(range);
            }
            if (width < 2) {
                width = 2;
            }
            if (depth < 2) {
                depth = 2;
            }
            createLand(ux, uz, width, depth);
            createTrafficRules(ux, uz, width, depth, TrafficRule.Stop);
            createTrafficRules(ux, nz - roadWidth, width, roadWidth, isRight ? TrafficRule.Right : TrafficRule.Left);
            createTrafficRules(ux + width, nz - roadWidth, landWidth2 - width, roadWidth, isRight ? TrafficRule.DownRight : TrafficRule.DownLeft);
            createTrafficRules(ux + width, uz, landWidth2 - width, depth, TrafficRule.Down);
            createTrafficRules(ux - leftwidth, uz, leftwidth, depth, TrafficRule.Down);
            if (!isRight) {
                createTrafficRules(ux - leftwidth, nz - roadWidth, leftwidth, roadWidth, TrafficRule.Down);
            }
            uz = nz;
        }
        if (uz + 2 <= m_Far) {
            createLand(ux, uz, landWidth, m_Far - uz);
            createTrafficRules(ux, uz, landWidth, m_Far - uz, TrafficRule.Stop);
            createTrafficRules(ux + landWidth, uz, landWidth2 - landWidth, m_Far - uz, TrafficRule.Down);
            createTrafficRules(ux - leftwidth, uz, leftwidth, m_Far - uz, TrafficRule.Down);
        }
        m_reservedLands = reserved;
        m_prevLands = lands;
        m_min = Mathf.Min(m_min, ux);
        m_max = Mathf.Max(m_max, ux);
        m_next = ux + landWidth + roadWidth;
        m_index++;
    }

    private void removePassedLands() {
        var minx = m_player.transform.position.x + (m_Backward * m_UnitSize);
        for (var i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if (child.position.x < minx) {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    private void createTrafficRules(int ux, int uz, int width, int depth, TrafficRule rule) {
        for (var x = ux; x < ux + width; x++) {
            for (var z = uz; z < uz + depth; z++) {
                SetTrafficRule(x, z, rule);
            }
        }
    }

    private void createLand(int ux, int uz, int width, int depth) {
        GameObject obj = null;
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3(ux * m_UnitSize, 0.0f, uz * m_UnitSize);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3((ux + width - 1) * m_UnitSize, 0.0f, uz * m_UnitSize);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3((ux + width - 1) * m_UnitSize, 0.0f, (uz + depth - 1) * m_UnitSize);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 270.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3(ux * m_UnitSize, 0.0f, (uz + depth - 1) * m_UnitSize);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
        for (var x = ux + 1; x < ux + width - 1; x++) {
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(x * m_UnitSize, 0.0f, uz * m_UnitSize);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(x * m_UnitSize, 0.0f, (uz + depth - 1) * m_UnitSize);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 270.0f));
        }
        for (var z = uz + 1; z < uz + depth - 1; z++) {
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(ux * m_UnitSize, 0.0f, z * m_UnitSize);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3((ux + width - 1) * m_UnitSize, 0.0f, z * m_UnitSize);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        }
        for (var x = ux + 1; x < ux + width - 1; x++) {
            for (var z = uz + 1; z < uz + depth - 1; z++) {
                obj = GameObject.Instantiate(m_Land);
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(x * m_UnitSize, 0.0f, z * m_UnitSize);
                obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
            }
        }
    }
}
