using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRPenNamespace
{
    internal class VRPenCore
    {
        public float      BrushSize;
        public Color      BrushColor;
        public Vector3    BrushPosition;
        public Quaternion BrushRotation = Quaternion.identity;
        public Material   BrushMaterial;

        private Matrix4x4 _matrix;
        private Mesh      _penMesh;

        internal List<StrokeMesh>      _strokeMeshes = new();
        internal StrokeMesh            _currentMesh;
        private  MaterialPropertyBlock _brushPB;

        private static readonly int _Color = Shader.PropertyToID("_Color");

        public void Start()
        {
            _matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            _penMesh = new Mesh();
            var shift = BrushSize;

            _penMesh.vertices = new[]
            {
                Vector3.forward * shift, Vector3.left * shift, Vector3.up * shift,
                Vector3.right * shift, Vector3.down * shift, Vector3.back * shift
            };

            _penMesh.uv = new[]
            {
                new Vector2(0.5f, 1f),   // Forward tip
                new Vector2(0f, 0.5f),   // Left
                new Vector2(0.5f, 0.5f), // Up (Center)
                new Vector2(1f, 0.5f),   // Right
                new Vector2(0.5f, 0f),   // Down
                new Vector2(0.5f, 0f)    // Back tip
            };

            _penMesh.triangles = new[]
            {
                1, 0, 2,
                2, 0, 3,
                3, 0, 4,
                4, 0, 1,

                1, 5, 2,
                2, 5, 3,
                3, 5, 4,
                4, 5, 1,
            };

            _penMesh.colors = Enumerable.Repeat(Color.white, _penMesh.vertexCount).ToArray();
            
            _brushPB ??= new MaterialPropertyBlock();
            _brushPB.SetColor(_Color, BrushColor);
        }

        internal void NewStroke(Color color, bool load)
        {
            _currentMesh = new StrokeMesh(BrushSize);
            _currentMesh.Stroke = new VRPen.Stroke()
            {
                Color  = color,
                Points = new List<Vector3>()
            };

            if (!load) _currentMesh.SetLast(BrushPosition);
            _strokeMeshes.Add(_currentMesh);
        }

        internal void Draw()
        {
            foreach (var strokeMesh in _strokeMeshes)
            {
                if (strokeMesh?.Mesh != null)
                    Graphics.DrawMesh(strokeMesh.Mesh, _matrix, BrushMaterial, 0);
            }

            Graphics.DrawMesh(_penMesh, Matrix4x4.TRS(BrushPosition, BrushRotation, Vector3.one),
                              BrushMaterial, 0, null, 0, _brushPB);
        }

        public void Clear() => _strokeMeshes.Clear();

        internal class StrokeMesh
        {
            public VRPen.Stroke Stroke;
            public Mesh Mesh;

            private List<Node> dirs = new();
            internal readonly List<Vector3> vertex = new();
            internal readonly List<Color> colors = new();
            // --- UPDATED: Added UV List ---
            internal readonly List<Vector2> uvs = new(); 
            // ------------------------------
            internal readonly List<(int, int, int, int)> quads = new();
            private List<int> tris = new();

            private readonly float _shift;

            public StrokeMesh(float brushSize) => _shift = brushSize / 2;

            struct Node
            {
                public Vector3 forward, left, up, pos;
                public float distance; // Added to track length for UV.v
            }

            public void BuildMesh()
            {
                var points = Stroke.Points;
                if (points.Count < 2) return;

                if (Mesh == null) { Mesh = new Mesh(); Mesh.MarkDynamic(); }

                dirs.Clear();
                float totalDist = 0;

                // 1. Calculate Nodes and Accumulated Distance
                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 forward, left, up;
                    if (i == 0) forward = (points[1] - points[0]).normalized;
                    else if (i == points.Count - 1) forward = (points[i] - points[i - 1]).normalized;
                    else forward = Vector3.Slerp((points[i] - points[i - 1]).normalized, (points[i + 1] - points[i]).normalized, 0.5f);

                    if (i > 0) totalDist += Vector3.Distance(points[i], points[i - 1]);

                    left = (dirs.Count > 0) ? dirs[^1].left : Vector3.left;
                    up = (dirs.Count > 0) ? dirs[^1].up : Vector3.up;
                    Vector3.OrthoNormalize(ref forward, ref left, ref up);

                    dirs.Add(new Node { forward = forward, left = left, up = up, pos = points[i], distance = totalDist });
                }

                vertex.Clear();
                uvs.Clear();
                colors.Clear();
                tris.Clear();

                // 2. Generate Vertices and UVs
                for (int i = 0; i < dirs.Count; i++)
                {
                    var node = dirs[i];
                    float v = node.distance; // This controls the tiling along the length

                    // Create 4 vertices for the "tube" cross-section
                    vertex.Add(node.pos - node.left * _shift); // Left
                    vertex.Add(node.pos + node.up * _shift);   // Top
                    vertex.Add(node.pos + node.left * _shift); // Right
                    vertex.Add(node.pos - node.up * _shift);   // Bottom

                    // Apply UVs: X is around the tube (0 to 1), Y is the length
                    uvs.Add(new Vector2(0.00f, v));
                    uvs.Add(new Vector2(0.25f, v));
                    uvs.Add(new Vector2(0.50f, v));
                    uvs.Add(new Vector2(0.75f, v));

                    for (int j = 0; j < 4; j++) colors.Add(Stroke.Color);
                }

                // 3. Generate Triangles (unchanged logic, just populating Mesh)
                for (int i = 0; i < vertex.Count - 4; i += 4)
                {
                    AddQuad(i + 0, i + 1, i + 4, i + 5);
                    AddQuad(i + 1, i + 2, i + 5, i + 6);
                    AddQuad(i + 2, i + 3, i + 6, i + 7);
                    AddQuad(i + 3, i + 0, i + 7, i + 4);
                }

                Mesh.Clear();
                Mesh.SetVertices(vertex);
                Mesh.SetUVs(0, uvs); // Apply the UVs
                Mesh.SetColors(colors);
                Mesh.SetTriangles(tris, 0);
                Mesh.RecalculateNormals();
            }

            private void AddQuad(int a, int b, int c, int d)
            {
                tris.AddRange(new[] { a, c, b, b, c, d });
            }

            public void AddPoint(Vector3 position, bool load)
            {
                if (!load) Stroke.Points.Insert(Stroke.Points.Count - 1, position);
                else Stroke.Points.Add(position);
                BuildMesh();
            }

            public void SetLast(Vector3 position)
            {
                if (Stroke.Points.Count == 0) Stroke.Points.Add(position);
                else Stroke.Points[^1] = position;
            }
        }

        public void AddPoint(Vector3 position, bool load) => _currentMesh.AddPoint(position, load);
        public void SetColor(Color color)
        {
            if (color == BrushColor) return;
            BrushColor = color;

            _brushPB ??= new MaterialPropertyBlock();
            _brushPB.SetColor(_Color, BrushColor);
        }
        public void SetBrushPoint(Vector3 pos, Quaternion rot)
        {
            BrushPosition = pos;
            BrushRotation = rot;
        }

        public void SetLast(Vector3 brushPosition)
        {
            BrushPosition = brushPosition;
            if (_currentMesh != null)
            {
                _currentMesh.SetLast(BrushPosition);
                _currentMesh.BuildMesh();
            }
        }
    }
}