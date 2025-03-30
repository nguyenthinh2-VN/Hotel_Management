using System;
using System.Drawing;
using System.Windows.Forms;

namespace HotelManagement.Forms
{
    public partial class MainForm : Form
    {
        private readonly int employeeId;
        private readonly string employeeName;
        private readonly string role;

        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem roomsMenu;
        private ToolStripMenuItem bookingsMenu;
        private ToolStripMenuItem customersMenu;
        private ToolStripMenuItem reportsMenu;
        private ToolStripMenuItem adminMenu;
        private ToolStripMenuItem homeMenu;
        private Panel homePanel;

        public MainForm(int empId, string empName, string empRole)
        {
            employeeId = empId;
            employeeName = empName;
            role = empRole;
            InitializeComponent();
            InitializeMenus();
            InitializeHomePanel();
            this.IsMdiContainer = true;
            this.WindowState = FormWindowState.Maximized;
            this.Text = $"Hotel Management System - {employeeName} ({role})";
            ShowHomePanel();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip = new System.Windows.Forms.MenuStrip();
            homeMenu = new System.Windows.Forms.ToolStripMenuItem();
            fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            roomsMenu = new System.Windows.Forms.ToolStripMenuItem();
            bookingsMenu = new System.Windows.Forms.ToolStripMenuItem();
            customersMenu = new System.Windows.Forms.ToolStripMenuItem();
            reportsMenu = new System.Windows.Forms.ToolStripMenuItem();
            adminMenu = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { homeMenu, fileMenu, roomsMenu, bookingsMenu, customersMenu, reportsMenu, adminMenu });
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(800, 24);
            menuStrip.TabIndex = 0;
            // 
            // homeMenu
            // 
            homeMenu.Name = "homeMenu";
            homeMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // fileMenu
            // 
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // roomsMenu
            // 
            roomsMenu.Name = "roomsMenu";
            roomsMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // bookingsMenu
            // 
            bookingsMenu.Name = "bookingsMenu";
            bookingsMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // customersMenu
            // 
            customersMenu.Name = "customersMenu";
            customersMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // reportsMenu
            // 
            reportsMenu.Name = "reportsMenu";
            reportsMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // adminMenu
            // 
            adminMenu.Name = "adminMenu";
            adminMenu.Size = new System.Drawing.Size(12, 20);
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(800, 600);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeMenus()
        {
            // Home Menu
            homeMenu.Text = "Home";
            homeMenu.Click += (s, e) => ShowHomePanel();

            // File Menu
            fileMenu.Text = "File";
            var logoutItem = new ToolStripMenuItem("Logout", null, (s, e) => Application.Restart());
            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { logoutItem, exitItem });

            // Rooms Menu
            roomsMenu.Text = "Rooms";
            var roomListItem = new ToolStripMenuItem("Room List", null, (s, e) => OpenForm(new RoomListForm()));
            var roomTypeItem = new ToolStripMenuItem("Room Types", null, (s, e) => OpenForm(new RoomTypeForm()));
            roomsMenu.DropDownItems.AddRange(new ToolStripItem[] { roomListItem, roomTypeItem });

            // Bookings Menu
            bookingsMenu.Text = "Bookings";
            var bookingListItem = new ToolStripMenuItem("Booking List", null, (s, e) => OpenForm(new BookingListForm(employeeId)));
            var newBookingItem = new ToolStripMenuItem("New Booking", null, (s, e) => OpenForm(new BookingForm(employeeId)));
            var checkInItem = new ToolStripMenuItem("Check-in", null, (s, e) => OpenForm(new CheckInForm(employeeId)));
            var checkOutItem = new ToolStripMenuItem("Check-out", null, (s, e) => OpenForm(new CheckOutForm(employeeId)));
            bookingsMenu.DropDownItems.AddRange(new ToolStripItem[] { bookingListItem, newBookingItem, checkInItem, checkOutItem });

            // Customers Menu
            customersMenu.Text = "Customers";
            var customerListItem = new ToolStripMenuItem("Customer List", null, (s, e) => OpenForm(new CustomerListForm()));
            customersMenu.DropDownItems.AddRange(new ToolStripItem[] { customerListItem });

            // Reports Menu
            reportsMenu.Text = "Reports";
            var revenueReportItem = new ToolStripMenuItem("Revenue Report", null, (s, e) => OpenForm(new RevenueReportForm()));
            var occupancyReportItem = new ToolStripMenuItem("Occupancy Report", null, (s, e) => OpenForm(new OccupancyReportForm()));
            reportsMenu.DropDownItems.AddRange(new ToolStripItem[] { revenueReportItem, occupancyReportItem });

