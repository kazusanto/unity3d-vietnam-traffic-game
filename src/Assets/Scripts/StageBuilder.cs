using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageBuilder : MonoBehaviour {

    private struct LandRange {
        public int begin;
        public int end;
    };
    [SerializeField] private GameObject m_Land = null;
    [SerializeField] private GameObject m_Sidewalk = null;
    [SerializeField] private GameObject m_SidewalkCorner = null;
    [SerializeField] private float m_UnitSize = 2.0f;
    [SerializeField] private int m_Backward = -5;
    [SerializeField] private int m_Forward = 5;
    [SerializeField] private int m_Near = -10;
    [SerializeField] private int m_Far = 30;
    [SerializeField] private int m_MaxLands = 5;
    [SerializeField] private float m_Straightness = 0.5f;
    [SerializeField] private float m_Blindness = 0.5f;
    [SerializeField] private int[] m_LandWidths = { 4, 4, 4 };
    [SerializeField] private int[] m_RoadWidths = { 2, 2, 2 };

    private GameObject m_player = null;
    private float m_min = 0;
    private float m_max = 0;
    private int m_next = 0;
    private int m_index = 0;
    private List<int> m_prevLands = null;
    private List<LandRange> m_reservedLands = null;

    private int LandWidth(int index) { 
        return GetLoopedValue<int>(m_LandWidths, index);
    }

    private int RoadWidth(int index) { 
        return GetLoopedValue<int>(m_RoadWidths, index);
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
        m_min = m_max = m_Backward;
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
            removePassed();
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

        var str = "";
        if (m_reservedLands != null) {
            m_reservedLands.ForEach(v => {
                str += "(" + v.begin.ToString() + "," + v.end.ToString() + "),";
            });
            print("reservedLands:" + str);
        }
        str = "";
        foreach (var v in lands) {
            str += v.ToString() + ",";
        }
        print("lands:" + str);

        List<LandRange> reserved = new List<LandRange>();
        var uz = m_Near;
        var landWidth2 = landWidth + roadWidth + LandWidth(m_index + 1);
        for (var i = 0; i < lands.Count; i++) {
            var nz = lands[i];
            if (uz == nz) {
                continue;
            }
            var skip = false;
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (uz == range.begin) {
                        skip = true;
                        break;
                    }
                }
            }
            if (skip) {
                uz = nz;
                continue;
            }
            var depth = nz - uz - roadWidth;
            var width = landWidth;
            if (uz != 0 && Random.Range(0.0f, 1.0f) < m_Blindness) {
                width = landWidth2;
                var range = new LandRange();
                range.begin = uz;
                range.end = uz + depth;
                reserved.Add(range);
            }
            if (width < 2) {
                width = 2;
            }
            if (depth < 2) {
                depth = 2;
            }
            createLand(ux, uz, width, depth);
            uz = nz;
        }
        if (uz + 2 <= m_Far) {
            createLand(ux, uz, landWidth, m_Far - uz);
        }
        m_reservedLands = reserved;
        m_prevLands = lands;
        m_min = Mathf.Min(m_min, ux);
        m_max = Mathf.Max(m_max, ux);
        m_next = ux + landWidth + roadWidth;
        m_index++;
    }

    private void removePassed() {
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
