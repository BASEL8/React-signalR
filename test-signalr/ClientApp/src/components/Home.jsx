import React from 'react';

import Lobby from './Lobby';
import GameForm from './LoginForm';


const Home = ({ connection }) => {
  const token = localStorage.getItem("token");
  const userId = localStorage.getItem("userId");;

  const Main = token && userId ?
    <Lobby connection={connection} /> :
    <GameForm connection={connection} token={token} />;
  return (
    <>
      {connection && connection.state === "Connected" ? Main : "... waiting for connection"}
    </>
  );
}

export default Home;