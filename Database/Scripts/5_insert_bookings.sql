USE hotel_management;

-- Insert sample bookings
INSERT INTO bookings (id, room_id, customer_id, employee_id, check_in, check_out, status, total_amount, payment_status, created_at) VALUES
(1, 1, 1, 2, '2024-03-19 14:00:00', '2024-03-21 12:00:00', 'Checked-out', 1000000, 'Paid', '2024-03-19 10:00:00'),
(2, 2, 2, 2, '2024-03-22 14:00:00', '2024-03-24 12:00:00', 'Checked-out', 1400000, 'Paid', '2024-03-22 09:00:00'),
(3, 3, 3, 3, '2024-03-28 14:00:00', '2024-03-31 12:00:00', 'Checked-in', 1500000, 'Paid', '2024-03-28 10:00:00'),
(4, 4, 4, 3, '2024-03-29 14:00:00', '2024-04-01 12:00:00', 'Checked-in', 2100000, 'Paid', '2024-03-29 09:00:00'),
(5, 5, 5, 2, '2024-04-03 14:00:00', '2024-04-05 12:00:00', 'Reserved', 1600000, 'Pending', '2024-03-29 11:00:00'),
(6, 6, 1, 2, '2024-04-08 14:00:00', '2024-04-10 12:00:00', 'Reserved', 1600000, 'Pending', '2024-03-29 11:30:00');

-- Update room status based on bookings
UPDATE rooms r
JOIN bookings b ON r.id = b.room_id
SET r.status = CASE 
    WHEN b.status = 'Checked-in' THEN 'Occupied'
    WHEN b.status = 'Reserved' THEN 'Booked'
    ELSE r.status
END
WHERE b.status IN ('Checked-in', 'Reserved');
