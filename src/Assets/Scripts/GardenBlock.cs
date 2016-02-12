using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class GardenBlock : UnitBlock
{
    [SerializeField] GameObject[] m_Grasses = null;
    [SerializeField] GameObject[] m_Trees = null;
    [SerializeField] GameObject[] m_Items1x1 = null;

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
        var rndx = UnitConst.harf;
        for (var x = 0; x < numx; x++) {
            for (var y = 0; y < numy; y++) {
                bool isSidewalk = (x == 0 || y == 0 || x == numx - 1 || y == numy - 1);
                bool grass = false;
                {
                    var idx = Random.Range(0, m_Grasses.Length * 2);
                    if (idx < m_Grasses.Length) {
                        createUnit(m_Grasses[idx], x, y, 0.0f, coord);
                        grass = true;
                    }
                }
                bool done = false;
                if (Mathf.Abs(coord.transform.position.z + y * UnitConst.size) < UnitConst.size) {
                    // don't put any items on the line where player will walk.
                    done = true;
                }
                if (!done && grass && m_Trees.Length > 0) {
                    var idx = Random.Range(0, m_Trees.Length * 2);
                    if (idx < m_Trees.Length) {
                        createUnit(m_Trees[idx], x, y, 0.0f, coord);
                        done = true;
                    }
                }
                if (!done && isSidewalk && m_Items1x1.Length > 0) {
                    var idx = Random.Range(0, m_Items1x1.Length * 2);
                    if (idx < m_Items1x1.Length) {
                        var obj = createUnit(m_Items1x1[idx], x, y, 0.0f, coord);
                        obj.transform.localPosition += new Vector3(Random.Range(-rndx, rndx), 0.0f, 0.0f);
                        done = true;
                    }
                }
            }
        }
    }
}
