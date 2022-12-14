USE [delfaTelegramBotData_18.00.0]
GO
/****** Object:  Table [dbo].[usersDelfaTelegram]    Script Date: 12.10.2022 8:27:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usersDelfaTelegram](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nickname] [nvarchar](50) NULL,
	[telegramId] [int] NULL,
	[username] [nvarchar](50) NULL,
	[phoneNumber] [nvarchar](12) NULL,
	[chatId] [int] NULL,
	[name] [nvarchar](50) NULL,
 CONSTRAINT [PK_usersDelfaTelegram] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[usersDelfaTelegram] ON 

INSERT [dbo].[usersDelfaTelegram] ([id], [nickname], [telegramId], [username], [phoneNumber], [chatId], [name]) VALUES (5, N'Danila .', 995734455, N'Flame_chanel', N'79504951460', 995734455, NULL)
SET IDENTITY_INSERT [dbo].[usersDelfaTelegram] OFF
GO
