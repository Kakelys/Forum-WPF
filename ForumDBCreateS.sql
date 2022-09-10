use Forum;


CREATE TABLE `Img` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Img` mediumblob DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `Role` (
  `Id` int(11) NOT NULL DEFAULT 10,
  `RoleName` varchar(20) CHARACTER SET utf8mb3 NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

insert into`Role`(`Id`,`RoleName`) VALUES
(0,'Admin'),
(1,'Moderator'),
(10,'User');

insert into `Img` values
(-1,0);

CREATE TABLE `User` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb3 DEFAULT NULL,
  `Passwd` varchar(32) CHARACTER SET utf8mb3 DEFAULT NULL,
  `CountOfMsg` int(11) DEFAULT 0,
  `CountOfTopics` int(11) DEFAULT 0,
  `RegDate` datetime DEFAULT NULL,
  `Role` int(11) NOT NULL DEFAULT 10,
  `Img` int(11) NOT NULL DEFAULT -1,
  `IsBanned` tinyint(1) DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserName` (`Name`),
  KEY `Role` (`Role`),
  KEY `Img` (`Img`),
  CONSTRAINT `USERS_img` FOREIGN KEY (`Img`) REFERENCES `Img` (`Id`),
  CONSTRAINT `USERS_role` FOREIGN KEY (`Role`) REFERENCES `Role` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

use Forum;
CREATE TABLE `Section` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb3 DEFAULT NULL,
  `CountOfForums` int(11) DEFAULT NULL,
  `SectionOrder` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `Forum` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb3 DEFAULT NULL,
  `SectionId` int(11) DEFAULT NULL,
  `CountOfTopics` int(11) DEFAULT NULL,
  `CountOfMsg` int(11) DEFAULT NULL,
  `ForumOrder` int(11) DEFAULT NULL,
  `LastMsgTime` datetime DEFAULT NULL,
  `LastMsgTopicId` int(11) DEFAULT 0,
  `LastMsgUsrId` int(11) DEFAULT 0,
  `ImgId` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `SectionId` (`SectionId`),
  KEY `ImgId` (`ImgId`),
  CONSTRAINT `FORUMS_secionId` FOREIGN KEY (`SectionId`) REFERENCES `Section` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `TOPICS_imgId` FOREIGN KEY (`ImgId`) REFERENCES `Img` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `Topic` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb3 DEFAULT NULL,
  `ForumId` int(11) DEFAULT NULL,
  `CountOfMsg` int(11) DEFAULT 0,
  `StartMsgUsrId` int(11) DEFAULT NULL,
  `LastMsgUsrId` int(11) DEFAULT NULL,
  `LastMsgTime` datetime DEFAULT current_timestamp(),
  `StartMsgTXT` varchar(3000) CHARACTER SET utf8mb3 DEFAULT NULL,
  `StartMsgTime` datetime default null,
  `IsClosed` tinyint(1) DEFAULT 0,
  `IsPinned` tinyint(1) DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `ForumId` (`ForumId`),
  KEY `StartMsgUsrId` (`StartMsgUsrId`),
  KEY `LastMsgUsrId` (`LastMsgUsrId`),
  CONSTRAINT `TOPICS_forumId` FOREIGN KEY (`ForumId`) REFERENCES `Forum` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `TOPICS_lastUserid` FOREIGN KEY (`LastMsgUsrId`) REFERENCES `User` (`Id`),
  CONSTRAINT `TOPICS_startUserId` FOREIGN KEY (`StartMsgUsrId`) REFERENCES `User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `Post` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `MsgTxt` varchar(3000) CHARACTER SET utf8mb3 DEFAULT NULL,
  `TopicId` int(11) DEFAULT NULL,
  `MsgTime` datetime DEFAULT null,
  `SenderId` int(11) DEFAULT NULL,
  `Ancestor` int(11) default 0,
  PRIMARY KEY (`Id`),
  KEY `TopicId` (`TopicId`),
  KEY `SenderId` (`SenderId`),
  CONSTRAINT `POST_senderId` FOREIGN KEY (`SenderId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `POST_topicId` FOREIGN KEY (`TopicId`) REFERENCES `Topic` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;

CREATE TABLE `Ban` (
  `Id` int(11) NOT NULL AUTO_INCREMENT primary key,
  `UserId` int(11) DEFAULT NULL,
  `AdminId` int(11) DEFAULT NULL,
  `Reason` varchar(1000) DEFAULT NULL,
  `BanTime` datetime default NULL,
  `UnbanTime` datetime DEFAULT NULL,
  `IsPerm` tinyint(1) DEFAULT NULL,
  `IsActive` tinyint(1) default true,
  CONSTRAINT `BAN_userId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`),
  CONSTRAINT `BAN_adminID` FOREIGN KEY (`AdminId`) REFERENCES `User` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1;


use Forum;
drop table `Ban`;








