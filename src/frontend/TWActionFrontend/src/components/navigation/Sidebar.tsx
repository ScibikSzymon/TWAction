import { NavLink } from "react-router-dom";
import type { User } from "../../types/user";
import styles from "./Sidebar.module.css";

interface SidebarProps {
  user: User;
  onLogout: () => void;
}

export const Sidebar = ({ user, onLogout }: SidebarProps) => {
  return (
    <aside className={styles.sidebar}>
      <div className={styles.sidebarHeader}>
        <h2 className={styles.logo}>TWAction</h2>
      </div>

      <nav className={styles.nav}>
        <NavLink
          to="/"
          end
          className={({ isActive }) =>
            `${styles.navLink} ${isActive ? styles.navLinkActive : ""}`
          }
        >
          Strona główna
        </NavLink>

        {user.role === "Admin" && (
          <NavLink
            to="/admin/users"
            className={({ isActive }) =>
              `${styles.navLink} ${isActive ? styles.navLinkActive : ""}`
            }
          >
            Panel użytkowników
          </NavLink>
        )}
      </nav>

      <div className={styles.sidebarFooter}>
        <div className={styles.userInfo}>
          <span className={styles.userEmail}>{user.email}</span>
          <span className={styles.userRole}>{user.role}</span>
        </div>
        <button onClick={onLogout} className={styles.logoutBtn}>
          Wyloguj się
        </button>
      </div>
    </aside>
  );
};
