import { Link, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { Sidebar } from "../components/navigation/Sidebar";
import styles from "./MainLayout.module.css";

const MainLayout = () => {
  const { user, isLoading, login, logout, isAuthenticated } = useAuth();
  const { pathname } = useLocation();
  const isPublicPage = pathname === "/about";

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <div className={styles.loading}>Ładowanie...</div>
      </div>
    );
  }

  if (!isAuthenticated && isPublicPage) {
    return (
      <div className={styles.publicLayout}>
        <header className={styles.publicHeader}>
          <Link to="/about" className={styles.publicLogo}>TWAction</Link>
          <button onClick={login} className={styles.publicLoginBtn}>
            Zaloguj się
          </button>
        </header>
        <main className={styles.publicContent}>
          <Outlet />
        </main>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className={styles.loginContainer}>
        <div className={styles.loginCard}>
          <h1>TWAction</h1>
          <p>Zarządzaj swoimi rozpiskami</p>
          <button onClick={login} className={styles.loginBtn}>
            Zaloguj się przez Google
          </button>
          <Link to="/about" className={styles.aboutLink}>
            Zobacz plany i limity
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.layout}>
      <Sidebar user={user!} onLogout={logout} />

      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;
