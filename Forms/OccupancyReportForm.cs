using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HotelManagement.Database;

namespace HotelManagement.Forms
{
    public partial class OccupancyReportForm : Form
    {
        private MySqlConnection conn;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private DataGridView dgvReport;
        private Label lblAverage;

        public OccupancyReportForm()
        {
            conn = DatabaseConnection.Instance.GetConnection();
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(800, 600);
            this.Text = "Occupancy Report";
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

            // Average label
            lblAverage = new Label
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
                lblAverage
            });
        }

        private void LoadReport()
        {
            try
            {
                string query = @"
                    WITH RECURSIVE dates AS (
                        SELECT @startDate as date
                        UNION ALL
                        SELECT date + INTERVAL 1 DAY
                        FROM dates
                        WHERE date < @endDate
                    ),
                    room_counts AS (
                        SELECT rt.id, rt.name, COUNT(r.id) as total_rooms
                        FROM room_types rt
                        JOIN rooms r ON r.room_type_id = rt.id
                        GROUP BY rt.id, rt.name
                    ),
                    occupied_rooms AS (
                        SELECT 
                            DATE(d.date) as date,
                            r.room_type_id,
                            COUNT(DISTINCT b.room_id) as occupied
                        FROM dates d
                        CROSS JOIN rooms r
                        LEFT JOIN bookings b ON b.room_id = r.id
                            AND d.date BETWEEN DATE(b.check_in) AND DATE(b.check_out)
                            AND b.status IN ('Reserved', 'Checked-in')
                        GROUP BY DATE(d.date), r.room_type_id
                    )
                    SELECT 
                        d.date,
                        rc.name as room_type,
                        rc.total_rooms,
                        COALESCE(o.occupied, 0) as occupied_rooms,
                        ROUND(COALESCE(o.occupied, 0) / rc.total_rooms * 100, 2) as occupancy_rate
                    FROM dates d
                    CROSS JOIN room_counts rc
                    LEFT JOIN occupied_rooms o ON o.date = d.date 
                        AND o.room_type_id = rc.id
                    ORDER BY d.date DESC, rc.name;";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                cmd.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date);
                DatabaseConnection.Instance.OpenConnection();

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                dgvReport.DataSource = dt;

                // Calculate average occupancy
                double totalRate = 0;
                int count = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalRate += Convert.ToDouble(row["occupancy_rate"]);
                    count++;
                }

                double averageRate = count > 0 ? totalRate / count : 0;
                lblAverage.Text = $"Average Occupancy Rate: {averageRate:F2}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}