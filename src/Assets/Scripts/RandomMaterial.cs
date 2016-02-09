using UnityEngine;
using System.Collections;

public class RandomMaterial : MonoBehaviour {

    [SerializeField] Material[] m_Materials = null;

    // Use this for initialization
	void Start () {
        if (m_Materials.Length > 0) {
            var i = Random.Range(0, m_Materials.Length + 1);
            if (i < m_Materials.Length) {
                GetComponent<Renderer>().material = m_Materials[i];
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
