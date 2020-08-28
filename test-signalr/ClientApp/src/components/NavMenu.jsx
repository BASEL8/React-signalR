import React, { useState } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

const NavMenu = ({ connectionStatus, nickname }) => {


  const [collapsed, setCollapsed] = useState(true);


  const toggleNavbar = () => setCollapsed(!setCollapsed);
  const isOnline = <div className="d-flex align-items-center nav-link"><div style={{ width: 10, height: 10, margin: 5, padding: 0, borderRadius: "50%", backgroundColor: connectionStatus ? "lightgreen" : "red" }}></div>{connectionStatus ? "online" : "offline"}</div>


  return (
    <header>
      <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
        <Container>
          <NavbarBrand tag={Link} to="/">SignalR / React</NavbarBrand>
          <NavbarToggler onClick={toggleNavbar} className="mr-2" />
          <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={collapsed} navbar>
            <ul className="navbar-nav flex-grow">
              <NavItem>
                {nickname && <NavLink tag={Link} className="text-dark" to="/">Welcome, {nickname}</NavLink>}
              </NavItem>
              <NavItem>
                {isOnline}
              </NavItem>
            </ul>
          </Collapse>
        </Container>
      </Navbar>
    </header>
  );

}
export default NavMenu;
