using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRPen.VRPenPro;
using Random = UnityEngine.Random;

namespace VRPenNamespace
{
    public class VRPenNetwork : MonoBehaviour
    {
        private UdpBeaconPeer _udp;
        private VRPen         _vrpen;

        [SerializeField] public Material _material;

        public Transform Head;
        public Transform RightHand;
        public Transform Lefthand;

        private (Color color, string name)[] _ghostColors = new[]
        {
            (Color.blue, "blue"),
            (Color.cyan, "cyan"),
            (Color.green, "green"),
            (Color.red, "red"),
            (Color.yellow, "yellow"),
            (new Color(0.7f, 0, 1), "purple")
        };

        private string      _deviceName;
        private IVrPenInput _input;
        private bool        _menuIsOpen;
        private VRPenText   _menu;
        private Vector3     _menuPos;
        private Quaternion  _menuRot;
        private VRPenScreen _mainScreen  = new VRPenScreen();
        private VRPenScreen _loadScreen  = new VRPenScreen();
        private VRPenScreen _colorScreen = new VRPenScreen();
        private string      _fileName;

        private List<PeerData> _peerDatas = new();
        private float          _lastGhostPoseSent;
        private Guid           _guid;
        private int            _ownColorIndex;

        public VRPenText Menu => _menu;

        public void LoadPrefs()
        {
            var guidStr = PlayerPrefs.GetString("vr_pen_net", null);
            if (!string.IsNullOrWhiteSpace(guidStr))
            {
                _guid          = Guid.Parse(guidStr);
                _ownColorIndex = PlayerPrefs.GetInt("vr_pen_net_color", 0);
            }
            else
            {
                _guid = Guid.NewGuid();
                PlayerPrefs.SetString("vr_pen_net", _guid.ToString());
                _ownColorIndex = Random.Range(0, _ghostColors.Length);
                PlayerPrefs.SetInt("vr_pen_net_color", _ownColorIndex);
            }
        }

        private void OnEnable()
        {
            _udp ??= new UdpBeaconPeer();

            _vrpen = GetComponent<VRPen>();
            _input = GetComponent<IVrPenInput>();

            _udp.IsHub          =  true;
            _udp.IsOpen         =  true;
            _vrpen.OnPointAdded += OnPointAdded;
            _vrpen.OnNewStroke  += OnNewStroke;

            _udp.PeerAdded     = ClientAdded;
            _udp.PeerLeave     = ClientLeft;
            _udp.PeerAnnounced = p => BuildMenu();
            _udp.Disconnected  = () => BuildMenu();

            _udp.DataFromPeer = DataFromClient;

            _deviceName           = SystemInfo.deviceName;
            _udp.Name             = _deviceName;
            _udp.HeartbeatTimeout = Single.MaxValue;

            LoadPrefs();

            _udp.Open(_guid);

            _menu = new VRPenText();
            _menu.Start();
            _menu.Screen = _mainScreen;
            BuildMenu();
        }

        private void ClientLeft(UdpBeaconPeer.Peer obj)
        {
            BuildMenu();
            var data = obj.UserData as PeerData;
            if (data != null)
            {
                data.connected      = false;
                data.disconnectTime = Time.time;
            }
        }

        private void OnNewStroke()
        {
            SendFullToEveryone();
        }

        private void DataFromClient(ByteBuffer arg1, UdpBeaconPeer.Peer peer)
        {
            ReceiveData(peer, arg1);
        }

        private void ClientAdded(UdpBeaconPeer.Peer peer)
        {
            var data = _peerDatas.FirstOrDefault(p => p.guid == peer.Guid);
            if (data == null)
            {
                data      = new PeerData();
                data.guid = peer.Guid;

                data.ghost = new VRPenGhost();
                data.ghost.Start();

                data.core = new VRPenCore();

                if (_material != null)
                    data.core.BrushMaterial = _material;
                else
                    data.core.BrushMaterial = _vrpen._material;

                data.core.SetColor(Color.blue);
                data.core.BrushSize  = _vrpen._brushSize;
                data.core.Start();
                _peerDatas.Add(data);
            }
            
            data.connected = true;

            peer.UserData = data;

            //_udp.SendToClient(peer, p => SendFullState(p));

            BuildMenu();

            SendFullToEveryone();
        }

