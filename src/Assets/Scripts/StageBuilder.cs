using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

using LandRange = Game.Range<int>;

public class StageBuilder : MonoBehaviour {

    [SerializeField] private GameObject[] m_LandBlocks = null;

    [SerializeField] private GameObject m_Arrow = null;
    [SerializeField] private bool m_isDebugMode = false;
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

    public float ForwardEnd { get { return worldForUnit(m_next, 0).x; } }
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
    private TrafficRuleMap m_trafficRuleMap = null;
    private bool m_inited = false;

    private Vector2 worldForUnit(int ux, int uy) {
        float wx = (float)(ux + m_Backward) * UnitConst.size;
        float wz = (float)(uy + m_Near) * UnitConst.size;
        return new Vector2(wx, wz);
    }

    private Vector3 vector3ForUnit(int ux, int uy) {
        Vector2 xy = worldForUnit(ux, uy);
        return new Vector3(xy.x, 0.0f, xy.y);
    }

    private Unit unitForWorld(float wx, float wz) {
        float harf = UnitConst.size / 2.0f;
        int x = (int)((wx + harf) / UnitConst.size) - m_Backward;
        int y = (int)((wz + harf) / UnitConst.size) - m_Near;
        return new Unit(x, y);
    }

    public bool isReverseRoad() {
        TrafficRule rule = TrafficRule.Stop;
        float x = m_player.transform.position.x;
        float z = m_player.transform.position.z;
        float offs = 0.0f;
        while (rule == TrafficRule.Stop && offs < 10.0f) {
            rule = GetTrafficRule(x + offs, z);
            offs += UnitConst.size;
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
        rule = m_trafficRuleMap.GetRule(x, z);
        if (beyond && rule == TrafficRule.Stop) { 
            rule = TrafficRule.Right;
        }
        return rule;
    }

    public Range<Vector2>[] GetRoadArea(float x) {
        return m_trafficRuleMap.GetRoadRanges(x);
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
        if (!m_inited) {
            m_inited = true;
            m_player = GameObject.FindGameObjectWithTag("Player");
            m_landBase = new GameObject();
            m_landBase.transform.SetParent(transform);
            float x = m_Backward * UnitConst.size;
            float y = m_Near * UnitConst.size;
            m_trafficRuleMap = new TrafficRuleMap(x, y, m_Far - m_Near + 1, m_Arrow, gameObject);
            var unit = unitForWorld(m_player.transform.position.x, 0.0f);
            m_fstep = 0;
            m_bstep = 0;
            while (m_next < unit.x) {
                buildNext();
            }
            m_fstep = 1;
        }
    }

	// Update is called once per frame
	void Update () {
        var unit = unitForWorld(m_player.transform.position.x, 0.0f);
        var built = false;
        while (m_next < unit.x + m_Forward) {
            m_trafficRuleMap.ShowArrows(m_isDebugMode);
            buildNext();
            m_index++;
            m_bstep = 1;
            built = true;
        }
        if (built) {
            removePassed();
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

    private void removePassed() {
        var minx = m_player.transform.position.x + (m_Backward * UnitConst.size) - 1.0f;
        for (var i = 0; i < m_landBase.transform.childCount; i++) {
            var child = m_landBase.transform.GetChild(i);
            if (child.localPosition.x <= minx) {
                GameObject.Destroy(child.gameObject);
            }
        }
        m_trafficRuleMap.RemovePassed(minx);
    }

    private void createTrafficRules(int ux, int uy, int width, int depth, TrafficRule rule) {
        for (var x = ux; x < ux + width; x++) {
            for (var y = uy; y < uy + depth; y++) {
                m_trafficRuleMap.SetRule(new Unit(x, y), rule);
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
        obj.transform.SetParent(m_landBase.transform);
        obj.transform.localPosition = vector3ForUnit(ux, uy);
    }
 }
