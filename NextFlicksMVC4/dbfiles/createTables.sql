USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[BoxArts]    Script Date: 02/08/2013 11:42:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BoxArts](
	[boxart_ID] [int] IDENTITY(1,1) NOT NULL,
	[movie_ID] [int] NOT NULL,
	[boxart_38] [nvarchar](max) NULL,
	[boxart_64] [nvarchar](max) NULL,
	[boxart_110] [nvarchar](max) NULL,
	[boxart_124] [nvarchar](max) NULL,
	[boxart_150] [nvarchar](max) NULL,
	[boxart_166] [nvarchar](max) NULL,
	[boxart_88] [nvarchar](max) NULL,
	[boxart_197] [nvarchar](max) NULL,
	[boxart_176] [nvarchar](max) NULL,
	[boxart_284] [nvarchar](max) NULL,
	[boxart_210] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[boxart_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[Genres]    Script Date: 02/08/2013 11:42:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Genres](
	[genre_ID] [int] NOT NULL,
	[genre_string] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[genre_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[jberg068_movies]    Script Date: 02/08/2013 11:42:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Movies](
	[movie_ID] [int] IDENTITY(1,1) NOT NULL,
	[short_title] [nvarchar](max) NULL,
	[year] [int] NOT NULL,
	[runtime] [int] NOT NULL,
	[avg_rating] [nvarchar](max) NULL,
	[tv_rating] [nvarchar](max) NULL,
	[web_page] [nvarchar](max) NULL,
	[current_season] [nvarchar](max) NULL,
	[is_movie] [bit] NOT NULL,
	[maturity_rating] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[movie_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[MovieTags]    Script Date: 02/08/2013 11:43:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MovieTags](
	[TagId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[TagId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[MovieToGenres]    Script Date: 02/08/2013 11:43:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MovieToGenres](
	[movie_to_genre_ID] [int] IDENTITY(1,1) NOT NULL,
	[movie_ID] [int] NOT NULL,
	[genre_ID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[movie_to_genre_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[OmdbEntries]    Script Date: 02/08/2013 11:43:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OmdbEntries](
	[ombd_ID] [int] IDENTITY(1,1) NOT NULL,
	[movie_ID] [int] NOT NULL,
	[title] [nvarchar](max) NULL,
	[year] [int] NOT NULL,
	[i_Rating] [nvarchar](max) NULL,
	[i_Votes] [nvarchar](max) NULL,
	[i_ID] [nvarchar](max) NULL,
	[t_Meter] [int] NOT NULL,
	[t_Image] [nvarchar](max) NULL,
	[t_Rating] [real] NOT NULL,
	[t_Reviews] [int] NOT NULL,
	[t_Fresh] [int] NOT NULL,
	[t_Rotten] [int] NOT NULL,
	[t_Consensus] [nvarchar](max) NULL,
	[t_UserMeter] [int] NOT NULL,
	[t_UserRating] [real] NOT NULL,
	[t_UserReviews] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ombd_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[UserLogs]    Script Date: 02/08/2013 11:43:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserLogs](
	[userlog_id] [int] IDENTITY(1,1) NOT NULL,
	[ip_addr] [nvarchar](max) NULL,
	[time] [datetime] NOT NULL,
	[useragent] [nvarchar](max) NULL,
	[raw_url] [nvarchar](max) NULL,
	[referrer] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[userlog_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[UserProfile]    Script Date: 02/08/2013 11:43:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserProfile](
	[userID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](15) NOT NULL,
	[email] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[userID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[UserToMovieToTags]    Script Date: 02/08/2013 11:43:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserToMovieToTags](
	[UtMtY_ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[TagId] [int] NOT NULL,
	[movie_ID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UtMtY_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[UtMtTisAnons]    Script Date: 02/08/2013 11:43:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UtMtTisAnons](
	[UtMtTiA_ID] [int] IDENTITY(1,1) NOT NULL,
	[UtMtT_ID] [int] NOT NULL,
	[IsAnon] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UtMtTiA_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[webpages_Membership]    Script Date: 02/08/2013 11:43:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[webpages_Membership](
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NULL,
	[ConfirmationToken] [nvarchar](128) NULL,
	[IsConfirmed] [bit] NULL,
	[LastPasswordFailureDate] [datetime] NULL,
	[PasswordFailuresSinceLastSuccess] [int] NOT NULL,
	[Password] [nvarchar](128) NOT NULL,
	[PasswordChangedDate] [datetime] NULL,
	[PasswordSalt] [nvarchar](128) NOT NULL,
	[PasswordVerificationToken] [nvarchar](128) NULL,
	[PasswordVerificationTokenExpirationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[webpages_Membership] ADD  DEFAULT ((0)) FOR [IsConfirmed]
GO

ALTER TABLE [dbo].[webpages_Membership] ADD  DEFAULT ((0)) FOR [PasswordFailuresSinceLastSuccess]
GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[webpages_OAuthMembership]    Script Date: 02/08/2013 11:43:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[webpages_OAuthMembership](
	[Provider] [nvarchar](30) NOT NULL,
	[ProviderUserId] [nvarchar](100) NOT NULL,
	[UserId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Provider] ASC,
	[ProviderUserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[webpages_Roles]    Script Date: 02/08/2013 11:43:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[webpages_Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [jberg068_movies]
GO

/****** Object:  Table [dbo].[webpages_UsersInRoles]    Script Date: 02/08/2013 11:43:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[webpages_UsersInRoles](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[webpages_UsersInRoles]  WITH CHECK ADD  CONSTRAINT [fk_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[webpages_Roles] ([RoleId])
GO

ALTER TABLE [dbo].[webpages_UsersInRoles] CHECK CONSTRAINT [fk_RoleId]
GO

ALTER TABLE [dbo].[webpages_UsersInRoles]  WITH CHECK ADD  CONSTRAINT [fk_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[UserProfile] ([userID])
GO

ALTER TABLE [dbo].[webpages_UsersInRoles] CHECK CONSTRAINT [fk_UserId]
GO


