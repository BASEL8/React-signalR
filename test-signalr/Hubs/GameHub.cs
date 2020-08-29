using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace test_signalr.Hubs {
    public class Game : Hub {
        public class User {
            public string Nickname { set; get; }
            public string UserId { set; get; }
            public string Token { set; get; }
            public int TimesOfWins { set; get; }
            public int Draws { set; get; }
            public int MatchPlayed { set; get; }
            public List<MatchRequest> MatchRequests = new List<MatchRequest> ();
            public bool PlayAgain { set; get; }
        }
        public class OnlineUser : User {
            //new string token = "";
        }
        public class MatchRequest : OnlineUser {
            public string MatchId { set; get; }
        }
        public class Match {
            public bool Started { set; get; }
            public bool End { set; get; }
            public string MatchId { set; get; }
            public OnlineUser Creator { set; get; }
            public OnlineUser Opponent { set; get; }
            public string Turn { get; set; }
            public string[] MatchData { get; set; }

            public int MatchViewersCount { get; set; }
            public string Winner { get; set; }
            public List<OnlineUser> MatchViewers = new List<OnlineUser> ();
            public List<string> WinnersHistory = new List<string> ();
            public int DrawCount { set; get; }
            public bool Draw { set; get; }

        }
        public static class GameData {
            public static List<User> Users = new List<User> ();
            public static List<OnlineUser> OnLineUsers = new List<OnlineUser> ();
            public static List<Match> Matches = new List<Match> ();
        }
        public string CalcWinner (string[] squares) {
            int[, ] lines = { { 0, 1, 2 },
                { 3, 4, 5 },
                { 6, 7, 8 },
                { 0, 3, 6 },
                { 1, 4, 7 },
                { 2, 5, 8 },
                { 0, 4, 8 },
                { 2, 4, 6 },
            };
            for (int i = 0; i < 8; i++) {
                int a = lines[i, 0];
                int b = lines[i, 1];
                int c = lines[i, 2];
                if (squares[a] != null && squares[b] != null && squares[c] != null) {
                    if (squares[a] == squares[b] && squares[a] == squares[c]) {
                        return squares[a];
                    }
                }
            }
            return "";
        }
        public override Task OnConnectedAsync () {
            return base.OnConnectedAsync ();
        }
        public override Task OnDisconnectedAsync (Exception exception) {
            RemoveUserFromOnline ();
            return base.OnDisconnectedAsync (exception);
        }
        public void RemoveUserFromOnline () {

            User user = GameData.Users.Find (user => user.Token == Context.ConnectionId);
            if (user != null) {
                string userId = user.UserId;
                int index = GameData.OnLineUsers.FindIndex (user => user.UserId == userId);
                if (index > -1) {
                    GameData.OnLineUsers.RemoveAt (index);
                    Clients.OthersInGroup ("OnLineUsers").SendAsync ("UpdateOnlineUsers", GameData.OnLineUsers);
                    Groups.RemoveFromGroupAsync (Context.ConnectionId, "OnLineUsers");
                }
            }
        }
        public async Task AddUserToOnline (User user) {
            bool index = GameData.OnLineUsers.FindIndex (_user => _user.UserId == user.UserId) == -1;
            if (user != null && index) {
                OnlineUser _onlineUser = new OnlineUser ();
                _onlineUser.Nickname = user.Nickname;
                _onlineUser.UserId = user.UserId;
                _onlineUser.TimesOfWins = user.TimesOfWins;
                GameData.OnLineUsers.Add (_onlineUser);
                await Groups.AddToGroupAsync (user.Token, "OnLineUsers");
                await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineUsers", GameData.OnLineUsers);
            }
        }
        public async Task LoginOrSignup (string nickname, string token, string userId) {
            bool nicknameExist = GameData.Users.FindIndex (user => user.Nickname.Equals (nickname)) != -1;
            bool userIdExist = !String.IsNullOrEmpty (userId);
            bool tokenExist = !String.IsNullOrEmpty (token);
            if (String.IsNullOrEmpty (nickname) && tokenExist && userIdExist) {
                //update token
                int index = GameData.Users.FindIndex (user => user.UserId.Equals (userId));
                if (index > -1 && GameData.Users[index].Token.Equals (token)) {
                    //update the current token 
                    GameData.Users[index].Token = Context.ConnectionId;
                    Match inGameAsCreator = GameData.Matches.Find (match => match.Creator.UserId == userId);
                    Match inGameAsOpponent = GameData.Matches.Find (match => match.Opponent.UserId == userId);
                    await Clients.Client (Context.ConnectionId).SendAsync ("LoginSuccess", GameData.Users[index]);
                    if (inGameAsCreator != null && inGameAsOpponent == null) {
                        if (inGameAsCreator.End) {
                            await AddUserToOnline (GameData.Users[index]);
                        }
                    }
                    if (inGameAsOpponent != null && inGameAsCreator == null) {
                        if (inGameAsOpponent.End) {
                            await AddUserToOnline (GameData.Users[index]);
                        }
                    }
                    if (inGameAsOpponent == null && inGameAsCreator == null) {
                        await AddUserToOnline (GameData.Users[index]);
                    }
                } else {
                    await Clients.Client (Context.ConnectionId).SendAsync ("ClearLocalStorage", "You don't have access to this account any more!, try to create another account!");
                }
            } else if (nicknameExist && !userIdExist && !tokenExist) {
                await Clients.Client (Context.ConnectionId).SendAsync ("SignupError", "this nickname is already exist, please try with anotherone ");
            } else if (!nicknameExist && !String.IsNullOrEmpty (nickname) && !userIdExist && !tokenExist) {
                //signup
                User _user = new User ();
                _user.Nickname = nickname;
                _user.UserId = Context.ConnectionId;
                _user.Token = Context.ConnectionId;
                GameData.Users.Add (_user);
                await Clients.Client (Context.ConnectionId).SendAsync ("SignupSuccess", _user);
                await AddUserToOnline (_user);
            }
        }
        public async Task SendPlayRequest (string opponentId, string creatorId) {
            if (!String.IsNullOrEmpty (opponentId) && !String.IsNullOrEmpty (creatorId)) {
                OnlineUser MatchCreator = GameData.OnLineUsers.Find (user => user.UserId == creatorId);
                OnlineUser OpponentPlayer = GameData.OnLineUsers.Find (user => user.UserId == opponentId);
                User _OpponentPlayer = GameData.Users.Find (user => user.UserId == opponentId);
                if (MatchCreator != null) {
                    if (OpponentPlayer != null) {
                        RemoveUserFromOnline ();
                        MatchRequest matchRequest = new MatchRequest ();
                        matchRequest.Nickname = MatchCreator.Nickname;
                        matchRequest.UserId = MatchCreator.UserId;
                        matchRequest.TimesOfWins = MatchCreator.TimesOfWins;
                        Guid guid = Guid.NewGuid ();
                        string id = guid.ToString ();
                        matchRequest.MatchId = id;
                        OpponentPlayer.MatchRequests.Add (matchRequest);
                        Match currentMatch = new Match ();
                        currentMatch.Creator = MatchCreator;
                        currentMatch.Opponent = OpponentPlayer;
                        currentMatch.MatchId = id;
                        currentMatch.MatchData = new string[] { "", "", "", "", "", "", "", "", "" };
                        currentMatch.Turn = MatchCreator.Nickname;
                        GameData.Matches.Add (currentMatch);
                        await Groups.AddToGroupAsync (Context.ConnectionId, currentMatch.MatchId);
                        await Clients.Client (_OpponentPlayer.Token).SendAsync ("GameRequest", OpponentPlayer.MatchRequests);
                        await Clients.Client (Context.ConnectionId).SendAsync ("GameCreated", currentMatch.MatchId);
                        await PlayersHistory (id);
                    } else {
                        await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineUsers", GameData.OnLineUsers);
                    }

                }
            }
        }
        public async Task UsersMatchRequests () {
            User _user = GameData.Users.Find (user => user.Token == Context.ConnectionId);
            if (_user != null) {
                OnlineUser _onlineuser = GameData.OnLineUsers.Find (user => user.UserId == _user.UserId);
                if (_onlineuser != null) {
                    await Clients.Client (Context.ConnectionId).SendAsync ("GameRequest", _onlineuser.MatchRequests);
                }
            }
        }
        public async Task AcceptGameRequest (string matchId) {
            Match currentMatch = GameData.Matches.Find (match => match.MatchId == matchId);
            if (currentMatch != null) {
                currentMatch.Started = true;
                currentMatch.End = false;
                currentMatch.Turn = currentMatch.Creator.Nickname;
                RemoveUserFromOnline ();
                List<Match> _matches = GameData.Matches.FindAll (match => match.Started == true && match.End == false);
                await Groups.AddToGroupAsync (Context.ConnectionId, matchId);
                await Clients.Client (Context.ConnectionId).SendAsync ("GameCreated", matchId);
                await Clients.Group (matchId).SendAsync ("MatchInformation", currentMatch);
                await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineMatches", _matches);
                //User _creator = GameData.Users.Find (user => user.UserId == currentMatch.Creator.UserId);
                //User _opponent = GameData.Users.Find (user => user.UserId == currentMatch.Opponent.UserId);
                await PlayersHistory (matchId);
            } else {
                OnlineUser _onlineplayer = GameData.OnLineUsers.Find (user => user.Token == Context.ConnectionId);
                if (_onlineplayer != null) {
                    await Clients.Client (Context.ConnectionId).SendAsync ("GameRequest", _onlineplayer.MatchRequests);
                }

            }
        }
        public async Task UserLeaveGame (string matchId) {
            Match _match = GameData.Matches.Find (match => match.MatchId == matchId);
            User AuthAsCreator = GameData.Users.Find (user => user.UserId == _match.Creator.UserId && user.Token == Context.ConnectionId);
            User AuthAsOpponent = GameData.Users.Find (user => user.UserId == _match.Opponent.UserId && user.Token == Context.ConnectionId);
            if (_match != null) {
                if (AuthAsCreator != null || AuthAsOpponent != null) {
                    User _creator = GameData.Users.Find (user => user.UserId == _match.Creator.UserId);
                    User _opponent = GameData.Users.Find (user => user.UserId == _match.Opponent.UserId);

                    if (_creator != null && _opponent != null) {
                        if (Context.ConnectionId == _creator.Token) {
                            if (_match.Started) {
                                if (!_match.End) {
                                    _opponent.TimesOfWins++;
                                    _match.WinnersHistory.Add (_opponent.Nickname);
                                }
                            } else {
                                OnlineUser _opponentOnline = GameData.OnLineUsers.Find (user => user.UserId == _opponent.UserId);
                                if (_opponentOnline != null) {
                                    int matchRequestIndex = _opponentOnline.MatchRequests.FindIndex (match => match.MatchId == matchId);
                                    if (matchRequestIndex > -1) {
                                        _opponentOnline.MatchRequests.RemoveAt (matchRequestIndex);
                                        await Clients.Client (_opponent.Token).SendAsync ("GameRequest", _opponentOnline.MatchRequests);
                                    }
                                }
                            }
                        } else {
                            //opponent leave
                            if (!_match.End) {
                                _creator.TimesOfWins++;
                                _match.WinnersHistory.Add (_creator.Nickname);
                            }
                        }
                        _match.End = true;
                        await Clients.Group (matchId).SendAsync ("UserLeaveGame");
                        await AddUserToOnline (_creator);
                        await AddUserToOnline (_opponent);
                        await Groups.RemoveFromGroupAsync (_opponent.Token, matchId);
                        await Groups.RemoveFromGroupAsync (Context.ConnectionId, matchId);
                        if (_match.MatchViewers.Count > 0) {
                            foreach (var viewer in _match.MatchViewers) {
                                User _user = GameData.Users.Find (user => user.UserId == viewer.UserId);
                                await Groups.RemoveFromGroupAsync (_user.Token, matchId);
                                await AddUserToOnline (_user);
                            }
                        }

                        List<Match> _matches = GameData.Matches.FindAll (match => match.Started == true && match.End == false);
                        await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineMatches", _matches);
                    }
                }
                User _userViewer = GameData.Users.Find (user => user.Token == Context.ConnectionId);
                if (_userViewer != null) {
                    int indexOfViewer = _match.MatchViewers.FindIndex (user => user.UserId == _userViewer.UserId);
                    if (indexOfViewer > -1) {
                        _match.MatchViewers.RemoveAt (indexOfViewer);
                        _match.MatchViewersCount = _match.MatchViewers.Count;
                        await Groups.RemoveFromGroupAsync (Context.ConnectionId, matchId);
                        await AddUserToOnline (_userViewer);
                        await Clients.Group (matchId).SendAsync ("UpdateMatchViewer", _match.MatchViewers);
                        List<Match> _matches = GameData.Matches.FindAll (match => match.Started == true && match.End == false);
                        await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineMatches", _matches);
                    }
                }
            }
        }
        public async Task OnlineMatches () {
            List<Match> _matches = GameData.Matches.FindAll (match => match.Started == true && match.End == false);
            await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineMatches", _matches);
        }
        public async Task RemovePlayRequests (string matchId) {
            int index = GameData.Matches.FindIndex (match => match.MatchId == matchId);
            if (index > -1) {
                OnlineUser _creator = GameData.Matches[index].Creator;
                User userCreator = GameData.Users.Find (user => user.UserId == _creator.UserId);
                await Clients.Client (userCreator.Token).SendAsync ("RequestReject");
                OnlineUser _opponent = GameData.Matches[index].Opponent;
                await AddUserToOnline (userCreator);
                int matchRequestIndex = _opponent.MatchRequests.FindIndex (match => match.MatchId == matchId);
                if (matchRequestIndex > -1) {
                    _opponent.MatchRequests.RemoveAt (matchRequestIndex);
                    GameData.Matches.RemoveAt (index);
                    string _opponentToken = GameData.Users.Find (user => user.UserId == _opponent.UserId).Token;
                    await Clients.Client (_opponentToken).SendAsync ("GameRequest", _opponent.MatchRequests);
                }
            }

        }
        public async Task GetMatchInformation (string matchId) {
            Match _match = GameData.Matches.Find (match => match.MatchId == matchId);
            if (_match != null) {
                await Clients.Client (Context.ConnectionId).SendAsync ("MatchInformation", _match);
            } else {
                await Clients.Group (matchId).SendAsync ("UserLeaveGame");

                await Clients.Client (Context.ConnectionId).SendAsync ("NoSuchMatch");
            }
        }
        public async Task WatchMatch (string matchId) {
            Match _match = GameData.Matches.Find (match => match.MatchId == matchId);
            if (_match != null && !_match.End) {
                User _user = GameData.Users.Find (user => user.Token == Context.ConnectionId);
                if (_user != null) {
                    OnlineUser _onlineUser = GameData.OnLineUsers.Find (user => user.UserId == _user.UserId);
                    if (_onlineUser != null) {
                        int AlreadyWatchingMatch = _match.MatchViewers.FindIndex (user => user.UserId == _onlineUser.UserId);
                        if (!(AlreadyWatchingMatch > -1)) {
                            _match.MatchViewers.Add (_onlineUser);
                            _match.MatchViewersCount = _match.MatchViewers.Count;
                            RemoveUserFromOnline ();
                            List<Match> _matches = GameData.Matches.FindAll (match => match.Started == true && match.End == false);
                            await Groups.AddToGroupAsync (Context.ConnectionId, matchId);
                            await Clients.Client (Context.ConnectionId).SendAsync ("WatchMatch", matchId);
                            await Clients.Group (matchId).SendAsync ("MatchInformation", _match);
                            await Clients.OthersInGroup (matchId).SendAsync ("UpdateMatchViewer", _match.MatchViewers);
                            await Clients.Group ("OnLineUsers").SendAsync ("UpdateOnlineMatches", _matches);
                        }
                    }
                }
            }
        }
        public async Task CheckUsersMove (int index, string matchId) {
            User _user = GameData.Users.Find (user => user.Token == Context.ConnectionId);
            Match _match = GameData.Matches.Find (match => match.MatchId == matchId);
            if (_user != null && _match != null && (_match.Creator.UserId == _user.UserId || _match.Opponent.UserId == _user.UserId)) {
                if (!_match.End && _match.Started) {
                    if (_match.MatchData[index] == "") {
                        bool userIsCreator = _match.Creator.UserId == _user.UserId;
                        User _opponent = !userIsCreator? GameData.Users.Find (user => user.UserId == _match.Creator.UserId) : GameData.Users.Find (user => user.UserId == _match.Opponent.UserId);
                        string mark = userIsCreator ? "x" : "o";
                        if (_match.Turn == _user.Nickname) {
                            _match.MatchData[index] = mark;
                            _match.Turn = userIsCreator ? _match.Opponent.Nickname : _match.Creator.Nickname;
                            var emptyIndexes = _match.MatchData.Select ((value, index) => new { value, index })
                                .Where (pair => pair.value == "")
                                .Select (pair => pair.index)
                                .ToList ();
                            string winner = CalcWinner (_match.MatchData);
                            if (!String.IsNullOrEmpty (winner)) {
                                _match.Winner = _user.Nickname;
                                _match.WinnersHistory.Add (_user.Nickname);
                                _user.TimesOfWins++;
                                _match.End = true;
                                //await Clients.Group (_match.MatchId).SendAsync ("Winner", _user.Nickname);
                            } else if (emptyIndexes.Count == 0) {
                                _user.Draws++;
                                _opponent.Draws++;
                                _match.DrawCount++;
                                _match.End = true;
                                _match.Draw = true;
                                //await Clients.Group (_match.MatchId).SendAsync ("Draw", true);
                            }
                            await Clients.Group (_match.MatchId).SendAsync ("MatchInformation", _match);
                        }
                    }
                }
            }

        }
        public async Task PlayersHistory (string matchId) {
            Match _currentMatch = GameData.Matches.Find (match => match.MatchId == matchId);
            if (_currentMatch != null) {
                User _creator = GameData.Users.Find (user => user.UserId == _currentMatch.Creator.UserId);
                User _opponent = GameData.Users.Find (user => user.UserId == _currentMatch.Opponent.UserId);
                int DrawCount = 0;
                List<string> playersMatches = new List<string> { };
                foreach (var match in GameData.Matches) {
                    if ((match.Creator.UserId == _creator.UserId && match.Opponent.UserId == _opponent.UserId) || (match.Opponent.UserId == _creator.UserId && match.Creator.UserId == _opponent.UserId)) {
                        DrawCount += match.DrawCount;
                        if (match.WinnersHistory.Count > 0) {
                            foreach (var nickname in match.WinnersHistory) {
                                playersMatches.Add (nickname);
                            }
                        };
                    }
                }

                await Clients.Group (_currentMatch.MatchId).SendAsync ("PlayersHistory", playersMatches, DrawCount);
            }
        }
        public async Task PlayAgain (string matchId) {
            Match _match = GameData.Matches.Find (match => match.MatchId == matchId);
            if (_match != null && _match.End) {
                User _user = GameData.Users.Find (user => user.Token == Context.ConnectionId);
                bool userIsCreator = _match.Creator.UserId == _user.UserId;
                bool userIsOpponent = _match.Opponent.UserId == _user.UserId;
                if (userIsCreator || userIsOpponent) {
                    User opponent = GameData.Users.Find (user => user.UserId == _match.Opponent.UserId);
                    User creator = GameData.Users.Find (user => user.UserId == _match.Creator.UserId);
                    if (userIsCreator) {
                        if (opponent != null) {
                            if (!opponent.PlayAgain) {
                                creator.PlayAgain = true;
                                await Clients.Client (opponent.Token).SendAsync ("PlayAgainRequest", creator.Nickname);
                            } else {
                                //start
                                _match.MatchData = new string[] { "", "", "", "", "", "", "", "", "" };
                                _match.End = false;
                                _match.Turn = opponent.Nickname;
                                _match.Winner = null;
                                _match.Draw = false;
                                opponent.PlayAgain = false;
                                creator.PlayAgain = false;
                                await Clients.Group (_match.MatchId).SendAsync ("MatchInformation", _match);
                                await PlayersHistory (_match.MatchId);
                            }
                        }
                    } else if (userIsOpponent) {
                        if (creator != null) {
                            if (!creator.PlayAgain) {
                                opponent.PlayAgain = true;
                                await Clients.Client (creator.Token).SendAsync ("PlayAgainRequest", opponent.Nickname);
                            } else {
                                _match.MatchData = new string[] { "", "", "", "", "", "", "", "", "" };
                                _match.End = false;
                                _match.Turn = creator.Nickname;
                                _match.Winner = null;
                                _match.Draw = false;
                                creator.PlayAgain = false;
                                opponent.PlayAgain = false;
                                await Clients.Group (_match.MatchId).SendAsync ("MatchInformation", _match);
                                await PlayersHistory (_match.MatchId);
                            }
                        }
                    }
                }
            }
        }
        public async Task GeneralInformation () {
            //The global statics should show the total amount of games played in the system
            //It should have a list of players saying how many games they won, lost and draw
            int Total = GameData.Matches.Count;
            int OnlineMatches = GameData.Matches.FindAll (match => match.Started && !match.End).Count;
            List<User> Users = new List<User> () { };

            foreach (var user in GameData.Users) {
                User _user = new User ();
                _user.Nickname = user.Nickname;
                _user.TimesOfWins = user.TimesOfWins;
                _user.Draws = user.Draws;
                _user.MatchPlayed = GameData.Matches.FindAll (match => match.End && (match.Creator.UserId == user.UserId || match.Opponent.UserId == user.UserId)).Count;
                Users.Add (_user);
            }
            await Clients.Client (Context.ConnectionId).SendAsync ("Statics", Total, OnlineMatches, Users);
        }
    }
}