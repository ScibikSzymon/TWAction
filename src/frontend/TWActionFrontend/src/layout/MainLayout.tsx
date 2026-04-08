import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import styles from "./MainLayout.module.css";

const MainLayout = () => {
  const { isAuthenticated } = useAuth();

  return (
    <>
      {isAuthenticated && (
        <nav className={styles.nav}>
          <NavLink
            to="/"
            end
            className={({ isActive }) =>
              `${styles.navLink} ${isActive ? styles.active : ""}`
            }
          >
            📋 Rozpiski
          </NavLink>
          <NavLink
            to="/templates"
            className={({ isActive }) =>
              `${styles.navLink} ${isActive ? styles.active : ""}`
            }
          >
            🎯 Szablony akcji
          </NavLink>
        </nav>
      )}
      <Outlet />
    </>
  );
};

export default MainLayout;