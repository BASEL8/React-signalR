using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace test_signalr.Hubs {
    public class Game : Hub {
        public class User {
            public string nickname { set; get; }
            public string userId { set; get; }
            public string token { set; get; }
            public int TimesOfWins { set; get; }
            public int MatchPlayed { set; get; }
            public List<MatchRequest> MatchRequests = new List<MatchRequest> ();
        }
        public class OnlineUser : User {
            // new string token = "";
        }
        public class MatchRequest : OnlineUser {
            public string matchId { set; get; }
        }
        public class Match {
            public bool started { set; get; }
            public bool end { set; get; }
            public string matchId { set; get; }
            public OnlineUser creator { set; get; }
            public OnlineUser opponent { set; get; }
            public string Turn { get; set; }
            public string matchData { get; set; }

            public static List<OnlineUser> matchViewer = new List<OnlineUser> ();

        }
        public static class GameData {
            public static List<User> users = new List<User> ();
            public static List<OnlineUser> OnLineUsers = new List<OnlineUser> ();
            public static List<Match> Matches = new List<Match> ();
            public static int AllGamesCount;
            public static int AllOnlineGamesCount;
        }
        public override Task OnConnectedAsync () {
            //Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnecID", Context.ConnectionId);
            return base.OnConnectedAsync ();
        }
        public override Task OnDisconnectedAsync (Exception exception) {
            RemoveUserFromOnline ();
            return base.OnDisconnectedAsync (exception);
        }
        public void RemoveUserFromOnline () {

            User user = GameData.users.Find (user => user.token == Context.ConnectionId);
            if (user != null) {
                string userId = user.userId;
                int index = GameData.OnLineUsers.FindIndex (user => user.userId == userId);
                if (index > -1) {
                    GameData.OnLineUsers.RemoveAt (index);
                    Groups.RemoveFromGroupAsync (Context.ConnectionId, "onLineUsers");
                    Clients.OthersInGroup ("onLineUsers").SendAsync ("updateOnlineUsers", GameData.OnLineUsers);
                }
            }
        }
        public async Task AddUserToOnline (User user) {
            bool index = GameData.OnLineUsers.FindIndex (_user => _user.userId == user.userId) == -1;
            if (user != null && index) {
                OnlineUser _onlineUser = new OnlineUser ();
                _onlineUser.nickname = user.nickname;
                _onlineUser.userId = user.userId;
                _onlineUser.TimesOfWins = user.TimesOfWins;
                _onlineUser.MatchPlayed = user.MatchPlayed;
                GameData.OnLineUsers.Add (_onlineUser);
                await Groups.AddToGroupAsync (Context.ConnectionId, "onLineUsers");
                await Clients.Group ("onLineUsers").SendAsync ("updateOnlineUsers", GameData.OnLineUsers);
            }
        }
        public async Task LoginOrSignup (string nickname, string token, string userId) {
            bool nicknameExist = GameData.users.FindIndex (user => user.nickname.Equals (nickname)) != -1;
            bool userIdExist = !String.IsNullOrEmpty (userId);
            bool tokenExist = !String.IsNullOrEmpty (token);
            if (String.IsNullOrEmpty (nickname) && tokenExist && userIdExist) {
                //update token
                int index = GameData.users.FindIndex (user => user.userId.Equals (userId));
                if (index > -1 && GameData.users[index].token.Equals (token)) {
                    await Clients.Client (Context.ConnectionId).SendAsync ("userNickname", GameData.users[index].nickname);
                    //update the current token 
                    GameData.users[index].token = Context.ConnectionId;
                    bool gameCreator = GameData.Matches.FindIndex (match => match.creator.userId == userId) > -1;
                    bool inGame = GameData.Matches.FindIndex (match => match.opponent.userId == userId && match.started) > -1;
                    await Clients.Client (Context.ConnectionId).SendAsync ("LoginSuccess", GameData.users[index]);
                    await AddUserToOnline (GameData.users[index]);
                    if (!gameCreator && !inGame) { }
                } else {
                    await Clients.Client (Context.ConnectionId).SendAsync ("ClearLocalStorage", "You don't have access to this account any more!, try to create another account!");
                }
            } else if (nicknameExist && !userIdExist && !tokenExist) {
                await Clients.Client (Context.ConnectionId).SendAsync ("SignupError", "this nickname is already exist, please try with anotherone ");
            } else if (!nicknameExist && !String.IsNullOrEmpty (nickname) && !userIdExist && !tokenExist) {
                //signup
                User _user = new User ();
                _user.nickname = nickname;
                _user.userId = Context.ConnectionId;
                _user.token = Context.ConnectionId;
                GameData.users.Add (_user);
                await Clients.Client (Context.ConnectionId).SendAsync ("SignupSuccess", _user);
                await AddUserToOnline (_user);
            }
        }
        public async Task SendPlayRequest (string OpponentId, string creatorId) {
            if (!String.IsNullOrEmpty (OpponentId) && !String.IsNullOrEmpty (creatorId)) {
                OnlineUser MatchCreator = GameData.OnLineUsers.Find (user => user.userId == creatorId);
                OnlineUser OpponentPlayer = GameData.OnLineUsers.Find (user => user.userId == OpponentId);
                User _OpponentPlayer = GameData.users.Find (user => user.userId == OpponentId);
                if (MatchCreator != null) {
                    if (OpponentPlayer != null) {
                        RemoveUserFromOnline ();
                        MatchRequest matchRequest = new MatchRequest ();
                        matchRequest.nickname = MatchCreator.nickname;
                        matchRequest.userId = MatchCreator.userId;
                        matchRequest.TimesOfWins = MatchCreator.TimesOfWins;
                        matchRequest.MatchPlayed = MatchCreator.MatchPlayed;
                        matchRequest.matchId = MatchCreator.userId + OpponentPlayer.userId;
                        OpponentPlayer.MatchRequests.Add (matchRequest);
                        Match currentMatch = new Match ();
                        currentMatch.creator = MatchCreator;
                        currentMatch.opponent = OpponentPlayer;
                        currentMatch.matchId = MatchCreator.userId + OpponentPlayer.userId;
                        GameData.Matches.Add (currentMatch);
                        await Groups.AddToGroupAsync (Context.ConnectionId, currentMatch.matchId);
                        await Clients.Client (_OpponentPlayer.token).SendAsync ("GameRequest", OpponentPlayer.MatchRequests);
                        await Clients.Client (Context.ConnectionId).SendAsync ("GameCreated", currentMatch.matchId);
                    } else {
                        await Clients.Group ("onLineUsers").SendAsync ("updateOnlineUsers", GameData.OnLineUsers);
                    }

                }
            }
        }
        public async Task UsersMatchRequests () {
            User _user = GameData.users.Find (user => user.token == Context.ConnectionId);
            if (_user != null) {
                OnlineUser _onlineuser = GameData.OnLineUsers.Find (user => user.userId == _user.userId);
                if (_onlineuser != null) {
                    await Clients.Client (Context.ConnectionId).SendAsync ("GameRequest", _onlineuser.MatchRequests);
                }
            }
        }
        public async Task AcceptGameRequest (string matchId) {
            RemoveUserFromOnline ();
            Match currentMatch = GameData.Matches.Find (match => match.matchId == matchId);
            currentMatch.started = true;
            currentMatch.end = false;
            currentMatch.Turn = currentMatch.opponent.userId;
            GameData.AllGamesCount++;
            GameData.AllOnlineGamesCount++;
            List<Match> _matches = GameData.Matches.FindAll (match => match.started == true && match.end == false);
            await Groups.AddToGroupAsync (Context.ConnectionId, matchId);
            await Clients.Client (Context.ConnectionId).SendAsync ("GameCreated", matchId);
            await Clients.Group (matchId).SendAsync ("gamestarted", currentMatch);
            await Clients.Group ("onLineUsers").SendAsync ("updateOnlineMatches", _matches);
        }
        public async Task UserLeaveGame (string matchId) {
            //todo remove the match if the first player cancel the invitation
            User _user = GameData.users.Find (user => user.token == Context.ConnectionId);
            Match _match = GameData.Matches.Find (match => match.matchId == matchId);
            if (_match != null) {
                _match.end = true;
                if (_user != null) {
                    await AddUserToOnline (_user);
                    await Groups.RemoveFromGroupAsync (Context.ConnectionId, matchId);
                    await Clients.Group (matchId).SendAsync ("UserLeaveGame");
                }
            }
        }
        public async Task OnlineMatches () {
            List<Match> _matches = GameData.Matches.FindAll (match => match.started == true && match.end == false);
            await Clients.Group ("onLineUsers").SendAsync ("updateOnlineMatches", _matches);
        }
    }
}