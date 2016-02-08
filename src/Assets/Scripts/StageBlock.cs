using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StageUnit;
using Common;

public class StageBlock : MonoBehaviour
{
    public int Width { get { return m_width; } }
    public int Length { get { return m_length; } }
    public bool Constructed { get { return (m_width > 0 && m_length > 0); } }

    int m_width;
    int m_length;

    // Construct must be called after GameObject.Instantiate
    public void Construct(int width, int length) {
        m_width = width;
        m_length = length;
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }
}
