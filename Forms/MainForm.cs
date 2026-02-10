using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DatabaseManager.Services;

namespace DatabaseManager.Forms
{
    public class MainForm : Form
    {
        private readonly ConnectionService _conn;
        private readonly MetadataService _meta;
        private readonly TableViewerService _tableViewer;
        private readonly DdlService _ddl;

        private TreeView tree;
        private Panel rightPanel;
        private Label lblRight;
        private DataGridView grid;
        private TextBox txtDDL;

        private ContextMenuStrip ctxTable;

        private const string TAG_DB = "DB";
        private const string TAG_TABLES = "TABLES";
        private const string TAG_VIEWS = "VIEWS";
        private const string TAG_PROCS = "PROCS";
        private const string TAG_FUNCS = "FUNCS";
        private const string TAG_OBJECT = "OBJECT";

        public MainForm(ConnectionService conn)
        {
            _conn = conn;
            _meta = new MetadataService(_conn);
            _tableViewer = new TableViewerService(_conn);
            _ddl = new DdlService(_conn);

            BuildUI();
            BuildTreeRoot();
        }

        private void BuildUI()
        {
            Text = "Database Manager - Explorer";
            Size = new Size(1100, 650);
            StartPosition = FormStartPosition.CenterScreen;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 320,
                FixedPanel = FixedPanel.Panel1
            };
            Controls.Add(split);

            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true
            };
            tree.BeforeExpand += Tree_BeforeExpand;
            tree.AfterSelect += Tree_AfterSelect;
            tree.NodeMouseClick += Tree_NodeMouseClick;

            split.Panel1.Controls.Add(tree);

            rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

            lblRight = new Label
            {
                Text = "Selecciona un nodo o haz click derecho en una tabla.",
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                Font = new Font(Font.FontFamily, 10, FontStyle.Regular)
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            txtDDL = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Visible = false,
                Font = new Font("Consolas", 10, FontStyle.Regular)
            };

            rightPanel.Controls.Add(grid);
            rightPanel.Controls.Add(txtDDL);
            rightPanel.Controls.Add(lblRight);

            split.Panel2.Controls.Add(rightPanel);

            BuildContextMenu();
        }

        private void BuildTreeRoot()
        {
            tree.Nodes.Clear();

            var dbNode = new TreeNode("Database")
            {
                Tag = TAG_DB
            };

            dbNode.Nodes.Add(MakeLazyNode("Tables", TAG_TABLES));
            dbNode.Nodes.Add(MakeLazyNode("Views", TAG_VIEWS));
            dbNode.Nodes.Add(MakeLazyNode("Procedures", TAG_PROCS));
            dbNode.Nodes.Add(MakeLazyNode("Functions", TAG_FUNCS));

            tree.Nodes.Add(dbNode);
            dbNode.Expand();
        }

        private TreeNode MakeLazyNode(string text, string tag)
        {
            var n = new TreeNode(text) { Tag = tag };
            n.Nodes.Add(new TreeNode("Cargando..."));
            return n;
        }

        private void Tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                var node = e.Node;

                if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Cargando...")
                {
                    node.Nodes.Clear();

                    string tag = node.Tag?.ToString() ?? "";

                    if (tag == TAG_TABLES) LoadObjects(node, _meta.GetTables());
                    else if (tag == TAG_VIEWS) LoadObjects(node, _meta.GetViews());
                    else if (tag == TAG_PROCS) LoadObjects(node, _meta.GetProcedures());
                    else if (tag == TAG_FUNCS) LoadObjects(node, _meta.GetFunctions());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadObjects(TreeNode categoryNode, DataTable dt)
        {
            string currentSchema = null;
            TreeNode schemaNode = null;

            foreach (DataRow row in dt.Rows)
            {
                string schema = row["SchemaName"].ToString();
                string name = row["ObjectName"].ToString();

                if (currentSchema != schema)
                {
                    currentSchema = schema;
                    schemaNode = new TreeNode(schema) { Tag = "SCHEMA_GROUP" };
                    categoryNode.Nodes.Add(schemaNode);
                }

                var objNode = new TreeNode(name)
                {
                    Tag = TAG_OBJECT,
                    ToolTipText = $"{schema}|{name}"
                };

                schemaNode.Nodes.Add(objNode);
            }

            if (categoryNode.Nodes.Count == 0)
                categoryNode.Nodes.Add(new TreeNode("(vacío)"));
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            lblRight.Text = $"Seleccionado: {node.FullPath}";
        }

        private void BuildContextMenu()
        {
            ctxTable = new ContextMenuStrip();

            var miViewData = new ToolStripMenuItem("Ver datos (TOP 200)");
            miViewData.Click += (_, __) => ViewSelectedTableData();

            var miViewCols = new ToolStripMenuItem("Ver columnas");
            miViewCols.Click += (_, __) => ViewSelectedTableColumns();

            var miViewDDL = new ToolStripMenuItem("Ver DDL");
            miViewDDL.Click += (_, __) => ViewSelectedDDL();

            ctxTable.Items.Add(miViewData);
            ctxTable.Items.Add(miViewCols);
            ctxTable.Items.Add(miViewDDL);
        }

        private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            tree.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right && (e.Node.Tag?.ToString() == TAG_OBJECT))
            {
                ctxTable.Show(tree, e.Location);
            }
        }

        private void ShowGrid()
        {
            txtDDL.Visible = false;
            grid.Visible = true;
        }

        private void ShowDDLBox()
        {
            grid.Visible = false;
            txtDDL.Visible = true;
        }

        private bool TryGetSelectedObject(out string schema, out string name)
        {
            schema = "";
            name = "";

            var node = tree.SelectedNode;
            if (node == null || node.Tag?.ToString() != TAG_OBJECT) return false;

            var parts = (node.ToolTipText ?? "").Split('|');
            if (parts.Length != 2) return false;

            schema = parts[0];
            name = parts[1];
            return true;
        }

        private void ViewSelectedTableData()
        {
            if (!TryGetSelectedObject(out string schema, out string table)) return;

            ShowGrid();
            var dt = _tableViewer.GetTopRows(schema, table, 200);
            grid.DataSource = dt;
            lblRight.Text = $"Datos: {schema}.{table} (TOP 200)";
        }

        private void ViewSelectedTableColumns()
        {
            if (!TryGetSelectedObject(out string schema, out string table)) return;

            ShowGrid();
            var dt = _tableViewer.GetColumns(schema, table);
            grid.DataSource = dt;
            lblRight.Text = $"Columnas: {schema}.{table}";
        }

        private void ViewSelectedDDL()
        {
            if (!TryGetSelectedObject(out string schema, out string objName)) return;

            string parentTag = tree.SelectedNode.Parent?.Parent?.Tag?.ToString() ?? "";
            string ddlText = "";

            if (parentTag == TAG_TABLES)
                ddlText = _ddl.GetTableDDL(schema, objName);
            else
                ddlText = _ddl.GetModuleDDL(schema, objName);

            ShowDDLBox();
            txtDDL.Text = ddlText;
            lblRight.Text = $"DDL: {schema}.{objName}";
        }
    }
}
