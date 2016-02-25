using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class StructureBlock : UnitBlock
{
    [SerializeField] float m_StructureHeight = 3.0f;
    [SerializeField] float m_CanopyHeight = 2.0f;
    [SerializeField] float m_FloorHeight = 0.1f;
    [SerializeField] GameObject[] m_Prefabs = null;
    [SerializeField] GameObject[] m_CornerPrefabs = null;
    [SerializeField] GameObject[] m_Rooms = null;
    [SerializeField] GameObject[] m_Doors = null;
    [SerializeField] GameObject[] m_Canopies = null;
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
        createParts(false, coord);
        if (m_isCorner) {
            createParts(true, coord);
        }
        idx = Random.Range(0, m_UpperStructures.Length);
        if (idx < m_UpperStructures.Length) {
            var obj = GameObject.Instantiate(m_UpperStructures[idx]);
            obj.GetComponent<StructureBlock>().Construct(Width, Length, m_isCorner);
            obj.transform.localPosition += new Vector3(0.0f, m_StructureHeight, 0.0f);
            obj.transform.SetParent(coord.transform);
        }
    }

    void createParts(bool isCorner, GameObject parent) {
        var angle = isCorner ? -90.0f : 0.0f;
        var offset = isCorner ? new Vector3(-2.0f, 0.0f, 0.0f) : new Vector3(0.0f, 0.0f, 2.0f);
        var idx = 0;
        idx = Random.Range(0, m_Doors.Length * 2);
        if (idx < m_Doors.Length) {
            var obj = createUnit(m_Doors[idx], 0, 0, angle, parent);
            obj.transform.localPosition += offset;
        }
        idx = Random.Range(0, m_Canopies.Length * 2);
        if (idx < m_Canopies.Length) {
            var obj = createUnit(m_Canopies[idx], 0, 0, angle, parent);
            obj.transform.localPosition += offset + new Vector3(0.0f, m_CanopyHeight, 0.0f);
        }
        idx = Random.Range(0, m_Signboards.Length * 2);
        if (idx < m_Signboards.Length) {
            var obj = createUnit(m_Signboards[idx], 0, 0, angle, parent);
            obj.transform.localPosition += offset + new Vector3(0.0f, m_CanopyHeight, 0.0f);
        }
    }
}
