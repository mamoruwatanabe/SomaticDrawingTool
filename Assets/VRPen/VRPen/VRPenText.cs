using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRPen.VRPenPro
{
    public class VRPenText
    {
        private Texture2D _texture;
        private Mesh      _mesh;
        private Material  _material;

        public VRPenScreen Screen = new VRPenScreen();
        public List<Line>  Lines => Screen.Lines;

        public int SelectedIndex
        {
            get => Screen.SelectedIndex;
            set { Screen.SelectedIndex = value; }
        }

        public void Build()
        {
            if (_texture == null)
            {
                Init();
            }

            var charH = _texture.height / 6;
            var charW = _texture.width / 16;

            var charsCount = 0;
            foreach (var line in Lines)
            {
                charsCount += line.Text.Length;
            }

            if (Lines.Any(p => !p.NotSelectable))
            {
                charsCount += 1;//for >
            }
            
            var verts     = new Vector3[charsCount * 4];
            var uvs       = new Vector2[charsCount * 4];
            var colors    = new Color[charsCount * 4];
            var triangles = new int[charsCount * 3 * 2];

            float uvY = 1f / 6f;
            float uvX = 1f / 16f;

            float yAspect = charH / (float)charW;

            int vshift = 0;
            int shift  = 0;

            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, Lines.Count - 1);
            if (Lines[SelectedIndex].NotSelectable)
            {
                SelectedIndex = Lines.FindIndex(p => !p.NotSelectable);
                SelectedIndex = Mathf.Clamp(SelectedIndex, 0, Lines.Count - 1);
            }

            for (int j = 0; j < Lines.Count; j++)
            {
                var line = Lines[j].Text;

                if (string.IsNullOrWhiteSpace(line)) continue;

                var color = new Color(0, 0.35f, 1, 1);

                bool selected = !Lines[SelectedIndex].NotSelectable && (j == SelectedIndex);
                
                if (Lines[j].Color != default)
                {
                    color = Lines[j].Color;
                }
                else if (selected)
                {
                    color = Color.white;
                }
                else if (Lines[j].NotSelectable)
                {
                    color = new Color(0.5f, 0.75f, 1, 1);
                }

                var yShift = (SelectedIndex - j) * (yAspect * 1.1f);

                //yShift += yAspect * Lines.Count;

                float xShift                   = 0;
                
                if (selected)
                {
                    xShift = - 4 / (float)charW;
                    line   = ">" + line;
                }

                for (int i = 0; i < line.Length; i++)
                {
                    int index = line[i] - 32;

                    int y = index / 16;
                    int x = index % 16;

                    verts[vshift + 0] = new Vector3(i + xShift,     yAspect + yShift);
                    verts[vshift + 1] = new Vector3(i + 1 + xShift, yAspect + yShift);
                    verts[vshift + 2] = new Vector3(i + xShift,     0 + yShift);
                    verts[vshift + 3] = new Vector3(i + 1 + xShift, 0 + yShift);

                    float   eps = 0.001f;
                    Vector2 uv  = new Vector2(x * uvX, 1 - (y + 1) * uvY);

                    uvs[vshift + 0] = uv + new Vector2(eps,     uvY-eps);
                    uvs[vshift + 1] = uv + new Vector2(uvX-eps, uvY-eps);
                    uvs[vshift + 2] = uv + new Vector2(eps,     eps);
                    uvs[vshift + 3] = uv + new Vector2(uvX-eps, eps);

                    colors[vshift + 0] = color;
                    colors[vshift + 1] = color;
                    colors[vshift + 2] = color;
                    colors[vshift + 3] = color;


                    triangles[shift + 0] = vshift + 0;
                    triangles[shift + 1] = vshift + 1;
                    triangles[shift + 2] = vshift + 2;

                    triangles[shift + 3] = vshift + 1;
                    triangles[shift + 4] = vshift + 3;
                    triangles[shift + 5] = vshift + 2;

                    vshift += 4;
                    shift  += 6;
                }
            }

            _mesh.Clear();
            _mesh.vertices  = verts;
            _mesh.uv        = uvs;
            _mesh.triangles = triangles;
            _mesh.colors    = colors;
        }

        public void MoveUp()
        {
            SelectedIndex--;
            while (SelectedIndex > 0 && Lines[SelectedIndex].NotSelectable)
            {
                SelectedIndex--;
            }
        }

        public void MoveDown()
        {
            SelectedIndex++;
            while (SelectedIndex < Lines.Count - 1 && Lines[SelectedIndex].NotSelectable)
            {
                SelectedIndex++;
            }
        }

        public Line SelectedLine()
        {
            if (Lines == null || Lines.Count == 0 || SelectedIndex >= Lines.Count)
                return default;
            return Lines[SelectedIndex];
        }

        private void Init()
        {
            var texture = Resources.Load<Texture2D>("vrpen_font");

            var shader   = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            material.mainTexture = texture;

            _texture  = texture;
            _material = material;

            _mesh = new Mesh();
            _mesh.MarkDynamic();
        }

        public void Draw(Vector3 position, Quaternion rotation, float scale = 0.02f)
        {
            if (_mesh != null)
            {
                Graphics.DrawMesh(_mesh,
                                  Matrix4x4.TRS(position, rotation, scale * Vector3.one),
                                  _material, 0, null);
            }
        }

        public void Start()
        {
            Init();
        }
    }

    public class VRPenScreen
    {
        public List<Line> Lines = new();
        public int        SelectedIndex;
    }


    public struct Line
    {
        public bool   NotSelectable => Action == null;
        public string Text;
        public Action Action;
        public Color  Color;
    }
}