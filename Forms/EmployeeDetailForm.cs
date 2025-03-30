using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class EmployeeDetailForm : Form
    {
        private readonly int? employeeId;
        private MySqlConnection conn;
        private TextBox txtName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private TextBox txtPhone;
        private TextBox txtEmail;

        public EmployeeDetailForm(int? id = null)
        {
            employeeId = id;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            if (employeeId.HasValue)
            {
                LoadEmployee();
                this.Text = "Edit Employee";
            }
            else
            {
                this.Text = "New Employee";
            }
        }

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

            Label lblUsername = new Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200
            };

            Label lblPassword = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200,
                UseSystemPasswordChar = true
            };

            Label lblRole = new Label
            {
                Text = "Role:",
                Location = new System.Drawing.Point(20, 140),
                AutoSize = true
            };

            cmbRole = new ComboBox
            {
                Location = new System.Drawing.Point(120, 140),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new object[] { "Admin", "Receptionist" });
            cmbRole.SelectedIndex = 1;

            Label lblPhone = new Label
            {
                Text = "Phone:",
                Location = new System.Drawing.Point(20, 180),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Location = new System.Drawing.Point(120, 180),
                Width = 200
            };

            Label lblEmail = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(20, 220),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new System.Drawing.Point(120, 220),
                Width = 200
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

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblRole, cmbRole,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                btnSave, btnCancel
            });
        }

        private void LoadEmployee()
        {
            try
            {
                string query = "SELECT * FROM employees WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", employeeId);
                DatabaseConnection.Instance.OpenConnection();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["name"].ToString();
                        txtUsername.Text = reader["username"].ToString();
                        txtPassword.Text = "";  // Don't load password
                        cmbRole.SelectedItem = reader["role"].ToString();
                        txtPhone.Text = reader["phone"].ToString();
                        txtEmail.Text = reader["email"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                (!employeeId.HasValue && string.IsNullOrWhiteSpace(txtPassword.Text)))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query;
                if (employeeId.HasValue)
                {
                    query = @"
                        UPDATE employees 
                        SET name = @name,
                            username = @username,
                            role = @role,
                            phone = @phone,
                            email = @email
                        WHERE id = @id";

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        query = query.Insert(query.IndexOf("SET"), 
                            "password = @password, ");
                    }
                }
                else
                {
                    query = @"
                        INSERT INTO employees 
                        (name, username, password, role, phone, email)
                        VALUES 
                        (@name, @username, @password, @role, @phone, @email)";
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                cmd.Parameters.AddWithValue("@role", cmbRole.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);

                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                }

                if (employeeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@id", employeeId.Value);
                }

                DatabaseConnection.Instance.OpenConnection();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Employee saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
