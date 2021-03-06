import React, { useEffect, useState } from 'react';
import { useParams, useHistory } from 'react-router-dom';
import { Button } from 'reactstrap';
import MatchViewers from './MatchViewers'
import Board from './Board';
const GameBoard = ({ connection }) => {
  const { matchId } = useParams();
  const [matchInformation, setMatchInformation] = useState();
  //  const [matchDrawEnd, setMatchDrawEnd] = useState(false);
  // const [matchWinner, setMatchWinner] = useState("");
  const [playAgainMessage, setPlayAgain] = useState("");
  const [playersHistory, setPlayersHistory] = useState([]);
  const history = useHistory();
  const userId = localStorage.getItem("userId");
  const handleLeaveMatch = () => {
    connection.invoke("UserLeaveGame", matchId);
    history.push("/");
  };
  const handlePlayAgain = () => {
    connection.invoke("PlayAgain", matchId).catch(err => console.log(err));
  }
  useEffect(() => {
    if (connection && connection.state === "Connected") {
      connection.on("MatchInformation", (_matchInformation) => {
        if (_matchInformation) {
          setMatchInformation(_matchInformation)
          setPlayAgain("");
        }
      })
      connection.on("PlayAgainRequest", (nickname) => {
        setPlayAgain(`${nickname}, want to play agin`)
      });
      connection.on("UserLeaveGame", () => {
        history.push("/");
      });
      connection.on("RequestReject", () => {
        history.push("/");
      });
      connection.on("NoSuchMatch", () => {
        history.push("/");
      });
      connection.on("PlayersHistory", (WinHistory, Draw) => {
        if (Array.isArray(WinHistory)) {
          let unique = [...new Set(WinHistory)];
          let result = unique.map(value => { return { [value]: WinHistory.filter(str => str === value).length } });

          if (Draw > 0) {
            result.push({ Draw });
          }
          setPlayersHistory(result);
        }
      });
      connection.invoke("GetMatchInformation", matchId).catch(err => console.log(err));
    }
    return () => {
      if (connection) {
        connection.off("MatchInformation");
        connection.off("PlayAgainRequest");
        connection.off("UserLeaveGame");
        connection.off("NoSuchMatch");
        connection.off("RequestReject");
        connection.off("PlayersHistory");
      }
    }
  }, [connection])
  if (!matchInformation) {
    return (
      <div>...waiting</div>
    )
  }
  let totalGames = 0;
  const userIsNotViewer = matchInformation && ((matchInformation.creator.userId === userId) || (matchInformation.opponent.userId === userId));
  const MatchWinnerComponent = matchInformation.winner && <h3>We Have A Winner, {matchInformation.winner}</h3>;
  const MatchDrawComponent = matchInformation.draw && <h3>Draw</h3>
  const PlayAgainComponent = userIsNotViewer && matchInformation.end
    &&
    <div className="d-flex algin-items-center"><Button size="sm" color="success" className="mr-2" onClick={handlePlayAgain}>play agin</Button> <Button size="sm" color="warning" onClick={handleLeaveMatch}>Leave</Button></div>
  return (
    <div className="f-grow-1 d-flex flex-column h-100 w-100">
      <div className="border border-bottom-0 p-4" >
        <h4 className="text-center">Player, {matchInformation.turn} turn</h4>
        <div className="w-100  pl-0 p-4">
          {MatchDrawComponent}
          {MatchWinnerComponent}
          <h5>{playAgainMessage}</h5>
          {PlayAgainComponent}
        </div>

      </div>
      <div className="flex-grow-1 row m-0 border p-2">
        <div className="col-md-8 col-sm-12 d-flex align-items-center justify-content-center border-right">
          <Board connection={connection} matchData={matchInformation.matchData} matchId={matchId} />
        </div>

        <div className="col-md-4 col-sm-12 d-flex flex-column pt-2">
          <h4>Match Information</h4>
          <div className="mt-2 mb-2 border-top border-bottom flex-grow-1 p-3">
            <h3 className="mt-2 mb-2">Players History</h3>
            {
              playersHistory.length > 0 ? playersHistory.map((obj, index) => {
                for (const key in obj) {
                  if (obj.hasOwnProperty(key)) {
                    totalGames += parseInt(obj[key]);
                    return (
                      <div key={index}>
                        <p>{key} : {obj[key]} {key !== "Draw" ? "wins" : ""}</p>
                      </div>
                    )
                  }
                  return null;
                }
              }) :
                <p>First match together!</p>
            }
            {totalGames > 0 && <p>total games : {totalGames}</p>}
          </div>
          <div className="mt-2 mb-2 border-top border-bottom flex-grow-1 p-3">
            {userIsNotViewer && <MatchViewers connection={connection} />}
          </div>
          <div className="mt-2 mb-2 border-top p-3 d-flex justify-content-end align-items-end">
            <Button size="sm" color="warning" onClick={handleLeaveMatch}>Leave</Button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default GameBoard;