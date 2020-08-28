import React, { useState, useEffect, useRef } from 'react'
const Board = ({ matchData, connection, matchId }) => {
  const [data, setData] = useState([]);
  const [width, setItemWidth] = useState(250);
  const bordRef = useRef();
  const handleClick = (index) => {
    connection.invoke("CheckUsersMove", index, matchId).catch(err => console.log(err));
  }
  const SquareStyle = {
    fontSize: "50px",
    fontWeight: "400",
    fontFamily: "fantasy",
    cursor: "pointer",
    outline: "none",
    display: "flex",
    alignItems: "center",
    justifyContent: "center"
  };

  const boardStyle = {
    borderRadius: "1px",
    width: "90%",
    height: width,
    margin: "0 auto",
    display: "grid",
    gridTemplate: "repeat(3, 1fr) / repeat(3, 1fr)",
  }
  useEffect(() => {
    if (matchData) {
      setData(matchData);
    }
  }, [connection, matchData])
  useEffect(() => {
    if (bordRef.current) {
      setItemWidth(window.getComputedStyle(bordRef.current).width);
    }
  }, [])
  if (!matchData) {
    return (<div>...wait</div>)
  }
  return (
    <div style={boardStyle} ref={bordRef}>
      {data.length > 0 && data.map((value, index) => {
        const top = index === 0 || index === 1 || index === 2;
        const left = index === 0 || index === 3 || index === 6;
        const bottom = index === 6 || index === 7 || index === 8;
        const right = index === 2 || index === 5 || index === 8;
        return <div
          key={index}
          style={SquareStyle}
          className={`border ${top && "border-top-0"} ${left && "border-left-0"} ${bottom && "border-bottom-0"} ${right && "border-right-0"}`}
          onClick={() => handleClick(index)}>
          {value}
        </div>
      })}
    </div>
  )
}
export default Board;