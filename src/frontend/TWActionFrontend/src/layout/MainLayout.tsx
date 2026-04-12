import { Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { Sidebar } from "../components/navigation/Sidebar";
import styles from "./MainLayout.module.css";

const MainLayout = () => {
  const { user, isLoading, login, logout, isAuthenticated } = useAuth();

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <div className={styles.loading}>Ładowanie...</div>
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