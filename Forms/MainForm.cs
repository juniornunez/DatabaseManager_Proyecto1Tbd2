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
        private readonly SqlExecutorService _sql;

        private SplitContainer split;
        private TreeView tree;

        private Panel rightPanel;
        private Label lblRight;

        private TabControl tabs;
        private int _sqlTabCounter = 0;

        private ContextMenuStrip ctxObject;
        private ContextMenuStrip ctxDb;

        private const string TAG_DB = "DB";
        private const string TAG_TABLES = "TABLES";
        private const string TAG_VIEWS = "VIEWS";
        private const string TAG_PROCS = "PROCS";
        private const string TAG_FUNCS = "FUNCS";
        private const string TAG_OBJECT = "OBJECT";
        private const string TAG_SCHEMA_GROUP = "SCHEMA_GROUP";

        public MainForm(ConnectionService conn)
        {
            _conn = conn;
            _meta = new MetadataService(_conn);
            _tableViewer = new TableViewerService(_conn);
            _ddl = new DdlService(_conn);
            _sql = new SqlExecutorService(_conn);

            BuildUI();
            BuildTreeRoot();
        }

        private void BuildUI()
        {
            Text = "Database Manager - Explorer";
            Size = new Size(1200, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel1,
                IsSplitterFixed = false,
               
            };

            Controls.Add(split);

            this.Shown += MainForm_Shown;

            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true
            };
            tree.BeforeExpand += Tree_BeforeExpand;
            tree.AfterSelect += Tree_AfterSelect;
            tree.NodeMouseClick += Tree_NodeMouseClick;

            split.Panel1.Controls.Add(tree);

            rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            lblRight = new Label
            {
                Text = "Selecciona un nodo o haz click derecho.",
                Dock = DockStyle.Top,
                Height = 26,
                AutoSize = false,
                Font = new Font(Font.FontFamily, 10, FontStyle.Regular)
            };

            tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            rightPanel.Controls.Add(tabs);
            rightPanel.Controls.Add(lblRight);
            split.Panel2.Controls.Add(rightPanel);

            BuildContextMenus();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                BeginInvoke(new Action(() =>
                {
                    ConfigureMainSplitter();
                }));
            }));
        }

        private void ConfigureMainSplitter()
        {
            try
            {
                if (split == null || split.IsDisposed) return;

                int totalWidth = split.ClientSize.Width;

                if (totalWidth <= 100) return;

                int minPanel1 = Math.Min(200, totalWidth / 4);
                int minPanel2 = Math.Min(400, totalWidth / 2);

                split.Panel1MinSize = minPanel1;
                split.Panel2MinSize = minPanel2;

                int desiredDistance = 360;
                int maxDistance = totalWidth - minPanel2 - split.SplitterWidth;

             
                if (desiredDistance < minPanel1)
                    desiredDistance = minPanel1;
                if (desiredDistance > maxDistance)
                    desiredDistance = maxDistance;

                if (desiredDistance >= minPanel1 && desiredDistance <= maxDistance)
                {
                    split.SplitterDistance = desiredDistance;
                }
            }
            catch (Exception ex)
            {
               
                System.Diagnostics.Debug.WriteLine($"Error configurando splitter: {ex.Message}");
            }
        }

        private void BuildTreeRoot()
        {
            tree.Nodes.Clear();

            var dbNode = new TreeNode("Database") { Tag = TAG_DB };
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
                    schemaNode = new TreeNode(schema) { Tag = TAG_SCHEMA_GROUP };
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

        private void BuildContextMenus()
        {
            ctxDb = new ContextMenuStrip();
            var miNewQuery = new ToolStripMenuItem("New Query");
            miNewQuery.Click += (_, __) => OpenQueryTab("");
            ctxDb.Items.Add(miNewQuery);

            ctxObject = new ContextMenuStrip();

            var miViewData = new ToolStripMenuItem("Ver datos (TOP 200)");
            miViewData.Click += (_, __) => OpenTableDataTab();

            var miViewCols = new ToolStripMenuItem("Ver columnas");
            miViewCols.Click += (_, __) => OpenColumnsTab();

            var miViewDDL = new ToolStripMenuItem("Ver DDL");
            miViewDDL.Click += (_, __) => OpenDdlTab();

            var miQuery = new ToolStripMenuItem("New Query");
            miQuery.Click += (_, __) =>
            {
                if (TryGetSelectedObject(out string schema, out string name))
                {
                    string parentTag = tree.SelectedNode.Parent?.Parent?.Tag?.ToString() ?? "";
                    string sql = parentTag == TAG_TABLES
                        ? $"SELECT TOP 200 * FROM [{schema}].[{name}];"
                        : $"SELECT * FROM [{schema}].[{name}];";
                    OpenQueryTab(sql);
                }
                else OpenQueryTab("");
            };

            ctxObject.Items.Add(miViewData);
            ctxObject.Items.Add(miViewCols);
            ctxObject.Items.Add(miViewDDL);
            ctxObject.Items.Add(new ToolStripSeparator());
            ctxObject.Items.Add(miQuery);
        }

        private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            tree.SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                string tag = e.Node.Tag?.ToString() ?? "";
                if (tag == TAG_DB)
                {
                    ctxDb.Show(tree, e.Location);
                    return;
                }

                if (tag == TAG_OBJECT)
                {
                    ctxObject.Show(tree, e.Location);
                }
            }
        }

        private bool TryGetSelectedObject(out string schema, out string name)
        {
            schema = null;
            name = null;

            if (tree.SelectedNode == null) return false;
            string tag = tree.SelectedNode.Tag?.ToString() ?? "";
            if (tag != TAG_OBJECT) return false;

            string tip = tree.SelectedNode.ToolTipText;
            if (string.IsNullOrEmpty(tip)) return false;

            var parts = tip.Split('|');
            if (parts.Length != 2) return false;

            schema = parts[0];
            name = parts[1];
            return true;
        }

        private TabPage UpsertTab(string key, string title)
        {
            foreach (TabPage page in tabs.TabPages)
            {
                if (page.Tag?.ToString() == key)
                {
                    tabs.SelectedTab = page;
                    return page;
                }
            }

            var newPage = new TabPage(title) { Tag = key };
            tabs.TabPages.Add(newPage);
            tabs.SelectedTab = newPage;
            return newPage;
        }

        private void OpenTableDataTab()
        {
            if (!TryGetSelectedObject(out string schema, out string objName)) return;

            var dt = _tableViewer.GetTableData(schema, objName);
            string key = $"DATA|{schema}.{objName}";
            var page = UpsertTab(key, $"Data: {objName}");
            page.Controls.Clear();

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = dt,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            page.Controls.Add(grid);
            lblRight.Text = $"Datos: {schema}.{objName}";
        }

        private void OpenColumnsTab()
        {
            if (!TryGetSelectedObject(out string schema, out string objName)) return;

            var dt = _meta.GetColumns(schema, objName);
            string key = $"COLS|{schema}.{objName}";
            var page = UpsertTab(key, $"Columns: {objName}");
            page.Controls.Clear();

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = dt,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            page.Controls.Add(grid);
            lblRight.Text = $"Columnas: {schema}.{objName}";
        }

        private void OpenDdlTab()
        {
            if (!TryGetSelectedObject(out string schema, out string objName)) return;

            string parentTag = tree.SelectedNode?.Parent?.Parent?.Tag?.ToString() ?? "";

            string ddlText = parentTag == TAG_TABLES
                ? _ddl.GetTableDDL(schema, objName)
                : _ddl.GetModuleDDL(schema, objName);

            string key = $"DDL|{schema}.{objName}";
            var page = UpsertTab(key, $"DDL: {objName}");
            page.Controls.Clear();

            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10, FontStyle.Regular)
            };

            txt.Text = ddlText;
            page.Controls.Add(txt);

            lblRight.Text = $"DDL: {schema}.{objName}";
        }

        private void OpenQueryTab(string initialSql)
        {
            _sqlTabCounter++;

            var key = $"SQL|{_sqlTabCounter}";
            var page = UpsertTab(key, $"SQLQuery{_sqlTabCounter}");
            page.Controls.Clear();

            var splitQ = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                FixedPanel = FixedPanel.Panel2,
                IsSplitterFixed = false
               
            };

            var editorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.White
            };

            var actions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42
            };

            var btnRun = new Button { Text = "Execute", Width = 110, Height = 30, Left = 0, Top = 6 };
            var btnClear = new Button { Text = "Clear", Width = 110, Height = 30, Left = 120, Top = 6 };
            var btnRefresh = new Button { Text = "Refresh Explorer", Width = 140, Height = 30, Left = 240, Top = 6 };

            actions.Controls.Add(btnRun);
            actions.Controls.Add(btnClear);
            actions.Controls.Add(btnRefresh);

            var txtSql = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = false,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 11, FontStyle.Regular),
                AcceptsTab = true,
                AcceptsReturn = true,
                BackColor = Color.White
            };

            if (!string.IsNullOrWhiteSpace(initialSql))
                txtSql.Text = initialSql;

            editorPanel.Controls.Add(txtSql);
            editorPanel.Controls.Add(actions);

            var resultTabs = new TabControl { Dock = DockStyle.Fill };

            var tpResults = new TabPage("Results");
            var tpMessages = new TabPage("Messages");

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var txtMsg = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            tpResults.Controls.Add(grid);
            tpMessages.Controls.Add(txtMsg);

            resultTabs.TabPages.Add(tpResults);
            resultTabs.TabPages.Add(tpMessages);

            splitQ.Panel1.Controls.Add(editorPanel);
            splitQ.Panel2.Controls.Add(resultTabs);

            page.Controls.Add(splitQ);

            System.Windows.Forms.Timer setupTimer = null;
            setupTimer = new System.Windows.Forms.Timer { Interval = 50 };
            int attempts = 0;

            setupTimer.Tick += (_, __) =>
            {
                attempts++;

                try
                {
                    if (splitQ.IsDisposed || attempts > 20)
                    {
                        setupTimer?.Stop();
                        setupTimer?.Dispose();
                        return;
                    }

                    int totalHeight = splitQ.ClientSize.Height;

                    if (totalHeight > 100) 
                    {
                        int minPanel1 = Math.Min(150, totalHeight / 3);
                        int minPanel2 = Math.Min(150, totalHeight / 3);

                        splitQ.Panel1MinSize = minPanel1;
                        splitQ.Panel2MinSize = minPanel2;

                        int desiredDistance = (int)(totalHeight * 0.60);
                        int maxDistance = totalHeight - minPanel2 - splitQ.SplitterWidth;

                        if (desiredDistance < minPanel1)
                            desiredDistance = minPanel1;
                        if (desiredDistance > maxDistance)
                            desiredDistance = maxDistance;

                        if (desiredDistance >= minPanel1 && desiredDistance <= maxDistance)
                        {
                            splitQ.SplitterDistance = desiredDistance;

                            setupTimer.Stop();
                            setupTimer.Dispose();
                        }
                    }
                }
                catch
                {
                   
                }
            };

            setupTimer.Start();

            btnClear.Click += (_, __) => txtSql.Clear();
            btnRefresh.Click += (_, __) => RefreshExplorer();

            btnRun.Click += (_, __) =>
            {
                try
                {
                    string sqlText = txtSql.Text ?? "";
                    if (string.IsNullOrWhiteSpace(sqlText))
                    {
                        txtMsg.Text = "Escribe una consulta.";
                        resultTabs.SelectedTab = tpMessages;
                        txtSql.Focus();
                        return;
                    }

                    if (LooksLikeSelect(sqlText))
                    {
                        var dt = _sql.ExecuteQuery(sqlText);
                        grid.DataSource = dt;
                        txtMsg.Text = $"OK - {dt.Rows.Count} filas.";
                        resultTabs.SelectedTab = tpResults;
                    }
                    else
                    {
                        int affected = _sql.ExecuteNonQuery(sqlText);
                        grid.DataSource = null;
                        txtMsg.Text = $"OK - filas afectadas: {affected}.";
                        resultTabs.SelectedTab = tpMessages;
                        RefreshExplorer();
                    }

                    txtSql.Focus();
                }
                catch (Exception ex)
                {
                    txtMsg.Text = ex.Message;
                    resultTabs.SelectedTab = tpMessages;
                    txtSql.Focus();
                }
            };

            BeginInvoke(new Action(() => txtSql.Focus()));
        }

        private void RefreshExplorer()
        {
            BuildTreeRoot();
        }

        private bool LooksLikeSelect(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            string trimmed = sql.TrimStart().ToUpperInvariant();
            return trimmed.StartsWith("SELECT") || trimmed.StartsWith("WITH");
        }
    }
}