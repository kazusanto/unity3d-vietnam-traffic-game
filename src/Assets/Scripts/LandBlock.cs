using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class LandBlock : UnitBlock
{
    [SerializeField] float LandHeight = 0.1f;
    [SerializeField] GameObject m_Sidewalk = null;
    [SerializeField] GameObject m_SidewalkCorner = null;
    [SerializeField] GameObject m_Land = null;
    [SerializeField] GameObject[] m_1x1Blocks = null;
    [SerializeField] GameObject[] m_2x2Blocks = null;
    [SerializeField] GameObject[] m_3x3Blocks = null;
    [SerializeField] GameObject[] m_4x4Blocks = null;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    protected override void create() {
        createLand(Width, Length, gameObject);
    }

    void createLand(int numx, int numy, GameObject coord) {
        if (numx < 2 || numy < 2) {
            return;
        }
        createUnit(m_SidewalkCorner, 0, 0, 180.0f, coord);
        createUnit(m_SidewalkCorner, numx - 1, 0, 90.0f, coord);
        createUnit(m_SidewalkCorner, 0, numy - 1, 270.0f, coord);
        createUnit(m_SidewalkCorner, numx - 1, numy - 1, 0.0f, coord);
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
        int size2 = (int)(UnitConst.size);
        var dx = numx <= size2 ? 1 : numx - size2;
        var dy = numy <= size2 ? 1 : numy - size2;
        var upperblock = createUpperBlock(dx, dy, coord);
        if (upperblock) {
            var offset = new Vector3(0.0f, LandHeight, 0.0f);
            offset.x += numx <= size2 ? UnitConst.harf : UnitConst.size;
            offset.z += numy <= size2 ? UnitConst.harf : UnitConst.size;
            upperblock.transform.localPosition = offset;
        }
    }

    GameObject createUpperBlock(int numx, int numy, GameObject parent) {
        GameObject obj = null;
        var list = new List<GameObject>();
        if (numx >= 4 && numy >= 4) {
            list.AddRange(m_4x4Blocks);
        }
        if (numx >= 3 && numy >= 3) {
            list.AddRange(m_3x3Blocks);
        }
        if (numx >= 2 && numy >= 2) {
            list.AddRange(m_2x2Blocks);
        }
        if (numx >= 1 && numy >= 1) {
            list.AddRange(m_1x1Blocks);
        }
        if (list.Count > 0) {
            obj = createBlock(list[Random.Range(0, list.Count)], 0, 0, numx, numy, parent);
        }
        return obj;
    }
}
