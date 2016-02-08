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

    public struct Unit {
        public int x;
        public int y;
        public Unit(int ux, int uy) { x = ux; y = uy; }
    }

    [SerializeField] private GameObject[] m_LandBlocks = null;

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
    private int StageHeight { get { return m_Far - m_Near; } }
    private int StageWidth  { get { return m_Forward - m_Backward; } }
    private int CentralUY { get { return -m_Near; } }

    private int RoadWidth{ get { return getRoadWidth(m_index); } }
    private int LandWidth{ get { return getLandWidth(m_index); } }
    private int PrevRoadWidth { get { return getRoadWidth(m_index - m_bstep); } }
    private int PrevLandWidth { get { return getLandWidth(m_index - m_bstep); } }
    private int NextRoadWidth { get { return getRoadWidth(m_index + m_fstep); } }
    private int NextLandWidth { get { return getLandWidth(m_index + m_fstep); } }
    private bool isReverseLine { get { return isReverseLineAt(m_index); } }
    private bool isPrevLineOpposite { get { return isReverseLine != isReverseLineAt(m_index - m_bstep); } }
    private bool isNextLineOpposite { get { return isReverseLine != isReverseLineAt(m_index + m_fstep); } }

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
        if (unit.y >= StageHeight) {
            beyond = true;
            unit.y = StageHeight - 1;
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
            for (int uy = 0; uy <= StageHeight; uy++) {
                if (rules[unit.y] == TrafficRule.Stop) {
                    unit.y = uy;
                    continue;
                }
                bool last = uy == StageHeight;
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

    private void setTrafficRule(Unit unit, TrafficRule rule) {
        setTrafficArrow(unit, rule);
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
            rules.AddRange(new TrafficRule[StageHeight + 1]);
            rules[unit.y] = rule;
            m_trafficRules.Add(unit.x, rules);
        }
    }

    private void setTrafficArrow(Unit unit, TrafficRule rule) {
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
            var num = StageHeight + 1;
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

    private int getLandWidth(int index) { 
        if (m_isLoop) {
            return getLoopedValue<int>(m_LandWidths, index);
        } else {
            return getExtendedValue<int>(m_LandWidths, index);
        }
    }

    private int getRoadWidth(int index) { 
        if (m_isLoop) {
            return getLoopedValue<int>(m_RoadWidths, index);
        } else {
            return getExtendedValue<int>(m_RoadWidths, index);
        }
    }

    private bool isReverseLineAt(int index) { 
        if (m_isLoop) {
            return getLoopedValue<bool>(m_ReverseRoads, index);
        } else {
            return getExtendedValue<bool>(m_ReverseRoads, index);
        }
    }

    private T getExtendedValue<T>(T[] array, int index) { 
        if (index < 0) {
            return array[0];
        } else {
            var len = array.Length;
            return array[index < len ? index : len - 1];
        }
    }

    private T getLoopedValue<T>(T[] array, int index) { 
        var len = array.Length;
        while (index < 0) {
            index += len;
        }
        while (index >= len) {
            index -= len;
        }
        return array[index];
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
        var ux = m_next;
        var minimum = LandWidth + RoadWidth;
        List<int> lands = new List<int>();
        lands.Add(CentralUY);
        if (m_prevLands != null) {
            if (m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    lands.AddUnique(range.Min);
                    lands.AddUnique(range.Max + PrevRoadWidth);
                }
            }
            if (PrevRoadWidth == RoadWidth) {
                foreach (var prevLand in m_prevLands) {
                    if (Random.Range(0.0f, 1.0f) < m_Straightness) {
                        lands.AddUnique(prevLand);
                    }
                }
            }
        }
        for (var i = lands.Count; i < m_MaxLands; i++) {
            var uy = Random.Range(minimum, StageHeight - LandWidth + 1);
            var safe = true;
            foreach (var yet in lands) {
                if (uy + minimum > yet && uy < yet + minimum) {
                    safe = false;
                    break;
                }
            }
            if (safe && m_reservedLands != null) {
                foreach (var range in m_reservedLands) {
                    if (uy + minimum > range.Min && uy < range.Max + RoadWidth) {
                        safe = false;
                        break;
                    }
                }
            }
            if (safe) {
                lands.AddUnique(uy);
            }
        }
        lands.Sort();
        m_reservedLands = createLandLine(ux, lands, isReverseLineAt(m_index));
        m_prevLands = lands;
        m_next = ux + LandWidth + RoadWidth;
    }

    private List<LandRange> createLandLine(int ux, List<int> lands, bool isReverse) {
        var LineRule = isReverse ? TrafficRule.Up : TrafficRule.Down;
        var OppositeRule = isReverse ? TrafficRule.Down : TrafficRule.Up;
        var RightRule = TrafficRule.Right;
        var LeftRule = TrafficRule.Left;
        var SlightRightRule = isReverse ? TrafficRule.UpRight : TrafficRule.DownRight;
        var SlightLeftRule = isReverse ? TrafficRule.UpLeft : TrafficRule.DownLeft;
        int ExtendedLandWidth = LandWidth + RoadWidth + NextLandWidth;
        var reserved = new List<LandRange>();
        bool extended = false;
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
                        createTrafficRules(ux + LandWidth, uy, RoadWidth, ny - uy - RoadWidth, LineRule);
                        createTrafficRules(ux + LandWidth, ny - RoadWidth, RoadWidth, RoadWidth, SlightRightRule);
                        skip = true;
                        break;
                    }
                }
            }
            if (skip) {
                uy = ny;
                extended = false;
                continue;
            }
            bool isRight = Random.Range(0, 2) == 0 ? true : false;
            var depth = ny - uy - RoadWidth;
            var width = LandWidth;
            bool isextendable = true;
            if (extended || uy == CentralUY || isPrevLineOpposite || isNextLineOpposite) {
                isextendable = false;
            }
            if (isextendable && Random.Range(0.0f, 1.0f) < m_Blindness) {
                width = ExtendedLandWidth;
                var range = new LandRange(uy, uy + depth);
                reserved.Add(range);
                extended = true;
            } else {
                extended = false;
            }
            width = Mathf.Max(2, width);
            depth = Mathf.Max(2, depth);
            createLand(ux, uy, width, depth);
            createTrafficRules(ux, uy, width, depth, TrafficRule.Stop);
            createTrafficRules(ux, ny - RoadWidth, width, RoadWidth, isRight ? SlightRightRule : SlightLeftRule);
            createTrafficRules(ux + width, ny - RoadWidth, ExtendedLandWidth - width, RoadWidth, isRight ? SlightRightRule : SlightLeftRule);
            createTrafficRules(ux + width, uy, ExtendedLandWidth - width, depth, LineRule);
            createTrafficRules(ux - PrevRoadWidth, uy, PrevRoadWidth, depth, isPrevLineOpposite ? OppositeRule : LineRule);
            if (!isRight) {
                createTrafficRules(ux - 2, ny - RoadWidth, 2, RoadWidth, isPrevLineOpposite ? OppositeRule : LineRule);
            }
            uy = ny;
        }
        if (uy + 2 <= StageHeight) {
            createLand(ux, uy, LandWidth, StageHeight - uy);
            createTrafficRules(ux, uy, LandWidth, StageHeight - uy, TrafficRule.Stop);
            createTrafficRules(ux + LandWidth, uy, ExtendedLandWidth - LandWidth, StageHeight - uy, LineRule);
            createTrafficRules(ux - PrevRoadWidth, uy, PrevRoadWidth, StageHeight - uy, isPrevLineOpposite ? OppositeRule : LineRule);
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
                setTrafficRule(new Unit(x, y), rule);
            }
        }
    }

    private void createLand(int ux, int uy, int width, int depth) {
        if (m_LandBlocks == null || m_LandBlocks.Length == 0) {
            return;
        }
        var obj = GameObject.Instantiate(m_LandBlocks[Random.Range(0, m_LandBlocks.Length)]);
        var land = obj.GetComponent<LandBlock>();
        land.Construct(width, depth);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = vector3ForUnit(ux, uy);
    }
 }
