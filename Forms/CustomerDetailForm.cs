using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class CustomerDetailForm : Form
    {
        private readonly int? customerId;
        private MySqlConnection conn;

        public CustomerDetailForm(int? id = null)
        {
            customerId = id;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            
            if (customerId.HasValue)
            {
                LoadCustomer();
                this.Text = "Edit Customer";
            }
            else
            {
                this.Text = "Add New Customer";
            }
        }

        private TextBox txtName;
        private TextBox txtIdCard;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 400);
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

            Label lblIdCard = new Label
            {
                Text = "ID Card:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            txtIdCard = new TextBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200
            };

            Label lblPhone = new Label
            {
                Text = "Phone:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200
            };

            Label lblEmail = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(20, 140),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new System.Drawing.Point(120, 140),
                Width = 200
            };

            Label lblAddress = new Label
            {
                Text = "Address:",
                Location = new System.Drawing.Point(20, 180),
                AutoSize = true
            };

            txtAddress = new TextBox
            {
                Location = new System.Drawing.Point(120, 180),
                Width = 200,
                Height = 100,
                Multiline = true
            };

            Button btnSave = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(120, 300),
                Width = 80
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(220, 300),
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
                lblIdCard, txtIdCard,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblAddress, txtAddress,
                btnSave, btnCancel
            });
        }

        private void LoadCustomer()
        {
            try
            {
                string query = "SELECT name, id_card, phone, email, address FROM customers WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", customerId);
                DatabaseConnection.Instance.OpenConnection();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["name"].ToString();
                        txtIdCard.Text = reader["id_card"].ToString();
                        txtPhone.Text = reader["phone"].ToString();
                        txtEmail.Text = reader["email"].ToString();
                        txtAddress.Text = reader["address"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || 
                string.IsNullOrWhiteSpace(txtIdCard.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please fill in all required fields (Name, ID Card, and Phone).", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check for duplicate ID Card
                string checkQuery = "SELECT id FROM customers WHERE id_card = @id_card AND id != @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@id_card", txtIdCard.Text);
                checkCmd.Parameters.AddWithValue("@id", customerId ?? 0);
                DatabaseConnection.Instance.OpenConnection();

                if (checkCmd.ExecuteScalar() != null)
                {
                    MessageBox.Show("A customer with this ID Card already exists.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query;
                if (customerId.HasValue)
                {
                    query = @"
                        UPDATE customers 
                        SET name = @name, 
                            id_card = @id_card,
                            phone = @phone,
                            email = @email,
                            address = @address
                        WHERE id = @id";
                }
                else
                {
                    query = @"
                        INSERT INTO customers 
                        (name, id_card, phone, email, address)
                        VALUES 
                        (@name, @id_card, @phone, @email, @address)";
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@id_card", txtIdCard.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                
                if (customerId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                }

                cmd.ExecuteNonQuery();

                MessageBox.Show("Customer saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
