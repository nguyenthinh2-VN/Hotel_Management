using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class CheckInForm : Form
    {
        private readonly int employeeId;
        private readonly int? bookingId;
        private MySqlConnection conn;

        public CheckInForm(int empId, int? bookId = null)
        {
            employeeId = empId;
            bookingId = bookId;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            if (bookingId.HasValue)
            {
                LoadBooking();
            }
        }

        private TextBox txtBookingId;
        private TextBox txtRoom;
        private TextBox txtCustomer;
        private TextBox txtCheckIn;
        private TextBox txtCheckOut;
        private TextBox txtAmount;
        private ComboBox cmbPaymentStatus;

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(450, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Check In";

            // Create controls
            Label lblBookingId = new Label
            {
                Text = "Booking ID:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true,
                BackColor = System.Drawing.Color.LightGray
            };

            txtBookingId = new TextBox
            {
                Location = new System.Drawing.Point(150, 20),
                Width = 250,
                ReadOnly = true
            };

            Label lblRoom = new Label
            {
                Text = "Room:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            txtRoom = new TextBox
            {
                Location = new System.Drawing.Point(120, 60),
                Width = 200,
                ReadOnly = true
            };

            Label lblCustomer = new Label
            {
                Text = "Customer:",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true
            };

            txtCustomer = new TextBox
            {
                Location = new System.Drawing.Point(120, 100),
                Width = 200,
                ReadOnly = true
            };

            Label lblCheckIn = new Label
            {
                Text = "Check In:",
                Location = new System.Drawing.Point(20, 140),
                AutoSize = true
            };

            txtCheckIn = new TextBox
            {
                Location = new System.Drawing.Point(120, 140),
                Width = 200,
                ReadOnly = true
            };

            Label lblCheckOut = new Label
            {
                Text = "Check Out:",
                Location = new System.Drawing.Point(20, 180),
                AutoSize = true
            };

            txtCheckOut = new TextBox
            {
                Location = new System.Drawing.Point(120, 180),
                Width = 200,
                ReadOnly = true
            };

            Label lblAmount = new Label
            {
                Text = "Amount:",
                Location = new System.Drawing.Point(20, 220),
                AutoSize = true
            };

            txtAmount = new TextBox
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

            Button btnCheckIn = new Button
            {
                Text = "Check In",
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
            btnCheckIn.Click += BtnCheckIn_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblBookingId, txtBookingId,
                lblRoom, txtRoom,
                lblCustomer, txtCustomer,
                lblCheckIn, txtCheckIn,
                lblCheckOut, txtCheckOut,
                lblAmount, txtAmount,
                lblPaymentStatus, cmbPaymentStatus,
                btnCheckIn, btnCancel
            });
        }

        private void LoadBooking()
        {
            try
            {
                string query = @"
                    SELECT b.*, 
                           r.room_number,
                           c.name as customer_name
                    FROM bookings b
                    JOIN rooms r ON b.room_id = r.id
                    JOIN customers c ON b.customer_id = c.id
                    WHERE b.id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", bookingId);
                DatabaseConnection.Instance.OpenConnection();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string status = reader["status"].ToString();
                        if (status != "Reserved")
                        {
                            MessageBox.Show("This booking cannot be checked in.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                            return;
                        }

                        txtBookingId.Text = reader["id"].ToString();
                        txtRoom.Text = reader["room_number"].ToString();
                        txtCustomer.Text = reader["customer_name"].ToString();
                        txtCheckIn.Text = Convert.ToDateTime(reader["check_in"]).ToString("dd/MM/yyyy HH:mm");
                        txtCheckOut.Text = Convert.ToDateTime(reader["check_out"]).ToString("dd/MM/yyyy HH:mm");
                        txtAmount.Text = Convert.ToDecimal(reader["total_amount"]).ToString("N0");
                        cmbPaymentStatus.SelectedItem = reader["payment_status"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading booking: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {
            if (cmbPaymentStatus.SelectedItem.ToString() == "Pending")
            {
                if (MessageBox.Show("Payment is still pending. Do you want to proceed with check-in?",
                    "Confirm Check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                string query = @"
                    UPDATE bookings 
                    SET status = 'Checked-in',
                        payment_status = @paymentStatus
                    WHERE id = @id;

                    UPDATE rooms r
                    JOIN bookings b ON r.id = b.room_id
                    SET r.status = 'Occupied'
                    WHERE b.id = @id;";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", bookingId);
                cmd.Parameters.AddWithValue("@paymentStatus", cmbPaymentStatus.SelectedItem.ToString());
                DatabaseConnection.Instance.OpenConnection();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Check-in successful!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during check-in: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
