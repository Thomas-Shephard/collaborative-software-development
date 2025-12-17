CREATE TABLE StudentSubject
(
    student_id INT NOT NULL,
    subject_id INT NOT NULL,
    PRIMARY KEY (student_id, subject_id),
    FOREIGN KEY (student_id) REFERENCES Student (student_id) ON DELETE CASCADE,
    FOREIGN KEY (subject_id) REFERENCES Subject (subject_id) ON DELETE CASCADE
);
