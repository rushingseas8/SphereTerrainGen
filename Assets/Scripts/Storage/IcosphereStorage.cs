using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the vertices, faces, and adjacency information for an icosphere.
/// TODO use this when generating IcoSphere2, and then implement a randomized flood fill
/// algorithm to make land/water tiles.
/// </summary>
public class IcosphereStorage
{

    private Triangle[] triangles;

    public IcosphereStorage(Vector3[] vertices, int[] triangles)
    {
        Debug.Assert(vertices.Length == triangles.Length);

        this.triangles = new Triangle[vertices.Length / 3];

        Dictionary<Edge, EdgeConnection> dic = new Dictionary<Edge, EdgeConnection>(vertices.Length, new EdgeComparer());

        for (int i = 0; i < vertices.Length; i += 3)
        {
            // Create a new triangle object.
            Triangle t = new Triangle
            {
                v0 = vertices[i + 0],
                v1 = vertices[i + 1],
                v2 = vertices[i + 2],

                t0 = triangles[i + 0],
                t1 = triangles[i + 1],
                t2 = triangles[i + 2]
            };

            this.triangles[i / 3] = t;

            // Generate edges for new triangle
            Edge e0 = new Edge(vertices[i + 0], vertices[i + 1]);
            Edge e1 = new Edge(vertices[i + 1], vertices[i + 2]);
            Edge e2 = new Edge(vertices[i + 2], vertices[i + 0]);

            // Make sure the edge (or its reverse equivalent) isn't 
            if (dic.ContainsKey(e0))
            {
                dic[e0].Add(t);
            }
            else if (dic.ContainsKey(e0.Flip()))
            {
                dic[e0.Flip()].Add(t);
            }
            else
            {
                dic.Add(e0, new EdgeConnection(t));
            }

            if (dic.ContainsKey(e1))
            {
                dic[e1].Add(t);
            }
            else if (dic.ContainsKey(e1.Flip()))
            {
                dic[e1.Flip()].Add(t);
            }
            else
            {
                dic.Add(e1, new EdgeConnection(t));
            }

            if (dic.ContainsKey(e2))
            {
                dic[e2].Add(t);
            }
            else if (dic.ContainsKey(e2.Flip()))
            {
                dic[e2.Flip()].Add(t);
            }
            else
            {
                dic.Add(e2, new EdgeConnection(t));
            }
        }

        // Make sure all triangles are connected
        foreach (Triangle t in this.triangles)
        {
            Debug.Assert(t.IsValid());
            if (t.IsValid())
            {
                vertices[t.t0] *= 2;
                vertices[t.t1] *= 2;
                vertices[t.t2] *= 2;
            }
        }
    }

    private struct Edge
    {
        public Vector3 v0, v1;

        public Edge(Vector3 v0, Vector3 v1)
        {
            this.v0 = v0;
            this.v1 = v1;
        }

        public Edge Flip()
        {
            return new Edge(v1, v0);
        }

        public override string ToString()
        {
            return "v0: " + v0 + " v1: " + v1;
        }
    }

    private class EdgeComparer : IEqualityComparer<Edge>
    {
        public bool Equals(Edge e0, Edge e1)
        {
            return (Vector3.Distance(e0.v0, e1.v0) < 0.0001f && Vector3.Distance(e0.v1, e1.v1) < 0.0001f);

            // TODO fix this so that we don't have to compute flipped edges.
            //return (Vector3.Distance(e0.v0, e1.v0) < 0.0001f && Vector3.Distance(e0.v1, e1.v1) < 0.0001f) ||
            //(Vector3.Distance(e0.v0, e1.v1) < 0.0001f && Vector3.Distance(e0.v1, e1.v0) < 0.0001f);
        }

        public int GetHashCode(Edge edge)
        {
            const int prime = 31;

            // We want roughly 3 digits of precision in the float. Primes give better
            // distribution in lower bits (probably), so we pick one close to 1000.
            const int floatToInt = 997;
            int hash = (int)(floatToInt * edge.v0.x) + prime;
            hash = (prime * hash) + (int)(floatToInt * edge.v0.y);
            hash = (prime * hash) + (int)(floatToInt * edge.v0.z);
            hash = (prime * hash) + (int)(floatToInt * edge.v1.x);
            hash = (prime * hash) + (int)(floatToInt * edge.v1.y);
            hash = (prime * hash) + (int)(floatToInt * edge.v1.z);

            return hash;
        }
    }

    private class EdgeConnection
    {
        //public Triangle[] connections;
        private Triangle first;
        private bool done = false;

        public EdgeConnection(Triangle first)
        {
            //Debug.Log("Creating first edge connection");
            this.first = first;
        }

        public void Add(Triangle second)
        {
            //Debug.Log("Adding second triangle");
            if (done)
            {
                throw new System.Exception("Tried to make edge with more than 2 connections!");
            }
            first.AddNeighbor(second);
            second.AddNeighbor(first);
            done = true;
        }
    }

    private class Triangle
    {
        public Vector3 v0, v1, v2;
        public int t0, t1, t2;

        private Triangle neighbor0, neighbor1, neighbor2;
        public Triangle[] neighbors;

        public void AddNeighbor(Triangle other)
        {
            if (neighbor0 == null)
            {
                neighbor0 = other;
                return;
            }
            if (neighbor1 == null)
            {
                neighbor1 = other;
                return;
            }
            if (neighbor2 == null)
            {
                neighbor2 = other;

                // Init the neighbors array
                neighbors = new Triangle[] {
                    neighbor0, neighbor1, neighbor2
                };
                return;
            }

            throw new System.Exception("Tried to add fourth neighbor to triangle!");
        }

        public bool IsValid()
        {
            return neighbor0 != null && neighbor1 != null && neighbor2 != null && neighbors != null;
        }
    }
}