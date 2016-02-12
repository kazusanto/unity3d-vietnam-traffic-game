using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class StructureBlock : UnitBlock
{
    [SerializeField] float m_StructureHeight = 3.0f;
    [SerializeField] float m_FloorHeight = 0.1f;
    [SerializeField] GameObject[] m_Prefabs = null;
    [SerializeField] GameObject[] m_CornerPrefabs = null;
    [SerializeField] GameObject[] m_Rooms = null;
    [SerializeField] GameObject[] m_Eaves = null;
    [SerializeField] GameObject[] m_Doors = null;
    [SerializeField] GameObject[] m_Signboards = null;
    [SerializeField] GameObject[] m_UpperStructures = null;

    bool m_isCorner = false;

    // Construct must be called after GameObject.Instantiate
    public void Construct(int width, int length, bool isCorner) {
        m_isCorner = isCorner;
        base.Construct(width, length);
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    protected override void create() {
        createStructure(gameObject);
    }

    void createStructure(GameObject coord) {
        int idx;
        if (m_isCorner) {
            idx = Random.Range(0, m_CornerPrefabs.Length);
            createUnit(m_CornerPrefabs[idx], 0, 0, 0.0f, coord);
        } else {
            idx = Random.Range(0, m_Prefabs.Length);
            createUnit(m_Prefabs[idx], 0, 0, 0.0f, coord);
        }
        idx = Random.Range(0, m_Rooms.Length);
        if (idx < m_Rooms.Length) {
            var obj = createUnit(m_Rooms[idx], 0, 0, 0.0f, coord);
            obj.transform.localPosition += new Vector3(0.0f, m_FloorHeight, 0.0f);
        }
        createParts(0.0f, coord);
        if (m_isCorner) {
            createParts(-90.0f, coord);
        }
        idx = Random.Range(0, m_UpperStructures.Length);
        if (idx < m_UpperStructures.Length) {
            var obj = GameObject.Instantiate(m_UpperStructures[idx]);
            obj.GetComponent<StructureBlock>().Construct(Width, Length, m_isCorner);
            obj.transform.localPosition += new Vector3(0.0f, m_StructureHeight, 0.0f);
        }
    }

    void createParts(float angle, GameObject parent) {
        var idx = Random.Range(0, m_Eaves.Length * 2);
        if (idx < m_Eaves.Length) {
            createUnit(m_Eaves[idx], 0, 0, angle, parent);
        }
        idx = Random.Range(0, m_Doors.Length * 2);
        if (idx < m_Doors.Length) {
            createUnit(m_Doors[idx], 0, 0, angle, parent);
        }
        idx = Random.Range(0, m_Signboards.Length * 2);
        if (idx < m_Signboards.Length) {
            createUnit(m_Signboards[idx], 0, 0, angle, parent);
        }
    }
}
