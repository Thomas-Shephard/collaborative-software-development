CREATE TABLE PasswordResetToken
(
    token_id   INT AUTO_INCREMENT PRIMARY KEY,
    user_id    INT          NOT NULL,
    token_hash VARCHAR(511) NOT NULL,
    expiration TIMESTAMP    NOT NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL 15 MINUTE),
    is_used    BOOL         NOT NULL DEFAULT FALSE,
    is_revoked BOOL         NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP             DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES User (user_id) ON DELETE CASCADE
);
