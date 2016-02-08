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
        List<TrafficRuleData> m_list = null;

        public TrafficRuleLine(float x, float y, int count, GameObject arrow, GameObject parent) {
            m_list = new List<TrafficRuleData>(count);
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
            foreach (var data in m_list) {
                if (data.Contains(x, y)) {
                    data.rule = rule;
                }
            }
        }

        public TrafficRule GetRule(float x, float y) {
            foreach (var data in m_list) {
                if (data.Contains(x, y)) {
                    return data.rule;
                }
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
    }

    public class TrafficRuleMap
    {
        Dictionary<int, TrafficRuleLine> m_list;
        Vector2 m_coord;
        int m_length;
        GameObject m_arrow;
        GameObject m_parent;
        bool m_visible;

        public TrafficRuleMap(float x, float y, int length, GameObject arrow, GameObject parent) {
            m_list = new Dictionary<int, TrafficRuleLine>();
            m_coord = new Vector2(x, y);
            m_length = length;
            m_arrow = arrow;
            m_parent = parent;
            m_visible = false;
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

        public Range<Vector2>[] GetRoadRanges(float x) {
            Unit unit = new Vector2(x - m_coord.x, 0.0f).toUnit();
            return GetRoadRanges(unit.x);
        }

        public Range<Vector2>[] GetRoadRanges(int ux) {
            var result = new List<Range<Vector2>>();
            Unit unit = new Unit(ux, 0);
            if (m_list.ContainsKey(ux)) {
                var line = m_list[ux];
                var pos = unit.toVector2() + m_coord;
                for (int uy = 0; uy <= m_length; uy++) {
                    var rule = line.GetRule(pos.x, pos.y + UnitConst.size * unit.y);
                    if (rule == TrafficRule.Stop) {
                        unit.y = uy;
                        continue;
                    }
                    bool last = uy == m_length;
                    rule = line.GetRule(pos.x, pos.y + UnitConst.size * uy);
                    if (rule == TrafficRule.Stop || last) {
                        var min = unit.toVector2() + m_coord;
                        var max = Unit.vector2(unit.x + 1, uy) + m_coord;
                        min.x -= UnitConst.harf;
                        min.y -= UnitConst.harf;
                        max.x -= UnitConst.harf;
                        max.y -= UnitConst.harf;
                        result.Add(new Range<Vector2>(min, max));
                        unit.y = uy;
                    }
                }
            }
            if (result.Count == 0) {
                return null;
            }
            return result.ToArray();
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

        public void ShowArrows(bool show) {
            m_visible = show;
            foreach (var pair in m_list) {
                pair.Value.ShowArrows(show);
            }
        }
    }
}
