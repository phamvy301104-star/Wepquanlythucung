using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nhom6_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckIn1_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckIn1_PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckIn2_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckIn2_PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckIn3_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckIn3_PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckIn4_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckIn4_PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScheduledStart = table.Column<TimeSpan>(type: "time", nullable: false),
                    ScheduledBreakStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    ScheduledBreakEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    ScheduledEnd = table.Column<TimeSpan>(type: "time", nullable: false),
                    LateMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyLeaveMinutes = table.Column<int>(type: "int", nullable: false),
                    OverBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalWorkMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CheckCount = table.Column<int>(type: "int", nullable: false),
                    LatePenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverBreakPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarlyLeavePenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MissedCheckPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalarySlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    WorkDays = table.Column<int>(type: "int", nullable: false),
                    ActualWorkDays = table.Column<int>(type: "int", nullable: false),
                    PaidLeaveDays = table.Column<int>(type: "int", nullable: false),
                    UnpaidLeaveDays = table.Column<int>(type: "int", nullable: false),
                    TotalLateMinutes = table.Column<int>(type: "int", nullable: false),
                    LateCount = table.Column<int>(type: "int", nullable: false),
                    MissedCheckDays = table.Column<int>(type: "int", nullable: false),
                    TotalOvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimeBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LatePenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MissedCheckPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsentDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BHXH = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BHYT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BHTN = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalarySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalarySlips_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffChatRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Staff1Id = table.Column<int>(type: "int", nullable: false),
                    Staff2Id = table.Column<int>(type: "int", nullable: false),
                    LastMessageText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastMessageSenderId = table.Column<int>(type: "int", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Staff1UnreadCount = table.Column<int>(type: "int", nullable: false),
                    Staff2UnreadCount = table.Column<int>(type: "int", nullable: false),
                    Staff1Muted = table.Column<bool>(type: "bit", nullable: false),
                    Staff2Muted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffChatRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffChatRooms_Staff_Staff1Id",
                        column: x => x.Staff1Id,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffChatRooms_Staff_Staff2Id",
                        column: x => x.Staff2Id,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatRoomId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffChatMessages_StaffChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalTable: "StaffChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffChatMessages_Staff_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StaffId_WorkDate",
                table: "Attendances",
                columns: new[] { "StaffId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_Status",
                table: "Attendances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_WorkDate",
                table: "Attendances",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_Month_Year",
                table: "SalarySlips",
                columns: new[] { "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_StaffId_Month_Year",
                table: "SalarySlips",
                columns: new[] { "StaffId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_Status",
                table: "SalarySlips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatMessages_ChatRoomId_CreatedAt",
                table: "StaffChatMessages",
                columns: new[] { "ChatRoomId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatMessages_IsRead",
                table: "StaffChatMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatMessages_SenderId",
                table: "StaffChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatRooms_LastMessageAt",
                table: "StaffChatRooms",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatRooms_Staff1Id_Staff2Id",
                table: "StaffChatRooms",
                columns: new[] { "Staff1Id", "Staff2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffChatRooms_Staff2Id",
                table: "StaffChatRooms",
                column: "Staff2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotifications");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "SalarySlips");

            migrationBuilder.DropTable(
                name: "StaffChatMessages");

            migrationBuilder.DropTable(
                name: "StaffChatRooms");
        }
    }
}
