using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using Cell = Voronoi.Cell; 

namespace Haptic {
	public class HapticHandler
	{
		private float impactTime = -1;
		List<GameObject> chunks;
		private Bounds bounds;
		private Vector3 impactPoint;

		public HapticHandler(List<GameObject> chunks, Bounds bounds, Vector3 impactPoint) {
			impactTime = Time.timeSinceLevelLoad;

			this.chunks = chunks;
			this.bounds = bounds;
			this.impactPoint = impactPoint;

			foreach (GameObject chunk in chunks)
			{
				Cell cell = chunk.GetComponent<FractureChunk>().cell;
				float length = 2.0f * 2.4f / 6.0f;
				//float length = Mathf.Min((Time.timeSinceLevelLoad - impactTime)*2, 2.0f * 2.4f / 6.0f);
				bool s = (cell.site.ToVector3() - bounds.center - impactPoint).magnitude < length;
				if(s) chunk.GetComponent<FractureChunk>().ApplyForce(impactPoint);
			}
		}
	}
}