        private void OnPointAdded(Vector3 position)
        {
            foreach (var client in _udp._availableClients.Values)
            {
                if (client.IsConnected)
                {
                    _udp.SendToClient(client, p => SendNewPoint(p, position));
                }
            }
        }

        private void SendGhostPose(ByteBuffer p)
        {
            p.WriteByte((byte)MsgType.GhostPose);

            p.WriteVector3(Head.position);
            p.WriteQuaternion(Head.rotation);

            p.WriteVector3(Lefthand.position);
            p.WriteVector3(RightHand.position);

            p.WriteVector3(_vrpen._core.BrushPosition);
            p.WriteQuaternion(_vrpen._core.BrushRotation);

            p.WriteColor(_ghostColors[_ownColorIndex].color);
            p.WriteColor(_vrpen._core.BrushColor);
        }

        private void SendNewPoint(ByteBuffer p, Vector3 position)
        {
            p.WriteByte((byte)MsgType.NewPoint);

            p.WriteVector3(Head.position);
            p.WriteQuaternion(Head.rotation);

            p.WriteVector3(Lefthand.position);
            p.WriteVector3(RightHand.position);

            p.WriteVector3(_vrpen._core.BrushPosition);
            p.WriteQuaternion(_vrpen._core.BrushRotation);

            p.WriteVector3(position);
        }

        private void SendFullToEveryone()
        {
            foreach (var clientsValue in _udp._availableClients.Values)
            {
                _udp.SendToClient(clientsValue, p => SendFullState(p));
            }
        }

        private void SendFullState(ByteBuffer p)
        {
            p.WriteByte((byte)MsgType.FullState);

            p.WriteColor(_ghostColors[_ownColorIndex].color);

            p.WriteVector3(Head.position);
            p.WriteQuaternion(Head.rotation);

            p.WriteVector3(Lefthand.position);
            p.WriteVector3(RightHand.position);

            p.WriteVector3(_vrpen._core.BrushPosition);
            p.WriteQuaternion(_vrpen._core.BrushRotation);

            p.WriteInt(_vrpen._core._strokeMeshes.Count);
            foreach (var strokeMesh in _vrpen._core._strokeMeshes)
            {
                var stroke = strokeMesh.Stroke;
                p.WriteColor(stroke.Color);
                p.WriteInt(stroke.Points.Count);
                foreach (var point in stroke.Points)
                {
                    p.WriteVector3(point);
                }
            }
        }

