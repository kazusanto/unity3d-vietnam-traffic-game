using UnityEngine;
using System.Collections;

public class CabDrive : MonoBehaviour {

    [SerializeField] private string m_StageTag = "Stage";
    [SerializeField] private AudioClip m_Horn = null;
    [SerializeField] private AudioClip m_Drive = null;
    [SerializeField] private AudioClip m_Idle = null;

	private GameObject m_target = null;
    private CabController m_cab = null;
    private StageBuilder m_stage = null;
    private float m_steering = 0.0f;
    private float m_steering_to = 0.0f;
    private int m_count = 0;
    private bool m_hit = false;
    private bool m_near = false;
    private AudioSource m_audio = null;
    private float m_horn_pitch = 1.0f;
    private float m_accel = 0.0f;
    private float m_brake = 0.0f;

	// Use this for initialization
	void Start() {
        m_target = GameObject.FindGameObjectWithTag("Player");
        m_cab = this.GetComponent<CabController> ();
        m_stage = GameObject.FindGameObjectWithTag(m_StageTag).GetComponent<StageBuilder>();
        m_audio = gameObject.GetComponent<AudioSource>();
        m_horn_pitch = (Random.Range(0, 2) == 0) ? 1.0f : 1.1f;
	}
	
	// Update is called once per frame
	void Update() {
        var mydir2d = new Vector2(transform.forward.x, transform.forward.z);
        var mypos2d = new Vector2(transform.position.x, transform.position.z);
        var targetpos2d = new Vector2(m_target.transform.position.x, m_target.transform.position.z);
        var targetdir2d = targetpos2d - mypos2d;
        if (m_hit && m_count-- > 0) {
            m_cab.Move(m_steering, 0.0f, 0.2f, 0.2f);
            return;
        }
        m_hit = false;
        if (m_count <= 0) {
            m_count = Random.Range(5, 10);
            m_steering_to = 0.0f;
            m_accel = 1.0f;
            m_brake = 0.0f;
            var roaddir2d = getRoadDirection(transform.position.x, transform.position.z, mydir2d, 1.0f);
            if (roaddir2d.Equals(Vector2.zero)) {
                roaddir2d = getRoadDirection(transform.position.x, transform.position.z, mydir2d, 0.0f);
            }
            var targetangle = Vector2.Angle(mydir2d, targetdir2d);
            var roadangle = Vector2.Angle(mydir2d, roaddir2d);
            var distance = Vector2.Distance(mypos2d, targetpos2d);
            if (targetangle < 5.0f && distance < 20.0f) {
                m_near = true;
                var targetcross = mydir2d.x * targetdir2d.y - mydir2d.y * targetdir2d.x;
                m_steering_to += (targetcross > 0.0f ? 1.0f : -1.0f) * Random.Range(0.05f, 0.1f);
                m_accel = 0.1f;
            } else if (!roaddir2d.Equals(Vector2.zero)) {
                m_near = false;
                if (roadangle > 5.0f) {
                    var roadcross = mydir2d.x * roaddir2d.y - mydir2d.y * roaddir2d.x;
                    m_steering_to += (roadcross > 0.0f ? -1.0f : 1.0f) * Random.Range(0.4f, 0.8f);
                    m_accel = 0.05f;
                    m_brake = 0.0f;
                    if (roadangle < 10.0f) {
                        m_count = Random.Range(1, 2);
                    } else if (roadangle < 20.0f) {
                        m_count = Random.Range(2, 4);
                    } else if (roadangle < 30.0f) {
                        m_count = Random.Range(4, 6);
                    } else {
                        m_count = Random.Range(6, 8);
                    }
                }
            } else {
                m_accel = 0.1f;
                m_brake = 0.1f;
                Destroy(gameObject, 2.0f);
            }
            if (targetangle > 120) {
                Destroy(gameObject, 2.0f);
            }
            if (Random.Range(0, 20) == 0) {
                m_audio.pitch = 1.0f;
                m_audio.PlayOneShot(m_Drive);
            }
        }
        if (m_count > 0) {
            m_count--;
            if (m_steering < m_steering_to - 0.1f) {
                m_steering += 0.1f;
            } else if (m_steering > m_steering_to + 0.1f) {
                m_steering -= 0.1f;
            }
        }
        if (m_near && Random.Range(0, 10) == 0) {
            m_audio.pitch = m_horn_pitch;
            m_audio.PlayOneShot(m_Horn);
        }
//        if (Vector2.Dot(mydir2d, targetdir2d) < 0.0f) {
//            Destroy(gameObject, 2.0f);
//        }
        if (m_cab) {
            m_cab.Move(m_steering, m_accel, m_brake, m_brake);
        }	
    }

    void OnCollisionEnter(Collision other) {
        if (!m_hit && other.gameObject.CompareTag("Vehicle")) {
            if (other.gameObject.transform.position.z < transform.position.z) {
                if (Random.Range(1, 4) == 1) {
                    m_hit = true;
                    m_count = Random.Range(5, 20);
                    m_audio.pitch = 1.0f;
                    m_audio.PlayOneShot(m_Idle);
                } else {
                    var mydir2d = new Vector2(transform.forward.x, transform.forward.z);
                    var mypos2d = new Vector2(transform.position.x, transform.position.z);
                    var otherpos2d = new Vector2(other.transform.position.x, other.transform.position.z);
                    var otherdir2d = otherpos2d - mypos2d;
                    var cross = mydir2d.x * otherdir2d.y - mydir2d.y * otherdir2d.x;
                    m_steering_to += (cross > 0.0f) ? 1 : -1 * Random.Range(0.4f, 1.0f);
                    m_count = Random.Range(5, 20);
                }
            }
        }
    }

    private Vector2 getRoadDirection(float x, float z, Vector2 vec, float speed)
    {
        Vector2 dist = vec.normalized * 4.0f * speed;
        x += dist.x;
        z += dist.y;
        StageBuilder.TrafficRule rule = m_stage.GetTrafficRule(x, z);
        StageBuilder.TrafficRule choose = rule;
        switch (rule) {
        case StageBuilder.TrafficRule.DownLeft:
            choose = Random.Range(0, 2) == 0 ? StageBuilder.TrafficRule.Down : StageBuilder.TrafficRule.Left;
            break;
        case StageBuilder.TrafficRule.DownRight:
            choose = Random.Range(0, 2) == 0 ? StageBuilder.TrafficRule.Down : StageBuilder.TrafficRule.Right;
            break;
        case StageBuilder.TrafficRule.UpLeft:
            choose = Random.Range(0, 2) == 0 ? StageBuilder.TrafficRule.Up : StageBuilder.TrafficRule.Left;
            break;
        case StageBuilder.TrafficRule.UpRight:
            choose = Random.Range(0, 2) == 0 ? StageBuilder.TrafficRule.Up : StageBuilder.TrafficRule.Right;
            break;
        }
        Vector2 dir = Vector2.zero;
        switch (choose) {
        case StageBuilder.TrafficRule.Down:
            dir = new Vector2(0.0f, -1.0f);
            break;
        case StageBuilder.TrafficRule.Up:
            dir = new Vector2(0.0f, 1.0f);
            break;
        case StageBuilder.TrafficRule.Right:
            dir = new Vector2(1.0f, 0.0f);
            break;
        case StageBuilder.TrafficRule.Left:
            dir = new Vector2(-1.0f, -0.0f);
            break;
        }
        return dir;
    }
}
