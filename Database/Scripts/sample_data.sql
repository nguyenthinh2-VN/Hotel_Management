-- Clear existing data
DELETE FROM bookings;
DELETE FROM rooms;
DELETE FROM room_types;
DELETE FROM customers;
DELETE FROM employees;

-- Reset auto-increment
ALTER TABLE bookings AUTO_INCREMENT = 1;
ALTER TABLE rooms AUTO_INCREMENT = 1;
ALTER TABLE room_types AUTO_INCREMENT = 1;
ALTER TABLE customers AUTO_INCREMENT = 1;
ALTER TABLE employees AUTO_INCREMENT = 1;

-- Insert room types
INSERT INTO room_types (name, price, description) VALUES
('Standard Single', 500000, 'Standard room with a single bed'),
('Standard Double', 700000, 'Standard room with a double bed'),
('Deluxe Single', 800000, 'Deluxe room with a single bed and city view'),
('Deluxe Double', 1000000, 'Deluxe room with a double bed and city view'),
('Suite', 1500000, 'Luxury suite with separate living area and ocean view'),
('Family Suite', 2000000, 'Large suite with two bedrooms and living area');

-- Insert rooms (4 floors, multiple rooms of each type)
INSERT INTO rooms (room_number, room_type_id, floor, status) VALUES
-- First floor - Standard rooms
('101', 1, 1, 'Available'),
('102', 1, 1, 'Available'),
('103', 2, 1, 'Available'),
('104', 2, 1, 'Available'),
-- Second floor - Deluxe rooms
('201', 3, 2, 'Available'),
('202', 3, 2, 'Available'),
('203', 4, 2, 'Available'),
('204', 4, 2, 'Available'),
-- Third floor - Suites
('301', 5, 3, 'Available'),
('302', 5, 3, 'Available'),
-- Fourth floor - Family Suites
('401', 6, 4, 'Available'),
('402', 6, 4, 'Available');

-- Insert employees
INSERT INTO employees (username, password, name, role, phone, email) VALUES
('admin', 'admin123', 'System Administrator', 'Admin', '0901234567', 'admin@hotel.com'),
('receptionist1', 'rec123', 'John Smith', 'Receptionist', '0901234568', 'john@hotel.com'),
('receptionist2', 'rec123', 'Mary Johnson', 'Receptionist', '0901234569', 'mary@hotel.com'),
('staff1', 'staff123', 'Peter Wilson', 'Staff', '0901234570', 'peter@hotel.com'),
('staff2', 'staff123', 'Sarah Davis', 'Staff', '0901234571', 'sarah@hotel.com');

-- Insert customers
INSERT INTO customers (name, id_card, phone, email, address) VALUES
('James Brown', '001234567890', '0911234567', 'james@email.com', '123 Main St, City'),
('Emily White', '001234567891', '0911234568', 'emily@email.com', '456 Park Ave, City'),
('Michael Lee', '001234567892', '0911234569', 'michael@email.com', '789 Oak Rd, City'),
('Linda Chen', '001234567893', '0911234570', 'linda@email.com', '321 Pine St, City'),
('Robert Taylor', '001234567894', '0911234571', 'robert@email.com', '654 Elm St, City');

-- Insert bookings (mix of current and historical bookings)
INSERT INTO bookings (room_id, customer_id, employee_id, check_in, check_out, status, total_amount, payment_status, created_at) VALUES
-- Completed bookings
(1, 1, 2, DATE_SUB(NOW(), INTERVAL 10 DAY), DATE_SUB(NOW(), INTERVAL 8 DAY), 'Checked-out', 1000000, 'Paid', DATE_SUB(NOW(), INTERVAL 15 DAY)),
(3, 2, 2, DATE_SUB(NOW(), INTERVAL 7 DAY), DATE_SUB(NOW(), INTERVAL 5 DAY), 'Checked-out', 1400000, 'Paid', DATE_SUB(NOW(), INTERVAL 10 DAY)),

-- Current active bookings
(2, 3, 3, DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_ADD(NOW(), INTERVAL 2 DAY), 'Checked-in', 1500000, 'Paid', DATE_SUB(NOW(), INTERVAL 5 DAY)),
(4, 4, 3, NOW(), DATE_ADD(NOW(), INTERVAL 3 DAY), 'Checked-in', 2100000, 'Paid', DATE_SUB(NOW(), INTERVAL 2 DAY)),

-- Future bookings
(5, 5, 2, DATE_ADD(NOW(), INTERVAL 5 DAY), DATE_ADD(NOW(), INTERVAL 7 DAY), 'Reserved', 1600000, 'Pending', DATE_SUB(NOW(), INTERVAL 1 DAY)),
(6, 1, 2, DATE_ADD(NOW(), INTERVAL 10 DAY), DATE_ADD(NOW(), INTERVAL 12 DAY), 'Reserved', 1600000, 'Pending', NOW());

-- Update room status based on bookings
UPDATE rooms r
JOIN bookings b ON r.id = b.room_id
SET r.status = CASE 
    WHEN b.status = 'Checked-in' THEN 'Occupied'
    WHEN b.status = 'Reserved' AND b.check_in > NOW() THEN 'Booked'
    ELSE r.status
END
WHERE b.status IN ('Checked-in', 'Reserved');
