import { useState, useEffect } from "react";
import type { User } from "../types/user";
import { userService } from "../services/userService";
import styles from "./UserPanel.module.css";

const UserPanel = () => {
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
        setError("Nie udało się załadować użytkowników");
      } finally {
        setIsLoading(false);
      }
    };

    loadUsers();
  }, []);

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h1>Panel użytkowników</h1>
        <span className={styles.badge}>{users.length} użytkowników</span>
      </header>

      {error && <div className={styles.error}>{error}</div>}

      {isLoading ? (
        <div className={styles.loading}>Ładowanie użytkowników...</div>
      ) : users.length === 0 ? (
        <div className={styles.empty}>Brak użytkowników</div>
      ) : (
        <div className={styles.tableWrapper}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Email</th>
                <th>Nazwa</th>
                <th>Dostawca</th>
                <th>Rola</th>
                <th>Data utworzenia</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.email}</td>
                  <td>{user.displayName ?? "—"}</td>
                  <td>{user.provider}</td>
                  <td>
                    <span
                      className={`${styles.roleBadge} ${user.role === "Admin" ? styles.roleAdmin : styles.roleUser}`}
                    >
                      {user.role}
                    </span>
                  </td>
                  <td>
                    {new Date(user.createdAt).toLocaleDateString("pl-PL", {
                      year: "numeric",
                      month: "short",
                      day: "numeric",
                    })}
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
