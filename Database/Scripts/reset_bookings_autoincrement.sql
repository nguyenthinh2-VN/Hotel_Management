-- Reset auto_increment for bookings table
SET FOREIGN_KEY_CHECKS = 0;

-- Delete all records and reset auto_increment
TRUNCATE TABLE bookings;

-- Or just reset auto_increment without deleting data
-- ALTER TABLE bookings AUTO_INCREMENT = 1;

SET FOREIGN_KEY_CHECKS = 1;
