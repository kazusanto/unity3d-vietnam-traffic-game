using UnityEngine;
using System.Collections;

public class CabDrive : MonoBehaviour {

	public float Accel = 1.0f;

	private GameObject target = null;
	private CabController cab = null;
    private float steering = 0.0f;
    private float steering_to = 0.0f;
    private int count = 0;
    private bool hit = false;

	// Use this for initialization
	void Start() {
		target = GameObject.FindGameObjectWithTag("Player");
		cab = this.GetComponent<CabController> ();
	}
	
	// Update is called once per frame
	void Update() {
        var mydir2d = new Vector2(transform.forward.x, transform.forward.z);
        var mypos2d = new Vector2(transform.position.x, transform.position.z);
        var targetpos2d = new Vector2(target.transform.position.x, target.transform.position.z);
        var targetdir2d = targetpos2d - mypos2d;
        if (hit && count-- > 0) {
            cab.Move(steering, 0.0f, 0.2f, 0.2f);
            return;
        }
        hit = false;
        if (count <= 0) {
            count = Random.Range(5, 20);
            steering_to = 0.0f;
            var roaddir2d = new Vector2(0.0f, -1.0f);
            var targetangle = Vector2.Angle(mydir2d, targetdir2d);
            var roadangle = Vector2.Angle(mydir2d, roaddir2d);
            if (targetangle < 5.0f) {
                var targetcross = mydir2d.x * targetdir2d.y - mydir2d.y * targetdir2d.x;
                steering_to += (targetcross > 0.0f) ? 1 : -1 * Random.Range(0.1f, 1.0f);
            } else if (roadangle > 10.0f) {
                var roadcross = mydir2d.x * roaddir2d.y - mydir2d.y * roaddir2d.x;
                steering_to += (roadcross > 0.0f) ? -1 : 1 * Random.Range(0.1f, 1.0f);
            }
        }
        if (count > 0) {
            count--;
            if (steering < steering_to - 0.1f) {
                steering += 0.1f;
            } else if (steering > steering_to + 0.1f) {
                steering -= 0.1f;
            }
        }
        if (Vector2.Dot(mydir2d, targetdir2d) < 0.0f) {
            Destroy(gameObject, 2.0f);
        }
        if (cab) {
            cab.Move(steering, Accel, 0.0f, 0.0f);
        }	
    }

    void OnCollisionEnter(Collision other) {
        if (!hit && other.gameObject.CompareTag("Vehicle")) {
            if (other.gameObject.transform.position.z < transform.position.z) {
                if (Random.Range(1, 4) == 1) {
                    hit = true;
                    count = Random.Range(5, 40);
                } else {
                    var mydir2d = new Vector2(transform.forward.x, transform.forward.z);
                    var mypos2d = new Vector2(transform.position.x, transform.position.z);
                    var otherpos2d = new Vector2(other.transform.position.x, other.transform.position.z);
                    var otherdir2d = otherpos2d - mypos2d;
                    var cross = mydir2d.x * otherdir2d.y - mydir2d.y * otherdir2d.x;
                    steering_to += (cross > 0.0f) ? 1 : -1 * Random.Range(0.4f, 1.0f);
                    count = Random.Range(5, 20);
                }
            }
        }
    }
}
