using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class CustomerListForm : Form
    {
        private DataTable dataTable = new DataTable();
        private MySqlConnection conn;

        public CustomerListForm()
        {
            InitializeComponent();
            conn = DatabaseConnection.Instance.GetConnection();
            LoadCustomers();
        }

        private DataGridView dgvCustomers;
        private TextBox txtSearch;

        private void InitializeComponent()
        {
            this.Text = "Customer List";
            this.Size = new System.Drawing.Size(1000, 600);

            // Create controls
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new System.Drawing.Point(10, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(70, 12),
                Width = 200
            };

            Button btnSearch = new Button
            {
                Text = "Search",
                Location = new System.Drawing.Point(280, 10),
                Width = 80
            };

            dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            Button btnAdd = new Button
            {
                Text = "Add Customer",
                Width = 100,
                Location = new System.Drawing.Point(10, 10)
            };

            Button btnEdit = new Button
            {
                Text = "Edit",
                Width = 100,
                Location = new System.Drawing.Point(120, 10)
            };

            Button btnDelete = new Button
            {
                Text = "Delete",
                Width = 100,
                Location = new System.Drawing.Point(230, 10)
            };

            Button btnViewBookings = new Button
            {
                Text = "View Bookings",
                Width = 100,
                Location = new System.Drawing.Point(340, 10)
            };

            // Add event handlers
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnViewBookings.Click += BtnViewBookings_Click;
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnSearch_Click(s, e); };

            // Add controls to panels
            topPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch });
            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnViewBookings });

            // Add controls to form
            this.Controls.Add(dgvCustomers);
            this.Controls.Add(topPanel);
            this.Controls.Add(buttonPanel);
        }

        private void LoadCustomers(string searchTerm = "")
        {
            try
            {
                string whereClause = string.IsNullOrWhiteSpace(searchTerm) ? "" :
                    $@" WHERE name LIKE @search 
                        OR id_card LIKE @search 
                        OR phone LIKE @search 
                        OR email LIKE @search";

                string query = $@"
                    SELECT id, name, id_card, phone, email, address
                    FROM customers
                    {whereClause}
                    ORDER BY name";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DatabaseConnection.Instance.OpenConnection();
                dataTable.Clear();
                adapter.Fill(dataTable);
                dgvCustomers.DataSource = dataTable;

                // Rename columns for better display
                dgvCustomers.Columns["id"].HeaderText = "ID";
                dgvCustomers.Columns["name"].HeaderText = "Name";
                dgvCustomers.Columns["id_card"].HeaderText = "ID Card";
                dgvCustomers.Columns["phone"].HeaderText = "Phone";
                dgvCustomers.Columns["email"].HeaderText = "Email";
                dgvCustomers.Columns["address"].HeaderText = "Address";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadCustomers(txtSearch.Text.Trim());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new CustomerDetailForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["id"].Value);
                using (var form = new CustomerDetailForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadCustomers();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["id"].Value);

                // Check if customer has any bookings
                try
                {
                    string checkQuery = "SELECT COUNT(*) FROM bookings WHERE customer_id = @id";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@id", id);
                    DatabaseConnection.Instance.OpenConnection();
                    int bookingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (bookingCount > 0)
                    {
                        MessageBox.Show("Cannot delete customer with existing bookings.", "Delete Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking customer bookings: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM customers WHERE id = @id";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        DatabaseConnection.Instance.OpenConnection();
                        cmd.ExecuteNonQuery();
                        LoadCustomers();
                        MessageBox.Show("Customer deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnViewBookings_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                int customerId = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["id"].Value);
                string customerName = dgvCustomers.SelectedRows[0].Cells["name"].Value.ToString();
                
                var form = new BookingListForm(customerId: customerId);
                form.Text = $"Bookings - {customerName}";
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a customer.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
