import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import styles from "./ProtectedRoute.module.css";

interface ProtectedRouteProps {
  requiredRole?: string;
}

export const ProtectedRoute = ({ requiredRole }: ProtectedRouteProps) => {
  const { user, isLoading, isAuthenticated } = useAuth();

  if (isLoading) {
    return <div className={styles.loading}>Ładowanie...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (requiredRole && user?.role !== requiredRole) {
    return (
      <div className={styles.accessDenied}>
        <div className={styles.accessDeniedCard}>
          <h2>Brak dostępu</h2>
          <p>Nie masz uprawnień do wyświetlenia tej strony.</p>
        </div>
      </div>
    );
  }

  return <Outlet />;
};
