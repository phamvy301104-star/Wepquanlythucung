using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models.Entities;

namespace nhom6_backend.Models
{
    /// <summary>
    /// ApplicationDbContext kế thừa từ IdentityDbContext để hỗ trợ Identity
    /// Quản lý 45+ tables cho UME Salon App
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ==========================================
        // USER MANAGEMENT (8 tables)
        // ==========================================
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }
        public DbSet<UserBan> UserBans { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }

        // ==========================================
        // PRODUCTS & INVENTORY (8 tables)
        // ==========================================
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        // ==========================================
        // SERVICES & APPOINTMENTS (9 tables)
        // ==========================================
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffSchedule> StaffSchedules { get; set; }
        public DbSet<StaffService> StaffServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }

        // ==========================================
        // ORDERS & PAYMENTS (9 tables)
        // ==========================================
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShippingMethod> ShippingMethods { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }

        // ==========================================
        // BLOG & SOCIAL (12 tables)
        // ==========================================
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Share> Shares { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReadStatus> MessageReadStatuses { get; set; }

        // ==========================================
        // AI CHATBOT (4 tables)
        // ==========================================
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<FaceAnalysis> FaceAnalyses { get; set; }
        public DbSet<HairStyleTryOn> HairStyleTryOns { get; set; }

        // ==========================================
        // STAFF MANAGEMENT (4 tables)
        // ==========================================
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<SalarySlip> SalarySlips { get; set; }
        public DbSet<StaffChatRoom> StaffChatRooms { get; set; }
        public DbSet<StaffChatMessage> StaffChatMessages { get; set; }

        // ==========================================
        // NOTIFICATIONS & SYSTEM (5 tables)
        // ==========================================
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<StoreInfo> StoreInfos { get; set; }
        public DbSet<BannedWord> BannedWords { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Banner> Banners { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Gọi base để cấu hình Identity tables
            base.OnModelCreating(builder);

            // ==========================================
            // USER ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.Initials).HasMaxLength(5);
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.AvatarUrl).HasMaxLength(500);
                entity.Property(u => u.Address).HasMaxLength(500);

                // Navigation properties
                entity.HasMany<UserProfile>()
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<UserAddress>()
                      .WithOne(a => a.User)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<RefreshToken>()
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // PRODUCT ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.OriginalPrice).HasPrecision(18, 2);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(p => p.SKU).IsUnique();
                entity.HasIndex(p => p.Slug);
            });

            builder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.ChildCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.Slug);
            });

            builder.Entity<Brand>(entity =>
            {
                entity.HasIndex(b => b.Slug);
            });

            builder.Entity<ProductVariant>(entity =>
            {
                entity.Property(v => v.OriginalPrice).HasPrecision(18, 2);
                entity.Property(v => v.Price).HasPrecision(18, 2);
                entity.HasIndex(v => v.SKU);
            });

            // ==========================================
            // SERVICE ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Service>(entity =>
            {
                entity.Property(s => s.Price).HasPrecision(18, 2);
                entity.Property(s => s.OriginalPrice).HasPrecision(18, 2);
                entity.Property(s => s.MinPrice).HasPrecision(18, 2);
                entity.Property(s => s.MaxPrice).HasPrecision(18, 2);

                entity.HasIndex(s => s.Slug);
                entity.HasIndex(s => s.ServiceCode).IsUnique();
            });

            builder.Entity<Staff>(entity =>
            {
                entity.Property(s => s.BaseSalary).HasPrecision(18, 2);
                entity.Property(s => s.TotalRevenue).HasPrecision(18, 2);
                entity.HasIndex(s => s.StaffCode).IsUnique();
            });

            builder.Entity<StaffService>(entity =>
            {
                entity.Property(ss => ss.CustomPrice).HasPrecision(18, 2);
                entity.HasIndex(ss => new { ss.StaffId, ss.ServiceId }).IsUnique();
            });

            // ==========================================
            // APPOINTMENT ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Appointment>(entity =>
            {
                entity.Property(a => a.TotalAmount).HasPrecision(18, 2);
                entity.Property(a => a.DiscountAmount).HasPrecision(18, 2);
                entity.Property(a => a.PaidAmount).HasPrecision(18, 2);

                entity.HasIndex(a => a.AppointmentCode).IsUnique();
                entity.HasIndex(a => new { a.AppointmentDate, a.StaffId });
            });

            builder.Entity<AppointmentService>(entity =>
            {
                entity.Property(a => a.Price).HasPrecision(18, 2);
            });

            // ==========================================
            // ORDER ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Order>(entity =>
            {
                entity.Property(o => o.SubTotal).HasPrecision(18, 2);
                entity.Property(o => o.ShippingFee).HasPrecision(18, 2);
                entity.Property(o => o.DiscountAmount).HasPrecision(18, 2);
                entity.Property(o => o.TaxAmount).HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
                entity.Property(o => o.PaidAmount).HasPrecision(18, 2);

                entity.HasIndex(o => o.OrderCode).IsUnique();
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.CreatedAt);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.OriginalPrice).HasPrecision(18, 2);
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
                entity.Property(oi => oi.DiscountAmount).HasPrecision(18, 2);
                entity.Property(oi => oi.TotalPrice).HasPrecision(18, 2);
            });

            builder.Entity<Cart>(entity =>
            {
                entity.Property(c => c.TotalAmount).HasPrecision(18, 2);
                entity.Property(c => c.DiscountAmount).HasPrecision(18, 2);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.Property(ci => ci.UnitPrice).HasPrecision(18, 2);
                entity.Property(ci => ci.TotalPrice).HasPrecision(18, 2);
            });

            builder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasPrecision(18, 2);
                entity.Property(p => p.Fee).HasPrecision(18, 2);
                entity.Property(p => p.NetAmount).HasPrecision(18, 2);

                entity.HasIndex(p => p.TransactionCode).IsUnique();
            });

            builder.Entity<Coupon>(entity =>
            {
                entity.Property(c => c.DiscountValue).HasPrecision(18, 2);
                entity.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);
                entity.Property(c => c.MinOrderAmount).HasPrecision(18, 2);

                entity.HasIndex(c => c.Code).IsUnique();
            });

            builder.Entity<CouponUsage>(entity =>
            {
                entity.Property(cu => cu.DiscountAmount).HasPrecision(18, 2);
            });

            // ==========================================
            // BLOG & SOCIAL ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Post>(entity =>
            {
                entity.HasOne(p => p.OriginalPost)
                      .WithMany(p => p.SharedPosts)
                      .HasForeignKey(p => p.OriginalPostId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.Slug);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.CreatedAt);
            });

            builder.Entity<Follow>(entity =>
            {
                entity.HasOne(f => f.Follower)
                      .WithMany()
                      .HasForeignKey(f => f.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Following)
                      .WithMany()
                      .HasForeignKey(f => f.FollowingId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
            });

            builder.Entity<Bookmark>(entity =>
            {
                entity.HasIndex(b => new { b.PostId, b.UserId }).IsUnique();

                entity.HasOne(b => b.Post)
                      .WithMany(p => p.Bookmarks)
                      .HasForeignKey(b => b.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.User)
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Like>(entity =>
            {
                entity.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();

                entity.HasOne(l => l.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CommentLike>(entity =>
            {
                entity.HasIndex(cl => new { cl.CommentId, cl.UserId }).IsUnique();

                entity.HasOne(cl => cl.Comment)
                      .WithMany(c => c.CommentLikes)
                      .HasForeignKey(cl => cl.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cl => cl.User)
                      .WithMany()
                      .HasForeignKey(cl => cl.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.ParentComment)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentCommentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Share>(entity =>
            {
                // Share.PostId -> Post được share
                entity.HasOne(s => s.Post)
                      .WithMany(p => p.Shares)
                      .HasForeignKey(s => s.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Share.SharedPostId -> Bài viết mới được tạo khi share (optional)
                entity.HasOne(s => s.SharedPost)
                      .WithMany()
                      .HasForeignKey(s => s.SharedPostId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Wishlist>(entity =>
            {
                entity.HasIndex(w => new { w.ProductId, w.UserId }).IsUnique();

                entity.HasOne(w => w.Product)
                      .WithMany(p => p.Wishlists)
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.User)
                      .WithMany()
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.ReplyToMessage)
                      .WithMany()
                      .HasForeignKey(m => m.ReplyToMessageId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.ReadStatuses)
                      .WithOne(rs => rs.Message)
                      .HasForeignKey(rs => rs.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<MessageReadStatus>(entity =>
            {
                entity.HasOne(mrs => mrs.User)
                      .WithMany()
                      .HasForeignKey(mrs => mrs.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasOne(cp => cp.User)
                      .WithMany()
                      .HasForeignKey(cp => cp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cp => cp.Conversation)
                      .WithMany(c => c.Participants)
                      .HasForeignKey(cp => cp.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // AI CHATBOT ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<FaceAnalysis>(entity =>
            {
                entity.Property(f => f.FaceShapeConfidence).HasPrecision(5, 4);
            });

            builder.Entity<ChatMessage>(entity =>
            {
                entity.Property(cm => cm.IntentConfidence).HasPrecision(5, 4);
            });

            // ==========================================
            // NOTIFICATION ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasIndex(n => n.CreatedAt);
            });

            // ==========================================
            // AUDIT LOG ENTITY CONFIGURATIONS
            // ==========================================
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.Action);
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => new { a.EntityType, a.EntityId });
            });

            // ==========================================
            // OTHER INDEXES
            // ==========================================
            builder.Entity<UserLoginHistory>(entity =>
            {
                entity.HasIndex(u => new { u.UserId, u.LoginTime });
            });

            builder.Entity<UserBan>(entity =>
            {
                entity.HasIndex(u => new { u.UserId, u.IsActive });
            });

            builder.Entity<StaffSchedule>(entity =>
            {
                entity.HasIndex(ss => new { ss.StaffId, ss.DayOfWeek });
            });

            builder.Entity<Banner>(entity =>
            {
                entity.HasIndex(b => new { b.Position, b.IsActive, b.DisplayOrder });
            });

            // ==========================================
            // ADDITIONAL DECIMAL PRECISION CONFIGURATIONS
            // ==========================================
            builder.Entity<PaymentMethod>(entity =>
            {
                entity.Property(pm => pm.FixedFee).HasPrecision(18, 2);
                entity.Property(pm => pm.TransactionFeePercent).HasPrecision(5, 2);
                entity.Property(pm => pm.MinAmount).HasPrecision(18, 2);
                entity.Property(pm => pm.MaxAmount).HasPrecision(18, 2);
                entity.HasIndex(pm => pm.Code).IsUnique();
            });

            builder.Entity<ShippingMethod>(entity =>
            {
                entity.Property(sm => sm.BaseFee).HasPrecision(18, 2);
                entity.Property(sm => sm.FeePerKg).HasPrecision(18, 2);
                entity.Property(sm => sm.FreeShippingMinAmount).HasPrecision(18, 2);
                entity.Property(sm => sm.MaxWeight).HasPrecision(10, 2);
                entity.HasIndex(sm => sm.Code).IsUnique();
            });

            builder.Entity<Service>(entity =>
            {
                entity.Property(s => s.AverageRating).HasPrecision(3, 2);
            });

            builder.Entity<Staff>(entity =>
            {
                entity.Property(s => s.AverageRating).HasPrecision(3, 2);
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.AverageRating).HasPrecision(3, 2);
            });

            // ==========================================
            // STAFF MANAGEMENT CONFIGURATIONS
            // ==========================================
            builder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(a => new { a.StaffId, a.WorkDate }).IsUnique();
                entity.HasIndex(a => a.WorkDate);
                entity.HasIndex(a => a.Status);
            });

            builder.Entity<SalarySlip>(entity =>
            {
                entity.HasIndex(s => new { s.StaffId, s.Month, s.Year }).IsUnique();
                entity.HasIndex(s => new { s.Month, s.Year });
                entity.HasIndex(s => s.Status);
            });

            builder.Entity<StaffChatRoom>(entity =>
            {
                // Đảm bảo chỉ có 1 phòng chat giữa 2 người
                entity.HasIndex(r => new { r.Staff1Id, r.Staff2Id }).IsUnique();
                entity.HasIndex(r => r.LastMessageAt);
                
                // Không cascade delete khi xóa Staff
                entity.HasOne(r => r.Staff1)
                      .WithMany()
                      .HasForeignKey(r => r.Staff1Id)
                      .OnDelete(DeleteBehavior.NoAction);
                      
                entity.HasOne(r => r.Staff2)
                      .WithMany()
                      .HasForeignKey(r => r.Staff2Id)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<StaffChatMessage>(entity =>
            {
                entity.HasIndex(m => new { m.ChatRoomId, m.CreatedAt });
                entity.HasIndex(m => m.SenderId);
                entity.HasIndex(m => m.IsRead);
                
                // Không cascade delete
                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
