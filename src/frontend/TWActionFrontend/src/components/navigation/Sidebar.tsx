import { NavLink } from "react-router-dom";
import type { User } from "../../types/user";
import { useI18n } from "../../i18n/I18nProvider";
import styles from "./Sidebar.module.css";

interface SidebarProps {
  user: User;
  onLogout: () => void;
}

export const Sidebar = ({ user, onLogout }: SidebarProps) => {
  const { t, language, setLanguage } = useI18n();

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
          {t.sidebar.home}
        </NavLink>

        {user.role === "Admin" && (
          <NavLink
            to="/admin/users"
            className={({ isActive }) =>
              `${styles.navLink} ${isActive ? styles.navLinkActive : ""}`
            }
          >
            {t.sidebar.userPanel}
          </NavLink>
        )}
      </nav>

      <div className={styles.sidebarFooter}>
        <div className={styles.userInfo}>
          <span className={styles.userEmail}>{user.email}</span>
          <span className={styles.userRole}>{user.role}</span>
        </div>
        <div className={styles.langSwitch}>
          <button
            onClick={() => setLanguage("pl")}
            className={`${styles.langBtn} ${language === "pl" ? styles.langBtnActive : ""}`}
          >
            PL
          </button>
          <button
            onClick={() => setLanguage("en")}
            className={`${styles.langBtn} ${language === "en" ? styles.langBtnActive : ""}`}
          >
            EN
          </button>
        </div>
        <button onClick={onLogout} className={styles.logoutBtn}>
          {t.sidebar.logout}
        </button>
      </div>
    </aside>
  );
};
