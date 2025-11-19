using System;
using System.Drawing;
using System.Windows.Forms;
using ERP.UI.Factories;
using ERP.UI.UI;

namespace ERP.UI.Forms
{
    public partial class OrderListForm : UserControl
    {
        private Panel _mainPanel;
        private DataGridView _dgvOrders;
        private Button _btnRefresh;

        public event EventHandler<int> OrderSelected;
        public event EventHandler<int> OrderUpdateRequested;
        public event EventHandler<int> OrderDeleteRequested;

        public OrderListForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.BackColor = ThemeColors.Background;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);

            CreateMainPanel();
            LoadOrders();
        }

        private void CreateMainPanel()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(30)
            };

            UIHelper.ApplyCardStyle(_mainPanel, 12);

            // Başlık
            var titleLabel = new Label
            {
                Text = "Siparişleri Görüntüle",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeColors.Primary,
                AutoSize = true,
                Location = new Point(30, 30)
            };

            // Yenile butonu
            _btnRefresh = ButtonFactory.CreateActionButton("🔄 Yenile", ThemeColors.Info, Color.White, 120, 35);
            _btnRefresh.Location = new Point(_mainPanel.Width - 150, 30);
            _btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnRefresh.Click += BtnRefresh_Click;

            // DataGridView
            _dgvOrders = new DataGridView
            {
                Location = new Point(30, 80),
                Width = _mainPanel.Width - 60,
                Height = _mainPanel.Height - 120,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = ThemeColors.Surface,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _mainPanel.Resize += (s, e) =>
            {
                _dgvOrders.Width = _mainPanel.Width - 60;
                _dgvOrders.Height = _mainPanel.Height - 120;
            };

            SetupDataGridView();

            _mainPanel.Controls.Add(titleLabel);
            _mainPanel.Controls.Add(_btnRefresh);
            _mainPanel.Controls.Add(_dgvOrders);

            this.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();
        }

        private void SetupDataGridView()
        {
            _dgvOrders.Columns.Clear();

            _dgvOrders.Columns.Add("OrderId", "Sipariş No");
            _dgvOrders.Columns.Add("OrderDate", "Tarih");
            _dgvOrders.Columns.Add("CustomerName", "Müşteri");
            _dgvOrders.Columns.Add("TotalAmount", "Toplam");
            _dgvOrders.Columns.Add("Status", "Durum");

            // Action butonları için kolon
            var actionColumn = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "İşlemler",
                Text = "Düzenle",
                UseColumnTextForButtonValue = false,
                Width = 200
            };
            _dgvOrders.Columns.Add(actionColumn);

            // Kolon genişlikleri
            _dgvOrders.Columns["OrderId"].Width = 100;
            _dgvOrders.Columns["OrderDate"].Width = 120;
            _dgvOrders.Columns["CustomerName"].Width = 200;
            _dgvOrders.Columns["TotalAmount"].Width = 120;
            _dgvOrders.Columns["Status"].Width = 100;
            _dgvOrders.Columns["Actions"].Width = 200;

            // Stil ayarları
            foreach (DataGridViewColumn column in _dgvOrders.Columns)
            {
                column.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            }

            _dgvOrders.CellPainting += DgvOrders_CellPainting;
            _dgvOrders.CellContentClick += DgvOrders_CellContentClick;
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == _dgvOrders.Columns["Actions"].Index && e.RowIndex >= 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var updateRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 5, 80, e.CellBounds.Height - 10);
                var deleteRect = new Rectangle(e.CellBounds.X + 95, e.CellBounds.Y + 5, 80, e.CellBounds.Height - 10);

                // Update butonu
                using (var brush = new SolidBrush(ThemeColors.Info))
                {
                    e.Graphics.FillRectangle(brush, updateRect);
                }
                TextRenderer.DrawText(e.Graphics, "Güncelle", _dgvOrders.Font, updateRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // Delete butonu
                using (var brush = new SolidBrush(ThemeColors.Error))
                {
                    e.Graphics.FillRectangle(brush, deleteRect);
                }
                TextRenderer.DrawText(e.Graphics, "Sil", _dgvOrders.Font, deleteRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        private void DgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != _dgvOrders.Columns["Actions"].Index)
                return;

            var orderId = Convert.ToInt32(_dgvOrders.Rows[e.RowIndex].Cells["OrderId"].Value);
            
            // Mouse pozisyonunu al
            var mousePos = _dgvOrders.PointToClient(Cursor.Position);
            var hitTest = _dgvOrders.HitTest(mousePos.X, mousePos.Y);
            
            if (hitTest.RowIndex != e.RowIndex || hitTest.ColumnIndex != e.ColumnIndex)
                return;

            var cellRect = _dgvOrders.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var relativeX = mousePos.X - cellRect.X;
            
            // Buton pozisyonlarına göre kontrol et
            if (relativeX >= 5 && relativeX <= 85) // Update butonu
            {
                OrderUpdateRequested?.Invoke(this, orderId);
            }
            else if (relativeX >= 95 && relativeX <= 175) // Delete butonu
            {
                var result = MessageBox.Show(
                    $"Sipariş #{orderId} silinecek. Emin misiniz?",
                    "Sipariş Sil",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    OrderDeleteRequested?.Invoke(this, orderId);
                }
            }
        }

        private void LoadOrders()
        {
            // Şimdilik örnek veri - sonra DAL'dan gelecek
            _dgvOrders.Rows.Clear();

            // Örnek veriler
            _dgvOrders.Rows.Add(1, "2024-01-15", "ABC Şirketi", "15.000,00 ₺", "Aktif");
            _dgvOrders.Rows.Add(2, "2024-01-16", "XYZ Ltd.", "25.500,00 ₺", "Aktif");
            _dgvOrders.Rows.Add(3, "2024-01-17", "Test Müşteri", "8.750,00 ₺", "İptal");
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        public void RefreshData()
        {
            LoadOrders();
        }
    }
}

