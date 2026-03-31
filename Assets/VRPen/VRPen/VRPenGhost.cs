using UnityEngine;

namespace VRPen.VRPenPro
{
    public class VRPenGhost
    {
        public Color      Color = Color.blue;
        public Vector3    HeadPosition;
        public Quaternion HeadRotation = Quaternion.identity;
        public Vector3    LeftHandPosition;
        public Vector3    RightHandPosition;
        public string     Name;

        private Mesh                  _headMesh;
        private Mesh                  _sphereMesh;
        private Material              _material;
        private MaterialPropertyBlock _matBlock;
        private VRPenText             _name;
        
        private Vector3    _lerpHeadPos;
        private Vector3    _lerpLeftPos;
        private Vector3    _lerpRightPos;
        private Quaternion _lerpHeadRot = Quaternion.identity;

        public void Start()
        {
            _headMesh   = Resources.Load<Mesh>("vrpen_ghost");
            _sphereMesh = Resources.Load<Mesh>("vrpen_sphere");
            _material   = Resources.Load<Material>("vrpen_ghost_material");
            _matBlock   = new MaterialPropertyBlock();
            _name       = new VRPenText();
            _name.Start();
        }

        public void SetName(string name)
        {
            Name         = name;
            _name.Lines.Clear();
            _name.Lines.Add(new Line()
            {
                Text = name
            });
            _name.Build();
        }

        public void Draw()
        {
            _matBlock.SetColor("_Color", Color);

            var scale = 0.35f * Vector3.one;

            _lerpHeadPos  = Vector3.Lerp(_lerpHeadPos,  HeadPosition,      Time.deltaTime*4);
            _lerpLeftPos  = Vector3.Lerp(_lerpLeftPos,  LeftHandPosition,  Time.deltaTime*4);
            _lerpRightPos = Vector3.Lerp(_lerpRightPos, RightHandPosition, Time.deltaTime*4);
            _lerpHeadRot  = Quaternion.Slerp(_lerpHeadRot, HeadRotation, Time.deltaTime*4).normalized;
            
            if (_headMesh != null)
            {
                var matrix = Matrix4x4.TRS(_lerpHeadPos, _lerpHeadRot, scale );
                Graphics.DrawMesh(_headMesh, matrix, _material, 0, null, 0, _matBlock);
            }

            if (_sphereMesh != null)
            {
                var matrix = Matrix4x4.TRS(_lerpLeftPos, Quaternion.identity, scale);
                Graphics.DrawMesh(_sphereMesh, matrix, _material, 0, null, 0, _matBlock);

                var matrix2 = Matrix4x4.TRS(_lerpRightPos, Quaternion.identity, scale);
                Graphics.DrawMesh(_sphereMesh, matrix2, _material, 0, null, 0, _matBlock);
            }

            if (Name != null)
            {
                var namePos = _lerpHeadPos + new Vector3(0, 0.5f, 0);
                var nameRot = _lerpHeadRot;
                if (Camera.main != null)
                {
                    nameRot = Quaternion.LookRotation(-(Camera.main.transform.position - namePos).normalized);
                }

                _name.Draw(namePos, nameRot);
            }
        }
    }
}