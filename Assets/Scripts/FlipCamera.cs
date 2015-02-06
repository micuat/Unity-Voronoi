﻿using UnityEngine;
using System.Collections;

public class FlipCamera : MonoBehaviour {

	// http://wiki.unity3d.com/index.php?title=InvertCamera

	Matrix4x4 m;

	// Use this for initialization
	void Start () {
		m = Matrix4x4.identity;
		m [1, 1] = -1;

		camera.transform.position = new Vector3 (Screen.width / 2.0f, Screen.height / 2.0f, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnPreCull () {
		camera.ResetWorldToCameraMatrix ();
		camera.ResetProjectionMatrix ();
		camera.projectionMatrix = camera.projectionMatrix * m;
	}
	
	void OnPreRender () {
		GL.SetRevertBackfacing (true);
	}
	
	void OnPostRender () {
		GL.SetRevertBackfacing (false);
	}}
