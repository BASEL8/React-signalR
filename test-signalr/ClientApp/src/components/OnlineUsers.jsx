import React, { useEffect, useState } from 'react';
import { ListGroup, ListGroupItem, Button, Badge } from 'reactstrap';



const OnlineUsers = ({ connection }) => {
  const [users, setUsers] = useState([])
  const userId = localStorage.getItem("userId");
  const sendGameRequest = (id) => {
    connection.invoke("SendPlayRequest", id, userId).catch(err => console.log(err));
  };

  useEffect(() => {
    if (connection) {
      connection.on("UpdateOnlineUsers", (updatedUsers) => {
        setUsers(updatedUsers);
      });
    }
    return () => {
      connection.off("UpdateOnlineUsers");
    }
  }, [connection, users])
  console.log(users);
  return (
    <div className="col-md-6 col-sm-12 mt-sm-4">
      <h5>Users Online</h5>
      <ListGroup>
        {
          users.map((user, index) =>
            <ListGroupItem key={index} className="d-flex align-items-center justify-content-between" disabled={user.userId === userId}>
              <div>
                <div>
                  Nickname :  {user.nickname}
                </div>
                <div>
                  w : <Badge color="success">{user.timesOfWins}</Badge> ,
                      L : <Badge color="danger">{user.matchPlayed}</Badge>
                </div>
              </div>
              <Button color={user.userId === userId ? "" : "primary"} size="sm" onClick={() => sendGameRequest(user.userId)}>{user.userId === userId ? "You" : "Challenge"}</Button>
            </ListGroupItem>)
        }
      </ListGroup>
    </div>
  )
}
export default OnlineUsers;