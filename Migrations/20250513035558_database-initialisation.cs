using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class databaseinitialisation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userID = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "hased"),
                    role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValueSql: "'client'::character varying"),
                    organisationID = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValueSql: "'ClientOrgMSP'::character varying"),
                    createdAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    lastLogin = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    isActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    refreshToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tokenExpiry = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    documentID = table.Column<Guid>(type: "uuid", nullable: false),
                    ipfsCID = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    blockchainTxID = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ownerID = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    documentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValueSql: "'other'::character varying"),
                    jurisdiction = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValueSql: "'GB'::character varying", comment: "Focus on UK"),
                    encryptionKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    accessControl = table.Column<string>(type: "jsonb", nullable: false),
                    embedding = table.Column<Vector>(type: "vector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Document_pkey", x => x.documentID);
                    table.ForeignKey(
                        name: "FK_document_ownerID",
                        column: x => x.ownerID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "RevokedToken",
                columns: table => new
                {
                    tokenID = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    userID = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true),
                    revokedAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevokedToken_pkey", x => x.tokenID);
                    table.ForeignKey(
                        name: "FK_userID_revoked_token",
                        column: x => x.userID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    sessionID = table.Column<Guid>(type: "uuid", nullable: false),
                    userID = table.Column<Guid>(type: "uuid", nullable: false),
                    documentID = table.Column<Guid>(type: "uuid", nullable: true),
                    contextWindow = table.Column<string>(type: "jsonb", nullable: false),
                    legalTopics = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    analysisParameter = table.Column<string>(type: "jsonb", nullable: false),
                    sessionTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    updatedAt = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("Session_pkey", x => x.sessionID);
                    table.ForeignKey(
                        name: "FK_session_documentID",
                        column: x => x.documentID,
                        principalTable: "Document",
                        principalColumn: "documentID");
                    table.ForeignKey(
                        name: "FK_session_userID",
                        column: x => x.userID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "AccessLog",
                columns: table => new
                {
                    logID = table.Column<Guid>(type: "uuid", nullable: false),
                    userID = table.Column<Guid>(type: "uuid", nullable: false),
                    documentID = table.Column<Guid>(type: "uuid", nullable: false),
                    sessionID = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    timeStamp = table.Column<DateTime>(type: "timestamp(2) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("AccessLog_pkey", x => x.logID);
                    table.ForeignKey(
                        name: "FK_accessed_documentID",
                        column: x => x.documentID,
                        principalTable: "Document",
                        principalColumn: "documentID");
                    table.ForeignKey(
                        name: "FK_accessed_sessionID",
                        column: x => x.sessionID,
                        principalTable: "Session",
                        principalColumn: "sessionID");
                    table.ForeignKey(
                        name: "FK_accessed_userID",
                        column: x => x.userID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "ContextSummary",
                columns: table => new
                {
                    summaryID = table.Column<Guid>(type: "uuid", nullable: false),
                    sessionID = table.Column<Guid>(type: "uuid", nullable: false),
                    summaryText = table.Column<string>(type: "text", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(1536)", maxLength: 1536, nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ContextSummary_pkey", x => x.summaryID);
                    table.ForeignKey(
                        name: "FK_summary_session",
                        column: x => x.sessionID,
                        principalTable: "Session",
                        principalColumn: "sessionID");
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    messageID = table.Column<Guid>(type: "uuid", nullable: false),
                    sessionID = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false),
                    response = table.Column<string>(type: "text", nullable: false),
                    sequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Message_pkey", x => x.messageID);
                    table.ForeignKey(
                        name: "FK_message_session",
                        column: x => x.sessionID,
                        principalTable: "Session",
                        principalColumn: "sessionID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_documentID",
                table: "AccessLog",
                column: "documentID");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_sessionID",
                table: "AccessLog",
                column: "sessionID");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_userID",
                table: "AccessLog",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_ContextSummary_sessionID",
                table: "ContextSummary",
                column: "sessionID");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ownerID",
                table: "Document",
                column: "ownerID");

            migrationBuilder.CreateIndex(
                name: "IX_Message_sessionID",
                table: "Message",
                column: "sessionID");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedToken_userID",
                table: "RevokedToken",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_Session_documentID",
                table: "Session",
                column: "documentID");

            migrationBuilder.CreateIndex(
                name: "IX_Session_userID",
                table: "Session",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "user_email_unique",
                table: "User",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLog");

            migrationBuilder.DropTable(
                name: "ContextSummary");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "RevokedToken");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
