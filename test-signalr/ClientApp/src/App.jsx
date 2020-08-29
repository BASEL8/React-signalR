import React, { useEffect, useState } from 'react';
import { Route, Switch } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import GameBoard from './components/GameBoard';
import Statics from './components/Statics'
import * as signalR from '@microsoft/signalr'
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import './custom.css'

const App = () => {
  const [reRender, setReRender] = useState(false);
  const [connection, setConnection] = useState(null);
  const userId = localStorage.getItem("userId");
  const token = localStorage.getItem("token");
  const [user, setUser] = useState(null);
  const [connectionStatus, setConnectionStatus] = useState(false)
  useEffect(() => {
    if (connection === null) {
      setConnection(new HubConnectionBuilder()
        .withUrl("/game")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build());
    }
    if (connection && connection.state !== HubConnectionState.Connected) {
      setConnectionStatus(false);
      connection.start().then(res => {
        setConnectionStatus(true);
        if (userId && token) {
          connection.invoke("LoginOrSignup", null, token, userId).catch(error => console.log(error))
        }
      }).catch(err => setConnectionStatus(false))
    }
  }, [connection, userId, token])
  useEffect(() => {
    if (connection) {
      connection.on("SignupSuccess", (user) => {
        localStorage.setItem("token", user.token);
        localStorage.setItem("userId", user.userId);
        setUser(user);
        setReRender(!reRender);
      });
      connection.on("LoginSuccess", (user) => {
        setUser(user);
        localStorage.setItem("token", user.token);
      });
      connection.on("ClearLocalStorage", (err) => {
        localStorage.removeItem("userId");
        localStorage.removeItem("token");
        setReRender(!reRender);
      })
    }
  }, [connection, reRender])
  return (
    <Layout connectionStatus={connectionStatus} nickname={user ? user.nickname : ""}>
      <Switch>
        <Route exact path='/statics' component={(props) => <Statics {...props} connection={connection} />} />
        <Route exact path="/:matchId" component={(props) => <GameBoard {...props} connection={connection} />} />
        <Route exact path='/' component={(props) => <Home {...props} connection={connection} />} />
      </Switch>
    </Layout>
  );
}
export default App;
