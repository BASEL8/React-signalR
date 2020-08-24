import React, { useEffect, useState } from 'react'
import { Button, ListGroup, ListGroupItem, Badge } from 'reactstrap';

const Requests = ({ connection }) => {
  const [comingRequest, setComingRequest] = useState([]);
  const AcceptChallenge = (matchId) => {
    connection.invoke("AcceptGameRequest", matchId);
  }
  useEffect(() => {
    if (connection) {
      connection.invoke("UsersMatchRequests").catch(err => console.log(err));
      connection.on("GameRequest", (requests) => {
        setComingRequest(requests);
      });
    }
    return () => {
      connection.off("GameRequest");
    }
  }, [connection])

  return <div className="p-3 border">
    <h5 className="text-center">Requests</h5>
    <ListGroup>

      {comingRequest.map((request, index) =>
        <ListGroupItem key={index} className="d-flex align-items-center justify-content-between">
          <div>
            <div>
              {request.nickname}, wants to Challenge You
          </div>
            <div>
              w : <Badge color="success">{request.timesOfWins}</Badge> ,
                      L : <Badge color="danger">{request.matchPlayed}</Badge>
            </div>
          </div>
          <div>
            <Button size="sm" color="success" className="mr-2" onClick={() => AcceptChallenge(request.matchId)}>Accept</Button>
            <Button size="sm" color="warning">Decline</Button>
          </div>
        </ListGroupItem>
      )}
    </ListGroup>

  </div>
}
export default Requests;