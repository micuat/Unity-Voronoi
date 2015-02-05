﻿using UnityEngine;
//using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using Cell = Voronoi.Cell;
using HapticHandler = Haptic.HapticHandler;

using UnityOSC;

public class VoronoiDemo : MonoBehaviour
{
    public int numSites = 36;
    public Bounds bounds;
	public GameObject chunkObj;

    private List<Point> sites;
    private FortuneVoronoi voronoi;
    public VoronoiGraph graph;
	private List<GameObject> chunks;
	private GameObject chunkParent;
	private List<bool> separated;

	private List<HapticHandler> handlers;

	private Dictionary<string, ServerLog> servers;

    void Start()
    {
        sites = new List<Point>();
        voronoi = new FortuneVoronoi();
		chunks = new List<GameObject>();
		handlers = new List<HapticHandler> ();

		CreateSites(true, false);
		CreateChunks();

		OSCHandler.Instance.Init(); //init OSC
		servers = new Dictionary<string, ServerLog>();

		//OSCHandler.Instance.SendMessageToClient("NiwServer", "/niw/server/config/invert/low/avg/zero", 0);
	}

    void Update()
	{
		OSCHandler.Instance.UpdateLogs();
		servers = OSCHandler.Instance.Servers;
		 
		foreach (KeyValuePair<string, ServerLog> item in servers)
		{
			foreach(OSCPacket packet in item.Value.packets)
			{
				if(packet.Address.Equals("/niw/client/aggregator/floorcontact")) {
					if(string.Compare((string)packet.Data[0], "add") == 0) {
						if((float)packet.Data[4] > 20000) {
							float x = Mathf.Lerp (bounds.min.x, bounds.max.x, (float)packet.Data[2] / 6.0f);
							float y = Mathf.Lerp (bounds.min.z, bounds.max.z, (float)packet.Data[3] / 6.0f);
//							float x = (float)packet.Data[2] / 6.0f * 2.4f;
//							float y = (float)packet.Data[3] / 6.0f * 2.4f;
							handlers.Add(new HapticHandler(chunks, bounds, new Vector3(x,0,y)));
						}
					}
				}
			}
		}

		if (graph) {
			foreach( var h in handlers) {
				h.Update();
			}
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			handlers.Add(new HapticHandler(chunks, bounds, new Vector3(Random.Range (-2.4f, 2.4f), 0, Random.Range (-2.4f, 2.4f))));
		}
		if (Input.GetKeyDown(KeyCode.G))
        {
            CreateSites(true, false);
			CreateChunks();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RelaxSites(1);
        }
		if (Input.GetKeyDown(KeyCode.M) && graph)
		{
			CreateChunks();
		}
    }

	void CreateChunks()
	{
		foreach (GameObject obj in chunks)
		{
			Destroy(obj);
		}
		chunks.Clear();
		Destroy (chunkParent);

		foreach (Cell cell in graph.cells)
		{
			GameObject chunk = Instantiate(chunkObj, cell.site.ToVector3(), Quaternion.identity) as GameObject;
            chunk.name = "Chunk " + cell.site.id;
			chunk.GetComponent<FractureChunk>().cell = cell;
			chunks.Add(chunk);

			chunk.GetComponent<FractureChunk>().separated = false;
			chunk.GetComponent<FractureChunk>().CreateFanMesh();
			chunk.transform.position = cell.site.ToVector3() + new Vector3(0,bounds.center.y,0);
		}
	}
	
    void Compute(List<Point> sites)
    {
        this.sites = sites;
        this.graph = this.voronoi.Compute(sites, this.bounds);
    }

    void CreateSites(bool clear = true, bool relax = false, int relaxCount = 2)
    {
        List<Point> sites = new List<Point>();
        if (!clear)
        {
            sites = this.sites.Take(this.sites.Count).ToList();
        }

        // create vertices
        for (int i = 0; i < numSites; i++)
        {
			Point site = new Point(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z), 0);
			sites.Add(site);
        }
		
		Compute(sites);

        if (relax)
        {
            RelaxSites(relaxCount);
        }
	}

	void CreateGridSites(bool clear = true, int x = 10, int y = 10, float d = 0.1f)
	{
		List<Point> sites = new List<Point>();
		if (!clear)
		{
			sites = this.sites.Take(this.sites.Count).ToList();
		}
		
		// create vertices
		for (int i = 1; i < x; i++)
		{
			for (int j = 1; j < y; j++)
			{
				Point site = new Point(Mathf.Lerp(bounds.min.x, bounds.max.x, ((float)i + Random.Range(-d, d) + (j%2 == 0?0.5f:0)) / (float)x),
				                       Mathf.Lerp(bounds.min.z, bounds.max.z, ((float)j + Random.Range(-d, d)) / (float)y),
				                       0);
				sites.Add(site);
			}
		}
		
		Compute(sites);
	}

	void RelaxSites(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (!this.graph)
            {
                return;
            }

            Point site;
            List<Point> sites = new List<Point>();
            float dist = 0;

            float p = 1 / graph.cells.Count * 0.1f;

            for (int iCell = graph.cells.Count - 1; iCell >= 0; iCell--)
            {
                Voronoi.Cell cell = graph.cells[iCell];
                float rn = Random.value;

                // probability of apoptosis
                if (rn < p)
                {
                    continue;
                }

                site = CellCentroid(cell);
                dist = Distance(site, cell.site);

                // don't relax too fast
                if (dist > 2)
                {
                    site.x = (site.x + cell.site.x) / 2;
                    site.y = (site.y + cell.site.y) / 2;
                }
                // probability of mytosis
                if (rn > (1 - p))
                {
                    dist /= 2;
                    sites.Add(new Point(site.x + (site.x - cell.site.x) / dist, site.y + (site.y - cell.site.y) / dist));
                }
                sites.Add(site);
            }

            Compute(sites);
        }
    }

    float Distance(Point a, Point b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    Point CellCentroid(Voronoi.Cell cell)
    {
        float x = 0f;
        float y = 0f;
        Point p1, p2;
        float v;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            v = p1.x * p2.y - p2.x * p1.y;
            x += (p1.x + p2.x) * v;
            y += (p1.y + p2.y) * v;
        }
        v = CellArea(cell) * 6;
        return new Point(x / v, y / v);
    }

    float CellArea(Voronoi.Cell cell)
    {
        float area = 0.0f;
        Point p1, p2;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            area += p1.x * p2.y;
            area -= p1.y * p2.x;
        }
        area /= 2;
        return area;
    }

    void OnDrawGizmos()
    {
        if (graph)
        {
            foreach (Voronoi.Cell cell in graph.cells)
            {
                Gizmos.color = Color.black;
                //Gizmos.DrawCube(new Vector3(cell.site.x, 0, cell.site.y), Vector3.one);

                if (cell.halfEdges.Count > 0)
                {
                    for (int i = 0; i < cell.halfEdges.Count; i++)
                    {
						HalfEdge halfEdge = cell.halfEdges[i];

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(halfEdge.GetStartPoint().ToVector3(),
                                            halfEdge.GetEndPoint().ToVector3());
                    }
                }
            }
        }
    }
}