ALTER TABLE Test ADD COLUMN number_of_questions INT NOT NULL DEFAULT 0;

-- Set the initial value to the number of questions currently in the test
UPDATE Test test SET number_of_questions = (SELECT COUNT(*) FROM TestQuestion test_question WHERE test_question.test_id = test.test_id);
