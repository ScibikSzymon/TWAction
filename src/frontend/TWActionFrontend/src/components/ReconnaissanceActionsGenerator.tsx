import { useState, useEffect } from "react";
import { reconnaissanceActionsService } from "../services/reconnaissanceActionsService";
import { reconnaissanceSettingsService } from "../services/reconnaissanceSettingsService";
import { troopsStateService } from "../services/troopsStateService";
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
  const [hasSettings, setHasSettings] = useState(false);
  const [hasTroopsState, setHasTroopsState] = useState(false);
  const [isCheckingPrerequisites, setIsCheckingPrerequisites] = useState(true);

  // Check prerequisites on component mount
  useEffect(() => {
    const checkPrerequisites = async () => {
      setIsCheckingPrerequisites(true);

      try {
        // Check if reconnaissance settings exist
        try {
          await reconnaissanceSettingsService.getReconnaissanceSettings(
            scheduleId,
          );
          setHasSettings(true);
        } catch {
          setHasSettings(false);
        }

        // Check if troops state exists
        try {
          await troopsStateService.getTroopsState(scheduleId);
          setHasTroopsState(true);
        } catch {
          setHasTroopsState(false);
        }
      } finally {
        setIsCheckingPrerequisites(false);
      }
    };

    checkPrerequisites();
  }, [scheduleId]);

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
  const hasEnemies = schedule?.enemyIds && schedule.enemyIds.length > 0;
  const canGenerate =
    hasEnemies && hasSettings && hasTroopsState && !isCheckingPrerequisites;

  const getWarningMessage = (): string | null => {
    if (!schedule) {
      return "Ładowanie rozpiski...";
    }

    if (isCheckingPrerequisites) {
      return "Sprawdzanie wymaganych danych...";
    }

    const missingItems: string[] = [];

    if (!hasEnemies) {
      missingItems.push("Wybierz wrogów w ustawieniach rozpiski");
    }

    if (!hasTroopsState) {
      missingItems.push('Wgraj stan wojsk w zakładce "Stan Armii"');
    }

    if (!hasSettings) {
      missingItems.push(
        'Zapisz ustawienia zwiadowcze w zakładce "Ustawienia Zwiadowcze"',
      );
    }

    if (missingItems.length > 0) {
      return (
        "Aby wygenerować akcje zwiadowcze, musisz:\n" +
        missingItems.map((item) => `• ${item}`).join("\n")
      );
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
          <p style={{ whiteSpace: "pre-line" }}>{warningMessage}</p>
        </div>
      )}

      {!warningMessage && (
        <div className={styles.info}>
          <p>Wszystkie wymagane dane zostały wprowadzone:</p>
          <ul>
            <li>✓ Wybrani są wrogowie (plemię lub gracze)</li>
            <li>✓ Wgrany jest stan wojsk</li>
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

        {!canGenerate && !isCheckingPrerequisites && (
          <p className={styles.hint}>
            Wypełnij wszystkie wymagane dane, aby móc wygenerować akcje.
          </p>
        )}
      </div>
    </div>
  );
};
