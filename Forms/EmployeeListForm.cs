using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class EmployeeListForm : Form
    {
        private MySqlConnection conn;
        private DataGridView dgvEmployees;
        private TextBox txtSearch;

        public EmployeeListForm()
        {
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadEmployees();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(800, 600);
            this.Text = "Employee Management";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Search controls
            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(80, 20),
                Width = 200
            };
            txtSearch.TextChanged += (s, e) => LoadEmployees();

            Button btnAdd = new Button
            {
                Text = "Add Employee",
                Location = new System.Drawing.Point(680, 20),
                Width = 100
            };
            btnAdd.Click += BtnAdd_Click;

            // Grid view
            dgvEmployees = new DataGridView
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(750, 450),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvEmployees.CellDoubleClick += DgvEmployees_CellDoubleClick;

            // Context menu
            var contextMenu = new ContextMenuStrip();
            var editItem = new ToolStripMenuItem("Edit", null, (s, e) => EditEmployee());
            var deleteItem = new ToolStripMenuItem("Delete", null, (s, e) => DeleteEmployee());
            contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
            dgvEmployees.ContextMenuStrip = contextMenu;

            // Add controls
            this.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch,
                btnAdd,
                dgvEmployees
            });
        }

        private void LoadEmployees()
        {
            try
            {
                string query = @"
                    SELECT id, name, username, role, phone, email 
                    FROM employees
                    WHERE name LIKE @search 
                       OR username LIKE @search
                       OR phone LIKE @search
                       OR email LIKE @search
                    ORDER BY name";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", $"%{txtSearch.Text}%");
                DatabaseConnection.Instance.OpenConnection();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                dgvEmployees.DataSource = dt;
                dgvEmployees.Columns["id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new EmployeeDetailForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadEmployees();
                }
            }
        }

        private void DgvEmployees_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditEmployee();
            }
        }

        private void EditEmployee()
        {
            if (dgvEmployees.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["id"].Value);
            using (var form = new EmployeeDetailForm(id))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadEmployees();
                }
            }
        }

        private void DeleteEmployee()
        {
            if (dgvEmployees.SelectedRows.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to delete this employee?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                int id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["id"].Value);

                string query = "DELETE FROM employees WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                DatabaseConnection.Instance.OpenConnection();
                cmd.ExecuteNonQuery();

                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
