using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class RoomListForm : Form
    {
        private DataTable dataTable = new DataTable();
        private MySqlConnection conn;

        public RoomListForm()
        {
            InitializeComponent();
            conn = DatabaseConnection.Instance.GetConnection();
            LoadRooms();
        }

        private DataGridView dgvRooms;
        private ComboBox cmbFilter;

        private void InitializeComponent()
        {
            this.Text = "Room List";
            this.Size = new System.Drawing.Size(1000, 600);

            // Create controls
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            Label lblFilter = new Label
            {
                Text = "Filter by Status:",
                Location = new System.Drawing.Point(10, 15),
                AutoSize = true
            };

            cmbFilter = new ComboBox
            {
                Location = new System.Drawing.Point(100, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilter.Items.AddRange(new string[] { "All", "Available", "Booked", "Occupied", "Maintenance" });
            cmbFilter.SelectedIndex = 0;

            dgvRooms = new DataGridView
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
                Text = "Add Room",
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

            Button btnMaintenance = new Button
            {
                Text = "Set Maintenance",
                Width = 120,
                Location = new System.Drawing.Point(340, 10)
            };

            // Add event handlers
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnMaintenance.Click += BtnMaintenance_Click;
            cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;

            // Add controls to panels
            topPanel.Controls.AddRange(new Control[] { lblFilter, cmbFilter });
            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnMaintenance });

            // Add controls to form
            this.Controls.Add(dgvRooms);
            this.Controls.Add(topPanel);
            this.Controls.Add(buttonPanel);
        }

        private void LoadRooms()
        {
            try
            {
                string filter = cmbFilter.SelectedItem.ToString();
                string whereClause = filter == "All" ? "" : $" WHERE r.status = '{filter}'";

                string query = $@"
                    SELECT r.id, r.room_number, rt.name as room_type, r.status, 
                           rt.price, r.floor
                    FROM rooms r
                    JOIN room_types rt ON r.room_type_id = rt.id
                    {whereClause}
                    ORDER BY r.room_number";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                
                DatabaseConnection.Instance.OpenConnection();
                dataTable.Clear();
                adapter.Fill(dataTable);
                dgvRooms.DataSource = dataTable;

                // Rename columns for better display
                dgvRooms.Columns["id"].HeaderText = "ID";
                dgvRooms.Columns["room_number"].HeaderText = "Room Number";
                dgvRooms.Columns["room_type"].HeaderText = "Room Type";
                dgvRooms.Columns["status"].HeaderText = "Status";
                dgvRooms.Columns["price"].HeaderText = "Price";
                dgvRooms.Columns["floor"].HeaderText = "Floor";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new RoomDetailForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRooms();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["id"].Value);
                using (var form = new RoomDetailForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadRooms();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this room?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        int id = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["id"].Value);
                        string query = "DELETE FROM rooms WHERE id = @id";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        
                        DatabaseConnection.Instance.OpenConnection();
                        cmd.ExecuteNonQuery();
                        LoadRooms();
                        
                        MessageBox.Show("Room deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting room: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnMaintenance_Click(object sender, EventArgs e)
        {
            if (dgvRooms.SelectedRows.Count > 0)
            {
                try
                {
                    int id = Convert.ToInt32(dgvRooms.SelectedRows[0].Cells["id"].Value);
                    string currentStatus = dgvRooms.SelectedRows[0].Cells["status"].Value.ToString();
                    string newStatus = currentStatus == "Maintenance" ? "Available" : "Maintenance";

                    string query = "UPDATE rooms SET status = @status WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", id);
                    
                    DatabaseConnection.Instance.OpenConnection();
                    cmd.ExecuteNonQuery();
                    LoadRooms();
                    
                    MessageBox.Show($"Room status updated to {newStatus}!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating room status: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRooms();
        }
    }
}
