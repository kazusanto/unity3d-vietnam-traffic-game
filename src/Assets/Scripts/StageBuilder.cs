using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;

using LandRange = Common.Range<int>;

public class StageBuilder : MonoBehaviour {

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
    }

    private struct Unit {
        public int x;
        public int y;
        public Unit(int ux, int uy) { x = ux; y = uy; }
    }

    [SerializeField] private GameObject m_Land = null;
    [SerializeField] private GameObject m_Sidewalk = null;
    [SerializeField] private GameObject m_SidewalkCorner = null;
    [SerializeField] private GameObject m_Grass = null;
    [SerializeField] private GameObject m_Wood = null;
    [SerializeField] private GameObject[] m_Items1x1 = null;

    [SerializeField] private GameObject m_Arrow = null;
    [SerializeField] private bool m_isDebugMode = false;
    [SerializeField] private float m_UnitSize = 2.0f;
    [SerializeField] private int m_Backward = -20;
    [SerializeField] private int m_Forward = 20;
    [SerializeField] private int m_Near = -20;
    [SerializeField] private int m_Far = 30;
    [SerializeField] private int m_MaxLands = 5;
    [SerializeField] private float m_Straightness = 0.5f;
    [SerializeField] private float m_Blindness = 0.5f;
    [SerializeField] private bool m_isLoop = true;
    [SerializeField] private int[] m_LandWidths = { 3, 3, 3 };
    [SerializeField] private int[] m_RoadWidths = { 4, 4, 4 };
    [SerializeField] private bool[] m_ReverseRoads = { false, false, true };

    public float UnitSize { get { return m_UnitSize; } }
    public float Constructed { get { return worldForUnit(m_next, 0).x; } }
    private int UnitHeight { get { return m_Far - m_Near; } }
    private int UnitWidth  { get { return m_Forward - m_Backward; } }
    private int CentralUY { get { return -m_Near; } }

    private GameObject m_player = null;
    private int m_next = 0;
    private int m_index = 0;
    private int m_fstep = 0;
    private int m_bstep = 0;
    private List<int> m_prevLands = null;
    private List<LandRange> m_reservedLands = null;
    private GameObject m_landBase = null;
    private Dictionary<int, List<TrafficRule>> m_trafficRules = new Dictionary<int, List<TrafficRule>>();
    private Dictionary<int, List<GameObject>> m_trafficArrows = new Dictionary<int, List<GameObject>>();
    private bool m_inited = false;

    private Dictionary<TrafficRule, float> rotationForRule = new Dictionary<TrafficRule, float>() {
        { TrafficRule.Stop,        0.0f },
        { TrafficRule.Right,      90.0f },
        { TrafficRule.Left,      -90.0f },
        { TrafficRule.Down,      180.0f },
        { TrafficRule.DownRight, 135.0f },
        { TrafficRule.DownLeft, -135.0f },
        { TrafficRule.Up,          0.0f },
        { TrafficRule.UpRight,    45.0f },
        { TrafficRule.UpLeft,    -45.0f }
    };

    private Vector2 worldForUnit(int ux, int uy) {
        float wx = (float)(ux + m_Backward) * m_UnitSize;
        float wz = (float)(uy + m_Near) * m_UnitSize;
        return new Vector2(wx, wz);
    }

    private Vector3 vector3ForUnit(int ux, int uy) {
        Vector2 xy = worldForUnit(ux, uy);
        return new Vector3(xy.x, 0.0f, xy.y);
    }

    private Unit unitForWorld(float wx, float wz) {
        float harf = m_UnitSize / 2.0f;
        int x = (int)((wx + harf) / m_UnitSize) - m_Backward;
        int y = (int)((wz + harf) / m_UnitSize) - m_Near;
        return new Unit(x, y);
    }

    public bool isReverseRoad() {
        TrafficRule rule = TrafficRule.Stop;
        float x = m_player.transform.position.x;
        float z = m_player.transform.position.z;
        float offs = 0.0f;
        while (rule == TrafficRule.Stop && offs < 10.0f) {
            rule = GetTrafficRule(x + offs, z);
            offs += m_UnitSize;
        }
        return (rule & TrafficRule.Up) != 0;
    }

    public TrafficRule GetTrafficRule(float x, float z) {
        bool beyond = false;
        TrafficRule rule = TrafficRule.Stop;
        Unit unit = unitForWorld(x, z);
        if (unit.y >= UnitHeight) {
            beyond = true;
            unit.y = UnitHeight - 1;
        }
        if (unit.y < 0) {
            beyond = true;
            unit.y = 0;
        }
        if (unit.x < 0) {
            return TrafficRule.Stop;
        }
        if (m_trafficRules.ContainsKey(unit.x)) {
            var rules = m_trafficRules[unit.x];
            rule = unit.y < rules.Count ? rules[unit.y] : TrafficRule.Stop;
        }
        if (beyond && rule == TrafficRule.Stop) { 
            rule = TrafficRule.Right;
        }
        return rule;
    }

    public Range<Vector2>[] GetRoadArea(float x) {
        float harf = m_UnitSize / 2.0f;
        var result = new List<Range<Vector2>>();
        Unit unit = unitForWorld(x, 0);
        unit.y = 0;
        if (m_trafficRules.ContainsKey(unit.x)) {
            var rules = m_trafficRules[unit.x];
            for (int uy = 0; uy <= UnitHeight; uy++) {
                if (rules[unit.y] == TrafficRule.Stop) {
                    unit.y = uy;
                    continue;
                }
                bool last = uy == UnitHeight;
                if (rules[uy] == TrafficRule.Stop || last) {
                    var min = worldForUnit(unit.x, unit.y);
                    var max = worldForUnit(unit.x + 1, uy);
                    min.x -= harf;
                    min.y -= harf;
                    max.x -= harf;
                    max.y -= harf;
                    result.Add(new Range<Vector2>(min, max));
                    unit.y = uy;
                }
            }
            if (result.Count == 0) {
                return null;
            }
        }
        return result.ToArray();
    }

    private void SetTrafficRule(Unit unit, TrafficRule rule) {
        SetTrafficArrow(unit, rule);
        if (unit.x < 0 || unit.y < 0) {
            return;
        }
        if (m_trafficRules.ContainsKey(unit.x)) {
            var rules = m_trafficRules[unit.x];
            if (unit.y < rules.Count) {
                rules[unit.y] = rule;
            } else {
                rules.AddRange(new TrafficRule[unit.y - rules.Count + 1]);
                rules[unit.y] = rule;
            }
            m_trafficRules[unit.x] = rules;
        } else {
            var rules = new List<TrafficRule>();
            rules.AddRange(new TrafficRule[UnitHeight + 1]);
            rules[unit.y] = rule;
            m_trafficRules.Add(unit.x, rules);
        }
    }

    private void SetTrafficArrow(Unit unit, TrafficRule rule) {
        if (m_Arrow == null) {
            return;
        }
        var angle = new Vector3(90.0f, rotationForRule[rule], 0.0f);
        var pos = vector3ForUnit(unit.x, unit.y);
        pos.y = 0.01f;
        if (unit.x < 0 || unit.y < 0) {
            return;
        }
        if (m_trafficArrows.ContainsKey(unit.x)) {
            var arrows = m_trafficArrows[unit.x];
            if (unit.y < arrows.Count) {
                var obj = arrows[unit.y];
                obj.transform.position = pos;
                obj.transform.eulerAngles = angle;
                obj.SetActive(rule != TrafficRule.Stop && m_isDebugMode);
                arrows[unit.y] = obj;
            }
            m_trafficArrows[unit.x] = arrows;
        } else {
            var num = UnitHeight + 1;
            var arrows = new List<GameObject>(num);
            for (var i = 0; i < num; i++) {
                var obj = GameObject.Instantiate(m_Arrow);
                obj.transform.SetParent(transform);
                if (i == unit.y) {
                    obj.transform.position = pos;
                    obj.transform.eulerAngles = angle;
                    obj.SetActive(rule != TrafficRule.Stop && m_isDebugMode);
                } else {
                    obj.SetActive(false);
                }
                arrows.Add(obj);
            }
            m_trafficArrows.Add(unit.x, arrows);
        }
    }

    private void removePassedTrafficRules() {
        var minux = (int)(m_player.transform.position.x / m_UnitSize);
        var list = new List<int>();
        foreach (var ux in m_trafficRules.Keys) {
            if (ux < minux) {
                list.Add(ux);
            }
        }
        foreach (var ux in list) {
            m_trafficRules.Remove(ux);
            removePassedTrafficArrows(ux);
        }
    }

    private void removePassedTrafficArrows(int ux) {
        if (m_trafficArrows.ContainsKey(ux)) {
            var arrows = m_trafficArrows[ux];
            m_trafficArrows.Remove(ux);
            foreach (var obj in arrows) {
                GameObject.Destroy(obj);
            }
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

    private bool isReverseRoad(int index) { 
        if (m_isLoop) {
            return GetLoopedValue<bool>(m_ReverseRoads, index);
        } else {
            return GetExpandedValue<bool>(m_ReverseRoads, index);
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
        if (!m_inited) {
            PrepareStage();
        }
	}

    public void PrepareStage() {
        m_inited = true;
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_landBase = new GameObject();
        m_landBase.transform.SetParent(transform);
        var unit = unitForWorld(m_player.transform.position.x, 0.0f);
        m_fstep = 0;
        m_bstep = 0;
        while (m_next < unit.x) {
            buildNext();
        }
        m_fstep = 1;
    }

	// Update is called once per frame
	void Update () {
        var unit = unitForWorld(m_player.transform.position.x, 0.0f);
        var built = false;
        while (m_next < unit.x + m_Forward) {
            buildNext();
            m_index++;
            m_bstep = 1;
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
        var minimum = landWidth + roadWidth;
        List<int> lands = new List<int>();
        lands.Add(CentralUY);
        if (m_prevLands != null) {
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    AddUnique(lands, range.Min);
                    AddUnique(lands, range.Max + RoadWidth(m_index - m_bstep));
                }
            }
            if (RoadWidth(m_index - m_bstep) == roadWidth) {
                foreach (var prevLand in m_prevLands) {
                    if (Random.Range(0.0f, 1.0f) < m_Straightness) {
                        AddUnique(lands, prevLand);
                    }
                }
            }
        }
        for (var i = lands.Count; i < m_MaxLands; i++) {
            var uy = Random.Range(minimum, UnitHeight - landWidth + 1);
            var safe = true;
            foreach (var yet in lands) {
                if (uy + minimum > yet && uy < yet + minimum) {
                    safe = false;
                    break;
                }
            }
            if (safe && m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (uy + minimum > range.Min && uy < range.Max + roadWidth) {
                        safe = false;
                        break;
                    }
                }
            }
            if (safe) {
                AddUnique(lands, uy);
            }
        }
        lands.Sort();
        m_reservedLands = createLandLine(ux, lands, isReverseRoad(m_index));
        m_prevLands = lands;
        m_next = ux + landWidth + roadWidth;
    }

    private List<LandRange> createLandLine(int ux, List<int> lands, bool isReverse) {
        var Vert = isReverse ? TrafficRule.Up : TrafficRule.Down;
        var VertRight = isReverse ? TrafficRule.UpRight : TrafficRule.DownRight;
        var VertLeft = isReverse ? TrafficRule.UpLeft : TrafficRule.DownLeft;
        var VertOpposite = isReverse ? TrafficRule.Down : TrafficRule.Up;
        var roadWidth = RoadWidth(m_index);
        var landWidth = LandWidth(m_index);
        var landWidth2 = landWidth + roadWidth + LandWidth(m_index + m_fstep);
        var leftwidth = RoadWidth(m_index - m_bstep);
        bool isPreviousOpposite = isReverseRoad(m_index - m_bstep) != isReverse;
        bool isNextOpposite = isReverseRoad(m_index + m_fstep) != isReverse;
        var reserved = new List<LandRange>();
        var uy = 0;
        for (var i = 0; i < lands.Count; i++) {
            var ny = lands[i];
            if (uy == ny) {
                continue;
            }
            var skip = false;
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (uy == range.Min) {
                        createTrafficRules(ux + landWidth, uy, roadWidth, ny - uy - roadWidth, Vert);
                        createTrafficRules(ux + landWidth, ny - roadWidth, roadWidth, roadWidth, VertRight);
                        skip = true;
                        break;
                    }
                }
            }
            if (skip) {
                uy = ny;
                continue;
            }
            bool isRight = Random.Range(0, 2) == 0 ? true : false;
            var depth = ny - uy - roadWidth;
            var width = landWidth;
            if (uy != CentralUY && !isNextOpposite && Random.Range(0.0f, 1.0f) < m_Blindness) {
                width = landWidth2;
                var range = new LandRange(uy, uy + depth);
                reserved.Add(range);
            }
            if (width < 2) {
                width = 2;
            }
            if (depth < 2) {
                depth = 2;
            }
            createLand(ux, uy, width, depth);
            createTrafficRules(ux, uy, width, depth, TrafficRule.Stop);
            createTrafficRules(ux, ny - roadWidth, width, roadWidth, isRight ? VertRight : VertLeft);
            createTrafficRules(ux + width, ny - roadWidth, landWidth2 - width, roadWidth, isRight ? VertRight : VertLeft);
            createTrafficRules(ux + width, uy, landWidth2 - width, depth, Vert);
            createTrafficRules(ux - leftwidth, uy, leftwidth, depth, isPreviousOpposite ? VertOpposite : Vert);
            if (!isRight) {
                createTrafficRules(ux - 2, ny - roadWidth, 2, roadWidth, isPreviousOpposite ? VertOpposite : Vert);
            }
            uy = ny;
        }
        if (uy + 2 <= UnitHeight) {
            createLand(ux, uy, landWidth, UnitHeight - uy);
            createTrafficRules(ux, uy, landWidth, UnitHeight - uy, TrafficRule.Stop);
            createTrafficRules(ux + landWidth, uy, landWidth2 - landWidth, UnitHeight - uy, Vert);
            createTrafficRules(ux - leftwidth, uy, leftwidth, UnitHeight - uy, isPreviousOpposite ? VertOpposite : Vert);
        }
        return reserved;
    }

    private void removePassedLands() {
        var minx = m_player.transform.position.x + (m_Backward * m_UnitSize) - 1.0f;
        for (var i = 0; i < m_landBase.transform.childCount; i++) {
            var child = m_landBase.transform.GetChild(i);
            if (child.position.x <= minx) {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    private void createTrafficRules(int ux, int uy, int width, int depth, TrafficRule rule) {
        for (var x = ux; x < ux + width; x++) {
            for (var y = uy; y < uy + depth; y++) {
                SetTrafficRule(new Unit(x, y), rule);
            }
        }
    }

    private void createLand(int ux, int uy, int width, int depth) {
        GameObject obj = null;
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(m_landBase.transform);
        obj.transform.position = vector3ForUnit(ux, uy);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(m_landBase.transform);
        obj.transform.position = vector3ForUnit(ux + width - 1, uy);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(m_landBase.transform);
        obj.transform.position = vector3ForUnit(ux + width - 1, uy + depth - 1);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 270.0f));
        obj = GameObject.Instantiate(m_SidewalkCorner);
        obj.transform.SetParent(m_landBase.transform);
        obj.transform.position = vector3ForUnit(ux, uy + depth - 1);
        obj.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
        for (var x = ux + 1; x < ux + width - 1; x++) {
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(m_landBase.transform);
            obj.transform.position = vector3ForUnit(x, uy);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(m_landBase.transform);
            obj.transform.position = vector3ForUnit(x, uy + depth - 1);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 270.0f));
        }
        for (var y = uy + 1; y < uy + depth - 1; y++) {
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(m_landBase.transform);
            obj.transform.position = vector3ForUnit(ux, y);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
            obj = GameObject.Instantiate(m_Sidewalk);
            obj.transform.SetParent(m_landBase.transform);
            obj.transform.position = vector3ForUnit(ux + width - 1, y);
            obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
        }
        for (var x = ux + 1; x < ux + width - 1; x++) {
            for (var y = uy + 1; y < uy + depth - 1; y++) {
                obj = GameObject.Instantiate(m_Land);
                obj.transform.SetParent(m_landBase.transform);
                obj.transform.position = vector3ForUnit(x, y);
                obj.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
            }
        }
        if (width >= 2 && depth >= 2) {
            var offset = Vector3.zero;
            var numx = width <= 3 ? 1 : width - 2;
            var numy = depth <= 3 ? 1 : depth - 2;
            offset.x = -m_UnitSize * ((width - numx) % 2 == 1 ? 0.5f : 0.0f);
            offset.y = 0.1f;
            offset.z = -m_UnitSize * ((depth - numy) % 2 == 1 ? 0.5f : 0.0f);
            createGarden(ux + 1, uy + 1, numx, numy, offset);
        }
    }

    private void createGarden(int ux, int uy, int width, int depth, Vector3 offset) {
        var dx = 1.5f + offset.x;
        for (var x = ux; x < ux + width; x++) {
            for (var y = uy; y < uy + depth; y++) {
                bool isSidewalk = (x == ux || y == uy || x == width - 1 || y == depth - 1);
                bool grass = false;
                if (Random.Range(0, 2) == 0) {
                    var obj = GameObject.Instantiate(m_Grass);
                    obj.transform.SetParent(m_landBase.transform);
                    obj.transform.position = vector3ForUnit(x, y) + offset;
                    grass = true;
                }
                bool done = false;
                if (y == CentralUY + 1 && depth == 1) {
                    done = true;
                }
                if (!done && grass && Random.Range(0, 2) == 0) {
                    var obj = GameObject.Instantiate(m_Wood);
                    obj.transform.SetParent(m_landBase.transform);
                    obj.transform.position = vector3ForUnit(x, y) + offset;
                    done = true;
                }
                if (!done && isSidewalk && m_Items1x1.Length > 0) {
                    var idx = Random.Range(0, m_Items1x1.Length * 2);
                    if (idx < m_Items1x1.Length) {
                        var itemoffset = offset;
                        itemoffset.x += Random.Range(-dx, dx);
                        var obj = GameObject.Instantiate(m_Items1x1[idx]);
                        obj.transform.SetParent(m_landBase.transform);
                        obj.transform.position = vector3ForUnit(x, y) + itemoffset;
                        done = true;
                    }
                }
            }
        }
    }
 }
