-- =============================================
-- Script: create_all_missing_tables.sql
-- Purpose: Create ALL missing tables for UME Backend
-- Date: 2026-01-14
-- =============================================

-- ==========================================
-- USER MANAGEMENT TABLES
-- ==========================================

-- UserProfiles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserProfiles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserProfiles] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Bio] nvarchar(500) NULL,
        [Gender] nvarchar(20) NULL,
        [DateOfBirth] datetime2 NULL,
        [Occupation] nvarchar(100) NULL,
        [Website] nvarchar(200) NULL,
        [SocialLinks] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserProfiles_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: UserProfiles';
END
GO

-- UserAddresses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserAddresses] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(100) NULL,
        [RecipientName] nvarchar(100) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Province] nvarchar(100) NULL,
        [District] nvarchar(100) NULL,
        [Ward] nvarchar(100) NULL,
        [AddressLine] nvarchar(500) NOT NULL,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserAddresses_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: UserAddresses';
END
GO

-- RefreshTokens
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Token] nvarchar(500) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL DEFAULT 0,
        [RevokedAt] datetime2 NULL,
        [ReplacedByToken] nvarchar(500) NULL,
        [CreatedByIp] nvarchar(50) NULL,
        [RevokedByIp] nvarchar(50) NULL,
        [DeviceInfo] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: RefreshTokens';
END
GO

-- UserLoginHistories
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserLoginHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserLoginHistories] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [LoginTime] datetime2 NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [DeviceType] nvarchar(50) NULL,
        [Browser] nvarchar(100) NULL,
        [OperatingSystem] nvarchar(100) NULL,
        [Location] nvarchar(200) NULL,
        [LoginStatus] nvarchar(20) NOT NULL DEFAULT 'Success',
        [FailureReason] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserLoginHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserLoginHistories_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: UserLoginHistories';
END
GO

-- UserBans
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserBans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserBans] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [BannedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [BannedUntil] datetime2 NULL,
        [BannedBy] nvarchar(450) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [UnbannedAt] datetime2 NULL,
        [UnbannedBy] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserBans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserBans_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: UserBans';
END
GO

-- UserWarnings
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserWarnings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserWarnings] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [WarnedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [WarnedBy] nvarchar(450) NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserWarnings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserWarnings_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: UserWarnings';
END
GO

-- ==========================================
-- PRODUCTS TABLES
-- ==========================================

-- ProductVariants
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariants]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductVariants] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ProductId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [SKU] nvarchar(50) NULL,
        [Price] decimal(18,2) NOT NULL,
        [OriginalPrice] decimal(18,2) NULL,
        [Stock] int NOT NULL DEFAULT 0,
        [ImageUrl] nvarchar(500) NULL,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductVariants_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ProductVariants';
END
GO

-- ProductReviews
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductReviews] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ProductId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [ImageUrls] nvarchar(max) NULL,
        [IsVerifiedPurchase] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ProductReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductReviews_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ProductReviews';
END
GO

-- Wishlists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wishlists]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Wishlists] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [ProductId] int NOT NULL,
        [AddedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Wishlists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wishlists_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Wishlists';
END
GO

-- ServiceImages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ServiceImages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ServiceId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ServiceImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceImages_Services] FOREIGN KEY ([ServiceId]) REFERENCES [Services]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ServiceImages';
END
GO

-- ServiceReviews
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ServiceReviews] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ServiceId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [AppointmentId] int NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [ImageUrls] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ServiceReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceReviews_Services] FOREIGN KEY ([ServiceId]) REFERENCES [Services]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ServiceReviews';
END
GO

-- ==========================================
-- ORDERS & PAYMENTS TABLES
-- ==========================================

-- Carts
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Carts] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [TotalItems] int NOT NULL DEFAULT 0,
        [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Carts_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Carts';
END
GO

-- CartItems
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CartItems] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CartId] int NOT NULL,
        [ProductId] int NOT NULL,
        [VariantId] int NULL,
        [Quantity] int NOT NULL DEFAULT 1,
        [Price] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_Carts] FOREIGN KEY ([CartId]) REFERENCES [Carts]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE NO ACTION
    );
    PRINT 'Created table: CartItems';
END
GO

