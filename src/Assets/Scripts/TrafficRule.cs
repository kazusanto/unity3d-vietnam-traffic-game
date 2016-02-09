using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public enum TrafficRule {
        Stop          = 0,
        Left          = 1,
        Right         = 2,
        Up            = 4,
        UpLeft        = (Up | Left),
        UpRight       = (Up | Right),
        Down          = 8,
        DownLeft      = (Down | Left),
        DownRight     = (Down | Right),
    }

    internal class TrafficRuleData
    {
        public TrafficRule rule { 
            get { return m_rule; }
            set {
                m_rule = value;
                m_arrow.transform.localEulerAngles = m_orgAngle + new Vector3(0.0f, angleForRule[value], 0.0f);
                ShowArrow(m_visible);
            }
        }
        public Vector2 position { get { return m_xy; } }

        Vector2 m_xy;
        TrafficRule m_rule;
        GameObject m_arrow;
        Vector3 m_orgAngle;
        bool m_visible;

        Dictionary<TrafficRule, float> angleForRule = new Dictionary<TrafficRule, float>() {
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

        public TrafficRuleData(float x, float y, GameObject arrow, GameObject parent)
        {
            m_xy = new Vector2(x, y);
            m_rule = TrafficRule.Stop;
            m_arrow = GameObject.Instantiate(arrow);
            m_arrow.transform.SetParent(parent.transform);
            m_arrow.transform.localPosition = new Vector3(x, 0.01f, y);
            m_orgAngle = m_arrow.transform.localEulerAngles;
            m_visible = false;
        }

        public void ShowArrow(bool show) {
            m_visible = show;
            if (m_arrow) {
                m_arrow.SetActive(m_rule != TrafficRule.Stop && show);
            }
        }

        public bool Contains(float x, float y) {
            if (m_arrow == null) {
                return false;
            }
            Vector3 pos = m_arrow.transform.localPosition;
            if (x < pos.x - UnitConst.harf || x > pos.x + UnitConst.harf ||
                y < pos.z - UnitConst.harf || y > pos.z + UnitConst.harf) {
                return false;
            }
            return true;
        }

        public void Remove() {
            GameObject.Destroy(m_arrow);
            m_arrow = null;
        }
    }

    internal class TrafficRuleLine
    {
        struct RoadCache {
            public Vector2 position;
            public bool isUp;
        };
        List<TrafficRuleData> m_list = null;
        Dictionary<int, RoadCache> m_cacheForRoad = null;

        public TrafficRuleLine(float x, float y, int count, GameObject arrow, GameObject parent) {
            m_list = new List<TrafficRuleData>(count);
            m_cacheForRoad = new Dictionary<int, RoadCache>(count);
            for (var i = 0; i < count; i++) {
                m_list.Add(new TrafficRuleData(x, y + UnitConst.size * i, arrow, parent));
            }
        }

        public bool Contains(float x, float y) {
            var first = m_list[0].position;
            if (x < first.x - UnitConst.harf || x > first.x + UnitConst.harf) {
                return false;
            }
            var last = m_list[m_list.Count - 1].position;
            if (y < first.y - UnitConst.harf || y > last.y + UnitConst.harf) {
                return false;
            }
            return true;
        }

        public void SetRule(float x, float y, TrafficRule rule) {
            int index = getIndex(x, y);
            if (index != -1) {
                m_list[index].rule = rule;
                updateCacheForRoad(index, rule);
            }
        }

        public TrafficRule GetRule(float x, float y) {
            var data = getData(x, y);
            if (data != null) {
                return data.rule;
            }
            return TrafficRule.Stop;
        }

        public void ShowArrows(bool show) {
            foreach (var data in m_list) {
                data.ShowArrow(show);
            }
        }

        public void Remove() {
            foreach (var data in m_list) {
                data.Remove();
            }
        }

        public List<Vector2> FindRoadPoints(Vector2 center, Range<float> radius, bool withoutOpposite) {
            var result = new List<Vector2>(m_cacheForRoad.Count);
            foreach (var pair in m_cacheForRoad) {
                var diff = pair.Value.position - center;
                if (withoutOpposite && ((diff.y < 0.0f) != pair.Value.isUp)) {
                    continue;
                }
                var distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
                if (distance < radius.Min || distance > radius.Max) {
                    continue;
                }
                result.Add(pair.Value.position);
            }
            return result;
        }

        int getIndex(float x, float y) {
            var first = m_list[0].position;
            var unit = new Vector2(x - first.x, y - first.y).toUnit();
            if (unit.y < 0 || unit.y >= m_list.Count) {
                return -1;
            }
            if (!m_list[unit.y].Contains(x, y)) {
                return -1;
            }
            return unit.y;
        }

        TrafficRuleData getData(float x, float y) {
            var index = getIndex(x, y);
            if (index != -1) {
                return m_list[index];
            }
            return null;
        }

        void updateCacheForRoad(int index, TrafficRule rule) {
            if (rule == TrafficRule.Stop) {
                if (m_cacheForRoad.ContainsKey(index)) {
                    m_cacheForRoad.Remove(index);
                }
            } else {
                if (!m_cacheForRoad.ContainsKey(index)) {
                    RoadCache data = new RoadCache();
                    data.position = m_list[index].position;
                    data.isUp = (rule & TrafficRule.Up) != 0;
                    m_cacheForRoad[index] = data;
                }
            }
        }
    }

    public class TrafficRuleMap
    {
        Dictionary<int, TrafficRuleLine> m_list;
        Vector2 m_coord;
        int m_length;
        GameObject m_arrow;
        GameObject m_parent;
        bool m_visible;
        Vector2 m_cached_center;
        Range<float> m_cached_radius;
        bool m_cached_opposite;
        List<Vector2> m_cached_points = null;

        public TrafficRuleMap(float x, float y, int length, GameObject arrow, GameObject parent) {
            m_list = new Dictionary<int, TrafficRuleLine>();
            m_coord = new Vector2(x, y);
            m_length = length;
            m_arrow = arrow;
            m_parent = parent;
            m_visible = false;
            m_cached_points = null;
        }

        public void SetRule(float x, float y, TrafficRule rule) {
            Unit unit = new Vector2(x - m_coord.x, y - m_coord.y).toUnit();
            SetRule(unit, rule);
        }

        public void SetRule(Unit unit, TrafficRule rule) {
            if (unit.x < 0 || unit.y < 0 || unit.y >= m_length) {
                return;
            }
            Vector2 pos = unit.toVector2() + m_coord;
            if (m_list.ContainsKey(unit.x)) {
                var line = m_list[unit.x];
                line.SetRule(pos.x, pos.y, rule);
            } else {
                var line = new TrafficRuleLine(pos.x, m_coord.y, m_length, m_arrow, m_parent);
                line.SetRule(pos.x, pos.y, rule);
                line.ShowArrows(m_visible);
                m_list[unit.x] = line;
            }
        }

        public TrafficRule GetRule(float x, float y) {
            Unit unit = new Vector2(x - m_coord.x, y - m_coord.y).toUnit();
            return GetRule(unit);
        }

        public TrafficRule GetRule(Unit unit) {
            if (unit.x < 0 || unit.y < 0 || unit.y >= m_length) {
                return TrafficRule.Stop;
            }
            Vector2 pos = unit.toVector2() + m_coord;
            if (m_list.ContainsKey(unit.x)) {
                var line = m_list[unit.x];
                return line.GetRule(pos.x, pos.y);
            } else {
                return TrafficRule.Stop;
            }
        }

        public void ShowArrows(bool show) {
            m_visible = show;
            foreach (var pair in m_list) {
                pair.Value.ShowArrows(show);
            }
        }

        public void RemovePassed(float x) {
            Unit unit = new Vector2(x - m_coord.x, 0.0f).toUnit();
            var list = new List<int>();
            foreach (var key in m_list.Keys) {
                if (key < unit.x) {
                    list.Add(key);
                }
            }
            foreach (var key in list) {
                m_list[key].Remove();
                m_list.Remove(key);
            }
        }

        public Vector2 GetRandomPosition(Vector2 center, Range<float> radius, bool withoutOpposite) {
            bool use_cache = true;
            if (m_cached_points == null ||
                Mathf.Abs(m_cached_center.x - center.x) > UnitConst.size ||
                Mathf.Abs(m_cached_center.y - center.y) > UnitConst.size ||
                Mathf.Abs(m_cached_radius.Min - radius.Min) > UnitConst.size ||
                Mathf.Abs(m_cached_radius.Max - radius.Max) > UnitConst.size ||
                m_cached_opposite != withoutOpposite) {
                use_cache = false;
            }
            List<Vector2> all;
            if (use_cache) {
                all = m_cached_points;
            } else {
                all = new List<Vector2>();
                foreach (var pair in m_list) {
                    var points = pair.Value.FindRoadPoints(center, radius, withoutOpposite);
                    all.AddRange(points);
                }
                m_cached_points = all;
                m_cached_center = center;
                m_cached_radius = radius;
                m_cached_opposite = withoutOpposite;
            }
            if (all.Count > 0) {
                var position = all[Random.Range(0, all.Count)];
                position.x += Random.Range(-UnitConst.harf, UnitConst.harf);
                position.y += Random.Range(-UnitConst.harf, UnitConst.harf);
                return position;
            }
            return Vector2.zero;
        }
    }
}
