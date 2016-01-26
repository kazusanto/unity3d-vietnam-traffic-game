using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour {

    public float Speed = 0.7f;

    public static bool Gameover = false;
    public static int Score = 0;

    private bool hit = false;
    private ThirdPersonCharacter controller = null;
    private Animator animator = null;

    // Use this for initialization
	void Start () {
        Score = 0;
        Gameover = false;
        hit = false;
        controller = GetComponent<ThirdPersonCharacter>();
        animator = GetComponent<Animator>();
        if (!controller) {
            animator.SetFloat("direction", -1.0f);
        }
        AudioListener.volume = 10.0f;
	}
	
	// Update is called once per frame
	void Update () {
        float heading = 0.0f;
        if (hit) {
            hit = false;
            Gameover = true;
            if (controller) {
                controller.Move(Vector3.zero, false, true);
            }
            if (!controller) {
                animator.SetFloat("direction", 0.0f);
                animator.SetFloat("speed", 0.0f);
                animator.SetTrigger("onHit");
            }
        } else if (Gameover) {
            if (controller) {
                controller.Move(new Vector3(0.05f, 0.0f, -0.1f), false, false);
            }
        }
        if (!Gameover) {
            var moving = false;
            if (Input.GetMouseButton(0)) {
                moving = true;
            }
            if (Input.GetKey(KeyCode.Space)) {
                moving = true;
            }
            if (moving) {
                heading = -0.6f;
                Move(new Vector3(Speed, 0.0f, 0.0f), false, false);
            } else {
                heading = -1.0f;
                Move(Vector3.zero, false, false);
            }
            Score = (int)transform.position.x;
        }
        if (controller) {
            animator.SetFloat("HeadDir", heading);
        }
	}

    public void Move(Vector3 direction, bool crouch, bool jump) {
        if (controller) {
            controller.Move(direction, crouch, jump);
        } else {
            transform.Translate(direction * Time.deltaTime, Space.World);
            animator.SetFloat("speed", direction.x > 0.0f ? 1.0f : 0.0f);
        }
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Vehicle")) {
            hit = true;
        }
     }
}
