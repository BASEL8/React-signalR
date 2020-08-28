import React, { useState, useEffect } from 'react';
import { ListGroup, ListGroupItem, Button } from 'reactstrap';
import { useHistory } from 'react-router-dom';

const OnlineMatches = ({ connection }) => {
  const [matches, setMatches] = useState([]);
  const history = useHistory();
  useEffect(() => {
    if (connection) {
      connection.invoke("OnlineMatches").catch(err => console.log(err));
      connection.on("UpdateOnlineMatches", (_matches) => {
        setMatches(_matches);
      });
      connection.on("UpdateOnlineMatchCount", (_matchId, _count) => {
        const _matches = matches.map(match => {
          if (match.matchId === _matchId) {
            match.matchViewersCount = _count;
            return match
          }
          return match
        });
        setMatches([..._matches]);
      });
      connection.on("WatchMatch", (matchId) => {
        history.push(`/${matchId}`);
      });
    }
    return () => {
      connection.off("updateOnlineMatches");
      connection.off("UpdateOnlineMatchCount");
    }
  }, [connection])
  const handleWatchMatch = (matchId) => {
    connection.invoke("WatchMatch", matchId).catch(err => console.log(err));
  }
  return (
    <div className="col-md-6 col-sm-12 mt-sm-4">
      <h5>Online Matches</h5>
      <ListGroup>
        {
          matches.length > 0 && matches.map((match) =>
            <ListGroupItem key={match.matchId} className="d-flex align-items-center justify-content-between">
              <div>
                <div>
                  {match.opponent.nickname + " vs " + match.creator.nickname}
                </div>
                <div>
                  <span role="img" aria-label="eye">👁️</span> : {match.matchViewersCount}
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