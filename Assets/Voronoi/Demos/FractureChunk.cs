using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Voronoi;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class FractureChunk : MonoBehaviour 
{
	public Material material;

	private Mesh meshSunnyside, meshDouble;

	public Cell cell;

	public bool separated;

	Vector3 prevAngularVelocity, prevVelocity;

	public void Update()
	{
		if(separated) {
			if(!rigidbody) {
				Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
				rigidbody.useGravity = false;
				rigidBody.mass = 1;
				rigidbody.drag = 1f;
				rigidbody.angularDrag = 1f;
				rigidbody.AddForce(new Vector3(0,-100,0));
				float d = 1.0f;
				rigidbody.AddTorque(new Vector3(-cell.site.y, 0, cell.site.x).normalized * Random.Range(1.0f, 10.0f) * 10 + new Vector3(Random.Range(-d,d), 0, Random.Range(-d,d)) * 3);

				prevVelocity = rigidbody.velocity;
				prevAngularVelocity = rigidbody.angularVelocity;
			}
			GetComponent<MeshFilter>().sharedMesh = meshDouble;
		} else {
			GetComponent<MeshFilter>().sharedMesh = meshSunnyside;
		}
	}

	public void CreateFanMesh()
	{
		if (cell.halfEdges.Count > 0)
		{
			meshSunnyside = new Mesh();
			meshDouble = new Mesh();
			meshSunnyside.name = "Chunk " + cell.site.id;
			meshDouble.name = "Chunk " + cell.site.id;

			int numSideVerts = cell.halfEdges.Count + 1;
			int numSideIndices = cell.halfEdges.Count * 3;

			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<int> triangles = new List<int>();
			
			Vector3 vThickness = new Vector3(0, -0.2f, 0);

			vertices.Add(cell.site.ToVector3() - transform.position);
			normals.Add (new Vector3(0,0,1));
			uvs.Add(vertices[0]);

			for (int v = 1; v < numSideVerts; v++)
			{
				vertices.Add(cell.halfEdges[v-1].GetStartPoint().ToVector3() - transform.position);// * 0.98f;
				normals.Add (new Vector3(0,0,1));
				uvs.Add (vertices[v]);
				triangles.Add (0);
				triangles.Add (v);
				triangles.Add (v % (numSideVerts-1) + 1);
			}

			vertices.Add (cell.site.ToVector3() - transform.position + vThickness);
			normals.Add (new Vector3(0,0,-1));
			uvs.Add (vertices[numSideVerts]);

			for (int v = 1, t = 1; v < numSideVerts; v++, t += 3)
			{
				vertices.Add (vertices[v] + vThickness);
				normals.Add (new Vector3(0,0,-1));
				uvs.Add (vertices[v + numSideVerts]);
				triangles.Add (v % (numSideVerts-1) + numSideVerts + 1);
				triangles.Add (v + numSideVerts);
				triangles.Add (numSideVerts);

				triangles.Add (v % (numSideVerts-1) + 1);
				triangles.Add (v);
				triangles.Add (v + numSideVerts);
				triangles.Add (v + numSideVerts);
				triangles.Add (v % (numSideVerts-1) + 1 + numSideVerts);
				triangles.Add (v % (numSideVerts-1) + 1);
			}

			meshSunnyside.vertices = vertices.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.normals = normals.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.uv = uvs.GetRange(0, numSideVerts).ToArray();
			meshSunnyside.triangles = triangles.GetRange(0, numSideIndices).ToArray();
			meshSunnyside.RecalculateBounds();

			meshDouble.vertices = vertices.ToArray();
			meshDouble.normals = normals.ToArray();
			meshDouble.uv = uvs.ToArray();
			meshDouble.triangles = triangles.ToArray();
			meshDouble.RecalculateBounds();

			Update();

			renderer.sharedMaterial = material;
		}
	}

//	public void CreateParentMesh(List<Cell> cells)
//	{
//		GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
//		mesh.name = "Chunk Parent";
//
//		List<Vector3> vertices = new List<Vector3>();
//		List<Vector3> normals = new List<Vector3>();
//		List<Vector2> uvs = new List<Vector2>();
//		List<int> triangles = new List<int>();
//
//		Vector3 vThickness = new Vector3(0, -0.05f, 0);
//
//		foreach (Cell cell in cells) {
//			if (cell.halfEdges.Count > 0 && !cell.separated)
//			{
//				int indexOffset = vertices.Count;
//				int numSideVerts = cell.halfEdges.Count + 1;
//				int numSideIndices = cell.halfEdges.Count * 3;
//
//				vertices.Add(cell.site.ToVector3() - transform.position);
//				normals.Add(new Vector3(0,0,1));
//				uvs.Add(vertices[indexOffset]);
//
//				for (int v = 1; v < numSideVerts; v++)
//				{
//					vertices.Add(cell.halfEdges[v-1].GetStartPoint().ToVector3() - transform.position);
//					normals.Add (new Vector3(0,0,1));
//					uvs.Add(vertices[vertices.Count - 1]);
//					triangles.Add (0 + indexOffset);
//					triangles.Add (v + indexOffset);
//					triangles.Add (v % (numSideVerts-1) + 1 + indexOffset);
//				}
//
////				triangles[numSideIndices - 1] = 1;
////				vertices[numSideVerts] = cell.site.ToVector3() - transform.position + vThickness;
////				normals[numSideVerts] = new Vector3(0,0,-1);
////				uvs[numSideVerts] = vertices[numSideVerts];
////				triangles[numSideIndices] = numSideVerts;
////
////				for (int v = 1, t = 1; v < numSideVerts; v++, t += 3)
////				{
////					vertices[v + numSideVerts] = vertices[v] + vThickness;
////					normals[v + numSideVerts] = new Vector3(0,0,-1);
////					uvs[v + numSideVerts] = vertices[v + numSideVerts];
////					triangles[t + numSideIndices] = v + numSideVerts + 1;
////					triangles[t + 1 + numSideIndices] = v + numSideVerts;
////					triangles[t - 1 + numSideIndices] = numSideVerts;
////				}
//			}
//		}
//		mesh.vertices = vertices.ToArray();
//		mesh.normals = normals.ToArray();
//		mesh.uv = uvs.ToArray();
//		mesh.triangles = triangles.ToArray();
//		mesh.RecalculateBounds();
//		
//		GetComponent<MeshCollider>().sharedMesh = mesh;
//		
//		renderer.sharedMaterial = material;
//	}

//    public void CreateStipMesh(Cell cell)
//    {
//        if (cell.halfEdges.Count > 0)
//        {
//            GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
//            mesh.name = "Chunk " + cell.site.id;
//
//            Vector3[] vertices = new Vector3[cell.halfEdges.Count + 1];
//            int[] triangles = new int[(cell.halfEdges.Count + 0) * 3];
//
//            vertices[0] = cell.site.ToVector3() - transform.position;
//            triangles[0] = 0;
//            for (int v = 1, t = 1; v < vertices.Length; v++, t += 3)
//            {
//                vertices[v] = cell.halfEdges[v - 1].GetStartPoint().ToVector3() - transform.position;
//                triangles[t] = v;
//                triangles[t + 1] = v + 1;
//            }
//            triangles[triangles.Length - 1] = 1;
//
//            mesh.vertices = vertices;
//            mesh.triangles = triangles;
//            mesh.RecalculateBounds();
//
//            renderer.sharedMaterial = material;
//        }
//    }
}