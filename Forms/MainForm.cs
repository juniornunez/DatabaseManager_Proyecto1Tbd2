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
        private const string TAG_TRIGGERS = "TRIGGERS";
        private const string TAG_INDEXES = "INDEXES";
        private const string TAG_SEQUENCES = "SEQUENCES";
        private const string TAG_USERS = "USERS";
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
                // CRÍTICO: NO establecer Panel1MinSize y Panel2MinSize aquí
            };

            Controls.Add(split);

            // SOLUCIÓN ROBUSTA: Usar evento Shown con doble BeginInvoke
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
                Dock = DockStyle.Fill,
                Padding = new Point(12, 4)  // AGREGADO: Padding para que quepa el texto y el botón X
            };

            // AGREGADO: Habilitar el cierre de pestañas con botón X
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += Tabs_DrawItem;
            tabs.MouseDown += Tabs_MouseDown;

            rightPanel.Controls.Add(tabs);
            rightPanel.Controls.Add(lblRight);
            split.Panel2.Controls.Add(rightPanel);

            BuildContextMenus();
        }

        /// <summary>
        /// Dibuja cada pestaña con un botón de cerrar (X)
        /// </summary>
        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                TabPage page = tabs.TabPages[e.Index];
                e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

                // Dibujar el texto de la pestaña
                string tabText = page.Text;
                SizeF textSize = e.Graphics.MeasureString(tabText, e.Font);
                int textX = e.Bounds.Left + 6;
                int textY = e.Bounds.Top + (e.Bounds.Height - (int)textSize.Height) / 2;

                e.Graphics.DrawString(tabText, e.Font, Brushes.Black, textX, textY);

                // Dibujar el botón X
                int closeButtonSize = 14;
                int closeX = e.Bounds.Right - closeButtonSize - 6;
                int closeY = e.Bounds.Top + (e.Bounds.Height - closeButtonSize) / 2;

                Rectangle closeRect = new Rectangle(closeX, closeY, closeButtonSize, closeButtonSize);

                // Fondo del botón X (rojo claro)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 80, 80)), closeRect);

                // Dibujar la X
                using (Pen whitePen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawLine(whitePen,
                        closeX + 3, closeY + 3,
                        closeX + closeButtonSize - 3, closeY + closeButtonSize - 3);
                    e.Graphics.DrawLine(whitePen,
                        closeX + closeButtonSize - 3, closeY + 3,
                        closeX + 3, closeY + closeButtonSize - 3);
                }

                e.DrawFocusRectangle();
            }
            catch { }
        }

        /// <summary>
        /// Detecta el click en el botón X para cerrar la pestaña
        /// </summary>
        private void Tabs_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                for (int i = 0; i < tabs.TabPages.Count; i++)
                {
                    Rectangle tabRect = tabs.GetTabRect(i);

                    // Área del botón X
                    int closeButtonSize = 14;
                    int closeX = tabRect.Right - closeButtonSize - 6;
                    int closeY = tabRect.Top + (tabRect.Height - closeButtonSize) / 2;
                    Rectangle closeRect = new Rectangle(closeX, closeY, closeButtonSize, closeButtonSize);

                    // Si se hizo click en el botón X
                    if (closeRect.Contains(e.Location))
                    {
                        // Cerrar la pestaña
                        tabs.TabPages.RemoveAt(i);
                        return;
                    }
                }
            }
            catch { }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // Doble BeginInvoke para asegurar que el formulario esté completamente renderizado
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

                // Verificar que tenemos un ancho válido
                if (totalWidth <= 100) return;

                // Establecer tamaños mínimos AHORA que tenemos dimensiones reales
                int minPanel1 = Math.Min(200, totalWidth / 4);
                int minPanel2 = Math.Min(400, totalWidth / 2);

                split.Panel1MinSize = minPanel1;
                split.Panel2MinSize = minPanel2;

                // Calcular la distancia deseada
                int desiredDistance = 360;
                int maxDistance = totalWidth - minPanel2 - split.SplitterWidth;

                // Asegurar que está en el rango válido
                if (desiredDistance < minPanel1)
                    desiredDistance = minPanel1;
                if (desiredDistance > maxDistance)
                    desiredDistance = maxDistance;

                // Solo aplicar si es válido
                if (desiredDistance >= minPanel1 && desiredDistance <= maxDistance)
                {
                    split.SplitterDistance = desiredDistance;
                }
            }
            catch (Exception ex)
            {
                // Si falla, usar valores por defecto seguros
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
            dbNode.Nodes.Add(MakeLazyNode("Triggers", TAG_TRIGGERS));
            dbNode.Nodes.Add(MakeLazyNode("Indexes", TAG_INDEXES));
            dbNode.Nodes.Add(MakeLazyNode("Sequences", TAG_SEQUENCES));
            dbNode.Nodes.Add(MakeLazyNode("Users", TAG_USERS));

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
                    else if (tag == TAG_TRIGGERS) LoadObjects(node, _meta.GetTriggers());
                    else if (tag == TAG_INDEXES) LoadObjects(node, _meta.GetIndexes());
                    else if (tag == TAG_SEQUENCES) LoadObjects(node, _meta.GetSequences());
                    else if (tag == TAG_USERS) LoadUsersFlat(node, _meta.GetUsers());
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

        private void LoadUsersFlat(TreeNode categoryNode, DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                string name = row["UserName"].ToString();

                var objNode = new TreeNode(name)
                {
                    Tag = TAG_OBJECT,
                    ToolTipText = name
                };

                categoryNode.Nodes.Add(objNode);
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

            var miRefresh = new ToolStripMenuItem("Refresh");
            miRefresh.Click += (_, __) => RefreshExplorer();

            var miDisconnect = new ToolStripMenuItem("Desconectar");
            miDisconnect.Click += (_, __) => DisconnectAndReturnToLogin();

            ctxDb.Items.Add(miNewQuery);
            ctxDb.Items.Add(new ToolStripSeparator());
            ctxDb.Items.Add(miRefresh);
            ctxDb.Items.Add(new ToolStripSeparator());
            ctxDb.Items.Add(miDisconnect);
        }

        private void DisconnectAndReturnToLogin()
        {
            var result = MessageBox.Show(
                "¿Deseas desconectarte y volver al inicio?",
                "Confirmar Desconexión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
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

                if (tag == TAG_TABLES || tag == TAG_VIEWS)
                {
                    ShowCreateMenu(tag, e.Location);
                    return;
                }

                if (tag == TAG_OBJECT)
                {
                    string parentTag = e.Node.Parent?.Parent?.Tag?.ToString() ?? "";
                    ShowContextMenuForObject(parentTag, e.Location);
                }
            }
        }

        private void ShowCreateMenu(string categoryTag, System.Drawing.Point location)
        {
            var contextMenu = new ContextMenuStrip();

            if (categoryTag == TAG_TABLES)
            {
                contextMenu.Items.Add(CreateMenuItem("Crear Tabla", (s, e) => OpenCreateTableWizard()));
            }
            else if (categoryTag == TAG_VIEWS)
            {
                contextMenu.Items.Add(CreateMenuItem("Crear Vista", (s, e) => OpenCreateViewWizard()));
            }

            contextMenu.Show(tree, location);
        }

        private void OpenCreateTableWizard()
        {
            var createForm = new CreateTableForm(_conn);
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                RefreshExplorer();
                MessageBox.Show("Tabla creada. El explorador se ha actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenCreateViewWizard()
        {
            var createForm = new CreateViewForm(_conn);
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                RefreshExplorer();
                MessageBox.Show("Vista creada. El explorador se ha actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Muestra el menú contextual apropiado según el tipo de objeto
        /// </summary>
        private void ShowContextMenuForObject(string objectType, System.Drawing.Point location)
        {
            var contextMenu = new ContextMenuStrip();

            switch (objectType)
            {
                case TAG_TABLES:
                    // TABLAS: Ver datos, Ver columnas, Ver DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver datos (TOP 200)", (s, e) => OpenTableDataTab()));
                    contextMenu.Items.Add(CreateMenuItem("Ver columnas", (s, e) => OpenColumnsTab()));
                    contextMenu.Items.Add(new ToolStripSeparator());
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;

                case TAG_VIEWS:
                    // VISTAS: Ver datos, Ver columnas, Ver DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver datos (TOP 1000)", (s, e) => OpenTableDataTab()));
                    contextMenu.Items.Add(CreateMenuItem("Ver columnas", (s, e) => OpenColumnsTab()));
                    contextMenu.Items.Add(new ToolStripSeparator());
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;

                case TAG_PROCS:
                    // PROCEDIMIENTOS: Solo Ver DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;

                case TAG_FUNCS:
                    // FUNCIONES: Solo Ver DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;

                case TAG_TRIGGERS:
                    // TRIGGERS: Solo Ver DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;

                default:
                    // Fallback: solo mostrar DDL
                    contextMenu.Items.Add(CreateMenuItem("Ver DDL", (s, e) => OpenDdlTab()));
                    break;
            }

            contextMenu.Show(tree, location);
        }

        /// <summary>
        /// Helper para crear menu items más fácilmente
        /// </summary>
        private ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += onClick;
            return item;
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
            string parentTag = tree.SelectedNode?.Parent?.Parent?.Tag?.ToString() ?? "";

            if (parentTag == TAG_USERS)
            {
                string userName = tree.SelectedNode?.Text ?? "";
                string ddlText = _ddl.GetUserDDL(userName);

                string key = $"DDL|{userName}";
                var page = UpsertTab(key, $"DDL: {userName}");
                page.Controls.Clear();

                var txt = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false,
                    Font = new Font("Consolas", 10, FontStyle.Regular),
                    BackColor = Color.FromArgb(250, 250, 250)
                };

                txt.Text = ddlText;
                page.Controls.Add(txt);
                lblRight.Text = $"DDL: {userName}";
                return;
            }

            if (!TryGetSelectedObject(out string schema, out string objName)) return;

            string ddlText2;
            switch (parentTag)
            {
                case TAG_TABLES:
                    ddlText2 = _ddl.GetTableDDL(schema, objName);
                    break;
                case TAG_VIEWS:
                    ddlText2 = _ddl.GetViewDDL(schema, objName);
                    break;
                case TAG_PROCS:
                    ddlText2 = _ddl.GetProcedureDDL(schema, objName);
                    break;
                case TAG_FUNCS:
                    ddlText2 = _ddl.GetFunctionDDL(schema, objName);
                    break;
                case TAG_TRIGGERS:
                    ddlText2 = _ddl.GetTriggerDDL(schema, objName);
                    break;
                case TAG_INDEXES:
                    ddlText2 = _ddl.GetIndexDDL(schema, objName);
                    break;
                case TAG_SEQUENCES:
                    ddlText2 = _ddl.GetSequenceDDL(schema, objName);
                    break;
                default:
                    ddlText2 = $"-- Tipo de objeto desconocido: {parentTag}";
                    break;
            }

            string key2 = $"DDL|{schema}.{objName}";
            var page2 = UpsertTab(key2, $"DDL: {objName}");
            page2.Controls.Clear();

            var txt2 = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            txt2.Text = ddlText2;
            page2.Controls.Add(txt2);

            lblRight.Text = $"DDL: {schema}.{objName}";
        }

        private void OpenQueryTab(string initialSql)
        {
            _sqlTabCounter++;

            var key = $"SQL|{_sqlTabCounter}";
            var page = UpsertTab(key, $"Query{_sqlTabCounter}");  // CAMBIADO: SQLQuery{n} → Query{n}
            page.Controls.Clear();

            var splitQ = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                FixedPanel = FixedPanel.Panel2,
                IsSplitterFixed = false
                // NO establecer MinSize aquí
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

            // SOLUCIÓN DEFINITIVA: Timer para configurar el splitter
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

                    if (totalHeight > 100) // Tenemos altura válida
                    {
                        // Configurar tamaños mínimos
                        int minPanel1 = Math.Min(150, totalHeight / 3);
                        int minPanel2 = Math.Min(150, totalHeight / 3);

                        splitQ.Panel1MinSize = minPanel1;
                        splitQ.Panel2MinSize = minPanel2;

                        // Calcular distancia deseada (60% para el editor)
                        int desiredDistance = (int)(totalHeight * 0.60);
                        int maxDistance = totalHeight - minPanel2 - splitQ.SplitterWidth;

                        if (desiredDistance < minPanel1)
                            desiredDistance = minPanel1;
                        if (desiredDistance > maxDistance)
                            desiredDistance = maxDistance;

                        // Aplicar
                        if (desiredDistance >= minPanel1 && desiredDistance <= maxDistance)
                        {
                            splitQ.SplitterDistance = desiredDistance;

                            // Éxito - detener el timer
                            setupTimer.Stop();
                            setupTimer.Dispose();
                        }
                    }
                }
                catch
                {
                    // Ignorar errores y seguir intentando
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