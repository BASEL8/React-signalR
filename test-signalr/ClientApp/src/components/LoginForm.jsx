import React, { useState } from 'react'
import { Button, Form, FormGroup, Label, Input } from 'reactstrap';

const LoginForm = ({ connection, token, userId }) => {
  const [nickname, setNickname] = useState("");
  const [error, setError] = useState("");
  const handleSignup = (event) => {
    event.preventDefault();
    connection.invoke("LoginOrSignup", nickname, token, userId).catch(err => console.log(err))
  };


  if (connection) {
    connection.on("SignupError", (err) => {
      setError(err);
    });
    connection.on("LoginError", (err) => {
      setError(err);
    });
  }

  return (
    <Form onSubmit={handleSignup} className="w-75 p-5 shadow-lg">
      <h2 className="mb-5">Signup</h2>
      <FormGroup>
        <Label for="nickname">Nickname</Label>
        <Input type="text" value={nickname} onChange={(e) => { setNickname(e.target.value); error && setError(""); }} placeholder="Insert Your Nickname" />
      </FormGroup>
      <FormGroup>
        <Button>Submit</Button>
      </FormGroup>
      {error && error}
    </Form>
  )
}

export default LoginForm;