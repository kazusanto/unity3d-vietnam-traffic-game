using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class HousesBlock : UnitBlock
{
    [SerializeField] GameObject m_Garden = null;
    [SerializeField] GameObject[] m_2x2Prefabs = null;
    [SerializeField] GameObject[] m_4x4Prefabs = null;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    protected override void create() {
        createHouses(Width, Length, gameObject);
    }

    void createHouses(int numx, int numy, GameObject coord) {
        int rtx = 0;
        int rty = 0;
        int nx4 = numx / 4;
        int ny4 = numy / 4;
        if (nx4 > 1 && ny4 > 1 && m_4x4Prefabs.Length > 0) {
            rtx = nx4 * 4;
            rty = ny4 * 4;
            createStructures(m_4x4Prefabs, 0, 0, 4, 4, nx4, ny4, coord);
        } else {
            int nx2 = numx / 2;
            int ny2 = numy / 2;
            if (nx2 > 1 && ny2 > 1 && m_2x2Prefabs.Length > 0) {
                rtx = nx2 * 2;
                rty = ny2 * 2;
                createStructures(m_2x2Prefabs, 0, 0, 2, 2, nx2, ny2, coord);
            }
        }
        if (rtx < numx && rty > 0) {
            createBlock(m_Garden, rtx + 1, 0, numx - rtx, rty, coord);
        }
        if (rty < numy) {
            createBlock(m_Garden, 0, rty + 1, numx, numy - rty, coord);
        }
    }

    void createStructures(GameObject[] prefabs, int x, int y, int dx, int dy, int numx, int numy, GameObject parent) {
        int x1 = dx / 2;
        int y1 = dy / 2;
        int x2 = x1 + (numx - 1) * dx;
        int y2 = y1 + (numy - 1) * dy;
        createStructure(prefabs[Random.Range(0, prefabs.Length)], x1, y1, dx, dy, 270.0f, true, parent);
        createStructure(prefabs[Random.Range(0, prefabs.Length)], x2, y1, dx, dy, 180.0f, true, parent);
        createStructure(prefabs[Random.Range(0, prefabs.Length)], x1, y2, dx, dy,   0.0f, true, parent);
        createStructure(prefabs[Random.Range(0, prefabs.Length)], x2, y2, dx, dy,  90.0f, true, parent);
        for (var i = 1; i < numx - 1; i++) {
            createStructure(prefabs[Random.Range(0, prefabs.Length)], x1 + i * dy, y1, dx, dy, 180.0f, false, parent);
            createStructure(prefabs[Random.Range(0, prefabs.Length)], x1 + i * dy, y2, dx, dy,   0.0f, false, parent);
        }
        for (var i = 1; i < numy - 1; i++) {
            createStructure(prefabs[Random.Range(0, prefabs.Length)], x1, y1 + i * dy, dx, dy, 270.0f, false, parent);
            createStructure(prefabs[Random.Range(0, prefabs.Length)], x2, y1 + i * dy, dx, dy,  90.0f, false, parent);
        }
        if (numx > 2 && numy > 2) {
            createBlock(m_Garden, x1 + dx, y1 + dy, numx - dx * 2, numy - dy * 2, parent);
        }
    }

    GameObject createStructure(GameObject prefab, int x, int y, int dx, int dy, float angle, bool isCorner, GameObject parent) {
        var obj = GameObject.Instantiate(prefab);
        obj.GetComponent<StructureBlock>().Construct(dx, dy, isCorner);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = new Vector3().unit(x, y);
        obj.transform.Rotate(new Vector3(0.0f, angle, 0.0f), Space.World);
        return obj;
    }
}
