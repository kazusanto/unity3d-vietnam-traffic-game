using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour {

    private static bool s_gameover = false;
    private static int s_score = 0;
    public static bool GameOver { get { return s_gameover; } }
    public static int Score { get { return s_score; } }

    [SerializeField] private float m_Speed = 0.5f;
    [SerializeField] private string[] m_Foots = { "LeftToeBase", "RightToeBase" };
    [SerializeField] private AudioClip m_Footstep = null;
    [SerializeField] private AudioClip m_Crash = null;
    [SerializeField] private bool m_isDemoPlay = false;

    private bool m_hit = false;
    private ThirdPersonCharacter m_controller = null;
    private Animator m_animator = null;
    private AudioSource m_audio = null;
    private Transform[] m_foots = null;
    private float[] m_footYs = null;

    // Use this for initialization
	void Start () {
        s_score = 0;
        s_gameover = false;
        m_hit = false;
        m_controller = GetComponent<ThirdPersonCharacter>();
        m_animator = GetComponent<Animator>();
        m_audio = gameObject.GetComponent<AudioSource>();

        if (!m_controller) {
            m_animator.SetFloat("direction", -1.0f);
        }
        m_foots = new Transform[m_Foots.Length];
        m_footYs = new float[m_Foots.Length];
        for (var i = 0; i < m_Foots.Length; i++) {
            m_foots[i] = searchTransform(transform, m_Foots[i]);
            m_footYs[i] = m_foots[i].position.y;
        }
	}
	
	// Update is called once per frame
	void Update () {
        float heading = 0.0f;
        if (m_hit) {
            m_hit = false;
            if (!m_isDemoPlay) {
                s_gameover = true;
            }
            if (m_controller) {
                m_controller.Move(Vector3.zero, false, true);
            }
            if (!m_controller) {
                m_animator.SetFloat("direction", 0.0f);
                m_animator.SetFloat("speed", 0.0f);
                m_animator.SetTrigger("onHit");
            }
            m_audio.PlayOneShot(m_Crash);
        } else if (s_gameover) {
            if (m_controller) {
                if (transform.position.z > -5.0f) {
                    m_controller.Move(new Vector3(0.05f, 0.0f, -0.1f), false, false);
                } else {
                    m_controller.Move(Vector3.zero, false, false);
                }
            }
        }
        if (!s_gameover) {
            var moving = false;
            if (Input.GetMouseButton(0)) {
                moving = true;
            }
            if (Input.GetKey(KeyCode.Space)) {
                moving = true;
            }
            if (moving) {
                heading = -0.6f;
                Move(new Vector3(m_Speed, 0.0f, 0.0f), false, false);
            } else {
                heading = -1.0f;
                Move(Vector3.zero, false, false);
            }
            s_score = (int)transform.position.x;
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
            if (transform.position.y > 0.05) {
                heading = 0.0f;
            }
        }
        if (m_controller) {
            m_animator.SetFloat("HeadDir", heading);
        }
	}

    public void Move(Vector3 direction, bool crouch, bool jump) {
        if (m_controller) {
            m_controller.Move(direction, crouch, jump);
        } else {
            transform.Translate(direction * Time.deltaTime, Space.World);
            m_animator.SetFloat("speed", direction.x > 0.0f ? 1.0f : 0.0f);
        }
        if (!s_gameover && direction.magnitude > 0.0f) {
            bool footstep = false;
            for (var i = 0; i < m_foots.Length; i++) {
                var y = m_foots[i].position.y - transform.position.y;
                if (m_footYs[i] >= 0.01 && y < 0.01) {
                    footstep = true;
                }
                m_footYs[i] = y;
            }
            if (footstep) {
                m_audio.PlayOneShot(m_Footstep);
            }
        }
    }

    void OnCollisionEnter(Collision other) {
        if (!s_gameover &&  other.gameObject.CompareTag("Vehicle")) {
            m_hit = true;
        }
     }

    private Transform searchTransform(Transform root, string name) {
        var found = root.Find(name);
        if (!found) {
            for (var i = 0; i < root.childCount; i++) {
                var child = root.GetChild(i);
                found = searchTransform(child, name);
                if (found) {
                    break;
                }
            }
        }
        return found;
    }
}
