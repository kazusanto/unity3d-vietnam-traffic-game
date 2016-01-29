using UnityEngine;
using System.Collections;

public class GroundController : MonoBehaviour {

    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private float m_NearSize;
    [SerializeField] private float m_FarSize;

    private GameObject player = null;
    private GameObject[] m_near;
    private GameObject[] m_far;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");

        m_near = buildObjects(3, m_NearSize, 0.0f);
        m_far = buildObjects(3, m_FarSize, (m_NearSize + m_FarSize) / 2.0f);
	}
	
	// Update is called once per frame
	void Update () {
        updateTiles(m_near, m_NearSize);
        updateTiles(m_far, m_FarSize);
    }

    private GameObject[] buildObjects(int num, float size, float z) {
        GameObject[] result = new GameObject[num];
        for (var i = 0; i < num; i++) {
            var obj = Instantiate(m_Prefab);
            obj.transform.localScale = new Vector3(size / 10.0f, 1.0f, size / 10.0f);
            obj.transform.position = new Vector3(size * i - ((float)num / 2.0f), 0.0f, z);
            result[i] = obj;
        }
        return result;
    }

    private void updateTiles(GameObject[] objs, float size) {
        var xp = player.transform.position.x;
        foreach (var obj in objs) {
            var x = obj.transform.position.x;
            if (x + size < xp) {
                obj.transform.Translate(Vector3.right * (size * objs.Length));
            }
        }
    }
}
