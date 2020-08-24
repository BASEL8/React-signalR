import React, { useEffect } from 'react';
import { useParams, useHistory } from 'react-router-dom';
import { Button } from 'reactstrap';

const GameBoard = ({ connection }) => {
  const { matchId } = useParams();
  const history = useHistory();
  const handleLeaveMatch = () => {
    connection.invoke("UserLeaveGame", matchId);
  };
  useEffect(() => {
    if (connection) {
      connection.on("GameStarted", (gameData) => {
        console.log("GameData", gameData);
      })
      connection.on("UserLeaveGame", () => {
        history.push("/");
      });
    }
    return () => {
      if (connection && connection.state == "Connected") {
        connection.invoke("UserLeaveGame", matchId);
      }
    }
  }, [connection])
  return (
    <div className="f-grow-1">
      <Button onClick={handleLeaveMatch}>Leave</Button>
      <div>
        gameBoard
      </div>
      <div>
        game control
      </div>
    </div>
  )
}

export default GameBoard;