using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RestaurantBookingNoDB
{
    public class MainForm : Form
    {
        // ========== МОДЕЛИ ДАННЫХ ==========
        public class Table
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public int Seats { get; set; }
            public string Location { get; set; }
            public bool IsAvailable { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
        }

        public class Booking
        {
            public int Id { get; set; }
            public DateTime DateTime { get; set; }
            public int Guests { get; set; }
            public string Notes { get; set; }
            public string Status { get; set; }
            public int TableId { get; set; }
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public int TableNumber { get; set; }
        }

        // ========== ДАННЫЕ В ПАМЯТИ ==========
        private List<Table> tables = new List<Table>();
        private List<Customer> customers = new List<Customer>();
        private List<Booking> bookings = new List<Booking>();
        private int nextCustomerId = 1;
        private int nextBookingId = 1;

        // ========== ЭЛЕМЕНТЫ ИНТЕРФЕЙСА ==========
        private TabControl tabControl;
        private DataGridView dgvTables, dgvCustomers, dgvBookings;
        private Panel hallPanel;
        private TextBox txtSearch;
        private Label lblCustomerInfo;
        private int selectedCustomerId = -1;
        private ComboBox cmbLocation;
        private DateTimePicker dtpDate;

        public MainForm()
        {
            InitializeData();
            InitializeComponent();
            UpdateAllViews();
        }

        private void InitializeData()
        {
            // Добавляем столики
            tables.AddRange(new[]
            {
                new Table { Id = 1, Number = 1, Seats = 2, Location = "Зал", IsAvailable = true },
                new Table { Id = 2, Number = 2, Seats = 4, Location = "Зал", IsAvailable = true },
                new Table { Id = 3, Number = 3, Seats = 2, Location = "Терраса", IsAvailable = true },
                new Table { Id = 4, Number = 4, Seats = 6, Location = "VIP", IsAvailable = true },
                new Table { Id = 5, Number = 5, Seats = 4, Location = "Зал", IsAvailable = true },
                new Table { Id = 6, Number = 6, Seats = 2, Location = "Терраса", IsAvailable = true },
                new Table { Id = 7, Number = 7, Seats = 8, Location = "VIP", IsAvailable = true },
                new Table { Id = 8, Number = 8, Seats = 4, Location = "Зал", IsAvailable = true }
            });

            // Добавляем тестовых клиентов
            customers.AddRange(new[]
            {
                new Customer { Id = nextCustomerId++, Name = "Иван Петров", Phone = "+7 (999) 123-45-67" },
                new Customer { Id = nextCustomerId++, Name = "Мария Сидорова", Phone = "+7 (999) 765-43-21" },
                new Customer { Id = nextCustomerId++, Name = "Алексей Иванов", Phone = "+7 (999) 555-12-34" }
            });

            // Добавляем тестовые бронирования
            bookings.Add(new Booking
            {
                Id = nextBookingId++,
                DateTime = DateTime.Today.AddHours(19),
                Guests = 2,
                Notes = "У окна",
                Status = "Подтверждено",
                TableId = 1,
                CustomerId = 1,
                CustomerName = "Иван Петров",
                TableNumber = 1
            });

            bookings.Add(new Booking
            {
                Id = nextBookingId++,
                DateTime = DateTime.Today.AddHours(20),
                Guests = 4,
                Notes = "День рождения",
                Status = "Подтверждено",
                TableId = 2,
                CustomerId = 2,
                CustomerName = "Мария Сидорова",
                TableNumber = 2
            });
        }

        private void InitializeComponent()
        {
            this.Text = "Бронирование столиков";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            // ===== ВКЛАДКА ПЛАНА ЗАЛА =====
            var hallTab = new TabPage("План зала");

            var hallTopPanel = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = Color.FromArgb(60, 60, 60) };

            var lblFilter = new Label { Text = "Зал:", ForeColor = Color.White, Location = new Point(10, 10), Size = new Size(40, 25) };
            cmbLocation = new ComboBox { Location = new Point(50, 8), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLocation.Items.AddRange(new[] { "Все", "Зал", "Терраса", "VIP" });
            cmbLocation.SelectedIndex = 0;
            cmbLocation.SelectedIndexChanged += (s, e) => DrawHallPlan();

            var lblDate = new Label { Text = "Дата:", ForeColor = Color.White, Location = new Point(220, 10), Size = new Size(40, 25) };
            dtpDate = new DateTimePicker { Location = new Point(260, 8), Width = 150, Format = DateTimePickerFormat.Short };
            dtpDate.ValueChanged += (s, e) => DrawHallPlan();

            var btnRefresh = new Button { Text = "Обновить", Location = new Point(430, 8), Width = 100, BackColor = Color.FromArgb(100, 100, 100), ForeColor = Color.White };
            btnRefresh.Click += (s, e) => DrawHallPlan();

            hallTopPanel.Controls.AddRange(new Control[] { lblFilter, cmbLocation, lblDate, dtpDate, btnRefresh });

            hallPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(240, 240, 240) };
            hallTab.Controls.Add(hallPanel);
            hallTab.Controls.Add(hallTopPanel);

            // ===== ВКЛАДКА СТОЛИКОВ =====
            var tablesTab = new TabPage("Столики");
            var tablesPanel = new Panel { Dock = DockStyle.Fill };

            dgvTables = new DataGridView
            {
                Location = new Point(10, 50),
                Width = 1060,
                Height = 500,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            var btnAddTable = new Button
            {
                Text = "Добавить столик",
                Location = new Point(10, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White
            };
            btnAddTable.Click += (s, e) => AddTable();

            var btnDeleteTable = new Button
            {
                Text = "Удалить столик",
                Location = new Point(170, 10),
                Size = new Size(150, 30),
                BackColor = Color.IndianRed,
                ForeColor = Color.White
            };
            btnDeleteTable.Click += (s, e) => DeleteTable();

            tablesPanel.Controls.Add(dgvTables);
            tablesPanel.Controls.Add(btnAddTable);
            tablesPanel.Controls.Add(btnDeleteTable);
            tablesTab.Controls.Add(tablesPanel);

            // ===== ВКЛАДКА КЛИЕНТОВ =====
            var customersTab = new TabPage("Клиенты");
            var custPanel = new Panel { Dock = DockStyle.Fill };

            var lblSearch = new Label { Text = "Поиск:", Location = new Point(10, 15), Size = new Size(50, 25) };
            txtSearch = new TextBox { Location = new Point(60, 12), Width = 300 };
            txtSearch.TextChanged += (s, e) => UpdateCustomersView();

            dgvCustomers = new DataGridView
            {
                Location = new Point(10, 50),
                Width = 1060,
                Height = 200,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvCustomers.SelectionChanged += (s, e) =>
            {
                if (dgvCustomers.SelectedRows.Count > 0 && dgvCustomers.SelectedRows[0].Cells[0].Value != null)
                {
                    selectedCustomerId = (int)dgvCustomers.SelectedRows[0].Cells[0].Value;
                    var name = dgvCustomers.SelectedRows[0].Cells[1].Value?.ToString() ?? "";
                    var phone = dgvCustomers.SelectedRows[0].Cells[2].Value?.ToString() ?? "";
                    lblCustomerInfo.Text = $"✓ Клиент: {name} ({phone})";
                    lblCustomerInfo.ForeColor = Color.Green;
                }
            };

            var btnAddCust = new Button
            {
                Text = "Новый клиент",
                Location = new Point(370, 10),
                Width = 120,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White
            };
            btnAddCust.Click += (s, e) => AddCustomer();

            lblCustomerInfo = new Label
            {
                Location = new Point(10, 260),
                Width = 600,
                Height = 30,
                Text = "✗ Клиент не выбран",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Red
            };

            var btnSelectCust = new Button
            {
                Text = "Выбрать клиента",
                Location = new Point(500, 10),
                Width = 120,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White
            };
            btnSelectCust.Click += (s, e) => SelectCustomer();

            custPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, btnAddCust, btnSelectCust, dgvCustomers, lblCustomerInfo
            });
            customersTab.Controls.Add(custPanel);

            // ===== ВКЛАДКА БРОНИРОВАНИЙ =====
            var bookingsTab = new TabPage("Бронирования");
            var bookingsPanel = new Panel { Dock = DockStyle.Fill };

            dgvBookings = new DataGridView
            {
                Location = new Point(10, 50),
                Width = 1060,
                Height = 500,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            var btnCancelBooking = new Button
            {
                Text = "Отменить бронь",
                Location = new Point(10, 10),
                Size = new Size(150, 30),
                BackColor = Color.IndianRed,
                ForeColor = Color.White
            };
            btnCancelBooking.Click += (s, e) => CancelBooking();

            bookingsPanel.Controls.Add(dgvBookings);
            bookingsPanel.Controls.Add(btnCancelBooking);
            bookingsTab.Controls.Add(bookingsPanel);

            tabControl.TabPages.AddRange(new[] { hallTab, tablesTab, customersTab, bookingsTab });
            this.Controls.Add(tabControl);

            // Верхняя панель с кнопкой нового бронирования
            var topPanel = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.FromArgb(230, 230, 230) };

            var btnNewBooking = new Button
            {
                Text = "Новое бронирование",
                Location = new Point(20, 10),
                Size = new Size(170, 30),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnNewBooking.Click += (s, e) => NewBooking();

            topPanel.Controls.Add(btnNewBooking);
            this.Controls.Add(topPanel);
        }

        private void UpdateAllViews()
        {
            UpdateTablesView();
            UpdateCustomersView();
            UpdateBookingsView();
            DrawHallPlan();
        }

        private void UpdateTablesView()
        {
            dgvTables.DataSource = null;
            dgvTables.DataSource = tables.Select(t => new
            {
                t.Id,
                Номер = t.Number,
                Мест = t.Seats,
                Зал = t.Location,
                Доступен = t.IsAvailable ? "Да" : "Нет"
            }).ToList();
        }

        private void UpdateCustomersView()
        {
            var filtered = customers.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filtered = filtered.Where(c =>
                    c.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                    c.Phone.Contains(txtSearch.Text));
            }
            dgvCustomers.DataSource = null;
            dgvCustomers.DataSource = filtered.Select(c => new
            {
                c.Id,
                Имя = c.Name,
                Телефон = c.Phone
            }).ToList();
        }

        private void UpdateBookingsView()
        {
            dgvBookings.DataSource = null;
            dgvBookings.DataSource = bookings.Select(b => new
            {
                b.Id,
                Клиент = b.CustomerName,
                Дата = b.DateTime.ToString("dd.MM.yyyy HH:mm"),
                Гости = b.Guests,
                Столик = b.TableNumber,
                Заметки = b.Notes,
                Статус = b.Status
            }).ToList();
        }

        private void DrawHallPlan()
        {
            hallPanel.Controls.Clear();

            var filteredTables = tables.Where(t => t.IsAvailable);
            string filter = cmbLocation.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(filter) && filter != "Все")
            {
                filteredTables = filteredTables.Where(t => t.Location == filter);
            }

            var selectedDateBookings = bookings.Where(b => b.DateTime.Date == dtpDate.Value.Date).ToList();

            int x = 50, y = 50;
            int buttonsInRow = 0;

            foreach (var table in filteredTables)
            {
                var tableBooking = selectedDateBookings.FirstOrDefault(b => b.TableId == table.Id && b.Status == "Подтверждено");
                bool isBooked = tableBooking != null;

                // Панель столика
                var tablePanel = new Panel
                {
                    Size = new Size(110, 130),
                    Location = new Point(x, y),
                    BackColor = isBooked ? Color.LightCoral : Color.LightGreen,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = table.Id,
                    Cursor = Cursors.Hand
                };

                // Номер
                var lblNumber = new Label
                {
                    Text = $"Столик {table.Number}",
                    Location = new Point(5, 5),
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    AutoSize = true
                };

                // Места
                var lblSeats = new Label
                {
                    Text = $"👤 {table.Seats} мест",
                    Location = new Point(5, 25),
                    Font = new Font("Arial", 8),
                    AutoSize = true
                };

                // Расположение
                var lblLocation = new Label
                {
                    Text = table.Location,
                    Location = new Point(5, 45),
                    Font = new Font("Arial", 8),
                    AutoSize = true
                };

                tablePanel.Controls.Add(lblNumber);
                tablePanel.Controls.Add(lblSeats);
                tablePanel.Controls.Add(lblLocation);

                if (isBooked && tableBooking != null)
                {
                    var lblCustomer = new Label
                    {
                        Text = tableBooking.CustomerName,
                        Location = new Point(5, 65),
                        Font = new Font("Arial", 8, FontStyle.Bold),
                        AutoSize = true,
                        ForeColor = Color.DarkRed
                    };
                    var lblTime = new Label
                    {
                        Text = tableBooking.DateTime.ToString("HH:mm"),
                        Location = new Point(5, 85),
                        Font = new Font("Arial", 8),
                        AutoSize = true
                    };
                    var lblGuests = new Label
                    {
                        Text = $"👥 {tableBooking.Guests} чел",
                        Location = new Point(5, 105),
                        Font = new Font("Arial", 7),
                        AutoSize = true
                    };
                    tablePanel.Controls.Add(lblCustomer);
                    tablePanel.Controls.Add(lblTime);
                    tablePanel.Controls.Add(lblGuests);
                }

                int currentTableId = table.Id;
                tablePanel.Click += (s, e) =>
                {
                    var booking = selectedDateBookings.FirstOrDefault(b => b.TableId == currentTableId);
                    if (booking != null)
                    {
                        MessageBox.Show($"Столик {table.Number} - ЗАНЯТ\n" +
                            $"Клиент: {booking.CustomerName}\n" +
                            $"Время: {booking.DateTime:HH:mm}\n" +
                            $"Гостей: {booking.Guests}\n" +
                            $"Заметки: {booking.Notes}",
                            "Информация о брони",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        var result = MessageBox.Show($"Столик {table.Number} свободен.\nСоздать бронирование?",
                            "Свободный столик",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            selectedCustomerId = -1;
                            lblCustomerInfo.Text = "✗ Клиент не выбран";
                            lblCustomerInfo.ForeColor = Color.Red;
                            tabControl.SelectedIndex = 2; // На вкладку клиентов
                        }
                    }
                };

                hallPanel.Controls.Add(tablePanel);

                x += 120;
                buttonsInRow++;

                if (buttonsInRow >= 8)
                {
                    x = 50;
                    y += 140;
                    buttonsInRow = 0;
                }
            }

            // Легенда
            var legendPanel = new Panel
            {
                Location = new Point(50, y + 20),
                Size = new Size(300, 70),
                BackColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.FixedSingle
            };

            legendPanel.Controls.Add(new Label
            {
                Text = "ЛЕГЕНДА:",
                Location = new Point(5, 5),
                Font = new Font("Arial", 9, FontStyle.Bold),
                AutoSize = true
            });

            var freeSample = new Panel { Location = new Point(10, 25), Size = new Size(20, 20), BackColor = Color.LightGreen, BorderStyle = BorderStyle.FixedSingle };
            var bookedSample = new Panel { Location = new Point(120, 25), Size = new Size(20, 20), BackColor = Color.LightCoral, BorderStyle = BorderStyle.FixedSingle };

            legendPanel.Controls.Add(freeSample);
            legendPanel.Controls.Add(new Label { Text = "- свободен", Location = new Point(35, 25), AutoSize = true });
            legendPanel.Controls.Add(bookedSample);
            legendPanel.Controls.Add(new Label { Text = "- занят", Location = new Point(145, 25), AutoSize = true });

            legendPanel.Controls.Add(new Label
            {
                Text = "👆 Нажмите на столик для просмотра/бронирования",
                Location = new Point(10, 50),
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic)
            });

            hallPanel.Controls.Add(legendPanel);
        }

        private void AddTable()
        {
            var form = new Form { Text = "Новый столик", Size = new Size(300, 250), StartPosition = FormStartPosition.CenterParent };

            var txtNumber = new TextBox { Location = new Point(120, 20), Width = 150 };
            var numSeats = new NumericUpDown { Location = new Point(120, 50), Width = 100, Minimum = 1, Maximum = 20, Value = 4 };
            var cmbLoc = new ComboBox { Location = new Point(120, 80), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLoc.Items.AddRange(new[] { "Зал", "Терраса", "VIP" });
            cmbLoc.SelectedIndex = 0;

            var btnOk = new Button { Text = "Добавить", Location = new Point(70, 130), Width = 80, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(160, 130), Width = 80, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] {
                new Label { Text = "Номер:", Location = new Point(20, 23) },
                new Label { Text = "Мест:", Location = new Point(20, 53) },
                new Label { Text = "Зал:", Location = new Point(20, 83) },
                txtNumber, numSeats, cmbLoc, btnOk, btnCancel
            });

            if (form.ShowDialog() == DialogResult.OK && int.TryParse(txtNumber.Text, out int number))
            {
                tables.Add(new Table
                {
                    Id = tables.Max(t => t.Id) + 1,
                    Number = number,
                    Seats = (int)numSeats.Value,
                    Location = cmbLoc.SelectedItem.ToString(),
                    IsAvailable = true
                });
                UpdateAllViews();
            }
        }

        private void DeleteTable()
        {
            if (dgvTables.SelectedRows.Count > 0)
            {
                int tableId = (int)dgvTables.SelectedRows[0].Cells[0].Value;
                if (bookings.Any(b => b.TableId == tableId && b.Status == "Подтверждено"))
                {
                    MessageBox.Show("Нельзя удалить столик, на который есть бронирования!");
                    return;
                }

                var table = tables.FirstOrDefault(t => t.Id == tableId);
                if (table != null)
                {
                    var result = MessageBox.Show($"Удалить столик №{table.Number}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        tables.Remove(table);
                        UpdateAllViews();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите столик для удаления!");
            }
        }

        private void AddCustomer()
        {
            var form = new Form { Text = "Новый клиент", Size = new Size(300, 200), StartPosition = FormStartPosition.CenterParent };
            var txtName = new TextBox { Location = new Point(100, 20), Width = 150 };
            var txtPhone = new TextBox { Location = new Point(100, 50), Width = 150 };
            var btnOk = new Button { Text = "OK", Location = new Point(70, 100), Width = 70, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(150, 100), Width = 70, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] {
                new Label { Text = "Имя:", Location = new Point(20, 23) },
                new Label { Text = "Телефон:", Location = new Point(20, 53) },
                txtName, txtPhone, btnOk, btnCancel
            });

            if (form.ShowDialog() == DialogResult.OK)
            {
                customers.Add(new Customer
                {
                    Id = nextCustomerId++,
                    Name = txtName.Text,
                    Phone = txtPhone.Text
                });
                UpdateCustomersView();
            }
        }

        private void SelectCustomer()
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                selectedCustomerId = (int)dgvCustomers.SelectedRows[0].Cells[0].Value;
                var name = dgvCustomers.SelectedRows[0].Cells[1].Value.ToString();
                var phone = dgvCustomers.SelectedRows[0].Cells[2].Value.ToString();
                lblCustomerInfo.Text = $"✓ Клиент: {name} ({phone})";
                lblCustomerInfo.ForeColor = Color.Green;
                MessageBox.Show($"Клиент {name} выбран!\nТеперь можно создать бронирование.", "Клиент выбран",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                tabControl.SelectedIndex = 0; // Возвращаемся на план зала
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка!");
            }
        }

        private void NewBooking()
        {
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Сначала выберите клиента на вкладке 'Клиенты'!");
                tabControl.SelectedIndex = 2;
                return;
            }

            var customer = customers.FirstOrDefault(c => c.Id == selectedCustomerId);
            if (customer == null) return;

            var form = new Form { Text = "Новое бронирование", Size = new Size(450, 350), StartPosition = FormStartPosition.CenterParent };

            var dtpDateTime = new DateTimePicker
            {
                Location = new Point(150, 20),
                Width = 220,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm",
                Value = DateTime.Now.AddHours(1)
            };

            var numGuests = new NumericUpDown { Location = new Point(150, 50), Width = 100, Minimum = 1, Maximum = 20, Value = 2 };

            var cmbTable = new ComboBox { Location = new Point(150, 80), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            var availableTables = tables.Where(t => t.IsAvailable).ToList();
            cmbTable.DataSource = availableTables;
            cmbTable.DisplayMember = "Number";
            cmbTable.ValueMember = "Id";

            var txtNotes = new TextBox { Location = new Point(150, 110), Width = 220, Height = 60, Multiline = true };

            var lblCustomer = new Label
            {
                Text = $"Клиент: {customer.Name}",
                Location = new Point(150, 180),
                Width = 220,
                ForeColor = Color.Green,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            var btnOk = new Button
            {
                Text = "Забронировать",
                Location = new Point(150, 220),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, 220),
                Width = 100,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };

            form.Controls.AddRange(new Control[] {
                new Label { Text = "Дата и время:", Location = new Point(20, 23) },
                new Label { Text = "Количество гостей:", Location = new Point(20, 53) },
                new Label { Text = "Столик:", Location = new Point(20, 83) },
                new Label { Text = "Заметки:", Location = new Point(20, 113) },
                dtpDateTime, numGuests, cmbTable, txtNotes, lblCustomer, btnOk, btnCancel
            });

            if (form.ShowDialog() == DialogResult.OK)
            {
                var table = availableTables.FirstOrDefault(t => t.Id == (int)cmbTable.SelectedValue);
                if (table != null)
                {
                    bookings.Add(new Booking
                    {
                        Id = nextBookingId++,
                        DateTime = dtpDateTime.Value,
                        Guests = (int)numGuests.Value,
                        Notes = txtNotes.Text,
                        Status = "Подтверждено",
                        TableId = table.Id,
                        TableNumber = table.Number,
                        CustomerId = customer.Id,
                        CustomerName = customer.Name
                    });

                    UpdateAllViews();
                    MessageBox.Show("Бронирование создано!", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void CancelBooking()
        {
            if (dgvBookings.SelectedRows.Count > 0)
            {
                int bookingId = (int)dgvBookings.SelectedRows[0].Cells[0].Value;
                var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
                if (booking != null)
                {
                    var result = MessageBox.Show($"Отменить бронь для {booking.CustomerName}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        booking.Status = "Отменено";
                        UpdateAllViews();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите бронирование для отмены!");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}