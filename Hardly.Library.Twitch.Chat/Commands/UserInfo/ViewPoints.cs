﻿using System;

namespace Hardly.Library.Twitch {
	class ViewPoints : TwitchCommandController {
		public ViewPoints(TwitchChatRoom room) : base(room) {
            ChatCommand.Create(room, "points", PointCommand, "View how many points you, or another user, has. !points <username>", new[] { "point", "kappas" }, false, TimeSpan.FromSeconds(30), true);
			ChatCommand.Create(room, "brag", BragCommand, "Shows everyone how many points you have.  This costs 50 to run.", null, false, TimeSpan.FromMinutes(1), true);
			ChatCommand.Create(room, "leaderboard", LeaderboardCommand, "Displays the peeps with the most points.", null, false, TimeSpan.FromMinutes(2), false);
			ChatCommand.Create(room, "aboutpoints", AboutPointsCommand, "Displays the point units and values.", null, false, TimeSpan.FromMinutes(2), true);
		}

		private void AboutPointsCommand(SqlTwitchUser speaker, string message) {
			room.SendWhisper(speaker, room.pointManager.GetAboutPoints());
		}

		private void LeaderboardCommand(SqlTwitchUser speaker, string message) {
			SqlTwitchUserPoints[] leadersPoints = SqlTwitchUserPoints.GetTopUsersForChannel(room.twitchConnection.channel, 5);
			if(leadersPoints != null) {
				string chatMessage = "";
				foreach(var points in leadersPoints) {
					chatMessage += points.user.name + " has " + room.pointManager.ToPointsString(points.points, true) + "  ";
				}

				room.SendChatMessage(chatMessage);
			}
		}

		private void BragCommand(SqlTwitchUser speaker, string message) {
			TwitchUserPointManager yourPoints = room.pointManager.ForUser(speaker);
			if(yourPoints.Points >= 50) {
				yourPoints.Award(0, -50);
				string chatMessage = speaker.name + " has " + room.pointManager.ToPointsString(yourPoints.Points);
				room.SendChatMessage(chatMessage);
			} else {
				room.SendWhisper(speaker, "You can't afford a brag...");
			}
		}

		private void PointCommand(SqlTwitchUser speaker, string message) {
			string otherUserName = message.GetBefore(" ");
			if(otherUserName == null) {
				otherUserName = message;
			}
			otherUserName = otherUserName?.Trim().ToLower();
			SqlTwitchUser otherUser = SqlTwitchUser.GetFromName(otherUserName);

			TwitchUserPointManager yourPoints = room.pointManager.ForUser(speaker);
			string chatMessage = "You have ";
			chatMessage += room.pointManager.ToPointsString(yourPoints.Points);

			if(otherUser != null && !otherUser.id.Equals(speaker.id)) {
				TwitchUserPointManager otherPoints = room.pointManager.ForUser(otherUser);

				chatMessage += " & " + otherUser.name + " has " + room.pointManager.ToPointsString(otherPoints.Points);
			}

			room.SendWhisper(speaker, chatMessage);
		}
	}
}
