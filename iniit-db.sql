-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" VARCHAR(36) PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "Nickname" VARCHAR(100) NOT NULL UNIQUE,
    "Name" VARCHAR(255) NOT NULL,
    "PasswordHash" VARCHAR(500),
    "RefreshToken" VARCHAR(500),
    "Active" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- Create AuditLogs table
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" VARCHAR(36) PRIMARY KEY,
    "TableName" VARCHAR(100) NOT NULL,
    "Operation" VARCHAR(50) NOT NULL,
    "UserId" VARCHAR(36),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "OldValues" TEXT,
    "NewValues" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Create indexes for better query performance
CREATE INDEX idx_users_email ON "Users"("Email");
CREATE INDEX idx_users_nickname ON "Users"("Nickname");
CREATE INDEX idx_auditlogs_userid ON "AuditLogs"("UserId");
CREATE INDEX idx_auditlogs_tablename ON "AuditLogs"("TableName");
CREATE INDEX idx_auditlogs_timestamp ON "AuditLogs"("Timestamp");