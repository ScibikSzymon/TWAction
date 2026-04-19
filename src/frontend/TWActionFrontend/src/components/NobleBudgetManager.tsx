import { useState, useEffect, useCallback } from "react";
import { nobleBudgetService } from "../services/nobleBudgetService";
import type { NobleBudget, PlayerBudgetItem } from "../types/nobleBudget";
import styles from "./NobleBudgetManager.module.css";

interface NobleBudgetManagerProps {
  scheduleId: string | null;
}

interface PlayerBudgetRow {
  playerId: number;
  playerName: string;
  totalNobles: number;
  budget: number;
}

export const NobleBudgetManager = ({ scheduleId }: NobleBudgetManagerProps) => {
  const [budgetRows, setBudgetRows] = useState<PlayerBudgetRow[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    if (!scheduleId) return;

    setIsLoading(true);
    setError(null);
    try {
      // Fetch player noble stats
      const stats = await nobleBudgetService.getPlayerNobleStats(scheduleId);

      // Fetch existing budgets
      let existingBudgets: NobleBudget[] = [];
      try {
        existingBudgets = await nobleBudgetService.getNobleBudgets(scheduleId);
      } catch {
        // No existing budgets is fine
        console.log("No existing budgets found");
      }

      // Create budget rows with existing budgets or defaults
      const budgetMap = new Map(
        existingBudgets.map((b) => [b.playerId, b.budget]),
      );

      const rows: PlayerBudgetRow[] = stats.map((stat) => ({
        playerId: stat.playerId,
        playerName: stat.playerName,
        totalNobles: stat.totalNobles,
        budget: budgetMap.get(stat.playerId) ?? stat.totalNobles, // Default to total nobles
      }));

      setBudgetRows(rows);
    } catch (err: unknown) {
      console.error("Error loading noble budget data:", err);
      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: {
            status?: number;
            data?: { error?: string };
          };
        };

        if (axiosError.response?.status === 404) {
          setError(
            "Nie znaleziono stanu wojsk dla tej rozpiski. Najpierw wgraj stan armii.",
          );
        } else if (axiosError.response?.data?.error) {
          setError(axiosError.response.data.error);
        } else {
          setError("Nie udało się załadować danych o szlachcicach");
        }
      } else {
        setError("Nie udało się załadować danych o szlachcicach");
      }
    } finally {
      setIsLoading(false);
    }
  }, [scheduleId]);

  useEffect(() => {
    if (scheduleId) {
      loadData();
    } else {
      setBudgetRows([]);
    }
  }, [scheduleId, loadData]);

  const handleBudgetChange = (playerId: number, newBudget: number) => {
    setBudgetRows((prevRows) =>
      prevRows.map((row) =>
        row.playerId === playerId ? { ...row, budget: newBudget } : row,
      ),
    );
  };

  const handleSave = async () => {
    if (!scheduleId) return;

    setIsSaving(true);
    setError(null);
    setSuccessMessage(null);
    try {
      const playerBudgets: PlayerBudgetItem[] = budgetRows.map((row) => ({
        playerId: row.playerId,
        budget: row.budget,
      }));

      await nobleBudgetService.saveNobleBudgets(scheduleId, {
        playerBudgets,
      });

      setSuccessMessage("Limity szlachciców zostały pomyślnie zapisane!");
      setTimeout(() => setSuccessMessage(null), 5000);
    } catch (err: unknown) {
      console.error("Error saving noble budgets:", err);
      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: { data?: { error?: string } };
        };
        setError(
          axiosError.response?.data?.error ||
            "Nie udało się zapisać limitów szlachciców",
        );
      } else {
        setError("Nie udało się zapisać limitów szlachciców");
      }
    } finally {
      setIsSaving(false);
    }
  };

  if (!scheduleId) {
    return (
      <div className={styles.container}>
        <h3>Limity Szlachciców</h3>
        <p className={styles.noActiveSchedule}>
          Wybierz aktywną rozpiskę główną, aby zarządzać limitami szlachciców
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className={styles.container}>
        <h3>Limity Szlachciców</h3>
        <div className={styles.loading}>Ładowanie danych...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.container}>
        <h3>Limity Szlachciców</h3>
        <div className={styles.error}>{error}</div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <h3>Limity Szlachciców</h3>

      {successMessage && <div className={styles.success}>{successMessage}</div>}

      <div className={styles.description}>
        <p>
          Ustaw maksymalną liczbę szlachciców, którą dany gracz może użyć w
          rozpiisce. Domyślnie ustawiona jest suma wszystkich szlachciców gracza
          ze wszystkich wiosek.
        </p>
      </div>

      {budgetRows.length === 0 ? (
        <div className={styles.noData}>
          Brak danych o graczach. Upewnij się, że wgrałeś stan armii.
        </div>
      ) : (
        <>
          <div className={styles.tableWrapper}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Nazwa Gracza</th>
                  <th>Suma Szlachciców</th>
                  <th>Maksymalny Limit</th>
                </tr>
              </thead>
              <tbody>
                {budgetRows.map((row) => (
                  <tr key={row.playerId}>
                    <td className={styles.playerName}>{row.playerName}</td>
                    <td className={styles.totalNobles}>{row.totalNobles}</td>
                    <td className={styles.budgetInput}>
                      <input
                        type="number"
                        min="0"
                        value={row.budget}
                        onChange={(e) =>
                          handleBudgetChange(
                            row.playerId,
                            parseInt(e.target.value) || 0,
                          )
                        }
                        className={styles.input}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className={styles.actions}>
            <button
              className={styles.saveBtn}
              onClick={handleSave}
              disabled={isSaving}
            >
              {isSaving ? "Zapisywanie..." : "Zapisz Limity"}
            </button>
          </div>
        </>
      )}
    </div>
  );
};
