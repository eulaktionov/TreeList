using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static TreeList.Properties.Resources;

namespace TreeList
{
    public partial class Start : Form
    {
        const string AppTitle = "Дерево папок + список файлов";

        TreeView tree;
        ListView list;

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
            list.Columns.Add("Files");
            list.SizeChanged += (sender, e) =>
                list.Columns[0].Width = (sender as ListView).Width;
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
        ToolStripButton MakeToolStripButton(
            Image image, string text, EventHandler handler)
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
                tree.Nodes.Clear();
                tree.Nodes.Add(dialog.SelectedPath);
                LoadFolderTree(dialog.SelectedPath, tree.Nodes[0]);
                tree.Nodes[0].Expand();
            }
        }
        void LoadFolderTree(string dirPath, TreeNode parent)
        {
            string[] folders = Directory.GetDirectories(dirPath);
            foreach (var folder in folders)
            {
                TreeNode node = new TreeNode(folder);
                parent.Nodes.Add(node);
                try
                {
                    LoadFolderTree(folder, node);
                }
                catch (UnauthorizedAccessException)
                {
                    node.Text += "(Отказано в доступе)";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void LoadFolderFiles(object? sender, EventArgs e)
        {
            list.Items.Clear();
            string dirPath = tree.SelectedNode.Text;
            string[] files = Directory.GetFiles(dirPath);
            foreach (var it in files)
            {
                FileInfo file = new FileInfo(it);
                ListViewItem item = new ListViewItem();
                item.Text = file.Name;
                list.Items.Add(item);
            }
        }
    }
}