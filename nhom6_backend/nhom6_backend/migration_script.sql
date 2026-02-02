IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(100) NULL,
        [Initials] nvarchar(5) NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [Address] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
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
        [IsError] bit NOT NULL,
        [ErrorMessage] nvarchar(1000) NULL,
        [StackTrace] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [BannedWords] (
        [Id] int NOT NULL IDENTITY,
        [Word] nvarchar(100) NOT NULL,
        [Category] nvarchar(30) NOT NULL,
        [SeverityLevel] int NOT NULL,
        [Action] nvarchar(20) NOT NULL,
        [Replacement] nvarchar(100) NULL,
        [IsRegex] bit NOT NULL,
        [IsCaseSensitive] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MatchCount] int NOT NULL,
        [Language] nvarchar(10) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BannedWords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Banners] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Subtitle] nvarchar(300) NULL,
        [Description] nvarchar(500) NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [MobileImageUrl] nvarchar(500) NULL,
        [LinkUrl] nvarchar(500) NULL,
        [OpenInNewTab] bit NOT NULL,
        [ButtonText] nvarchar(50) NULL,
        [ButtonColor] nvarchar(10) NULL,
        [Position] nvarchar(50) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [ViewCount] int NOT NULL,
        [ClickCount] int NOT NULL,
        [AltText] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Banners] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Brands] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(150) NULL,
        [Description] nvarchar(1000) NULL,
        [LogoUrl] nvarchar(500) NULL,
        [BannerUrl] nvarchar(500) NULL,
        [WebsiteUrl] nvarchar(200) NULL,
        [CountryOfOrigin] nvarchar(100) NULL,
        [YearEstablished] int NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsFeatured] bit NOT NULL,
        [ProductCount] int NOT NULL,
        [MetaTitle] nvarchar(200) NULL,
        [MetaDescription] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(150) NULL,
        [Description] nvarchar(500) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [Icon] nvarchar(50) NULL,
        [ParentCategoryId] int NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [ShowOnHomePage] bit NOT NULL,
        [ProductCount] int NOT NULL,
        [MetaTitle] nvarchar(200) NULL,
        [MetaDescription] nvarchar(500) NULL,
        [MetaKeywords] nvarchar(300) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Coupons] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DiscountType] nvarchar(20) NOT NULL,
        [DiscountValue] decimal(18,2) NOT NULL,
        [MaxDiscountAmount] decimal(18,2) NULL,
        [MinOrderAmount] decimal(18,2) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [MaxUsageCount] int NULL,
        [UsedCount] int NOT NULL,
        [MaxUsagePerUser] int NOT NULL,
        [ApplicableProductIds] nvarchar(max) NULL,
        [ApplicableCategoryIds] nvarchar(max) NULL,
        [ApplicableBrandIds] nvarchar(max) NULL,
        [ApplicableServiceIds] nvarchar(max) NULL,
        [ApplicableType] nvarchar(20) NOT NULL,
        [ForNewCustomersOnly] bit NOT NULL,
        [ForMembershipTiers] nvarchar(max) NULL,
        [Status] nvarchar(20) NOT NULL,
        [IsStackable] bit NOT NULL,
        [IsPublic] bit NOT NULL,
        [AutoApply] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [PaymentMethods] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Instructions] nvarchar(1000) NULL,
        [IconUrl] nvarchar(500) NULL,
        [Type] nvarchar(30) NOT NULL,
        [TransactionFeePercent] decimal(5,2) NOT NULL,
        [FixedFee] decimal(18,2) NOT NULL,
        [MinAmount] decimal(18,2) NULL,
        [MaxAmount] decimal(18,2) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [Configuration] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PaymentMethods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(150) NULL,
        [Description] nvarchar(500) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [Icon] nvarchar(50) NULL,
        [Color] nvarchar(10) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ShippingMethods] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IconUrl] nvarchar(500) NULL,
        [Type] nvarchar(30) NOT NULL,
        [BaseFee] decimal(18,2) NOT NULL,
        [FeePerKg] decimal(18,2) NOT NULL,
        [FreeShippingMinAmount] decimal(18,2) NULL,
        [EstimatedDays] int NOT NULL,
        [MinDays] int NOT NULL,
        [MaxDays] int NOT NULL,
        [MaxWeight] decimal(10,2) NULL,
        [SupportedProvinces] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CarrierCode] nvarchar(50) NULL,
        [ApiConfiguration] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ShippingMethods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [StoreInfos] (
        [Id] int NOT NULL IDENTITY,
        [StoreCode] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slogan] nvarchar(300) NULL,
        [Description] nvarchar(max) NULL,
        [LogoUrl] nvarchar(500) NULL,
        [BannerUrl] nvarchar(500) NULL,
        [FaviconUrl] nvarchar(500) NULL,
        [Address] nvarchar(500) NULL,
        [ProvinceCode] nvarchar(10) NULL,
        [DistrictCode] nvarchar(10) NULL,
        [WardCode] nvarchar(10) NULL,
        [Latitude] float NULL,
        [Longitude] float NULL,
        [PhoneNumber] nvarchar(15) NULL,
        [Hotline] nvarchar(15) NULL,
        [Email] nvarchar(100) NULL,
        [Website] nvarchar(200) NULL,
        [FacebookUrl] nvarchar(200) NULL,
        [InstagramUrl] nvarchar(200) NULL,
        [TikTokUrl] nvarchar(200) NULL,
        [YouTubeUrl] nvarchar(200) NULL,
        [ZaloNumber] nvarchar(50) NULL,
        [BusinessHours] nvarchar(max) NULL,
        [TaxCode] nvarchar(20) NULL,
        [BankName] nvarchar(100) NULL,
        [BankAccountNumber] nvarchar(30) NULL,
        [BankAccountName] nvarchar(100) NULL,
        [VietQRData] nvarchar(500) NULL,
        [Currency] nvarchar(3) NOT NULL,
        [TimeZone] nvarchar(50) NOT NULL,
        [MetaTitle] nvarchar(200) NULL,
        [MetaDescription] nvarchar(500) NULL,
        [MetaKeywords] nvarchar(300) NULL,
        [GoogleAnalyticsId] nvarchar(50) NULL,
        [FacebookPixelId] nvarchar(50) NULL,
        [Status] nvarchar(20) NOT NULL,
        [Announcement] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StoreInfos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatSessions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [GuestSessionId] nvarchar(100) NULL,
        [Title] nvarchar(200) NULL,
        [SessionType] nvarchar(30) NOT NULL,
        [AiModel] nvarchar(50) NULL,
        [MessageCount] int NOT NULL,
        [TotalTokensUsed] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [LastMessageAt] datetime2 NULL,
        [Context] nvarchar(max) NULL,
        [RelatedAppointmentId] int NULL,
        [Rating] int NULL,
        [Feedback] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatSessions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Conversations] (
        [Id] int NOT NULL IDENTITY,
        [Type] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [CreatorId] nvarchar(450) NULL,
        [LastMessageId] int NULL,
        [LastMessageContent] nvarchar(200) NULL,
        [LastMessageAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Conversations_AspNetUsers_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Follows] (
        [Id] int NOT NULL IDENTITY,
        [FollowerId] nvarchar(450) NOT NULL,
        [FollowingId] nvarchar(450) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [NotificationsEnabled] bit NOT NULL,
        [IsCloseFriend] bit NOT NULL,
        [IsMuted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Follows] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Follows_AspNetUsers_FollowerId] FOREIGN KEY ([FollowerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Follows_AspNetUsers_FollowingId] FOREIGN KEY ([FollowingId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Type] nvarchar(30) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(500) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [ActionUrl] nvarchar(500) NULL,
        [ReferenceType] nvarchar(50) NULL,
        [ReferenceId] nvarchar(50) NULL,
        [SenderUserId] nvarchar(450) NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [IsPushSent] bit NOT NULL,
        [PushSentAt] datetime2 NULL,
        [IsEmailSent] bit NOT NULL,
        [ExpiresAt] datetime2 NULL,
        [Metadata] nvarchar(max) NULL,
        [Priority] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_SenderUserId] FOREIGN KEY ([SenderUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Posts] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(300) NULL,
        [Slug] nvarchar(350) NULL,
        [Content] nvarchar(max) NULL,
        [PlainTextContent] nvarchar(max) NULL,
        [PostType] nvarchar(20) NOT NULL,
        [Visibility] nvarchar(20) NOT NULL,
        [ViewCount] int NOT NULL,
        [LikeCount] int NOT NULL,
        [CommentCount] int NOT NULL,
        [ShareCount] int NOT NULL,
        [BookmarkCount] int NOT NULL,
        [AllowComments] bit NOT NULL,
        [IsPinned] bit NOT NULL,
        [IsFeatured] bit NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [PublishedAt] datetime2 NULL,
        [EditedAt] datetime2 NULL,
        [Tags] nvarchar(500) NULL,
        [Hashtags] nvarchar(500) NULL,
        [Location] nvarchar(200) NULL,
        [Feeling] nvarchar(100) NULL,
        [OriginalPostId] int NULL,
        [IsReported] bit NOT NULL,
        [ReportCount] int NOT NULL,
        [IsHiddenByAdmin] bit NOT NULL,
        [HiddenReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Posts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Posts_Posts_OriginalPostId] FOREIGN KEY ([OriginalPostId]) REFERENCES [Posts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Token] nvarchar(500) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        [CreatedByIp] nvarchar(50) NULL,
        [RevokedByIp] nvarchar(50) NULL,
        [ReplacedByToken] nvarchar(500) NULL,
        [RevokeReason] nvarchar(200) NULL,
        [DeviceInfo] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Reports] (
        [Id] int NOT NULL IDENTITY,
        [ReporterUserId] nvarchar(450) NOT NULL,
        [ContentType] nvarchar(30) NOT NULL,
        [ContentId] nvarchar(50) NOT NULL,
        [ReportedUserId] nvarchar(450) NULL,
        [Reason] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Evidence] nvarchar(max) NULL,
        [Status] nvarchar(20) NOT NULL,
        [ReviewedByUserId] nvarchar(450) NULL,
        [ReviewedAt] datetime2 NULL,
        [ActionTaken] nvarchar(100) NULL,
        [AdminNotes] nvarchar(500) NULL,
        [AssessedSeverity] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reports_AspNetUsers_ReportedUserId] FOREIGN KEY ([ReportedUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Reports_AspNetUsers_ReporterUserId] FOREIGN KEY ([ReporterUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reports_AspNetUsers_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Staff] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [StaffCode] nvarchar(20) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [NickName] nvarchar(50) NULL,
        [Email] nvarchar(100) NULL,
        [PhoneNumber] nvarchar(15) NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [CoverImageUrl] nvarchar(500) NULL,
        [Bio] nvarchar(1000) NULL,
        [Position] nvarchar(50) NOT NULL,
        [Level] nvarchar(20) NOT NULL,
        [Specialties] nvarchar(500) NULL,
        [YearsOfExperience] int NOT NULL,
        [DateOfBirth] datetime2 NULL,
        [Gender] nvarchar(10) NULL,
        [HireDate] datetime2 NOT NULL,
        [BaseSalary] decimal(18,2) NULL,
        [CommissionPercent] int NOT NULL,
        [AverageRating] decimal(3,2) NOT NULL,
        [TotalReviews] int NOT NULL,
        [TotalCustomersServed] int NOT NULL,
        [TotalRevenue] decimal(18,2) NOT NULL,
        [FacebookUrl] nvarchar(200) NULL,
        [InstagramUrl] nvarchar(200) NULL,
        [TikTokUrl] nvarchar(200) NULL,
        [Status] nvarchar(20) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [AcceptOnlineBooking] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Staff] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Staff_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [UserAddresses] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Label] nvarchar(50) NOT NULL,
        [ReceiverName] nvarchar(100) NOT NULL,
        [ReceiverPhone] nvarchar(15) NOT NULL,
        [ProvinceCode] nvarchar(10) NULL,
        [ProvinceName] nvarchar(100) NULL,
        [DistrictCode] nvarchar(10) NULL,
        [DistrictName] nvarchar(100) NULL,
        [WardCode] nvarchar(10) NULL,
        [WardName] nvarchar(100) NULL,
        [AddressDetail] nvarchar(300) NOT NULL,
        [FullAddress] nvarchar(500) NULL,
        [Latitude] float NULL,
        [Longitude] float NULL,
        [IsDefault] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserAddresses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [UserBans] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [BannedByUserId] nvarchar(450) NULL,
        [BanType] nvarchar(20) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [UnbannedAt] datetime2 NULL,
        [UnbannedByUserId] nvarchar(max) NULL,
        [UnbanReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserBans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserBans_AspNetUsers_BannedByUserId] FOREIGN KEY ([BannedByUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_UserBans_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [UserLoginHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [LoginTime] datetime2 NOT NULL,
        [LogoutTime] datetime2 NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [DeviceType] nvarchar(20) NULL,
        [DeviceName] nvarchar(100) NULL,
        [OperatingSystem] nvarchar(50) NULL,
        [Browser] nvarchar(50) NULL,
        [Location] nvarchar(100) NULL,
        [Country] nvarchar(50) NULL,
        [IsSuccessful] bit NOT NULL,
        [FailureReason] nvarchar(200) NULL,
        [LoginMethod] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserLoginHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserLoginHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [UserProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [DateOfBirth] datetime2 NULL,
        [Gender] nvarchar(10) NULL,
        [IdentityNumber] nvarchar(20) NULL,
        [Occupation] nvarchar(100) NULL,
        [Bio] nvarchar(500) NULL,
        [FacebookUrl] nvarchar(200) NULL,
        [InstagramUrl] nvarchar(200) NULL,
        [TikTokUrl] nvarchar(200) NULL,
        [FaceShape] nvarchar(50) NULL,
        [PreferredHairStyle] nvarchar(100) NULL,
        [HairConditionNotes] nvarchar(500) NULL,
        [LoyaltyPoints] int NOT NULL,
        [MembershipTier] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [UserWarnings] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [WarnedByUserId] nvarchar(450) NULL,
        [ViolationType] nvarchar(50) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [SeverityLevel] int NOT NULL,
        [PenaltyPoints] int NOT NULL,
        [RelatedContentType] nvarchar(100) NULL,
        [RelatedContentId] int NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserWarnings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserWarnings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserWarnings_AspNetUsers_WarnedByUserId] FOREIGN KEY ([WarnedByUserId]) REFERENCES [AspNetUsers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [SKU] nvarchar(50) NOT NULL,
        [Barcode] nvarchar(50) NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(250) NULL,
        [ShortDescription] nvarchar(500) NULL,
        [Description] nvarchar(max) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [AdditionalImages] nvarchar(max) NULL,
        [VideoUrl] nvarchar(500) NULL,
        [OriginalPrice] decimal(18,2) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [DiscountPercent] int NOT NULL,
        [CostPrice] decimal(18,2) NULL,
        [StockQuantity] int NOT NULL,
        [LowStockThreshold] int NOT NULL,
        [CategoryId] int NULL,
        [BrandId] int NULL,
        [Weight] int NULL,
        [Volume] int NULL,
        [Unit] nvarchar(20) NULL,
        [Ingredients] nvarchar(1000) NULL,
        [Usage] nvarchar(1000) NULL,
        [Warnings] nvarchar(500) NULL,
        [ManufactureDate] datetime2 NULL,
        [ExpiryDate] datetime2 NULL,
        [Origin] nvarchar(100) NULL,
        [AverageRating] decimal(3,2) NOT NULL,
        [TotalReviews] int NOT NULL,
        [ViewCount] int NOT NULL,
        [SoldCount] int NOT NULL,
        [WishlistCount] int NOT NULL,
        [IsFeatured] bit NOT NULL,
        [IsBestSeller] bit NOT NULL,
        [IsNew] bit NOT NULL,
        [IsOnSale] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [AllowBackorder] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [MetaTitle] nvarchar(200) NULL,
        [MetaDescription] nvarchar(500) NULL,
        [MetaKeywords] nvarchar(300) NULL,
        [Tags] nvarchar(500) NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Carts] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [SessionId] nvarchar(100) NULL,
        [TotalItems] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [CouponId] int NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [ExpiresAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Carts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Carts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Carts_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Services] (
        [Id] int NOT NULL IDENTITY,
        [ServiceCode] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(250) NULL,
        [ShortDescription] nvarchar(500) NULL,
        [Description] nvarchar(max) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [GalleryImages] nvarchar(max) NULL,
        [VideoUrl] nvarchar(500) NULL,
        [ServiceCategoryId] int NULL,
        [Price] decimal(18,2) NOT NULL,
        [OriginalPrice] decimal(18,2) NULL,
        [MinPrice] decimal(18,2) NULL,
        [MaxPrice] decimal(18,2) NULL,
        [DurationMinutes] int NOT NULL,
        [BufferMinutes] int NOT NULL,
        [RequiredStaff] int NOT NULL,
        [Gender] nvarchar(10) NOT NULL,
        [RequiredAdvanceBookingHours] int NULL,
        [CancellationHours] int NULL,
        [IsFeatured] bit NOT NULL,
        [IsPopular] bit NOT NULL,
        [IsNew] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [AverageRating] decimal(3,2) NOT NULL,
        [TotalReviews] int NOT NULL,
        [TotalBookings] int NOT NULL,
        [Notes] nvarchar(500) NULL,
        [Warnings] nvarchar(500) NULL,
        [IncludedServices] nvarchar(max) NULL,
        [Tags] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Services_ServiceCategories_ServiceCategoryId] FOREIGN KEY ([ServiceCategoryId]) REFERENCES [ServiceCategories] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [ChatSessionId] int NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Content] nvarchar(max) NULL,
        [MessageType] nvarchar(30) NOT NULL,
        [Images] nvarchar(max) NULL,
        [SuggestedProductIds] nvarchar(max) NULL,
        [SuggestedServiceIds] nvarchar(max) NULL,
        [ActionButtons] nvarchar(max) NULL,
        [TokensUsed] int NULL,
        [ResponseTimeMs] int NULL,
        [DetectedIntent] nvarchar(100) NULL,
        [IntentConfidence] decimal(5,4) NULL,
        [IsHelpful] bit NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_ChatSessions_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [FaceAnalyses] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [ChatSessionId] int NULL,
        [OriginalImageUrl] nvarchar(500) NOT NULL,
        [ProcessedImageUrl] nvarchar(500) NULL,
        [FaceShape] nvarchar(30) NULL,
        [FaceShapeConfidence] decimal(5,4) NULL,
        [ForeheadType] nvarchar(30) NULL,
        [EyeShape] nvarchar(30) NULL,
        [NoseShape] nvarchar(30) NULL,
        [LipShape] nvarchar(30) NULL,
        [ChinShape] nvarchar(30) NULL,
        [SkinTone] nvarchar(30) NULL,
        [PredictedGender] nvarchar(10) NULL,
        [PredictedAge] int NULL,
        [FaceLandmarks] nvarchar(max) NULL,
        [FaceProportions] nvarchar(max) NULL,
        [SuggestedHairStyles] nvarchar(max) NULL,
        [SuggestionReasons] nvarchar(max) NULL,
        [SuggestedServiceIds] nvarchar(max) NULL,
        [RawApiResponse] nvarchar(max) NULL,
        [AiModel] nvarchar(50) NULL,
        [ProcessingTimeMs] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [ErrorMessage] nvarchar(500) NULL,
        [IsAccurate] bit NULL,
        [FeedbackNotes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FaceAnalyses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FaceAnalyses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_FaceAnalyses_ChatSessions_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSessions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [HairStyleTryOns] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [ChatSessionId] int NULL,
        [FaceImageUrl] nvarchar(500) NOT NULL,
        [HairStyleImageUrl] nvarchar(500) NOT NULL,
        [ResultImageUrl] nvarchar(500) NULL,
        [HairStyleName] nvarchar(100) NULL,
        [HairColor] nvarchar(50) NULL,
        [ProcessingTimeMs] int NULL,
        [AiModel] nvarchar(50) NULL,
        [Status] nvarchar(20) NOT NULL,
        [ErrorMessage] nvarchar(500) NULL,
        [IsSaved] bit NOT NULL,
        [IsShared] bit NOT NULL,
        [Rating] int NULL,
        [RelatedAppointmentId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HairStyleTryOns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HairStyleTryOns_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_HairStyleTryOns_ChatSessions_ChatSessionId] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSessions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ConversationParticipants] (
        [Id] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Nickname] nvarchar(50) NULL,
        [JoinedAt] datetime2 NOT NULL,
        [LastReadAt] datetime2 NULL,
        [LastReadMessageId] int NULL,
        [UnreadCount] int NOT NULL,
        [IsMuted] bit NOT NULL,
        [HasLeft] bit NOT NULL,
        [LeftAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ConversationParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConversationParticipants_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ConversationParticipants_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Messages] (
        [Id] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [SenderId] nvarchar(450) NOT NULL,
        [MessageType] nvarchar(20) NOT NULL,
        [Content] nvarchar(4000) NULL,
        [MediaUrl] nvarchar(500) NULL,
        [ThumbnailUrl] nvarchar(500) NULL,
        [FileName] nvarchar(200) NULL,
        [FileSize] bigint NULL,
        [Duration] int NULL,
        [ReplyToMessageId] int NULL,
        [ForwardedFromMessageId] int NULL,
        [IsEdited] bit NOT NULL,
        [EditedAt] datetime2 NULL,
        [IsDeletedBySender] bit NOT NULL,
        [IsDeletedForAll] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [IsSystemMessage] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Messages_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Messages_Messages_ReplyToMessageId] FOREIGN KEY ([ReplyToMessageId]) REFERENCES [Messages] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookmarks] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CollectionName] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Bookmarks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookmarks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookmarks_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ParentCommentId] int NULL,
        [Content] nvarchar(2000) NOT NULL,
        [Images] nvarchar(max) NULL,
        [GifUrl] nvarchar(500) NULL,
        [LikeCount] int NOT NULL,
        [ReplyCount] int NOT NULL,
        [IsEdited] bit NOT NULL,
        [EditedAt] datetime2 NULL,
        [IsHidden] bit NOT NULL,
        [IsReported] bit NOT NULL,
        [ReportCount] int NOT NULL,
        [IsPinned] bit NOT NULL,
        [HiddenReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [Comments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Likes] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ReactionType] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Likes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Likes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Likes_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [PostImages] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [ThumbnailUrl] nvarchar(500) NULL,
        [AltText] nvarchar(200) NULL,
        [Caption] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [Width] int NULL,
        [Height] int NULL,
        [FileSize] bigint NULL,
        [MediaType] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PostImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PostImages_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Shares] (
        [Id] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [SharedPostId] int NULL,
        [ShareType] nvarchar(20) NOT NULL,
        [Caption] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Shares] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Shares_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Shares_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Shares_Posts_SharedPostId] FOREIGN KEY ([SharedPostId]) REFERENCES [Posts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] int NOT NULL IDENTITY,
        [AppointmentCode] nvarchar(20) NOT NULL,
        [UserId] nvarchar(450) NULL,
        [GuestName] nvarchar(100) NULL,
        [GuestPhone] nvarchar(15) NULL,
        [GuestEmail] nvarchar(100) NULL,
        [StaffId] int NULL,
        [AppointmentDate] datetime2 NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [TotalDurationMinutes] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CancellationReason] nvarchar(500) NULL,
        [CancelledBy] nvarchar(20) NULL,
        [CancelledAt] datetime2 NULL,
        [CustomerNotes] nvarchar(500) NULL,
        [InternalNotes] nvarchar(500) NULL,
        [BookingSource] nvarchar(20) NOT NULL,
        [ReminderSent] bit NOT NULL,
        [ReminderSentAt] datetime2 NULL,
        [CustomerConfirmed] bit NOT NULL,
        [ConfirmedAt] datetime2 NULL,
        [CheckInTime] datetime2 NULL,
        [CheckOutTime] datetime2 NULL,
        [IsReviewed] bit NOT NULL,
        [RecurrenceType] nvarchar(20) NOT NULL,
        [ParentAppointmentId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Appointments_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffSchedules] (
        [Id] int NOT NULL IDENTITY,
        [StaffId] int NOT NULL,
        [DayOfWeek] int NOT NULL,
        [SpecificDate] datetime2 NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [BreakStartTime] time NULL,
        [BreakEndTime] time NULL,
        [IsWorking] bit NOT NULL,
        [IsLeave] bit NOT NULL,
        [LeaveReason] nvarchar(200) NULL,
        [MaxAppointmentsPerDay] int NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StaffSchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffSchedules_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [OrderCode] nvarchar(20) NOT NULL,
        [UserId] nvarchar(450) NULL,
        [CustomerName] nvarchar(100) NOT NULL,
        [CustomerEmail] nvarchar(100) NULL,
        [CustomerPhone] nvarchar(15) NOT NULL,
        [ShippingAddressId] int NULL,
        [ShippingAddressText] nvarchar(500) NULL,
        [ReceiverName] nvarchar(100) NULL,
        [ReceiverPhone] nvarchar(15) NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [ShippingFee] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [CouponId] int NULL,
        [CouponCode] nvarchar(50) NULL,
        [PaymentMethodId] int NULL,
        [PaymentMethodName] nvarchar(50) NULL,
        [PaymentStatus] nvarchar(20) NOT NULL,
        [ShippingMethodId] int NULL,
        [ShippingMethodName] nvarchar(100) NULL,
        [TrackingNumber] nvarchar(50) NULL,
        [Status] nvarchar(20) NOT NULL,
        [CustomerNotes] nvarchar(500) NULL,
        [InternalNotes] nvarchar(500) NULL,
        [CancellationReason] nvarchar(500) NULL,
        [CancelledBy] nvarchar(20) NULL,
        [CancelledAt] datetime2 NULL,
        [ConfirmedAt] datetime2 NULL,
        [ShippedAt] datetime2 NULL,
        [DeliveredAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [EstimatedDeliveryDate] datetime2 NULL,
        [OrderSource] nvarchar(20) NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [TotalWeight] int NULL,
        [UsedLoyaltyPoints] int NOT NULL,
        [EarnedLoyaltyPoints] int NOT NULL,
        [IsInvoicePrinted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Orders_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id]),
        CONSTRAINT [FK_Orders_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([Id]),
        CONSTRAINT [FK_Orders_ShippingMethods_ShippingMethodId] FOREIGN KEY ([ShippingMethodId]) REFERENCES [ShippingMethods] ([Id]),
        CONSTRAINT [FK_Orders_UserAddresses_ShippingAddressId] FOREIGN KEY ([ShippingAddressId]) REFERENCES [UserAddresses] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ProductImages] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [ImageName] nvarchar(200) NULL,
        [AltText] nvarchar(200) NULL,
        [DisplayOrder] int NOT NULL,
        [IsPrimary] bit NOT NULL,
        [FileSize] bigint NULL,
        [Width] int NULL,
        [Height] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ProductVariants] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [SKU] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [VariantType] nvarchar(50) NOT NULL,
        [VariantValue] nvarchar(100) NULL,
        [OriginalPrice] decimal(18,2) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [StockQuantity] int NOT NULL,
        [Weight] int NULL,
        [Volume] int NULL,
        [ImageUrl] nvarchar(500) NULL,
        [Barcode] nvarchar(50) NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Wishlists] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ProductId] int NOT NULL,
        [Notes] nvarchar(500) NULL,
        [NotifyOnSale] bit NOT NULL,
        [NotifyOnStock] bit NOT NULL,
        [Priority] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Wishlists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wishlists_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Wishlists_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceImages] (
        [Id] int NOT NULL IDENTITY,
        [ServiceId] int NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [ImageType] nvarchar(20) NOT NULL,
        [Title] nvarchar(200) NULL,
        [Description] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL,
        [IsPrimary] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceImages_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffServices] (
        [Id] int NOT NULL IDENTITY,
        [StaffId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ProficiencyLevel] int NOT NULL,
        [CustomPrice] decimal(18,2) NULL,
        [CustomDurationMinutes] int NULL,
        [TotalPerformed] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StaffServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StaffServices_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [MessageReadStatuses] (
        [Id] int NOT NULL IDENTITY,
        [MessageId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ReadAt] datetime2 NOT NULL,
        [IsDelivered] bit NOT NULL,
        [DeliveredAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MessageReadStatuses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MessageReadStatuses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MessageReadStatuses_Messages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Messages] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [CommentLikes] (
        [Id] int NOT NULL IDENTITY,
        [CommentId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ReactionType] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CommentLikes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommentLikes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CommentLikes_Comments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [AppointmentServices] (
        [Id] int NOT NULL IDENTITY,
        [AppointmentId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ServiceName] nvarchar(200) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [DurationMinutes] int NOT NULL,
        [ServiceOrder] int NOT NULL,
        [Notes] nvarchar(500) NULL,
        [PerformedByStaffId] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AppointmentServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppointmentServices_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppointmentServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceReviews] (
        [Id] int NOT NULL IDENTITY,
        [ServiceId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [AppointmentId] int NULL,
        [StaffId] int NULL,
        [ServiceRating] int NOT NULL,
        [StaffRating] int NULL,
        [FacilityRating] int NULL,
        [Title] nvarchar(200) NULL,
        [Content] nvarchar(2000) NULL,
        [BeforeImages] nvarchar(max) NULL,
        [AfterImages] nvarchar(max) NULL,
        [IsVerified] bit NOT NULL,
        [HelpfulCount] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SalonReply] nvarchar(1000) NULL,
        [SalonReplyAt] datetime2 NULL,
        [IsAnonymous] bit NOT NULL,
        [WouldRecommend] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceReviews_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]),
        CONSTRAINT [FK_ServiceReviews_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceReviews_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceReviews_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [CouponUsages] (
        [Id] int NOT NULL IDENTITY,
        [CouponId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [OrderId] int NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [UsedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CouponUsages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CouponUsages_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CouponUsages_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CouponUsages_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderStatusHistories] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [FromStatus] nvarchar(20) NULL,
        [ToStatus] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [ChangedByUserId] nvarchar(450) NULL,
        [Location] nvarchar(200) NULL,
        [IsAutomatic] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderStatusHistories_AspNetUsers_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_OrderStatusHistories_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [TransactionCode] nvarchar(50) NOT NULL,
        [OrderId] int NOT NULL,
        [PaymentMethodId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Fee] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [GatewayTransactionId] nvarchar(100) NULL,
        [GatewayResponse] nvarchar(max) NULL,
        [ErrorCode] nvarchar(50) NULL,
        [ErrorMessage] nvarchar(500) NULL,
        [PaidAt] datetime2 NULL,
        [BankAccountNumber] nvarchar(50) NULL,
        [BankName] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        [IpAddress] nvarchar(50) NULL,
        [RefundedByUserId] nvarchar(max) NULL,
        [RefundedAt] datetime2 NULL,
        [RefundReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payments_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [CartItems] (
        [Id] int NOT NULL IDENTITY,
        [CartId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductVariantId] int NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [IsSelected] bit NOT NULL,
        [Notes] nvarchar(300) NULL,
        [SavedForLater] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]),
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductVariantId] int NULL,
        [SKU] nvarchar(50) NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [VariantName] nvarchar(100) NULL,
        [ProductImageUrl] nvarchar(500) NULL,
        [OriginalPrice] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [Weight] int NULL,
        [Notes] nvarchar(300) NULL,
        [Status] nvarchar(20) NOT NULL,
        [ReturnedQuantity] int NOT NULL,
        [ReturnReason] nvarchar(500) NULL,
        [IsReviewed] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]),
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE TABLE [ProductReviews] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [OrderItemId] int NULL,
        [Rating] int NOT NULL,
        [Title] nvarchar(200) NULL,
        [Content] nvarchar(2000) NULL,
        [Pros] nvarchar(500) NULL,
        [Cons] nvarchar(500) NULL,
        [ReviewImages] nvarchar(max) NULL,
        [VideoUrl] nvarchar(500) NULL,
        [IsVerifiedPurchase] bit NOT NULL,
        [HelpfulCount] int NOT NULL,
        [NotHelpfulCount] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RejectionReason] nvarchar(500) NULL,
        [ApprovedByUserId] nvarchar(max) NULL,
        [ApprovedAt] datetime2 NULL,
        [AdminReply] nvarchar(1000) NULL,
        [AdminReplyAt] datetime2 NULL,
        [IsAnonymous] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProductReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductReviews_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductReviews_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [OrderItems] ([Id]),
        CONSTRAINT [FK_ProductReviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Appointments_AppointmentCode] ON [Appointments] ([AppointmentCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_AppointmentDate_StaffId] ON [Appointments] ([AppointmentDate], [StaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_StaffId] ON [Appointments] ([StaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_UserId] ON [Appointments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AppointmentServices_AppointmentId] ON [AppointmentServices] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AppointmentServices_ServiceId] ON [AppointmentServices] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Action] ON [AuditLogs] ([Action]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [AuditLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Banners_Position_IsActive_DisplayOrder] ON [Banners] ([Position], [IsActive], [DisplayOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bookmarks_PostId_UserId] ON [Bookmarks] ([PostId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookmarks_UserId] ON [Bookmarks] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Brands_Slug] ON [Brands] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_CartId] ON [CartItems] ([CartId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_ProductVariantId] ON [CartItems] ([ProductVariantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Carts_CouponId] ON [Carts] ([CouponId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Carts_UserId] ON [Carts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_ChatSessionId] ON [ChatMessages] ([ChatSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatSessions_UserId] ON [ChatSessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CommentLikes_CommentId_UserId] ON [CommentLikes] ([CommentId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CommentLikes_UserId] ON [CommentLikes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_ParentCommentId] ON [Comments] ([ParentCommentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_PostId] ON [Comments] ([PostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ConversationParticipants_ConversationId] ON [ConversationParticipants] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ConversationParticipants_UserId] ON [ConversationParticipants] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Conversations_CreatorId] ON [Conversations] ([CreatorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Coupons_Code] ON [Coupons] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CouponUsages_CouponId] ON [CouponUsages] ([CouponId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CouponUsages_OrderId] ON [CouponUsages] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CouponUsages_UserId] ON [CouponUsages] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaceAnalyses_ChatSessionId] ON [FaceAnalyses] ([ChatSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FaceAnalyses_UserId] ON [FaceAnalyses] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Follows_FollowerId_FollowingId] ON [Follows] ([FollowerId], [FollowingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Follows_FollowingId] ON [Follows] ([FollowingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HairStyleTryOns_ChatSessionId] ON [HairStyleTryOns] ([ChatSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HairStyleTryOns_UserId] ON [HairStyleTryOns] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Likes_PostId_UserId] ON [Likes] ([PostId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Likes_UserId] ON [Likes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MessageReadStatuses_MessageId] ON [MessageReadStatuses] ([MessageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MessageReadStatuses_UserId] ON [MessageReadStatuses] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_ConversationId] ON [Messages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_ReplyToMessageId] ON [Messages] ([ReplyToMessageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_SenderId] ON [Messages] ([SenderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_SenderUserId] ON [Notifications] ([SenderUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductVariantId] ON [OrderItems] ([ProductVariantId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_CouponId] ON [Orders] ([CouponId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_CreatedAt] ON [Orders] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderCode] ON [Orders] ([OrderCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_PaymentMethodId] ON [Orders] ([PaymentMethodId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_ShippingAddressId] ON [Orders] ([ShippingAddressId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_ShippingMethodId] ON [Orders] ([ShippingMethodId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderStatusHistories_ChangedByUserId] ON [OrderStatusHistories] ([ChangedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderStatusHistories_OrderId] ON [OrderStatusHistories] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentMethods_Code] ON [PaymentMethods] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_PaymentMethodId] ON [Payments] ([PaymentMethodId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_TransactionCode] ON [Payments] ([TransactionCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PostImages_PostId] ON [PostImages] ([PostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_CreatedAt] ON [Posts] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_OriginalPostId] ON [Posts] ([OriginalPostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_Slug] ON [Posts] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_Status] ON [Posts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Posts_UserId] ON [Posts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductImages_ProductId] ON [ProductImages] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductReviews_OrderItemId] ON [ProductReviews] ([OrderItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductReviews_ProductId] ON [ProductReviews] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductReviews_UserId] ON [ProductReviews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_Slug] ON [Products] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_ProductId] ON [ProductVariants] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_SKU] ON [ProductVariants] ([SKU]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reports_ReportedUserId] ON [Reports] ([ReportedUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reports_ReporterUserId] ON [Reports] ([ReporterUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reports_ReviewedByUserId] ON [Reports] ([ReviewedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceImages_ServiceId] ON [ServiceImages] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReviews_AppointmentId] ON [ServiceReviews] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReviews_ServiceId] ON [ServiceReviews] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReviews_StaffId] ON [ServiceReviews] ([StaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReviews_UserId] ON [ServiceReviews] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Services_ServiceCategoryId] ON [Services] ([ServiceCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Services_ServiceCode] ON [Services] ([ServiceCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Services_Slug] ON [Services] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Shares_PostId] ON [Shares] ([PostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Shares_SharedPostId] ON [Shares] ([SharedPostId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Shares_UserId] ON [Shares] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ShippingMethods_Code] ON [ShippingMethods] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Staff_StaffCode] ON [Staff] ([StaffCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Staff_UserId] ON [Staff] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StaffSchedules_StaffId_DayOfWeek] ON [StaffSchedules] ([StaffId], [DayOfWeek]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StaffServices_ServiceId] ON [StaffServices] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffServices_StaffId_ServiceId] ON [StaffServices] ([StaffId], [ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAddresses_UserId] ON [UserAddresses] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserBans_BannedByUserId] ON [UserBans] ([BannedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserBans_UserId_IsActive] ON [UserBans] ([UserId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserLoginHistories_UserId_LoginTime] ON [UserLoginHistories] ([UserId], [LoginTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserProfiles_UserId] ON [UserProfiles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserWarnings_UserId] ON [UserWarnings] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserWarnings_WarnedByUserId] ON [UserWarnings] ([WarnedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Wishlists_ProductId_UserId] ON [Wishlists] ([ProductId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Wishlists_UserId] ON [Wishlists] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260104055423_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260104055423_InitialCreate', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260107174812_AddColorToServiceCategory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260107174812_AddColorToServiceCategory', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108140348_AddStaffColumns'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceCategories]') AND [c].[name] = N'Color');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ServiceCategories] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [ServiceCategories] DROP COLUMN [Color];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260108140348_AddStaffColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260108140348_AddStaffColumns', N'8.0.3');
END;
GO

COMMIT;
GO

