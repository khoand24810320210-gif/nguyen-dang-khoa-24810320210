using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace bai3
{
    public partial class Form1 : Form
    {
        private BindingList<Product> products = new BindingList<Product>();
        private BindingSource bs = new BindingSource();

        public Form1()
        {
            InitializeComponent();

            // Initialize category combo
            cboCategory.Items.AddRange(new string[] { "Điện thoại", "Laptop", "Phụ kiện" });

            // Setup data binding
            bs.DataSource = products;
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.DataSource = bs;

            // Define columns
            dgvProducts.Columns.Clear();
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "ProductId", HeaderText = "Mã SP" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "ProductName", HeaderText = "Tên SP", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Category", HeaderText = "Danh Mục" });
            var colPrice = new DataGridViewTextBoxColumn() { DataPropertyName = "UnitPrice", HeaderText = "Đơn Giá" };
            dgvProducts.Columns.Add(colPrice);
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn() { DataPropertyName = "Quantity", HeaderText = "Số Lượng" });

            // Format price column
            dgvProducts.CellFormatting += (s, e) =>
            {
                if (dgvProducts.Columns[e.ColumnIndex].DataPropertyName == "UnitPrice" && e.Value is decimal dec)
                {
                    e.Value = string.Format("{0:N0}", dec);
                    e.FormattingApplied = true;
                }
            };

            // Wire events
            btnChooseImage.Click += BtnChooseImage_Click;
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            // Menu items
            exportCSVToolStripMenuItem.Click += ExportCSVToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;

            UpdateStatus();
        }

        private void BtnChooseImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    picAvatar.Image?.Dispose();
                    picAvatar.Image = Image.FromFile(ofd.FileName);
                    picAvatar.Tag = ofd.FileName;
                }
                catch
                {
                    MessageBox.Show("Không thể nạp ảnh.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            errorProvider1.Clear();
            bool valid = true;
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider1.SetError(txtProductName, "Tên SP không được để trống");
                valid = false;
            }
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider1.SetError(txtUnitPrice, "Đơn giá phải > 0");
                valid = false;
            }
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider1.SetError(txtQuantity, "Số lượng phải >= 0");
                valid = false;
            }
            if (!valid) return;

            var p = new Product
            {
                ProductId = txtProductId.Text?.Trim() ?? Guid.NewGuid().ToString(),
                ProductName = txtProductName.Text.Trim(),
                Category = cboCategory.SelectedItem?.ToString() ?? string.Empty,
                UnitPrice = price,
                Quantity = qty,
                ImagePath = picAvatar.Tag as string
            };
            products.Add(p);
            bs.ResetBindings(false);
            UpdateStatus();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (bs.Current is Product current)
            {
                // find in products by id
                var orig = products.FirstOrDefault(x => x.ProductId == current.ProductId);
                if (orig != null)
                {
                    errorProvider1.Clear();
                    bool valid = true;
                    if (string.IsNullOrWhiteSpace(txtProductName.Text))
                    {
                        errorProvider1.SetError(txtProductName, "Tên SP không được để trống");
                        valid = false;
                    }
                    if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
                    {
                        errorProvider1.SetError(txtUnitPrice, "Đơn giá phải > 0");
                        valid = false;
                    }
                    if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
                    {
                        errorProvider1.SetError(txtQuantity, "Số lượng phải >= 0");
                        valid = false;
                    }
                    if (!valid) return;

                    orig.ProductName = txtProductName.Text.Trim();
                    orig.Category = cboCategory.SelectedItem?.ToString() ?? string.Empty;
                    orig.UnitPrice = price;
                    orig.Quantity = qty;
                    orig.ImagePath = picAvatar.Tag as string;
                    bs.ResetBindings(false);
                    UpdateStatus();
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (bs.Current is Product current)
            {
                var resp = MessageBox.Show("Bạn có chắc muốn xóa sản phẩm đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    var orig = products.FirstOrDefault(x => x.ProductId == current.ProductId);
                    if (orig != null)
                    {
                        products.Remove(orig);
                        bs.ResetBindings(false);
                        UpdateStatus();
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            string q = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(q))
            {
                bs.DataSource = products;
            }
            else
            {
                var filtered = products.Where(p => (p.ProductName ?? string.Empty).ToLower().Contains(q)).ToList();
                bs.DataSource = new BindingList<Product>(filtered);
            }
            dgvProducts.DataSource = bs;
        }

        private void DgvProducts_SelectionChanged(object? sender, EventArgs e)
        {
            if (bs.Current is Product current)
            {
                txtProductId.Text = current.ProductId;
                txtProductName.Text = current.ProductName;
                txtUnitPrice.Text = current.UnitPrice.ToString();
                txtQuantity.Text = current.Quantity.ToString();
                cboCategory.SelectedItem = current.Category;
                if (!string.IsNullOrEmpty(current.ImagePath) && File.Exists(current.ImagePath))
                {
                    try
                    {
                        picAvatar.Image?.Dispose();
                        picAvatar.Image = Image.FromFile(current.ImagePath);
                        picAvatar.Tag = current.ImagePath;
                    }
                    catch { picAvatar.Image = null; picAvatar.Tag = null; }
                }
                else
                {
                    picAvatar.Image = null;
                    picAvatar.Tag = null;
                }
            }
        }

        private void ExportCSVToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            sfd.DefaultExt = "csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8);
                    sw.WriteLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");
                    foreach (var p in products)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                            p.ProductId, p.ProductName, p.Category, string.Format("{0:N0}", p.UnitPrice), p.Quantity);
                        sw.WriteLine(line);
                    }
                    MessageBox.Show("Export thành công.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void UpdateStatus()
        {
            toolStripStatusLabel1.Text = $"Tổng số sản phẩm: {products.Count}";
        }
    }
}