            // Admin Menu
            adminMenu.Text = "Administration";
            var employeeListItem = new ToolStripMenuItem("Employee Management", null, (s, e) => OpenForm(new EmployeeListForm()));
            adminMenu.DropDownItems.AddRange(new ToolStripItem[] { employeeListItem });

            // Set visibility based on role
            adminMenu.Visible = role == "Admin";
            reportsMenu.Visible = role == "Admin" || role == "Receptionist";
        }

        private void ShowHomePanel()
        {
            // Close all child forms
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }

            // Show home panel
            if (!this.Controls.Contains(homePanel))
            {
                this.Controls.Add(homePanel);
            }
            homePanel.Show();
            homePanel.BringToFront();
        }

        private void InitializeHomePanel()
        {
            homePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Welcome section
            Panel welcomeSection = new Panel
            {
                Width = 800,
                Height = 100,
                BackColor = Color.FromArgb(0, 122, 204),
                Dock = DockStyle.Top
            };

            Label lblWelcome = new Label
            {
                Text = $"Welcome, {employeeName}!",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 30)
            };
            welcomeSection.Controls.Add(lblWelcome);

            // Quick access buttons panel
            FlowLayoutPanel buttonsPanel = new FlowLayoutPanel
            {
                Width = 800,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
                Location = new Point(0, 120)
            };

            // Add quick access buttons
            buttonsPanel.Controls.Add(CreateQuickAccessButton("New Booking", "Create a new booking", () => OpenForm(new BookingForm(employeeId))));
            buttonsPanel.Controls.Add(CreateQuickAccessButton("Check In", "Check in a guest", () => OpenForm(new CheckInForm(employeeId))));
            buttonsPanel.Controls.Add(CreateQuickAccessButton("Check Out", "Check out a guest", () => OpenForm(new CheckOutForm(employeeId))));
            buttonsPanel.Controls.Add(CreateQuickAccessButton("Room List", "View all rooms", () => OpenForm(new RoomListForm())));
            buttonsPanel.Controls.Add(CreateQuickAccessButton("Customers", "Manage customers", () => OpenForm(new CustomerListForm())));

            if (role == "Admin" || role == "Receptionist")
            {
                buttonsPanel.Controls.Add(CreateQuickAccessButton("Revenue Report", "View revenue report", () => OpenForm(new RevenueReportForm())));
                buttonsPanel.Controls.Add(CreateQuickAccessButton("Occupancy Report", "View occupancy report", () => OpenForm(new OccupancyReportForm())));
            }

            if (role == "Admin")
            {
                buttonsPanel.Controls.Add(CreateQuickAccessButton("Employees", "Manage employees", () => OpenForm(new EmployeeListForm())));
            }

            // Stats panel
            Panel statsPanel = new Panel
            {
                Width = 800,
                Height = 150,
                Location = new Point(0, buttonsPanel.Bottom + 20),
                Padding = new Padding(20)
            };

            Label lblStats = new Label
            {
                Text = "Quick Statistics",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(20, 0),
                AutoSize = true
            };
            statsPanel.Controls.Add(lblStats);

            // Add panels to home panel
            homePanel.Controls.AddRange(new Control[] { welcomeSection, buttonsPanel, statsPanel });
        }

        private Panel CreateQuickAccessButton(string text, string description, Action onClick)
        {
            Panel buttonPanel = new Panel
            {
                Width = 180,
                Height = 120,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            Label lblTitle = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            Label lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            buttonPanel.Controls.AddRange(new Control[] { lblTitle, lblDescription });
            buttonPanel.Click += (s, e) => onClick();
            lblTitle.Click += (s, e) => onClick();
            lblDescription.Click += (s, e) => onClick();

            return buttonPanel;
        }

        private void OpenForm(Form form)
        {
            // Hide home panel
            if (this.Controls.Contains(homePanel))
            {
                homePanel.Hide();
                this.Controls.Remove(homePanel);
            }

            // Close any existing form of the same type
            foreach (Form child in this.MdiChildren)
            {
                if (child.GetType() == form.GetType())
                {
                    child.Activate();
                    return;
                }
            }

            form.MdiParent = this;
            form.FormClosed += (s, e) => 
            {
                if (this.MdiChildren.Length == 0)
                {
                    ShowHomePanel();
                }
            };
            form.Show();
        }
    }
}
