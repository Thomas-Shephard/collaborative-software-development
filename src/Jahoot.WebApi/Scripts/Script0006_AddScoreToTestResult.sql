ALTER TABLE TestResult ADD COLUMN score INT NOT NULL DEFAULT 0;

-- Update existing scores based on the points system: +30 for correct, -10 for incorrect
-- Incorrect answers are calculated as (Number of Questions - Correct Answers)
UPDATE TestResult test_result
    JOIN Test test ON test_result.test_id = test.test_id
    SET test_result.score = (test_result.questions_correct * 30) + ((test.number_of_questions - test_result.questions_correct) * -10);
