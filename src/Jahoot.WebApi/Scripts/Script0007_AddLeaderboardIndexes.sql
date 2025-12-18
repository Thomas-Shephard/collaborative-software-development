-- Optimize Leaderboard Query
CREATE INDEX idx_testresult_leaderboard ON TestResult (test_id, student_id, score);
