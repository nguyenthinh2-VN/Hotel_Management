using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class RoomTypeForm : Form
    {
        private DataTable dataTable = new DataTable();
        private MySqlConnection conn;

        public RoomTypeForm()
        {
            InitializeComponent();
            conn = DatabaseConnection.Instance.GetConnection();
            LoadRoomTypes();
        }

        private void InitializeComponent()
        {
            this.Text = "Room Types Management";
            this.Size = new System.Drawing.Size(800, 600);

            // Create controls
            DataGridView dgvRoomTypes = new DataGridView
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
                Text = "Add New",
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

            // Add event handlers
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            // Add controls to form
            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
            this.Controls.Add(dgvRoomTypes);
            this.Controls.Add(buttonPanel);

            // Store controls as fields
            this.dgvRoomTypes = dgvRoomTypes;
        }

        private DataGridView dgvRoomTypes;

        private void LoadRoomTypes()
        {
            try
            {
                DatabaseConnection.Instance.OpenConnection();
                string query = "SELECT id, name, price, description FROM room_types";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                dataTable.Clear();
                adapter.Fill(dataTable);
                dgvRoomTypes.DataSource = dataTable;

                // Rename columns for better display
                dgvRoomTypes.Columns["id"].HeaderText = "ID";
                dgvRoomTypes.Columns["name"].HeaderText = "Room Type";
                dgvRoomTypes.Columns["price"].HeaderText = "Price";
                dgvRoomTypes.Columns["description"].HeaderText = "Description";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room types: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new RoomTypeDetailForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRoomTypes();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRoomTypes.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvRoomTypes.SelectedRows[0].Cells["id"].Value);
                using (var form = new RoomTypeDetailForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadRoomTypes();
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRoomTypes.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this room type?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        int id = Convert.ToInt32(dgvRoomTypes.SelectedRows[0].Cells["id"].Value);
                        string query = "DELETE FROM room_types WHERE id = @id";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        DatabaseConnection.Instance.OpenConnection();
                        cmd.ExecuteNonQuery();
                        LoadRoomTypes();
                        MessageBox.Show("Room type deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting room type: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
