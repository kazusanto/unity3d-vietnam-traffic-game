using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;
using StageUnit;

public class LandBlock : StageBlock
{
    [SerializeField] private GameObject m_Sidewalk = null;
    [SerializeField] private GameObject m_SidewalkCorner = null;
    [SerializeField] private GameObject m_Land = null;
    [SerializeField] private GameObject m_Grass = null;
    [SerializeField] private GameObject[] m_Trees = null;
    [SerializeField] private GameObject[] m_Items1x1 = null;

    public const float LandHeight = 0.1f;

    // Use this for initialization
    void Start () {
        createLand(Width, Length, gameObject);
    }

    // Update is called once per frame
    void Update () {
    }

    void createLand(int numx, int numy, GameObject coord) {
        createUnit(m_SidewalkCorner, 0, 0, 90.0f, coord);
        createUnit(m_SidewalkCorner, numx - 1, 0, 0.0f, coord);
        createUnit(m_SidewalkCorner, numx - 1, numy - 1, 270.0f, coord);
        createUnit(m_SidewalkCorner, 0, numy - 1, 180.0f, coord);
        for (var x = 1; x < numx - 1; x++) {
            createUnit(m_Sidewalk, x, 0, 90.0f, coord);
            createUnit(m_Sidewalk, x, numy - 1, 270.0f, coord);
        }
        for (var y = 1; y < numy - 1; y++) {
            createUnit(m_Sidewalk, 0, y, 180.0f, coord);
            createUnit(m_Sidewalk, numx - 1, y, 0.0f, coord);
        }
        for (var x = 1; x < numx - 1; x++) {
            for (var y = 1; y < numy - 1; y++) {
                createUnit(m_Land, x, y, 0.0f, coord);
            }
        }
        if (numx >= 2 && numy >= 2) {
            var dx = numx <= 3 ? 1 : numx - 2;
            var dy = numy <= 3 ? 1 : numy - 2;
            var offset = Vector3.zero;
            offset.x = -UnitConst.size * ((numx - dx) % 2 == 1 ? 0.5f : 0.0f);
            offset.y = LandHeight;
            offset.z = -UnitConst.size * ((numy - dy) % 2 == 1 ? 0.5f : 0.0f);
            var obj = new GameObject();
            obj.transform.SetParent(coord.transform);
            obj.transform.localPosition = new Vector3().unit(1, 1) + offset;
            createGarden(dx, dy, obj);
        }
    }

    void createGarden(int numx, int numy, GameObject parent) {
        var rndx = UnitConst.harf;
        for (var x = 0; x < numx; x++) {
            for (var y = 0; y < numy; y++) {
                bool isSidewalk = (x == 0 || y == 0 || x == numx - 1 || y == numy - 1);
                bool grass = false;
                if (Random.Range(0, 2) == 0) {
                    createUnit(m_Grass, x, y, 0.0f, parent);
                    grass = true;
                }
                bool done = false;
                if (Mathf.Abs(parent.transform.position.z) < UnitConst.size) {
                    // don't put the items on the line for player walking.
                    done = true;
                }
                if (!done && grass && m_Trees.Length > 0) {
                    var idx = Random.Range(0, m_Trees.Length * 2);
                    if (idx < m_Trees.Length) {
                        createUnit(m_Trees[idx], x, y, 0.0f, parent);
                        done = true;
                    }
                }
                if (!done && isSidewalk && m_Items1x1.Length > 0) {
                    var idx = Random.Range(0, m_Items1x1.Length * 2);
                    if (idx < m_Items1x1.Length) {
                        var obj = createUnit(m_Items1x1[idx], x, y, 0.0f, parent);
                        obj.transform.localPosition += new Vector3(Random.Range(-rndx, rndx), 0.0f, 0.0f);
                        done = true;
                    }
                }
            }
        }
    }

    GameObject createUnit(GameObject prefab, int ux, int uy, float angle, GameObject parent) {
        var obj = GameObject.Instantiate(prefab);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = new Vector3().unit(ux, uy);
        obj.transform.localEulerAngles += new Vector3(0.0f, angle, 0.0f);
        return obj;
    }
}
