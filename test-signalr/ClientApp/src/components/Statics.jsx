import React, { useState, useEffect } from 'react'
import { ListGroup, ListGroupItem, Badge } from 'reactstrap';

const Statics = ({ connection }) => {
  const [state, setState] = useState({ users: [], total: 0, onlineMatches: 0 })
  const { total, onlineMatches, users } = state;
  useEffect(() => {
    if (connection && connection.state === "Connected") {
      connection.on("Statics", (total, onlineMatches, users) => {
        setState({ total, onlineMatches, users });
      })
      connection.invoke("GeneralInformation");
    }
    return () => {
      if (connection) {
        connection.off("Statics");
      }
    }
  }, [connection])
  return (
    <div className="h-100 flex-grow-1 d-flex flex-column">
      <h3 className="text-center">Statics</h3>
      <div className="flex-grow-1 d-flex flex-column" style={{ maxHeight: '100%' }}>
        <div className="flex-grow-4 d-flex flex-column">
          <div className="flex-grow-1 flex-shrink-1 p-4 mb-4 d-flex align-items-center justify-content-center shadow"><h5><span>{total}</span> Games Played until Now</h5></div>
          <div className="flex-grow-1 flex-shrink-1 p-4 mb-4 d-flex align-items-center justify-content-center shadow"><h5><span>{onlineMatches}</span> Online Games</h5></div>
        </div>
        <div className="flex-grow-1 p-4 mb-4 shadow overflow-auto">
          <h3 className="text-center">Users</h3>
          <ListGroup>
            {users && users.length > 0 && users.map((user, index) =>
              <ListGroupItem key={index} className="d-flex align-items-center justify-content-between">
                <div>
                  <div>
                    {user.nickname}
                  </div>
                  <div>
                    w : <Badge color="success">{user.timesOfWins}</Badge> ,
                    L : <Badge color="danger">{user.matchPlayed - user.draws - user.timesOfWins}</Badge> ,
                    D : <Badge color="danger">{user.draws}</Badge>
                  </div>
                </div>
              </ListGroupItem>
            )}
          </ListGroup>
        </div>
      </div>
    </div>
  )
};
export default Statics;