        private void ReceiveData(UdpBeaconPeer.Peer peer, ByteBuffer buffer)
        {
            MsgType msg = (MsgType)buffer.ReadByte();
            switch (msg)
            {
                case MsgType.None: break;
                case MsgType.FullState:
                {
                    var data = (peer.UserData as PeerData);
                    if (data == null) break;

                    data.core.Clear();
                    data.ghost.SetName(peer.Name);

                    data.ghost.Color             = buffer.ReadColor();
                    data.ghost.HeadPosition      = buffer.ReadVector3();
                    data.ghost.HeadRotation      = buffer.ReadQuaternion();
                    data.ghost.LeftHandPosition  = buffer.ReadVector3();
                    data.ghost.RightHandPosition = buffer.ReadVector3();

                    data.core.SetBrushPoint(
                        buffer.ReadVector3(),
                        buffer.ReadQuaternion());

                    int meshes = buffer.ReadInt();
                    for (int i = 0; i < meshes; i++)
                    {
                        var color       = buffer.ReadColor();
                        int pointsCount = buffer.ReadInt();

                        data.core.NewStroke(color, true);

                        for (int j = 0; j < pointsCount; j++)
                        {
                            var point = buffer.ReadVector3();
                            data.core._currentMesh.Stroke.Points.Add(point);
                        }

                        if (i == meshes - 1)
                        {
                            data.core._currentMesh.SetLast(data.core.BrushPosition);
                        }

                        data.core._currentMesh.BuildMesh();
                    }
                }
                    break;
                case MsgType.NewPoint:
                {
                    var data = (peer.UserData as PeerData);
                    if (data == null) break;

                    data.ghost.HeadPosition = buffer.ReadVector3();
                    data.ghost.HeadRotation = buffer.ReadQuaternion();

                    data.ghost.LeftHandPosition  = buffer.ReadVector3();
                    data.ghost.RightHandPosition = buffer.ReadVector3();

                    data.core.SetBrushPoint(
                        buffer.ReadVector3(),
                        buffer.ReadQuaternion());

                    var pos = buffer.ReadVector3();

                    data.core.AddPoint(pos, true);
                }
                    break;
                case MsgType.GhostPose:
                {
                    var data = (peer.UserData as PeerData);
                    if (data == null) break;

                    data.ghost.HeadPosition = buffer.ReadVector3();
                    data.ghost.HeadRotation = buffer.ReadQuaternion();

                    data.ghost.LeftHandPosition  = buffer.ReadVector3();
                    data.ghost.RightHandPosition = buffer.ReadVector3();

                    data.core.SetBrushPoint(
                        buffer.ReadVector3(),
                        buffer.ReadQuaternion());

                    data.ghost.Color     = buffer.ReadColor();
                    data.core.SetColor(buffer.ReadColor());
                }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (_udp == null) return;

            if (Time.time - _lastGhostPoseSent > 0.2f)
            {
                _lastGhostPoseSent = Time.time;
                foreach (var peer in _udp._availableClients.Values)
                {
                    if (peer.IsConnected)
                    {
                        _udp.SendToClient(peer, p => SendGhostPose(p));
                    }
                }
            }

            _udp.Update();

            foreach (var data in _peerDatas)
            {
                if (data.connected || (Time.time - data.disconnectTime) < 1)
                {
                    data.ghost.Draw();
                }
                
                data.core.Draw();
            }

            MenuUpdate();
        }

        public bool MenuIsOnMain()
        {
            return _menu.Screen == _mainScreen;
        }

        public bool MenuIsOpen()
        {
            return _menuIsOpen;
        }

        public void MenuClick()
        {
            if (MenuIsOnMain())
            {
                _menuIsOpen = !_menuIsOpen;

                if (_menuIsOpen)
                {
                    var pos = _vrpen._core.BrushPosition;
                    _menuPos = pos;
                    if (Camera.main != null)
                    {
                        _menuRot = 
                            Quaternion.LookRotation(_menuPos - Camera.main.transform.position);
                           // Camera.main.transform.rotation;
                    }
                    else
                    {
                        _menuRot = Quaternion.identity;
                    }

                    _menu.SelectedIndex = 0;

                    BuildMenu();
                }
            }
            else
            {
                _menu.Screen = _mainScreen;
                BuildMenu();
            }
        }

        private void MenuUpdate()
        {
            if (_input.MenuToggle)
            {
                MenuClick();
            }

            if (!MenuIsOpen()) return;

            if (_input.Up)
            {
                _menu.MoveUp();
                _menu.Build();
            }

            if (_input.Down)
            {
                _menu.MoveDown();
                _menu.Build();
            }

            if (_input.Enter)
            {
                _menu.SelectedLine().Action?.Invoke();
                BuildMenu();
            }

            if (_menuIsOpen)
            {
                _menu.Draw(_menuPos, _menuRot);
            }
        }

        private void BuildMenu()
        {
            _mainScreen.Lines.Clear();
            _mainScreen.Lines.Add(new Line()
            {
                Text   = "Open " + (_udp.IsOpen ? "[On]" : "[Off]"),
                Action = () => { _udp.IsOpen = !_udp.IsOpen; }
            });

            if (_udp._availableClients.Count > 0)
            {
                _mainScreen.Lines.Add(new Line()
                {
                    Text = "Connect To ----"
                });

                foreach (var peer in _udp._availableClients.Values)
                {
                    _mainScreen.Lines.Add(new Line()
                    {
                        Text = peer.Name + (peer.IsConnected ? " [connected]" : ""),
                        Action = () =>
                        {
                            if (!peer.IsConnected)
                            {
                                _udp.ConnectToPeer(peer);
                            }
                        }
                    });
                }

                _mainScreen.Lines.Add(new Line()
                {
                    Text = "----"
                });
            }

            _mainScreen.Lines.Add(new Line()
            {
                Text   = "Disconnect",
                Action = () => { _udp.Disconnect(); }
            });

            _mainScreen.Lines.Add(new Line()
            {
                Text = "Select Avatar Color",
                Action = () =>
                {
                    _colorScreen.Lines.Clear();

                    for (int i = 0; i < _ghostColors.Length; i++)
                    {
                        var pair  = _ghostColors[i];
                        int index = i;
                        _colorScreen.Lines.Add(new Line()
                        {
                            Text = pair.name,
                            Action = () =>
                            {
                                _ownColorIndex = index;
                                PlayerPrefs.SetInt("vr_pen_net_color", _ownColorIndex);
                                _menu.Screen = _mainScreen;
                                BuildMenu();
                            },
                            Color = pair.color
                        });

                        _menu.Screen        = _colorScreen;
                        _menu.SelectedIndex = _ownColorIndex;
                        BuildMenu();
                    }
                },
                Color = _ghostColors[_ownColorIndex].color
            });

            _mainScreen.Lines.Add(new Line()
            {
                Text = "Load",
                Action = () =>
                {
                    _menu.Screen = _loadScreen;

                    _loadScreen.Lines.Clear();
                    var files = _vrpen.GetSavedFiles();

                    if (files.Length == 0)
                    {
                        _loadScreen.Lines.Add(new Line()
                        {
                            Text = "<empty>"
                        });
                    }

                    for (int index = 0; index < files.Length; index++)
                    {
                        string file = files[index];
                        var line = new Line()
                        {
                            Text = file,
                            Action = () =>
                            {
                                _vrpen.LoadFile(file);
                                _menu.Screen = _mainScreen;
                                _fileName    = file;
                                BuildMenu();
                                SendFullToEveryone();
                            }
                        };

                        if (_fileName == file)
                        {
                            line.Color = new Color(0, 0.6f, 1);
                        }

                        _loadScreen.Lines.Add(line);
                    }
                }
            });

            _mainScreen.Lines.Add(new Line()
            {
                Text = "Save",
                Action = () =>
                {
                    var path = _vrpen.SaveAsFile();
                    _fileName = path;
                    
                }
            });
            
            _mainScreen.Lines.Add(new Line()
            {
                Text = "Clear",
                Action = () =>
                {
                    _vrpen.Clear();
                    SendFullToEveryone();
                }
            });

            if (!string.IsNullOrWhiteSpace(_fileName))
            {
                _mainScreen.Lines.Add(new Line()
                {
                    Text = "File:" + _fileName
                });
            }

            if (HasUnsavedPeerData())
            {
                _mainScreen.Lines.Add(new Line()
                {
                    Text   = "Save Peer To Local",
                    Action = () => { SaveToLocal(); }
                });
            }

            _menu.Build();
        }

        void OnDisable()
        {
            _vrpen.OnPointAdded -= OnPointAdded;
            _vrpen.OnNewStroke  -= OnNewStroke;
            _udp?.Disconnect();
        }

        enum MsgType : byte
        {
            None,
            FullState,
            NewPoint,
            GhostPose
        }

        class PeerData
        {
            public VRPenCore  core;
            public VRPenGhost ghost;
            public Guid       guid;
            public bool       connected;
            public float      disconnectTime;
        }

        public bool HasUnsavedPeerData()
        {
            return _peerDatas.Count > 0;
        }

        public void SaveToLocal()
        {
            var hash = _vrpen._core._strokeMeshes.ToHashSet();

            foreach (var peerData in _peerDatas)
            {
                if (_vrpen.Scribble == null) return;

                foreach (var strokeMesh in peerData.core._strokeMeshes)
                {
                    if (hash.Contains(strokeMesh)) continue;
                    var meshes = _vrpen._core._strokeMeshes;
                    if (meshes.Count == 0) meshes.Add(strokeMesh);
                    else meshes.Insert(meshes.Count - 1, strokeMesh);
                }
            }

            _vrpen.TrySave();
            SendFullToEveryone();
        }
    }
}