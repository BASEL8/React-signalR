import React, { useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import Requests from './Requests';
import OnlineMatches from './OnlineMatches';
import OnlineUsers from './OnlineUsers';

const Lobby = ({ connection }) => {
  const history = useHistory();

  useEffect(() => {
    if (connection) {
      connection.on("GameCreated", (gameId) => {
        if (gameId) {
          history.push(`/${gameId}`)
        }
      });

    }
    return () => {
      connection.off("GameCreated");
    }
  }, [connection, history])
  return (
    <div className="h-100 w-100">
      <h2 className="border-bottom pb-4 mb-4 text-center">
        Lobby
      </h2>
      <Requests connection={connection} />
      <div className="row m-0 mt-4 p-3 border">
        <OnlineUsers connection={connection} />
        <OnlineMatches connection={connection} />
      </div>
    </div >
  )
}
export default Lobby;