-- Payments
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments] (
        [Id] int NOT NULL IDENTITY(1,1),
        [OrderId] int NOT NULL,
        [PaymentMethodId] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [TransactionId] nvarchar(100) NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT 'Pending',
        [PaidAt] datetime2 NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Payments';
END
GO

-- Coupons
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Coupons] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DiscountType] nvarchar(20) NOT NULL DEFAULT 'Percentage',
        [DiscountValue] decimal(18,2) NOT NULL,
        [MinOrderAmount] decimal(18,2) NULL,
        [MaxDiscountAmount] decimal(18,2) NULL,
        [UsageLimit] int NULL,
        [UsedCount] int NOT NULL DEFAULT 0,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: Coupons';
END
GO

-- CouponUsages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CouponUsages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CouponUsages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CouponId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [OrderId] int NOT NULL,
        [UsedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_CouponUsages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CouponUsages_Coupons] FOREIGN KEY ([CouponId]) REFERENCES [Coupons]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: CouponUsages';
END
GO

-- ==========================================
-- BLOG & SOCIAL TABLES
-- ==========================================

-- Posts
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Posts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Posts] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [ImageUrls] nvarchar(max) NULL,
        [Privacy] nvarchar(20) NOT NULL DEFAULT 'Public',
        [LikeCount] int NOT NULL DEFAULT 0,
        [CommentCount] int NOT NULL DEFAULT 0,
        [ShareCount] int NOT NULL DEFAULT 0,
        [IsEdited] bit NOT NULL DEFAULT 0,
        [EditedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Posts_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Posts';
END
GO

-- PostImages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostImages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [PostId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_PostImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostImages_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: PostImages';
END
GO

-- Comments
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Comments] (
        [Id] int NOT NULL IDENTITY(1,1),
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ParentCommentId] int NULL,
        [Content] nvarchar(2000) NOT NULL,
        [LikeCount] int NOT NULL DEFAULT 0,
        [ReplyCount] int NOT NULL DEFAULT 0,
        [IsEdited] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Comments';
END
GO

-- Likes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Likes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Likes] (
        [Id] int NOT NULL IDENTITY(1,1),
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Likes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Likes_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Likes';
END
GO

-- CommentLikes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommentLikes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CommentLikes] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CommentId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_CommentLikes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommentLikes_Comments] FOREIGN KEY ([CommentId]) REFERENCES [Comments]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: CommentLikes';
END
GO

-- Shares
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shares]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Shares] (
        [Id] int NOT NULL IDENTITY(1,1),
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Caption] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Shares] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Shares_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Shares';
END
GO

-- Bookmarks
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bookmarks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bookmarks] (
        [Id] int NOT NULL IDENTITY(1,1),
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Bookmarks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookmarks_Posts] FOREIGN KEY ([PostId]) REFERENCES [Posts]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Bookmarks';
END
GO

-- Follows
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Follows]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Follows] (
        [Id] int NOT NULL IDENTITY(1,1),
        [FollowerId] nvarchar(450) NOT NULL,
        [FollowingId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Follows] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: Follows';
END
GO

-- Conversations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Conversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Conversations] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Title] nvarchar(200) NULL,
        [IsGroup] bit NOT NULL DEFAULT 0,
        [LastMessageAt] datetime2 NULL,
        [LastMessageText] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: Conversations';
END
GO

-- ConversationParticipants
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConversationParticipants]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConversationParticipants] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ConversationId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [UnreadCount] int NOT NULL DEFAULT 0,
        [IsMuted] bit NOT NULL DEFAULT 0,
        [JoinedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [LeftAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ConversationParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConversationParticipants_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ConversationParticipants';
END
GO

-- Messages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Messages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ConversationId] int NOT NULL,
        [SenderId] nvarchar(450) NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [MessageType] nvarchar(20) NOT NULL DEFAULT 'Text',
        [AttachmentUrl] nvarchar(500) NULL,
        [FileName] nvarchar(255) NULL,
        [FileSize] bigint NULL,
        [IsEdited] bit NOT NULL DEFAULT 0,
        [EditedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Messages';
END
GO

-- MessageReadStatuses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MessageReadStatuses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MessageReadStatuses] (
        [Id] int NOT NULL IDENTITY(1,1),
        [MessageId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ReadAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_MessageReadStatuses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MessageReadStatuses_Messages] FOREIGN KEY ([MessageId]) REFERENCES [Messages]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: MessageReadStatuses';
END
GO

-- ==========================================
-- AI CHATBOT TABLES
-- ==========================================

-- ChatSessions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatSessions] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NULL,
        [SessionId] nvarchar(100) NOT NULL,
        [Title] nvarchar(200) NULL,
        [LastMessageAt] datetime2 NULL,
        [MessageCount] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: ChatSessions';
END
GO

-- ChatMessages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [SessionId] int NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Model] nvarchar(50) NULL,
        [TokenCount] int NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_ChatSessions] FOREIGN KEY ([SessionId]) REFERENCES [ChatSessions]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: ChatMessages';
