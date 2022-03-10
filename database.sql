CREATE TABLE `greeting` (
  `url` varchar(500)
);

CREATE TABLE `keyword` (
  `trigger` varchar(100),
  `response` varchar(200)
);

CREATE TABLE `customcommand` (
  `serverId` varchar(20),
  `command` varchar(50),
  `url` varchar(500)
);

CREATE TABLE `lastfm` (
  `userId` varchar(20),
  `username` varchar(50)
);

CREATE TABLE `role` (
  `serverId` varchar(20),
  `roleName` varchar(50),
  `roleId` varchar(20)
);

CREATE TABLE `serversetting` (
  `serverId` varchar(20),
  `musicChannel` varchar(20),
  `roleChannel` varchar(20),
  `tChannelId` varchar(12),
  `tChannelLink` varchar(70) ,
  `tNotifChannel` varchar(20),
  `tNotifRole` varchar(20),
  PRIMARY KEY (`serverId`)
);