using Microsoft.EntityFrameworkCore;
using StudySpace.Core.Entities;
using StudySpace.Core.Enums;

namespace StudySpace.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(StudySpaceDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var admin = new User
            {
                FullName = "Quản trị viên",
                Email = "admin@studyspace.com",
                Phone = "0900000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                Bio = "Quản trị hệ thống StudySpace",
                CreatedAt = seedDate,
                IsActive = true
            };

            var leader = new User
            {
                FullName = "Nguyễn Văn Trưởng",
                Email = "leader@studyspace.com",
                Phone = "0900000001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Leader@123"),
                Role = UserRole.Leader,
                Bio = "Trưởng nhóm học mẫu",
                CreatedAt = seedDate,
                IsActive = true
            };

            var member1 = new User
            {
                FullName = "Trần Thị Học",
                Email = "member1@studyspace.com",
                Phone = "0900000002",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
                Role = UserRole.Member,
                Bio = "Sinh viên năm 3",
                CreatedAt = seedDate,
                IsActive = true
            };

            var member2 = new User
            {
                FullName = "Lê Quốc Học",
                Email = "member2@studyspace.com",
                Phone = "0900000003",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
                Role = UserRole.Member,
                CreatedAt = seedDate,
                IsActive = true
            };

            db.Users.AddRange(admin, leader, member1, member2);
            await db.SaveChangesAsync();

            var group1 = new StudyGroup
            {
                Name = "Lập trình C# nâng cao",
                Description = "Nhóm học C# cho sinh viên năm cuối, ôn thi tốt nghiệp.",
                Subject = "Lập trình",
                InviteCode = "CSHARP01",
                IsPublic = true,
                MaxMembers = 30,
                CreatedById = leader.Id,
                CreatedAt = seedDate
            };

            var group2 = new StudyGroup
            {
                Name = "Toán cao cấp - Nhóm A",
                Description = "Cùng nhau giải bài tập toán cao cấp, chia sẻ tài liệu.",
                Subject = "Toán",
                InviteCode = "MATHA001",
                IsPublic = true,
                MaxMembers = 25,
                CreatedById = leader.Id,
                CreatedAt = seedDate
            };

            db.StudyGroups.AddRange(group1, group2);
            await db.SaveChangesAsync();

            db.GroupMembers.AddRange(
                new GroupMember { StudyGroupId = group1.Id, UserId = leader.Id, Role = GroupMemberRole.Leader, JoinedAt = seedDate },
                new GroupMember { StudyGroupId = group1.Id, UserId = member1.Id, Role = GroupMemberRole.Member, JoinedAt = seedDate },
                new GroupMember { StudyGroupId = group1.Id, UserId = member2.Id, Role = GroupMemberRole.Member, JoinedAt = seedDate },
                new GroupMember { StudyGroupId = group2.Id, UserId = leader.Id, Role = GroupMemberRole.Leader, JoinedAt = seedDate },
                new GroupMember { StudyGroupId = group2.Id, UserId = member1.Id, Role = GroupMemberRole.Member, JoinedAt = seedDate }
            );

            db.Schedules.AddRange(
                new Schedule
                {
                    StudyGroupId = group1.Id,
                    Title = "Buổi học mở đầu - Tổng quan C#",
                    Description = "Cùng review lộ trình học và mục tiêu của nhóm.",
                    Location = "Online",
                    MeetingUrl = "https://meet.example.com/csharp01",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
                    Status = ScheduleStatus.Upcoming,
                    CreatedById = leader.Id,
                    CreatedAt = seedDate
                },
                new Schedule
                {
                    StudyGroupId = group2.Id,
                    Title = "Giải bài tập chương Giới hạn",
                    Location = "Phòng B201",
                    StartTime = DateTime.UtcNow.AddDays(3),
                    EndTime = DateTime.UtcNow.AddDays(3).AddHours(2),
                    Status = ScheduleStatus.Upcoming,
                    CreatedById = leader.Id,
                    CreatedAt = seedDate
                }
            );

            db.ProgressEntries.AddRange(
                new ProgressEntry
                {
                    StudyGroupId = group1.Id,
                    UserId = member1.Id,
                    Title = "Hoàn thành Module 1: Cú pháp cơ bản",
                    Status = ProgressStatus.Completed,
                    CompletionPercent = 100,
                    CompletedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = seedDate
                },
                new ProgressEntry
                {
                    StudyGroupId = group1.Id,
                    UserId = member1.Id,
                    Title = "Module 2: OOP",
                    Status = ProgressStatus.InProgress,
                    CompletionPercent = 40,
                    CreatedAt = seedDate
                }
            );

            db.ChatMessages.AddRange(
                new ChatMessage
                {
                    StudyGroupId = group1.Id,
                    SenderId = leader.Id,
                    Content = "Chào mừng các bạn đến với nhóm C# nâng cao!",
                    SentAt = DateTime.UtcNow.AddHours(-3)
                },
                new ChatMessage
                {
                    StudyGroupId = group1.Id,
                    SenderId = member1.Id,
                    Content = "Em cảm ơn anh, em đã sẵn sàng học!",
                    SentAt = DateTime.UtcNow.AddHours(-2)
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
