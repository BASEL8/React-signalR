import React, { useState, useEffect } from 'react'
import { ListGroup, ListGroupItem, Button, Badge } from 'reactstrap';

const OnlineMatches = ({ connection }) => {
  const [matches, setMatches] = useState([]);
  useEffect(() => {
    if (connection) {
      connection.invoke("OnlineMatches").catch(err => console.log(err));
      connection.on("updateOnlineMatches", (_matches) => {
        setMatches(_matches);
      });
    }
    return () => {
      connection.off("updateOnlineMatches");
    }
  }, [connection])
  const handleWatchMatch = (matchId) => {
  }
  return (
    <div className="col-md-6 col-sm-12 mt-sm-4">
      <h5>Online Matches</h5>
      <ListGroup>
        {
          matches.map((match) =>
            <ListGroupItem key={match.matchId} className="d-flex align-items-center justify-content-between">
              <div>
                <div>
                  {match.opponent.nickname + " vs " + match.creator.nickname}
                </div>
                <div>
                  watch : 100
                </div>
              </div>
              <Button color="success" size="sm" onClick={() => handleWatchMatch(match.matchId)}>Watch</Button>
            </ListGroupItem>)
        }
      </ListGroup>
    </div>
  )
}
export default OnlineMatches;