END
GO

-- FaceAnalyses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FaceAnalyses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FaceAnalyses] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [FaceShape] nvarchar(50) NULL,
        [SkinTone] nvarchar(50) NULL,
        [FaceFeatures] nvarchar(max) NULL,
        [Recommendations] nvarchar(max) NULL,
        [AnalysisData] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_FaceAnalyses] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: FaceAnalyses';
END
GO

-- HairStyleTryOns
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HairStyleTryOns]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HairStyleTryOns] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NULL,
        [OriginalImageUrl] nvarchar(500) NOT NULL,
        [ResultImageUrl] nvarchar(500) NOT NULL,
        [HairStyleName] nvarchar(100) NULL,
        [HairColor] nvarchar(50) NULL,
        [Prompt] nvarchar(max) NULL,
        [Model] nvarchar(100) NULL,
        [GenerationData] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_HairStyleTryOns] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: HairStyleTryOns';
END
GO

-- ==========================================
-- NOTIFICATIONS & SYSTEM TABLES
-- ==========================================

-- StoreInfos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreInfos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StoreInfos] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(max) NULL,
        [Category] nvarchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_StoreInfos] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: StoreInfos';
END
GO

-- BannedWords
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannedWords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BannedWords] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Word] nvarchar(100) NOT NULL,
        [Category] nvarchar(30) NOT NULL DEFAULT 'General',
        [SeverityLevel] int NOT NULL DEFAULT 1,
        [Action] nvarchar(20) NOT NULL DEFAULT 'Replace',
        [Replacement] nvarchar(100) NULL,
        [IsRegex] bit NOT NULL DEFAULT 0,
        [IsCaseSensitive] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [MatchCount] int NOT NULL DEFAULT 0,
        [Language] nvarchar(10) NULL DEFAULT 'vi',
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_BannedWords] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: BannedWords';
END
GO

-- Reports
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Reports] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ReporterId] nvarchar(450) NOT NULL,
        [ReportedEntityType] nvarchar(50) NOT NULL,
        [ReportedEntityId] int NOT NULL,
        [Reason] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT 'Pending',
        [ReviewedAt] datetime2 NULL,
        [ReviewedBy] nvarchar(450) NULL,
        [ReviewNote] nvarchar(500) NULL,
        [ActionTaken] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Reports] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: Reports';
END
GO

-- AuditLogs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NULL,
        [UserName] nvarchar(100) NULL,
        [Action] nvarchar(30) NOT NULL,
        [EntityType] nvarchar(50) NULL,
        [EntityId] nvarchar(50) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [AffectedColumns] nvarchar(500) NULL,
        [Description] nvarchar(500) NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [RequestUrl] nvarchar(500) NULL,
        [HttpMethod] nvarchar(10) NULL,
        [ResponseStatusCode] int NULL,
        [ExecutionTimeMs] int NULL,
        [Module] nvarchar(50) NULL,
        [IsError] bit NOT NULL DEFAULT 0,
        [ErrorMessage] nvarchar(1000) NULL,
        [StackTrace] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: AuditLogs';
END
GO

-- Banners
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Banners]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Banners] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Title] nvarchar(200) NOT NULL,
        [Subtitle] nvarchar(300) NULL,
        [Description] nvarchar(500) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [MobileImageUrl] nvarchar(500) NULL,
        [LinkUrl] nvarchar(500) NULL,
        [OpenInNewTab] bit NOT NULL DEFAULT 0,
        [ButtonText] nvarchar(50) NULL,
        [ButtonColor] nvarchar(10) NULL,
        [Position] nvarchar(50) NOT NULL DEFAULT 'Home',
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [ViewCount] int NOT NULL DEFAULT 0,
        [ClickCount] int NOT NULL DEFAULT 0,
        [AltText] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Banners] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: Banners';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'All missing tables creation completed!';
PRINT '=============================================';
