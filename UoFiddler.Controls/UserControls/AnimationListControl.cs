/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimationListControl : UserControl
    {
        public AnimationListControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public enum Anim {
            WalkUnarmed,
            WalkArmed,
            RunUnarmed,
            RunArmed,
            Fly,
            Stand,
            Fidget1,
            Fidget2,
            StandOneHandedAttack,
            StandTwoHandedAttack,
            AttackOneHanded,
            AttackUnarmed1,
            AttackUnarmed2,
            AttackUnarmed3,
            AttackTwoHandedDown,
            AttackTwoHandedWide,
            AttackTwoHandedJab,
            WalkWarMode,
            CastDirect,
            CastArea,
            AttackBow,
            AttackCrossbow,
            AttackThrow,
            Pillage,
            Stomp,
            Alert,
            GetHit1,
            GetHit2,
            GetHit3,
            Die1,
            Die2,
            OnMountRideSlow,
            OnMountRideFast,
            OnMountStand,
            OnMountAttack,
            OnMountAttackBow,
            OnMountAttackCrossbow,
            OnMountSlapHorse,
            ShieldBlock,
            AttackUnarmedAndWalk,
            Bow,
            Salute,
            Eat
        }

        private static readonly Dictionary<string, Anim?> _monsterMap = new()
        {
            { "Walk", Anim.WalkUnarmed },
            { "Idle1", Anim.Stand },
            { "Die1", Anim.Die1 },
            { "Die2", Anim.Die2 },
            { "Attack1", Anim.AttackUnarmed1 },
            { "Attack2", Anim.AttackUnarmed2 },
            { "Attack3", Anim.AttackUnarmed3 },
            { "AttackBow", Anim.AttackBow },
            { "AttackCrossBow", Anim.AttackCrossbow },
            { "AttackThrow", Anim.AttackThrow },
            { "GetHit", Anim.GetHit1 },
            { "Pillage", Anim.Pillage },
            { "Stomp", Anim.Stomp },
            { "Cast2", Anim.CastArea },
            { "Cast3", Anim.CastDirect },
            { "BlockRight", Anim.GetHit2 },
            { "BlockLeft", Anim.GetHit3 },
            { "Idle2", Anim.Fidget1 },
            { "Fidget", Anim.Fidget2 },
            { "Fly", Anim.Fly },
            { "TakeOff", null },
            { "GetHitInAir", null }
        };

        private static readonly Dictionary<string, Anim?> _seaMap = new()
        {
            { "Walk", Anim.WalkUnarmed },
            { "Run", Anim.RunUnarmed },
            { "Idle1", Anim.Stand },
            { "Idle2", Anim.Fidget1 },
            { "Fidget", Anim.Fidget2 },
            { "Attack1", Anim.AttackUnarmed1 },
            { "Attack2", Anim.AttackUnarmed2 },
            { "GetHit", Anim.GetHit1 },
            { "Die1", Anim.Die1 }
        };

        private static readonly Dictionary<string, Anim?> _animalMap = new()
        {
            { "Walk", Anim.WalkUnarmed },
            { "Run", Anim.RunUnarmed },
            { "Idle1", Anim.Stand },
            { "Eat", Anim.Eat },
            { "Alert", Anim.Alert },
            { "Attack1", Anim.AttackUnarmed1 },
            { "Attack2", Anim.AttackUnarmed2 },
            { "GetHit", Anim.GetHit1 },
            { "Die1", Anim.Die1 },
            { "Idle2", Anim.Fidget1 },
            { "Fidget", Anim.Fidget2 },
            { "LieDown", null },
            { "Die2", Anim.Die2 }
        };

        private static readonly Dictionary<string, Anim?> _humanMap = new()
        {
            { "Walk_01", Anim.WalkUnarmed },
            { "WalkStaff_01", Anim.WalkArmed },
            { "Run_01", Anim.RunUnarmed },
            { "RunStaff_01", Anim.RunArmed },
            { "Idle_01", Anim.Stand },
            { "Idle_02", Anim.Fidget1 },
            { "Fidget_Yawn_Stretch_01", Anim.Fidget2 },
            { "CombatIdle1H_01", Anim.StandOneHandedAttack },
            { "CombatIdle1H_02", Anim.StandTwoHandedAttack },
            { "AttackSlash1H_01", Anim.AttackOneHanded },
            { "AttackPierce1H_01", Anim.AttackUnarmed1 },
            { "AttackBash1H_01", Anim.AttackUnarmed2 },
            { "AttackBash2H_01", Anim.AttackTwoHandedDown },
            { "AttackSlash2H_01", Anim.AttackTwoHandedWide },
            { "AttackPierce2H_01", Anim.AttackTwoHandedJab },
            { "CombatAdvance_1H_01", Anim.WalkWarMode },
            { "Spell1", Anim.CastDirect },
            { "Spell2", Anim.CastArea },
            { "AttackBow_01", Anim.AttackBow },
            { "AttackCrossbow_01", Anim.AttackCrossbow },
            { "GetHit_Fr_Hi_01", Anim.GetHit1 },
            { "Die_Hard_Fwd_01", Anim.Die1 },
            { "Die_Hard_Back_01", Anim.Die2 },
            { "Horse_Walk_01", Anim.OnMountRideSlow },
            { "Horse_Run_01", Anim.OnMountRideFast },
            { "Horse_Idle_01", Anim.OnMountStand },
            { "Horse_Attack1H_SlashRight_01", Anim.OnMountAttack },
            { "Horse_AttackBow_01", Anim.OnMountAttackBow },
            { "Horse_AttackCrossbow_01", Anim.OnMountAttackCrossbow },
            { "Horse_Attack2H_SlashRight_01", Anim.OnMountSlapHorse },
            { "Block_Shield_Hard_01", Anim.ShieldBlock },
            { "Punch_Punch_Jab_01", Anim.AttackUnarmedAndWalk },
            { "Bow_Lesser_01", Anim.Bow },
            { "Salute_Armed1h_01", Anim.Salute },
            { "Ingest_Eat_01", Anim.Eat }
        };
        
        public string[][] GetActionNames { get; } = {
            // Monster
            new[]
            {
                "Walk",
                "Idle1",
                "Die1",
                "Die2",
                "Attack1",
                "Attack2",
                "Attack3",
                "AttackBow",
                "AttackCrossBow",
                "AttackThrow",
                "GetHit",
                "Pillage",
                "Stomp",
                "Cast2",
                "Cast3",
                "BlockRight",
                "BlockLeft",
                "Idle2",
                "Fidget",
                "Fly",
                "TakeOff",
                "GetHitInAir"
            },
            // Sea
            new[]
            {
                "Walk",
                "Run",
                "Idle1",
                "Idle2",
                "Fidget",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1"
            },
            // Animal
            new[]
            {
                "Walk",
                "Run",
                "Idle1",
                "Eat",
                "Alert",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1",
                "Idle2",
                "Fidget",
                "LieDown",
                "Die2"
            },
            // Human
            new[]
            {
                "Walk_01",
                "WalkStaff_01",
                "Run_01",
                "RunStaff_01",
                "Idle_01",
                "Idle_02",
                "Fidget_Yawn_Stretch_01",
                "CombatIdle1H_01",
                "CombatIdle1H_02",
                "AttackSlash1H_01",
                "AttackPierce1H_01",
                "AttackBash1H_01",
                "AttackBash2H_01",
                "AttackSlash2H_01",
                "AttackPierce2H_01",
                "CombatAdvance_1H_01",
                "Spell1",
                "Spell2",
                "AttackBow_01",
                "AttackCrossbow_01",
                "GetHit_Fr_Hi_01",
                "Die_Hard_Fwd_01",
                "Die_Hard_Back_01",
                "Horse_Walk_01",
                "Horse_Run_01",
                "Horse_Idle_01",
                "Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01",
                "Horse_AttackCrossbow_01",
                "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01",
                "Punch_Punch_Jab_01",
                "Bow_Lesser_01",
                "Salute_Armed1h_01",
                "Ingest_Eat_01"
            }
        };

        private Bitmap _mainPicture;
        private int _currentSelect;
        private int _currentSelectAction;
        private bool _animate;
        private int _frameIndex;
        private Bitmap[] _animationList;
        private bool _imageInvalidated = true;
        private Timer _timer;
        private AnimationFrame[] _frames;
        private int _customHue;
        private int _defHue;
        private int _facing = 1;
        private bool _sortAlpha;
        private int _displayType;
        private bool _loaded;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _mainPicture = null;
            _currentSelect = 0;
            _currentSelectAction = 0;
            _animate = false;
            _imageInvalidated = true;
            StopAnimation();
            _frames = null;
            _customHue = 0;
            _defHue = 0;
            _facing = 1;
            _sortAlpha = false;
            _displayType = 0;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Animations"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
            if (!LoadXml())
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            LoadListView();

            extractAnimationToolStripMenuItem.Visible = false;
            _currentSelect = 0;
            _currentSelectAction = 0;
            if (TreeViewMobs.Nodes[0].Nodes.Count > 0)
            {
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[0].Nodes[0];
            }

            FacingBar.Value = (_facing + 3) & 7;
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        /// <summary>
        /// Changes Hue of current Mob
        /// </summary>
        /// <param name="select"></param>
        public void ChangeHue(int select)
        {
            _customHue = select + 1;
            CurrentSelect = CurrentSelect;
        }

        /// <summary>
        /// Is Graphic already in TreeView
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public bool IsAlreadyDefined(int graphic)
        {
            return TreeViewMobs.Nodes[0].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic) ||
                   TreeViewMobs.Nodes[1].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic);
        }

        /// <summary>
        /// Adds Graphic with type and name to List
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void AddGraphic(int graphic, int type, string name)
        {
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.TreeViewNodeSorter = null;
            TreeNode nodeParent = new TreeNode(name)
            {
                Tag = new[] { graphic, type },
                ToolTipText = Animations.GetFileName(graphic)
            };

            if (type == 4)
            {
                TreeViewMobs.Nodes[1].Nodes.Add(nodeParent);
                type = 3;
            }
            else
            {
                TreeViewMobs.Nodes[0].Nodes.Add(nodeParent);
            }

            for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
            {
                if (!Animations.IsActionDefined(graphic, i, 0))
                {
                    continue;
                }

                TreeNode node = new TreeNode($"{i} {GetActionNames[type][i]}")
                {
                    Tag = i
                };

                nodeParent.Nodes.Add(node);
            }

            TreeViewMobs.TreeViewNodeSorter = !_sortAlpha
                ? new GraphicSorter()
                : (IComparer)new AlphaSorter();

            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();
            LoadListView();
            TreeViewMobs.SelectedNode = nodeParent;
            nodeParent.EnsureVisible();
        }

        private bool Animate
        {
            get => _animate;
            set
            {
                if (_animate == value)
                {
                    return;
                }

                _animate = value;
                extractAnimationToolStripMenuItem.Visible = _animate;
                StopAnimation();
                _imageInvalidated = true;
                MainPictureBox.Invalidate();
            }
        }

        private void StopAnimation()
        {
            if (_timer != null)
            {
                if (_timer.Enabled)
                {
                    _timer.Stop();
                }

                _timer.Dispose();
                _timer = null;
            }

            if (_animationList != null)
            {
                foreach (var animationBmp in _animationList)
                {
                    animationBmp?.Dispose();
                }
            }

            _animationList = null;
            _frameIndex = 0;
        }

        private int CurrentSelect
        {
            get => _currentSelect;
            set
            {
                _currentSelect = value;
                if (_timer != null)
                {
                    if (_timer.Enabled)
                    {
                        _timer.Stop();
                    }

                    _timer.Dispose();
                    _timer = null;
                }
                SetPicture();
                MainPictureBox.Invalidate();
            }
        }

        private void SetPicture()
        {
            _frames = null;
            _mainPicture?.Dispose();
            if (_currentSelect == 0)
            {
                return;
            }

            if (Animate)
            {
                _mainPicture = DoAnimation();
            }
            else
            {
                int body = _currentSelect;
                Animations.Translate(ref body);
                int hue = _customHue;
                if (hue != 0)
                {
                    _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
                }
                else
                {
                    _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
                    _defHue = hue;
                }

                if (_frames != null)
                {
                    if (_frames[0].Bitmap != null)
                    {
                        _mainPicture = new Bitmap(_frames[0].Bitmap);
                        BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
                        GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
                        HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
                    }
                    else
                    {
                        _mainPicture = null;
                    }
                }
                else
                {
                    _mainPicture = null;
                }
            }
        }

        private Bitmap DoAnimation()
        {
            if (_timer != null)
            {
                return _animationList[_frameIndex] != null
                    ? new Bitmap(_animationList[_frameIndex])
                    : null;
            }

            int body = _currentSelect;
            Animations.Translate(ref body);
            int hue = _customHue;
            if (hue != 0)
            {
                _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
            }
            else
            {
                _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
                _defHue = hue;
            }

            if (_frames == null)
            {
                return null;
            }

            BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
            GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
            HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
            int count = _frames.Length;
            _animationList = new Bitmap[count];

            for (int i = 0; i < count; ++i)
            {
                _animationList[i] = _frames[i].Bitmap;
            }

            // TODO: avoid division by 0 - needs checking if this is valid logic for count.
            if (count <= 0)
            {
                count = 1;
            }

            _timer = new Timer
            {
                Interval = 1000 / count
            };
            _timer.Tick += AnimTick;
            _timer.Start();

            _frameIndex = 0;

            LoadListViewFrames(); // Reload FrameTab

            return _animationList[0] != null ? new Bitmap(_animationList[0]) : null;
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++_frameIndex;

            if (_frameIndex == _animationList.Length)
            {
                _frameIndex = 0;
            }

            _imageInvalidated = true;

            MainPictureBox.Invalidate();
        }

        private void OnPaint_MainPicture(object sender, PaintEventArgs e)
        {
            if (_imageInvalidated)
            {
                SetPicture();
            }

            if (_mainPicture != null)
            {
                Point location = Point.Empty;
                Size size = _mainPicture.Size;
                location.X = (MainPictureBox.Width - _mainPicture.Width) / 2;
                location.Y = (MainPictureBox.Height - _mainPicture.Height) / 2;

                Rectangle destRect = new Rectangle(location, size);

                e.Graphics.DrawImage(_mainPicture, destRect, 0, 0, _mainPicture.Width, _mainPicture.Height, GraphicsUnit.Pixel);
            }
            else
            {
                _mainPicture = null;
            }
        }

        private void TreeViewMobs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                if (e.Node.Parent.Name == "Mobs" || e.Node.Parent.Name == "Equipment")
                {
                    _currentSelectAction = 0;
                    CurrentSelect = ((int[])e.Node.Tag)[0];
                    if (e.Node.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }
                }
                else
                {
                    _currentSelectAction = (int)e.Node.Tag;
                    CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                    if (e.Node.Parent.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }
                }
            }
            else
            {
                if (e.Node.Name == "Mobs" && _displayType == 1)
                {
                    _displayType = 0;
                    LoadListView();
                }
                else if (e.Node.Name == "Equipment" && _displayType == 0)
                {
                    _displayType = 1;
                    LoadListView();
                }
                TreeViewMobs.SelectedNode = e.Node.Nodes[0];
            }
        }

        private void Animate_Click(object sender, EventArgs e)
        {
            Animate = !Animate;
        }

        private bool LoadXml()
        {
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");
            if (!File.Exists(fileName))
            {
                return false;
            }

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.Nodes.Clear();

                XmlDocument dom = new XmlDocument();
                dom.Load(fileName);

                XmlElement xMobs = dom["Graphics"];
                List<TreeNode> nodes = new List<TreeNode>();
                TreeNode node;
                TreeNode typeNode;

                TreeNode rootNode = new TreeNode("Mobs")
                {
                    Name = "Mobs",
                    Tag = -1
                };
                nodes.Add(rootNode);

                foreach (XmlElement xMob in xMobs.SelectNodes("Mob"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    node = new TreeNode($"{name} (0x{value:X})")
                    {
                        Tag = new[] { value, type },
                        ToolTipText = Animations.GetFileName(value)
                    };
                    rootNode.Nodes.Add(node);

                    for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
                    {
                        if (!Animations.IsActionDefined(value, i, 0))
                        {
                            continue;
                        }

                        typeNode = new TreeNode($"{i} {GetActionNames[type][i]}")
                        {
                            Tag = i
                        };
                        node.Nodes.Add(typeNode);
                    }
                }

                rootNode = new TreeNode("Equipment")
                {
                    Name = "Equipment",
                    Tag = -2
                };
                nodes.Add(rootNode);

                foreach (XmlElement xMob in xMobs.SelectNodes("Equip"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    node = new TreeNode(name)
                    {
                        Tag = new[] { value, type },
                        ToolTipText = Animations.GetFileName(value)
                    };
                    rootNode.Nodes.Add(node);

                    for (int i = 0; i < GetActionNames[type].GetLength(0); ++i)
                    {
                        if (!Animations.IsActionDefined(value, i, 0))
                        {
                            continue;
                        }

                        typeNode = new TreeNode($"{i} {GetActionNames[type][i]}")
                        {
                            Tag = i
                        };
                        node.Nodes.Add(typeNode);
                    }
                }
                TreeViewMobs.Nodes.AddRange(nodes.ToArray());
                nodes.Clear();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            return true;
        }

        private void LoadListView()
        {
            listView.BeginUpdate();
            try
            {
                listView.Clear();
                foreach (TreeNode node in TreeViewMobs.Nodes[_displayType].Nodes)
                {
                    ListViewItem item = new ListViewItem($"({((int[])node.Tag)[0]})", 0)
                    {
                        Tag = ((int[])node.Tag)[0]
                    };
                    listView.Items.Add(item);
                }
            }
            finally
            {
                listView.EndUpdate();
            }
        }

        private void SelectChanged_listView(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[_displayType].Nodes[listView.SelectedItems[0].Index];
            }
        }

        private void ListView_DoubleClick(object sender, MouseEventArgs e)
        {
            tabControl1.SelectTab(tabPage1);
        }

        private void ListViewDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int graphic = (int)e.Item.Tag;
            int hue = 0;
            _frames = Animations.GetAnimation(graphic, 0, 1, ref hue, false, true);

            if (_frames == null)
            {
                return;
            }

            Bitmap bmp = _frames[0].Bitmap;
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
            {
                width = e.Bounds.Width;
            }

            if (height > e.Bounds.Height)
            {
                height = e.Bounds.Height;
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
            if (listView.SelectedItems.Contains(e.Item))
            {
                e.DrawFocusRectangle();
            }
            else
            {
                using (var pen = new Pen(Color.Gray))
                {
                    e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
            }
        }

        private HuePopUpForm _showForm;

        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = _customHue == 0
                ? new HuePopUpForm(ChangeHue, _defHue + 1)
                : new HuePopUpForm(ChangeHue, _customHue - 1);

            _showForm.TopMost = true;
            _showForm.Show();
        }

        private void LoadListViewFrames()
        {
            listView1.BeginUpdate();
            try
            {
                listView1.Clear();
                for (int frame = 0; frame < _animationList.Length; ++frame)
                {
                    ListViewItem item = new ListViewItem(frame.ToString(), 0)
                    {
                        Tag = frame
                    };
                    listView1.Items.Add(item);
                }
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        private void Frames_ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (_animationList == null)
            {
                return;
            }

            Bitmap bmp = _animationList[(int)e.Item.Tag];
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
            {
                width = e.Bounds.Width;
            }

            if (height > e.Bounds.Height)
            {
                height = e.Bounds.Height;
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
            using (var pen = new Pen(Color.Gray))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }

        private void OnScrollFacing(object sender, EventArgs e)
        {
            _facing = (FacingBar.Value - 3) & 7;
            CurrentSelect = CurrentSelect;
        }

        private void OnClick_Sort(object sender, EventArgs e)
        {
            _sortAlpha = !_sortAlpha;

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = !_sortAlpha ? new GraphicSorter() : (IComparer)new AlphaSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            LoadListView();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            TreeNode node = TreeViewMobs.SelectedNode;
            if (node?.Parent == null)
            {
                return;
            }

            if (node.Parent.Name != "Mobs" && node.Parent.Name != "Equipment")
            {
                node = node.Parent;
            }

            node.Remove();
            LoadListView();
        }

        public class Frame
        {
            public string name { get; set; }
            public int idx { get; set; }
            public int offset_x { get; set; }
            public int offset_y { get; set; }
        }

        public class Metadata
        {
            public Guid id { get; set; }
            public Dictionary<string, Dictionary<string, List<Frame>>> directions { get; set; }
        }
        
        private void OnClickCustomExport(object sender, EventArgs e)
        {
            string PascalToKebabCase(string value)
            {
                var builder = new StringBuilder();
                builder.Append(char.ToLower(value.First()));

                foreach (var c in value.Skip(1))
                {
                    if (char.IsUpper(c))
                    {
                        builder.Append('-');
                        builder.Append(char.ToLower(c));
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }

                return builder.ToString();
            }
            
            int hue = 0;
            var tag = (int[])TreeViewMobs.SelectedNode.Tag;
            int body = tag[0];
            int type = tag[1];
            var actions = GetActionNames[type];

            var map = type switch
            {
                0 => _monsterMap,
                1 => _seaMap,
                2 => _animalMap,
                3 => _humanMap,
            };

            var exportDir = Path.Combine(Options.OutputPath, $"custom-export-{body}");
            if (Directory.Exists(exportDir))
            {
                Directory.Delete(exportDir, true);
            }
            
            Directory.CreateDirectory(exportDir);

            var directionDict = new Dictionary<string, Dictionary<string, List<Frame>>>();
            
            for (int direction = 0; direction < 5; ++direction)
            {
                var actionDict = new Dictionary<string, List<Frame>>();
                
                var directionName = direction switch
                {
                    0 => "se",
                    1 => "s",
                    2 => "sw",
                    3 => "w",
                    4 => "nw"
                };
                
                for (int action = 0; action < actions.GetLength(0); ++action)
                {
                    if ((!map.TryGetValue(actions[action], out Anim? anim) || anim == null))
                    {
                        continue;
                    }
                        
                    var actionName = PascalToKebabCase(anim.ToString());
                                        
                    if (!Animations.IsActionDefined(body, action, 0))
                    {
                        continue;
                    }

                    var animFrames = Animations.GetAnimation(_currentSelect, action, direction, ref hue, false, false);
                    if (animFrames == null || animFrames.Any(f => f.Bitmap == null))
                    {
                        continue;
                    }
                    
                    var frames = new List<Frame>();
                    
                    for (int frameIdx = 0; frameIdx < animFrames.Length; ++frameIdx)
                    {
                        AnimationFrame animFrame = animFrames[frameIdx];

                        var framePngFile = $"{directionName}-{actionName}-{frameIdx}.png";
                        animFrame.Bitmap.Save(Path.Combine(exportDir, framePngFile));

                        var frame = new Frame
                        {
                            name = framePngFile.Replace(".png", string.Empty),
                            idx = frameIdx,
                            offset_x = animFrame.Center.X,
                            offset_y = animFrame.Center.Y
                        };

                        frames.Add(frame);
                    }

                    actionDict.Add(actionName.Replace("-", "_"), frames);
                }

                directionDict.Add(directionName, actionDict);
            }

            var metadata = new Metadata
            {
                id = Guid.NewGuid(),
                directions = directionDict
            };

            var metadataYaml = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Serialize(metadata);
            
            File.WriteAllText(Path.Combine(exportDir, "metadata.yaml"), metadataYaml);

            MessageBox.Show(
                $"Exported to {exportDir}",
                "Export",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1
            );
        }

        private AnimationEditForm _animEditFormEntry;

        private void OnClickAnimationEdit(object sender, EventArgs e)
        {
            if (_animEditFormEntry?.IsDisposed == false)
            {
                return;
            }

            _animEditFormEntry = new AnimationEditForm();
            //animEditEntry.TopMost = true; // TODO: should it be topMost?
            _animEditFormEntry.Show();
        }

        private AnimationListNewEntriesForm _animNewEntryForm;

        private void OnClickFindNewEntries(object sender, EventArgs e)
        {
            if (_animNewEntryForm?.IsDisposed == false)
            {
                return;
            }

            _animNewEntryForm = new AnimationListNewEntriesForm(IsAlreadyDefined, AddGraphic, GetActionNames)
            {
                TopMost = true
            };
            _animNewEntryForm.Show();
        }

        private void RewriteXml(object sender, EventArgs e)
        {
            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Graphics");
            XmlComment comment = dom.CreateComment("Entries in Mob tab");
            sr.AppendChild(comment);
            comment = dom.CreateComment("Name=Displayed name");
            sr.AppendChild(comment);
            comment = dom.CreateComment("body=Graphic");
            sr.AppendChild(comment);
            comment = dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment");
            sr.AppendChild(comment);

            XmlElement elem;
            foreach (TreeNode node in TreeViewMobs.Nodes[0].Nodes)
            {
                elem = dom.CreateElement("Mob");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());

                sr.AppendChild(elem);
            }

            foreach (TreeNode node in TreeViewMobs.Nodes[1].Nodes)
            {
                elem = dom.CreateElement("Equip");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());
                sr.AppendChild(elem);
            }
            dom.AppendChild(sr);
            dom.Save(fileName);

            MessageBox.Show("XML saved", "Rewrite", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Png);
        }

        private void ExtractImage(ImageFormat imageFormat)
        {
            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}.{fileExtension}");

            Bitmap sourceBitmap = Animate ? _animationList[0] : _mainPicture;
            using (Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(sourceBitmap, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save(fileName, imageFormat);
            }

            MessageBox.Show($"{what} saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Bmp);
        }

        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Tiff);
        }

        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Jpeg);
        }

        private void OnClickExtractAnimPng(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Png);
        }

        private void ExportAnimationFrames(ImageFormat imageFormat)
        {
            if (!Animate)
            {
                return;
            }

            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}");

            for (int i = 0; i < _animationList.Length; ++i)
            {
                using (Bitmap newBitmap = new Bitmap(_animationList[i].Width, _animationList[i].Height))
                {
                    using (Graphics newGraph = Graphics.FromImage(newBitmap))
                    {
                        newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                        newGraph.DrawImage(_animationList[i], new Point(0, 0));
                        newGraph.Save();
                    }

                    newBitmap.Save($"{fileName}-{i}.{fileExtension}", imageFormat);
                }
            }

            MessageBox.Show($"{what} saved to '{fileName}-X.{fileExtension}'", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportFrameBmp(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Bmp);
        }

        private void OnClickExportFrameTiff(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Tiff);
        }

        private void OnClickExportFrameJpg(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Jpeg);
        }

        private void OnClickExportFramePng(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Png);
        }

        private void ExportSingleFrame(ImageFormat imageFormat)
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }

            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {_currentSelect}");

            Bitmap bit = _animationList[(int)listView1.SelectedItems[0].Tag];
            using (Bitmap newBitmap = new Bitmap(bit.Width, bit.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(bit, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save($"{fileName}-{(int)listView1.SelectedItems[0].Tag}.{fileExtension}", imageFormat);
            }
        }
    }

    public class AlphaSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null) // don't change Mob and Equipment
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }
            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
            }

            return string.CompareOrdinal(tx.Text, ty.Text);
        }
    }

    public class GraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null)
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }

            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
            }

            int[] ix = (int[])tx.Tag;
            int[] iy = (int[])ty.Tag;

            if (ix[0] == iy[0])
            {
                return 0;
            }

            if (ix[0] < iy[0])
            {
                return -1;
            }

            return 1;
        }
    }
}
