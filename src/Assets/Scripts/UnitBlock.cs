using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;

public class UnitBlock : MonoBehaviour
{
    public int Width { get { return m_width; } }
    public int Length { get { return m_length; } }
    public bool Constructed { get { return (m_width > 0 && m_length > 0); } }

    int m_width = 1;
    int m_length = 1;

    // Construct must be called after GameObject.Instantiate
    public void Construct(int width, int length) {
        m_width = width;
        m_length = length;
        create();
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    protected virtual void create() {
    }

    protected GameObject createBlock(GameObject prefab, int ux, int uy, int numx, int numy, GameObject parent) {
        var obj = GameObject.Instantiate(prefab);
        obj.GetComponent<UnitBlock>().Construct(numx, numy);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = new Vector3().unit(ux, uy);
        return obj;
    }

    protected GameObject createUnit(GameObject prefab, int ux, int uy, float angle, GameObject parent) {
        var obj = GameObject.Instantiate(prefab);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = new Vector3().unit(ux, uy);
        obj.transform.Rotate(new Vector3(0.0f, angle, 0.0f), Space.World);
        return obj;
    }
}
