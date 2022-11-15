using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static TreeList.Properties.Resources;

namespace TreeList
{
    public partial class Start : Form
    {
        const string AppTitle = "Дерево папок + список файлов";
        const string AccessDeniedMsg = " /*Отказано в доступе*/ ";

        TreeView? tree;
        ListView? list;

        public Start()
        {
            InitializeComponent();
            MakeControls();
            SetForm();
        }

        void MakeControls()
        {
            tree = new TreeView();
            tree.Dock = DockStyle.Fill;
            tree.DoubleClick += LoadFolderFiles;
            list = MakeListView();

            var panel = MakeTabLayoutPanel(2);
            panel.Controls.Add(tree, 0, 0);
            panel.Controls.Add(list, 1, 0);
            Controls.Add(panel);

            Controls.Add(MakeToolBar());
        }
        void SetForm() =>
          (Text, Icon) = (AppTitle, AppIcon);

        ListView MakeListView()
        {
            ListView list = new ListView();
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.FullRowSelect = true;
            list.GridLines = true;
            list.Columns.Add("Ф А Й Л Ы   В   П А П К Е :");
            list.SizeChanged += (sender, e) => list.Columns[0].Width = (sender as ListView)?.Width ?? 0;
            list.DoubleClick += LoadFile;
            return list;
        }
        TableLayoutPanel MakeTabLayoutPanel(int colCount)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.CellBorderStyle =
                TableLayoutPanelCellBorderStyle.InsetDouble;
            for (int i = 0; i < colCount; i++)
            {
                panel.ColumnStyles.Add(new
                    ColumnStyle(SizeType.Percent, 100.0F / colCount));
            }
            panel.Dock = DockStyle.Fill;
            return panel;
        }
        ToolStrip MakeToolBar()
        {
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Items.Add(MakeToolStripButton(
                LoadTreeImage, "Загрузить дерево", LoadFolderTree));
            return toolStrip;
        }
        ToolStripButton MakeToolStripButton(Image image, string text, EventHandler handler)
        {
            ToolStripButton button = new ToolStripButton();
            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Image = image;
            button.Text = text;
            button.Click += handler;
            button.Alignment = ToolStripItemAlignment.Right;
            return button;
        }

        void LoadFolderTree(object? sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tree?.Nodes.Clear();

                var root = new TreeNode(dialog.SelectedPath);
                root.Tag = new DirectoryInfo(root.Text);
                tree?.Nodes.Add(root);

                LoadFolderTree(root);
                tree?.Nodes[0].Expand();
            }
        }
        void LoadFolderTree(TreeNode? parent)
        {
            var folders = Directory.GetDirectories(parent?.Tag?.ToString() ?? "");
            foreach (var it in folders)
            {
                var folder = new DirectoryInfo(it);
                var node = new TreeNode(folder.Name);
                node.Tag = folder;
                parent?.Nodes.Add(node);
                try
                {
                    LoadFolderTree(node);
                }
                catch (UnauthorizedAccessException)
                {
                    node.Text += AccessDeniedMsg;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void LoadFolderFiles(object? sender, EventArgs e)
        {
            list?.Items.Clear();
            string? dirPath = tree?.SelectedNode?.Tag?.ToString();
            if (dirPath == null) return;

            string[]? files = null;
            try
            {
                files = Directory.GetFiles(dirPath);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(AccessDeniedMsg, AppTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppTitle);
            }
            if (files == null) return;

            foreach (var it in files)
            {
                FileInfo file = new FileInfo(it);
                ListViewItem item = new ListViewItem();
                item.Text = file.Name;
                item.Tag = file;
                list?.Items.Add(item);
            }
        }
        private void LoadFile(object? sender, EventArgs e)
        {
            string? filePath = list?.SelectedItems[0].Tag?.ToString()??"";
            switch(Path.GetExtension(filePath))
            {
                case ".txt":
                    Process.Start("\"C:\\Program Files (x86)\\Notepad++\\notepad++.exe\"",
                        filePath);
                    break;
                case ".sln":
                    Process.Start("\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe\"",
                        filePath);
                    break;
            }
        }
    }
}