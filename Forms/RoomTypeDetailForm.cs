using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class RoomTypeDetailForm : Form
    {
        private readonly int? roomTypeId;
        private MySqlConnection conn;

        public RoomTypeDetailForm(int? id = null)
        {
            roomTypeId = id;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            if (roomTypeId.HasValue)
            {
                LoadRoomType();
                this.Text = "Edit Room Type";
            }
            else
            {
                this.Text = "Add New Room Type";
            }
        }

        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtDescription;

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Create controls
            Label lblName = new Label
            {
                Text = "Name:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Location = new System.Drawing.Point(120, 20),
                Width = 200
            };

            Label lblPrice = new Label
            {
                Text = "Price:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            txtPrice = new TextBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200
            };

            Label lblDescription = new Label
            {
                Text = "Description:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            txtDescription = new TextBox
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200,
                Height = 80,
                Multiline = true
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
                lblName, txtName,
                lblPrice, txtPrice,
                lblDescription, txtDescription,
                btnSave, btnCancel
            });
        }

        private void LoadRoomType()
        {
            try
            {
                string query = "SELECT name, price, description FROM room_types WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", roomTypeId);
                DatabaseConnection.Instance.OpenConnection();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["name"].ToString();
                        txtPrice.Text = reader["price"].ToString();
                        txtDescription.Text = reader["description"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room type: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query;
                if (roomTypeId.HasValue)
                {
                    query = @"UPDATE room_types 
                             SET name = @name, price = @price, description = @description 
                             WHERE id = @id";
                }
                else
                {
                    query = @"INSERT INTO room_types (name, price, description) 
                             VALUES (@name, @price, @description)";
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@description", txtDescription.Text);
                if (roomTypeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@id", roomTypeId);
                }

                DatabaseConnection.Instance.OpenConnection();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Room type saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving room type: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
