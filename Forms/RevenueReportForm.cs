using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class RevenueReportForm : Form
    {
        private MySqlConnection conn;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private DataGridView dgvReport;
        private Label lblTotal;

        public RevenueReportForm()
        {
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(800, 600);
            this.Text = "Revenue Report";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Date range controls
            Label lblStartDate = new Label
            {
                Text = "Start Date:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(100, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-30)
            };

            Label lblEndDate = new Label
            {
                Text = "End Date:",
                Location = new System.Drawing.Point(250, 20),
                AutoSize = true
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(330, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(550, 20),
                Width = 80
            };
            btnRefresh.Click += (s, e) => LoadReport();

            // Grid view
            dgvReport = new DataGridView
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(750, 450),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Total label
            lblTotal = new Label
            {
                Location = new System.Drawing.Point(20, 520),
                AutoSize = true,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10, System.Drawing.FontStyle.Bold)
            };

            // Add controls
            this.Controls.AddRange(new Control[]
            {
                lblStartDate, dtpStartDate,
                lblEndDate, dtpEndDate,
                btnRefresh,
                dgvReport,
                lblTotal
            });
        }

        private void LoadReport()
        {
            try
            {
                string query = @"
                    SELECT 
                        DATE(b.check_out) as date,
                        rt.name as room_type,
                        COUNT(*) as bookings,
                        SUM(b.total_amount) as revenue
                    FROM bookings b
                    JOIN rooms r ON b.room_id = r.id
                    JOIN room_types rt ON r.room_type_id = rt.id
                    WHERE b.payment_status = 'Paid'
                    AND b.check_out BETWEEN @startDate AND @endDate
                    GROUP BY DATE(b.check_out), rt.name
                    ORDER BY date DESC, room_type";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                cmd.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));
                DatabaseConnection.Instance.OpenConnection();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                dgvReport.DataSource = dt;

                // Calculate total
                decimal total = 0;
                foreach (DataRow row in dt.Rows)
                {
                    total += Convert.ToDecimal(row["revenue"]);
                }

                lblTotal.Text = $"Total Revenue: {total:N0} VND";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
