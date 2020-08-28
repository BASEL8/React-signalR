import React, { useEffect, useState } from 'react'
import { ListGroupItem, ListGroup, Badge } from 'reactstrap'
const MatchViewer = ({ connection }) => {
  const [viewer, setViewer] = useState([]);
  useEffect(() => {
    connection.on("UpdateMatchViewer", (_viewer => {
      if (_viewer) {
        setViewer(_viewer);
      }
    }));
    return () => {
      connection.off("UpdateMatchViewer")
    }
  }, [connection])
  return (
    <div>
      <h4>Match viewers</h4>
      <ListGroup>
        {
          viewer && viewer.length > 0 && viewer.map((user, index) =>
            <ListGroupItem key={index} className="d-flex align-items-center justify-content-between" disabled>
              <div>
                <div>
                  Nickname :  {user.nickname}
                </div>
                <div>
                  <span> w : </span><Badge color="success">{user.timesOfWins}</Badge> ,
                  <span>L : </span><Badge color="danger">{user.matchPlayed}</Badge>
                </div>
              </div>
            </ListGroupItem>)
        }
      </ListGroup>
    </div>
  )
}
export default MatchViewer;