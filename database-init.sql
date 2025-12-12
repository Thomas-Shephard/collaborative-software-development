CREATE TABLE User
(
    user_id       INT AUTO_INCREMENT PRIMARY KEY,
    email         VARCHAR(255) UNIQUE NOT NULL,
    name          VARCHAR(70)         NOT NULL,
    password_hash VARCHAR(511)        NOT NULL,
    last_login    TIMESTAMP DEFAULT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE Student
(
    student_id     INT AUTO_INCREMENT PRIMARY KEY,
    user_id        INT UNIQUE NOT NULL,
    account_status ENUM('pending_approval', 'active', 'disabled') NOT NULL DEFAULT 'pending_approval',
    FOREIGN KEY (user_id) REFERENCES User (user_id) ON DELETE CASCADE
);

CREATE TABLE Lecturer
(
    lecturer_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id     INT UNIQUE NOT NULL,
    is_admin    BOOL       NOT NULL DEFAULT FALSE,
    FOREIGN KEY (user_id) REFERENCES User (user_id) ON DELETE CASCADE
);

CREATE TABLE Subject
(
    subject_id INT AUTO_INCREMENT PRIMARY KEY,
    name       VARCHAR(255) UNIQUE NOT NULL,
    is_active  BOOL                NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP                    DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP                    DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE Test
(
    test_id    INT AUTO_INCREMENT PRIMARY KEY,
    subject_id INT          NOT NULL,
    name       VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (subject_id) REFERENCES Subject (subject_id) ON DELETE CASCADE
);

CREATE TABLE Question
(
    question_id INT AUTO_INCREMENT PRIMARY KEY,
    subject_id  INT  NOT NULL,
    text        TEXT NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (subject_id) REFERENCES Subject (subject_id) ON DELETE CASCADE
);

CREATE TABLE QuestionOption
(
    question_option_id INT AUTO_INCREMENT PRIMARY KEY,
    question_id        INT  NOT NULL,
    option_text        TEXT NOT NULL,
    is_correct         BOOL NOT NULL,
    FOREIGN KEY (question_id) REFERENCES Question (question_id) ON DELETE CASCADE
);

CREATE TABLE TestQuestion
(
    test_id     INT NOT NULL,
    question_id INT NOT NULL,
    PRIMARY KEY (test_id, question_id),
    FOREIGN KEY (test_id) REFERENCES Test (test_id) ON DELETE CASCADE,
    FOREIGN KEY (question_id) REFERENCES Question (question_id) ON DELETE CASCADE
);

CREATE TABLE TestResult
(
    test_result_id    INT AUTO_INCREMENT PRIMARY KEY,
    test_id           INT NOT NULL,
    student_id        INT NOT NULL,
    completion_date   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    questions_correct INT NOT NULL,
    FOREIGN KEY (test_id) REFERENCES Test (test_id) ON DELETE CASCADE,
    FOREIGN KEY (student_id) REFERENCES Student (student_id) ON DELETE CASCADE
);

CREATE TABLE StudentAnswer
(
    student_answer_id INT AUTO_INCREMENT PRIMARY KEY,
    test_result_id    INT  NOT NULL,
    question_id       INT  NOT NULL,
    answer_text       TEXT NOT NULL,
    is_correct        BOOL NOT NULL,
    FOREIGN KEY (test_result_id) REFERENCES TestResult (test_result_id) ON DELETE CASCADE,
    FOREIGN KEY (question_id) REFERENCES Question (question_id) ON DELETE CASCADE
);

CREATE TABLE DeniedToken
(
    jti    VARCHAR(36) PRIMARY KEY,
    expires_at TIMESTAMP NOT NULL,
    INDEX idx_expires_at (expires_at)
);
