import { useState } from "react";
import { reconnaissanceActionsService } from "../services/reconnaissanceActionsService";
import type { Schedule } from "../types/schedule";
import styles from "./TroopsStateManager.module.css";

interface ReconnaissanceActionsGeneratorProps {
  scheduleId: string;
  schedule: Schedule | null;
}

export const ReconnaissanceActionsGenerator = ({
  scheduleId,
  schedule,
}: ReconnaissanceActionsGeneratorProps) => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleGenerate = async () => {
    setIsGenerating(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const response =
        await reconnaissanceActionsService.generateReconnaissanceActions(
          scheduleId,
        );

      setSuccessMessage(
        `Pomyślnie wygenerowano ${response.generatedCommandsCount} komend zwiadowczych!`,
      );

      // Clear success message after 10 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
    } catch (err: unknown) {
      console.error("Error generating reconnaissance actions:", err);

      // Extract error message from backend response
      let errorMessage = "Nie udało się wygenerować akcji zwiadowczych";

      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: {
            status?: number;
            data?: { error?: string };
          };
        };

        // Handle specific error statuses
        if (axiosError.response?.status === 404) {
          errorMessage =
            "Rozpiska nie istnieje. Została prawdopodobnie usunięta. Odśwież stronę, aby zaktualizować listę rozpisek.";
        } else if (axiosError.response?.status === 400) {
          // Backend returns specific validation errors in 400 Bad Request
          errorMessage =
            axiosError.response.data?.error ||
            "Błąd walidacji. Sprawdź czy wszystkie wymagane dane zostały wprowadzone.";
        } else if (axiosError.response?.data?.error) {
          errorMessage = axiosError.response.data.error;
        }
      }

      setError(errorMessage);
    } finally {
      setIsGenerating(false);
    }
  };

  // Check if all prerequisites are met
  const canGenerate = schedule?.enemyIds && schedule.enemyIds.length > 0;

  const getWarningMessage = (): string | null => {
    if (!schedule) {
      return "Ładowanie rozpiski...";
    }

    if (!schedule.enemyIds || schedule.enemyIds.length === 0) {
      return "Wybierz wrogów w ustawieniach rozpiski, aby wygenerować akcje zwiadowcze.";
    }

    return null;
  };

  const warningMessage = getWarningMessage();

  return (
    <div className={styles.container}>
      <h3>Generuj Akcje Zwiadowcze</h3>

      {error && <div className={styles.error}>{error}</div>}
      {successMessage && <div className={styles.success}>{successMessage}</div>}

      {warningMessage && (
        <div className={styles.info}>
          <p>{warningMessage}</p>
        </div>
      )}

      {!warningMessage && (
        <div className={styles.info}>
          <p>Przed wygenerowaniem akcji zwiadowczych upewnij się, że:</p>
          <ul>
            <li>✓ Wybrani są wrogowie (plemię lub gracze)</li>
            <li>✓ Wgrany jest stan wojsk (zakładka "Stan Armii")</li>
            <li>✓ Zapisane są ustawienia zwiadowcze</li>
          </ul>
          <p>
            System automatycznie wygeneruje optymalną rozpiskę akcji
            zwiadowczych na podstawie wprowadzonych danych.
          </p>
        </div>
      )}

      <div className={styles.uploadSection}>
        <button
          className={styles.uploadBtn}
          onClick={handleGenerate}
          disabled={isGenerating || !canGenerate}
        >
          {isGenerating ? "Generowanie..." : "Generuj Akcje Zwiadowcze"}
        </button>

        {!canGenerate && (
          <p className={styles.hint}>
            Wypełnij wszystkie wymagane dane, aby móc wygenerować akcje.
          </p>
        )}
      </div>
    </div>
  );
};
