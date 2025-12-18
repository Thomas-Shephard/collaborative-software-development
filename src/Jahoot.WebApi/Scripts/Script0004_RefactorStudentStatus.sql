ALTER TABLE User ADD COLUMN is_disabled BOOL NOT NULL DEFAULT FALSE;
ALTER TABLE Student ADD COLUMN is_approved BOOL NOT NULL DEFAULT FALSE;

-- Migrate existing data
UPDATE Student SET is_approved = TRUE WHERE account_status = 'active';
UPDATE User JOIN Student ON User.user_id = Student.user_id SET User.is_disabled = TRUE WHERE Student.account_status = 'disabled';

-- Drop the old column
ALTER TABLE Student DROP COLUMN account_status;
