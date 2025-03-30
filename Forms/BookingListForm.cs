using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class BookingListForm : Form
    {
        private readonly int? customerId;
        private readonly string customerName;
        private readonly int employeeId;
        private readonly string role;
        private DataTable dataTable = new DataTable();
        private MySqlConnection conn;

        public BookingListForm(int? customerId = null, string customerName = null, int employeeId = 0, string role = "Staff")
        {
            this.customerId = customerId;
            this.customerName = customerName;
            this.employeeId = employeeId;
            this.role = role;
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadBookings();
        }

        private DataGridView dgvBookings;
        private ComboBox cmbStatus;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private TextBox txtSearch;

        private void InitializeComponent()
        {
            this.Text = customerId.HasValue ? $"Bookings for {customerName}" : "All Bookings";
            this.Size = new System.Drawing.Size(1200, 600);

            // Create controls
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            Label lblStatus = new Label
            {
                Text = "Status:",
                Location = new System.Drawing.Point(10, 15),
                AutoSize = true
            };

            cmbStatus = new ComboBox
            {
                Location = new System.Drawing.Point(60, 12),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new object[] { "All", "Reserved", "Checked-in", "Checked-out", "Cancelled" });
            cmbStatus.SelectedIndex = 0;

            Label lblDateFrom = new Label
            {
                Text = "From:",
                Location = new System.Drawing.Point(190, 15),
                AutoSize = true
            };

            dtpFrom = new DateTimePicker
            {
                Location = new System.Drawing.Point(240, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, 1, 1),
                ShowCheckBox = true,
                Checked = false
            };

            Label lblDateTo = new Label
            {
                Text = "To:",
                Location = new System.Drawing.Point(370, 15),
                AutoSize = true
            };

            dtpTo = new DateTimePicker
            {
                Location = new System.Drawing.Point(400, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(DateTime.Now.Year, 12, 31),
                ShowCheckBox = true,
                Checked = false
            };

            Button btnClearDate = new Button
            {
                Text = "Clear Date",
                Location = new System.Drawing.Point(530, 10),
                Width = 80
            };
            btnClearDate.Click += (s, e) => {
                dtpFrom.Checked = false;
                dtpTo.Checked = false;
                LoadBookings();
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new System.Drawing.Point(620, 15),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(670, 12),
                Width = 200
            };

            Button btnSearch = new Button
            {
                Text = "Search",
                Location = new System.Drawing.Point(880, 10),
                Width = 80
            };

            dgvBookings = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            Button btnAdd = new Button
            {
                Text = "New Booking",
                Width = 100,
                Location = new System.Drawing.Point(10, 10)
            };

            Button btnEdit = new Button
            {
                Text = "Edit",
                Width = 100,
                Location = new System.Drawing.Point(120, 10)
            };

            Button btnCancel = new Button
            {
                Text = "Cancel Booking",
                Width = 100,
                Location = new System.Drawing.Point(230, 10)
            };

            Button btnCheckIn = new Button
            {
                Text = "Check In",
                Width = 100,
                Location = new System.Drawing.Point(340, 10)
            };

            Button btnCheckOut = new Button
            {
                Text = "Check Out",
                Width = 100,
                Location = new System.Drawing.Point(450, 10)
            };

            // Add event handlers
            btnSearch.Click += BtnSearch_Click;
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnCancel.Click += BtnCancel_Click;
            btnCheckIn.Click += BtnCheckIn_Click;
            btnCheckOut.Click += BtnCheckOut_Click;
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnSearch_Click(s, e); };

            // Add controls to panels
            topPanel.Controls.AddRange(new Control[] { 
                lblStatus, cmbStatus,
                lblDateFrom, dtpFrom,
                lblDateTo, dtpTo,
                btnClearDate,
                lblSearch, txtSearch,
                btnSearch
            });

            buttonPanel.Controls.AddRange(new Control[] { 
                btnAdd, btnEdit, btnCancel,
                btnCheckIn, btnCheckOut
            });

            // Add controls to form
            this.Controls.Add(dgvBookings);
            this.Controls.Add(topPanel);
            this.Controls.Add(buttonPanel);
        }

        private void LoadBookings()
        {
            try
            {
                string whereClause = "WHERE 1=1";
                // Chỉ lọc theo customerId khi form được mở từ CustomerListForm
                if (this.Text.StartsWith("Bookings for") && customerId.HasValue)
                    whereClause += " AND b.customer_id = @customerId";
                if (cmbStatus.SelectedItem.ToString() != "All")
                    whereClause += " AND b.status = @status";
                if (txtSearch.Text.Trim() != "")
                    whereClause += @" AND (r.room_number LIKE @search 
                                        OR c.name LIKE @search 
                                        OR c.phone LIKE @search)";

                // Only add date filter if dates are selected
                if (dtpFrom.Checked && dtpTo.Checked)
                {
                    whereClause += " AND b.check_in BETWEEN @dateFrom AND @dateTo";
                }

                string query = $@"
                    SELECT b.id,
                           r.room_number,
                           c.name as customer_name,
                           c.phone as customer_phone,
                           b.check_in,
                           b.check_out,
                           b.status,
                           b.total_amount,
                           b.payment_status,
                           e.name as employee_name
                    FROM bookings b
                    JOIN rooms r ON b.room_id = r.id
                    JOIN customers c ON b.customer_id = c.id
                    JOIN employees e ON b.employee_id = e.id
                    {whereClause}
                    ORDER BY 
                        CASE b.status 
                            WHEN 'Reserved' THEN 1
                            WHEN 'Checked-in' THEN 2
                            WHEN 'Checked-out' THEN 3
                            ELSE 4
                        END,
                        b.check_in DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (this.Text.StartsWith("Bookings for") && customerId.HasValue)
                        cmd.Parameters.AddWithValue("@customerId", customerId.Value);
                    if (cmbStatus.SelectedItem.ToString() != "All")
                        cmd.Parameters.AddWithValue("@status", cmbStatus.SelectedItem.ToString());
                    if (txtSearch.Text.Trim() != "")
                        cmd.Parameters.AddWithValue("@search", $"%{txtSearch.Text.Trim()}%");
                    if (dtpFrom.Checked && dtpTo.Checked)
                    {
                        cmd.Parameters.AddWithValue("@dateFrom", dtpFrom.Value.Date);
                        cmd.Parameters.AddWithValue("@dateTo", dtpTo.Value.Date.AddDays(1));
                    }

                    DatabaseConnection.Instance.OpenConnection();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        dataTable.Clear();
                        adapter.Fill(dataTable);
                    }
                }

                dgvBookings.DataSource = dataTable;

                // Format columns
                dgvBookings.Columns["id"].HeaderText = "ID";
                dgvBookings.Columns["room_number"].HeaderText = "Room";
                dgvBookings.Columns["customer_name"].HeaderText = "Customer";
                dgvBookings.Columns["customer_phone"].HeaderText = "Phone";
                dgvBookings.Columns["check_in"].HeaderText = "Check In";
                dgvBookings.Columns["check_out"].HeaderText = "Check Out";
                dgvBookings.Columns["status"].HeaderText = "Status";
                dgvBookings.Columns["total_amount"].HeaderText = "Total Amount";
                dgvBookings.Columns["payment_status"].HeaderText = "Payment";
                dgvBookings.Columns["employee_name"].HeaderText = "Employee";

                // Format date columns
                dgvBookings.Columns["check_in"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvBookings.Columns["check_out"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                // Format number columns
                dgvBookings.Columns["total_amount"].DefaultCellStyle.Format = "N0";
                dgvBookings.Columns["total_amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // Color coding for status
                foreach (DataGridViewRow row in dgvBookings.Rows)
                {
                    string status = row.Cells["status"].Value.ToString();
                    string payment = row.Cells["payment_status"].Value.ToString();

                    if (status == "Reserved")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
                    else if (status == "Checked-in")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                    else if (status == "Checked-out" && payment == "Paid")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                    else if (status == "Checked-out" && payment == "Pending")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightPink;
                    else if (status == "Cancelled")
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DatabaseConnection.Instance.CloseConnection();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadBookings();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new BookingForm(employeeId, customerId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadBookings();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["id"].Value);
                using (var form = new BookingForm(employeeId, customerId, id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadBookings();
                    }
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["id"].Value);
                string status = dgvBookings.SelectedRows[0].Cells["status"].Value.ToString();

                if (status == "Checked-out" || status == "Cancelled")
                {
                    MessageBox.Show("Cannot cancel a completed or already cancelled booking.", 
                        "Cancel Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to cancel this booking?", "Confirm Cancel",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string query = @"
                            UPDATE bookings 
                            SET status = 'Cancelled'
                            WHERE id = @id;
                            
                            UPDATE rooms r
                            JOIN bookings b ON r.id = b.room_id
                            SET r.status = 'Available'
                            WHERE b.id = @id;";

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        DatabaseConnection.Instance.OpenConnection();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Booking cancelled successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBookings();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error cancelling booking: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["id"].Value);
                using (var form = new CheckInForm(employeeId, id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadBookings();
                    }
                }
            }
        }

        private void BtnCheckOut_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["id"].Value);
                using (var form = new CheckOutForm(employeeId, id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadBookings();
                    }
                }
            }
        }
    }
}
