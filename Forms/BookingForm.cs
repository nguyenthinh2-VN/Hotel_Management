using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class BookingForm : Form
    {
        private readonly int employeeId;
        private readonly int? customerId;
        private readonly int? bookingId;
        private MySqlConnection conn;
        private DataTable dtRooms;

        public BookingForm(int empId, int? custId = null, int? bookId = null)
        {
            employeeId = empId;
            customerId = custId;
            bookingId = bookId;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadRooms();
            if (bookingId.HasValue)
            {
                LoadBooking();
                this.Text = "Edit Booking";
            }
            else
            {
                this.Text = "New Booking";
            }
        }

        private ComboBox cmbRoomType;
        private ComboBox cmbRoom;
        private DateTimePicker dtpCheckIn;
        private DateTimePicker dtpCheckOut;
        private ComboBox cmbCustomer;
        private TextBox txtTotalAmount;
        private ComboBox cmbPaymentStatus;
        private Button btnSelectCustomer;

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Create controls
            Label lblRoomType = new Label
            {
                Text = "Room Type:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true,
                BackColor = System.Drawing.Color.LightGray
            };

            cmbRoomType = new ComboBox
            {
                Location = new System.Drawing.Point(150, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblRoom = new Label
            {
                Text = "Room:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            cmbRoom = new ComboBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblCheckIn = new Label
            {
                Text = "Check In:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            dtpCheckIn = new DateTimePicker
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                Value = DateTime.Today.AddHours(14) // Default check-in time: 2 PM
            };

            Label lblCheckOut = new Label
            {
                Text = "Check Out:",
                Location = new System.Drawing.Point(20, 140),
                AutoSize = true
            };

            dtpCheckOut = new DateTimePicker
            {
                Location = new System.Drawing.Point(120, 140),
                Width = 200,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                Value = DateTime.Today.AddDays(1).AddHours(12) // Default check-out time: 12 PM next day
            };

            Label lblCustomer = new Label
            {
                Text = "Customer:",
                Location = new System.Drawing.Point(20, 180),
                AutoSize = true
            };

            cmbCustomer = new ComboBox
            {
                Location = new System.Drawing.Point(120, 180),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnSelectCustomer = new Button
            {
                Text = "...",
                Location = new System.Drawing.Point(330, 180),
                Width = 30
            };

            Label lblTotalAmount = new Label
            {
                Text = "Total Amount:",
                Location = new System.Drawing.Point(20, 220),
                AutoSize = true
            };

            txtTotalAmount = new TextBox
            {
                Location = new System.Drawing.Point(120, 220),
                Width = 200,
                ReadOnly = true
            };

            Label lblPaymentStatus = new Label
            {
                Text = "Payment:",
                Location = new System.Drawing.Point(20, 260),
                AutoSize = true
            };

            cmbPaymentStatus = new ComboBox
            {
                Location = new System.Drawing.Point(120, 260),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPaymentStatus.Items.AddRange(new object[] { "Pending", "Paid" });
            cmbPaymentStatus.SelectedIndex = 0;

            Button btnSave = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(150, 350),
                Width = 100
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(270, 350),
                Width = 100
            };

            // Add event handlers
            btnSave.Click += BtnSave_Click;
            cmbRoomType.SelectedIndexChanged += CmbRoomType_SelectedIndexChanged;
            btnSelectCustomer.Click += BtnSelectCustomer_Click;
            dtpCheckIn.ValueChanged += DatePicker_ValueChanged;
            dtpCheckOut.ValueChanged += DatePicker_ValueChanged;
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblRoomType, cmbRoomType,
                lblRoom, cmbRoom,
                lblCheckIn, dtpCheckIn,
                lblCheckOut, dtpCheckOut,
                lblCustomer, cmbCustomer, btnSelectCustomer,
                lblTotalAmount, txtTotalAmount,
                lblPaymentStatus, cmbPaymentStatus,
                btnSave, btnCancel
            });

            // Load room types
            LoadRoomTypes();
            LoadCustomers();
        }

        private void LoadRoomTypes()
        {
            try
            {
                string query = "SELECT id, name FROM room_types ORDER BY name";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    DatabaseConnection.Instance.OpenConnection();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        cmbRoomType.DisplayMember = "name";
                        cmbRoomType.ValueMember = "id";
                        cmbRoomType.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room types: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }

        private void LoadRooms()
        {
            if (cmbRoomType.SelectedValue == null) return;

            try
            {
                string query = @"
                    SELECT r.id, r.room_number, r.status, rt.price
                    FROM rooms r
                    JOIN room_types rt ON r.room_type_id = rt.id
                    WHERE r.room_type_id = @typeId
                    ORDER BY r.room_number";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@typeId", cmbRoomType.SelectedValue);
                    DatabaseConnection.Instance.OpenConnection();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        dtRooms = new DataTable();
                        dtRooms.Load(reader);
                        cmbRoom.DisplayMember = "room_number";
                        cmbRoom.ValueMember = "id";
                        cmbRoom.DataSource = dtRooms;

                        // Calculate total amount
                        if (cmbRoom.SelectedIndex >= 0)
                        {
                            decimal price = Convert.ToDecimal(dtRooms.Rows[cmbRoom.SelectedIndex]["price"]);
                            int days = (int)(dtpCheckOut.Value - dtpCheckIn.Value).TotalDays;
                            if (days < 1) days = 1;
                            txtTotalAmount.Text = (price * days).ToString("N0");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }

        private void LoadCustomers()
        {
            try
            {
                string query = "SELECT id, name, phone FROM customers ORDER BY name";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    DatabaseConnection.Instance.OpenConnection();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        cmbCustomer.DisplayMember = "name";
                        cmbCustomer.ValueMember = "id";
                        cmbCustomer.DataSource = dt;

                        if (customerId.HasValue)
                        {
                            cmbCustomer.SelectedValue = customerId;
                            cmbCustomer.Enabled = false;
                            btnSelectCustomer.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }

        private void LoadBooking()
        {
            try
            {
                string query = @"
                    SELECT b.*, r.room_type_id 
                    FROM bookings b
                    JOIN rooms r ON b.room_id = r.id
                    WHERE b.id = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", bookingId);
                    DatabaseConnection.Instance.OpenConnection();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cmbRoomType.SelectedValue = reader["room_type_id"];
                            cmbRoom.SelectedValue = reader["room_id"];
                            dtpCheckIn.Value = Convert.ToDateTime(reader["check_in"]);
                            dtpCheckOut.Value = Convert.ToDateTime(reader["check_out"]);
                            cmbCustomer.SelectedValue = reader["customer_id"];
                            txtTotalAmount.Text = Convert.ToDecimal(reader["total_amount"]).ToString("N0");
                            cmbPaymentStatus.SelectedItem = reader["payment_status"].ToString();

                            // Disable editing if booking is not in Reserved status
                            string status = reader["status"].ToString();
                            if (status != "Reserved")
                            {
                                MessageBox.Show("You can only edit bookings in Reserved status.", "Information",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading booking: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }

        private void CmbRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRooms();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            // Ensure check-out is after check-in
            if (dtpCheckOut.Value <= dtpCheckIn.Value)
            {
                dtpCheckOut.Value = dtpCheckIn.Value.AddDays(1);
            }

            // Recalculate total amount
            if (cmbRoom.SelectedIndex >= 0 && dtRooms != null)
            {
                decimal price = Convert.ToDecimal(dtRooms.Rows[cmbRoom.SelectedIndex]["price"]);
                int days = (int)(dtpCheckOut.Value - dtpCheckIn.Value).TotalDays;
                if (days < 1) days = 1;
                txtTotalAmount.Text = (price * days).ToString("N0");
            }
        }

        private void BtnSelectCustomer_Click(object sender, EventArgs e)
        {
            var form = new CustomerListForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbRoom.SelectedValue == null || cmbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select room and customer.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseConnection.Instance.OpenConnection();

                // Kiểm tra employee_id có tồn tại không
                string checkEmployeeQuery = "SELECT COUNT(*) FROM employees WHERE id = @employeeId";
                using (MySqlCommand checkEmployeeCmd = new MySqlCommand(checkEmployeeQuery, conn))
                {
                    checkEmployeeCmd.Parameters.AddWithValue("@employeeId", employeeId);
                    int employeeExists = Convert.ToInt32(checkEmployeeCmd.ExecuteScalar());
                    if (employeeExists == 0)
                    {
                        MessageBox.Show("Invalid employee ID. Please log in again.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Restart();
                        return;
                    }
                }

                // Check if room is available for the selected dates
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM bookings 
                    WHERE room_id = @roomId 
                    AND status IN ('Reserved', 'Checked-in')
                    AND ((check_in BETWEEN @checkIn AND @checkOut)
                    OR (check_out BETWEEN @checkIn AND @checkOut)
                    OR (@checkIn BETWEEN check_in AND check_out))
                    AND id != @id";

                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@roomId", cmbRoom.SelectedValue);
                    checkCmd.Parameters.AddWithValue("@checkIn", dtpCheckIn.Value);
                    checkCmd.Parameters.AddWithValue("@checkOut", dtpCheckOut.Value);
                    checkCmd.Parameters.AddWithValue("@id", bookingId ?? 0);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Room is not available for the selected dates.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string query;
                if (bookingId.HasValue)
                {
                    query = @"
                        UPDATE bookings 
                        SET room_id = @roomId,
                            customer_id = @customerId,
                            check_in = @checkIn,
                            check_out = @checkOut,
                            total_amount = @totalAmount,
                            payment_status = @paymentStatus
                        WHERE id = @id";
                }
                else
                {
                    query = @"
                        INSERT INTO bookings 
                        (room_id, customer_id, employee_id, check_in, check_out, 
                         status, total_amount, payment_status)
                        VALUES 
                        (@roomId, @customerId, @employeeId, @checkIn, @checkOut,
                         'Reserved', @totalAmount, @paymentStatus)";
                }

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@roomId", cmbRoom.SelectedValue);
                    cmd.Parameters.AddWithValue("@customerId", cmbCustomer.SelectedValue);
                    cmd.Parameters.AddWithValue("@employeeId", employeeId);
                    cmd.Parameters.AddWithValue("@checkIn", dtpCheckIn.Value);
                    cmd.Parameters.AddWithValue("@checkOut", dtpCheckOut.Value);
                    cmd.Parameters.AddWithValue("@totalAmount", decimal.Parse(txtTotalAmount.Text));
                    cmd.Parameters.AddWithValue("@paymentStatus", cmbPaymentStatus.SelectedItem.ToString());

                    if (bookingId.HasValue)
                        cmd.Parameters.AddWithValue("@id", bookingId.Value);

                    cmd.ExecuteNonQuery();
                }

                // Update room status
                string updateRoomQuery = @"
                    UPDATE rooms 
                    SET status = 'Booked'
                    WHERE id = @roomId";

                using (MySqlCommand updateRoomCmd = new MySqlCommand(updateRoomQuery, conn))
                {
                    updateRoomCmd.Parameters.AddWithValue("@roomId", cmbRoom.SelectedValue);
                    updateRoomCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Booking saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving booking: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }
    }
}
