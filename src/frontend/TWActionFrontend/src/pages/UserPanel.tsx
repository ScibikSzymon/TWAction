import { useState, useEffect } from "react";
import type { User } from "../types/user";
import { userService } from "../services/userService";
import { useI18n } from "../i18n/I18nProvider";
import styles from "./UserPanel.module.css";

const UserPanel = () => {
  const { t, language } = useI18n();
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadUsers = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await userService.getAllUsers();
        setUsers(data);
      } catch (err) {
        console.error("Error loading users:", err);
        setError(t.userPanel.error);
      } finally {
        setIsLoading(false);
      }
    };

    loadUsers();
  }, []);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h1>{t.userPanel.title}</h1>
        <span className={styles.badge}>{users.length} {t.userPanel.usersCount}</span>
      </header>

      {error && <div className={styles.error}>{error}</div>}

      {isLoading ? (
        <div className={styles.loading}>{t.userPanel.loading}</div>
      ) : users.length === 0 ? (
        <div className={styles.empty}>{t.userPanel.empty}</div>
      ) : (
        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>{t.userPanel.columns.email}</th>
                <th>{t.userPanel.columns.name}</th>
                <th>{t.userPanel.columns.provider}</th>
                <th>{t.userPanel.columns.role}</th>
                <th>{t.userPanel.columns.createdAt}</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>{user.displayName ?? t.userPanel.noName}</td>
                  <td>{user.provider}</td>
                  <td>
                    <span
                      className={`${styles.roleBadge} ${user.role === "Admin" ? styles.roleAdmin : styles.roleUser}`}
                    >
                      {user.role}
                    </span>
                  </td>
                  <td>
                    {new Date(user.createdAt).toLocaleDateString(
                      language === "pl" ? "pl-PL" : "en-US",
                      {
                        year: "numeric",
                        month: "short",
                        day: "numeric",
                      },
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default UserPanel;
