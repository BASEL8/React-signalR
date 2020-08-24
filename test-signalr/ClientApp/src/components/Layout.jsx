import React from 'react';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';

const Layout = ({ connectionStatus, nickname, children }) => {

  return (
    <div className="flex-grow-1 d-flex flex-column">
      <NavMenu connectionStatus={connectionStatus} nickname={nickname} />
      <Container fluid className="flex-grow-1 d-flex align-items-center justify-content-center">
        {children}
      </Container>
    </div>
  );
}
export default Layout;