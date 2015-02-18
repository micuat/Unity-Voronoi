using UnityEngine;
using System.Collections;

public class BubbleManager : MonoBehaviour {

	int dieCount = 3;
	bool floating = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y < 0f) {
//			rigidbody.AddForce (new Vector3 (0, 10, 0));
			rigidbody.AddForce(new Vector3(Mathf.PerlinNoise(Time.time * 10 + transform.localScale.x, 0.1f * transform.localScale.x) - 0.5f,
			                               1f,
			                               Mathf.PerlinNoise(Time.time * 10 + transform.localScale.x, 0.3f * transform.localScale.x) - 0.5f
			                               ) * 10f);
		} else {
			if( !floating ) {
				rigidbody.velocity = Vector3.zero;
				floating = true;
			}
			rigidbody.AddForce(new Vector3(Mathf.PerlinNoise(Time.time * 10 + transform.localScale.x, 0.1f * transform.localScale.x) - 0.5f,
			                               Mathf.PerlinNoise(Time.time * 10, 0.2f * transform.localScale.x) - 0.5f,
			                               Mathf.PerlinNoise(Time.time * 10 + transform.localScale.x, 0.3f * transform.localScale.x) - 0.5f
			                                 ) * 10f);
			dieCount--;
			transform.localScale *= 1.2f;
			renderer.material.SetColor("_ReflectColor", renderer.material.GetColor("_ReflectColor") - new Color(0.1f,0.1f,0.1f,0.1f) * 3);

			if( dieCount < 0 )
				Destroy(gameObject);
		}
	}
}
