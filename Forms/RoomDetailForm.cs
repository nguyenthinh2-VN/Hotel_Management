using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class RoomDetailForm : Form
    {
        private readonly int? roomId;
        private MySqlConnection conn;

        public RoomDetailForm(int? id = null)
        {
            roomId = id;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadRoomTypes();
            
            if (roomId.HasValue)
            {
                LoadRoom();
                this.Text = "Edit Room";
            }
            else
            {
                this.Text = "Add New Room";
                cmbStatus.SelectedIndex = 0; // Set default status to Available
            }
        }

        private TextBox txtRoomNumber;
        private ComboBox cmbRoomType;
        private ComboBox cmbStatus;
        private TextBox txtFloor;

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Create controls
            Label lblRoomNumber = new Label
            {
                Text = "Room Number:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtRoomNumber = new TextBox
            {
                Location = new System.Drawing.Point(120, 20),
                Width = 200
            };

            Label lblRoomType = new Label
            {
                Text = "Room Type:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            cmbRoomType = new ComboBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblStatus = new Label
            {
                Text = "Status:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            cmbStatus = new ComboBox
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "Available", "Booked", "Occupied", "Maintenance" });

            Label lblFloor = new Label
            {
                Text = "Floor:",
                Location = new System.Drawing.Point(20, 140),
                AutoSize = true
            };

            txtFloor = new TextBox
            {
                Location = new System.Drawing.Point(120, 140),
                Width = 200
            };

            Button btnSave = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(120, 200),
                Width = 80
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(220, 200),
                Width = 80
            };

            // Add event handlers
            btnSave.Click += BtnSave_Click;
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblRoomNumber, txtRoomNumber,
                lblRoomType, cmbRoomType,
                lblStatus, cmbStatus,
                lblFloor, txtFloor,
                btnSave, btnCancel
            });
        }

        private void LoadRoomTypes()
        {
            try
            {
                string query = "SELECT id, name FROM room_types ORDER BY name";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                DatabaseConnection.Instance.OpenConnection();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ComboBoxItem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        };
                        cmbRoomType.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room types: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void LoadRoom()
        {
            try
            {
                string query = @"
                    SELECT r.room_number, r.room_type_id, r.status, r.floor
                    FROM rooms r
                    WHERE r.id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", roomId);
                DatabaseConnection.Instance.OpenConnection();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtRoomNumber.Text = reader["room_number"].ToString();
                        int roomTypeId = Convert.ToInt32(reader["room_type_id"]);
                        foreach (ComboBoxItem item in cmbRoomType.Items)
                        {
                            if (item.Id == roomTypeId)
                            {
                                cmbRoomType.SelectedItem = item;
                                break;
                            }
                        }
                        cmbStatus.Text = reader["status"].ToString();
                        txtFloor.Text = reader["floor"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text) || 
                cmbRoomType.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtFloor.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtFloor.Text, out _))
            {
                MessageBox.Show("Please enter a valid floor number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query;
                if (roomId.HasValue)
                {
                    query = @"
                        UPDATE rooms 
                        SET room_number = @room_number, 
                            room_type_id = @room_type_id,
                            status = @status,
                            floor = @floor
                        WHERE id = @id";
                }
                else
                {
                    query = @"
                        INSERT INTO rooms 
                        (room_number, room_type_id, status, floor)
                        VALUES 
                        (@room_number, @room_type_id, @status, @floor)";
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@room_number", txtRoomNumber.Text);
                cmd.Parameters.AddWithValue("@room_type_id", ((ComboBoxItem)cmbRoomType.SelectedItem).Id);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@floor", Convert.ToInt32(txtFloor.Text));
                
                if (roomId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@id", roomId);
                }

                DatabaseConnection.Instance.OpenConnection();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Room saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving room: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }

    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
