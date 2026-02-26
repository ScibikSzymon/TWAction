import { useState, useEffect, useCallback } from "react";
import { reconnaissanceActionsService } from "../services/reconnaissanceActionsService";
import { reconnaissanceSettingsService } from "../services/reconnaissanceSettingsService";
import { troopsStateService } from "../services/troopsStateService";
import { attackCommandsService } from "../services/attackCommandsService";
import type { Schedule } from "../types/schedule";
import type { AttackCommandsSummary } from "../types/attackCommands";
import styles from "./TroopsStateManager.module.css";

interface ReconnaissanceActionsGeneratorProps {
  scheduleId: string;
  schedule: Schedule | null;
}

const formatDate = (isoString: string): string =>
  new Date(isoString).toLocaleString("pl-PL", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });

const COMMAND_TYPE_LABELS: Record<string, string> = {
  Reconnaissance: "Zwiad",
};

const getCommandTypeLabel = (type: string): string =>
  COMMAND_TYPE_LABELS[type] ?? type;

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
  const [summary, setSummary] = useState<AttackCommandsSummary | null>(null);

  const fetchSummary = useCallback(async () => {
    try {
      const s =
        await attackCommandsService.getAttackCommandsSummary(scheduleId);
      setSummary(s);
    } catch {
      setSummary(null);
    }
  }, [scheduleId]);

  // Check prerequisites and existing commands on mount
  useEffect(() => {
    const checkPrerequisites = async () => {
      setIsCheckingPrerequisites(true);

      try {
        try {
          await reconnaissanceSettingsService.getReconnaissanceSettings(
            scheduleId,
          );
          setHasSettings(true);
        } catch {
          setHasSettings(false);
        }

        try {
          await troopsStateService.getTroopsState(scheduleId);
          setHasTroopsState(true);
        } catch {
          setHasTroopsState(false);
        }

        await fetchSummary();
      } finally {
        setIsCheckingPrerequisites(false);
      }
    };

    checkPrerequisites();
  }, [scheduleId, fetchSummary]);

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

      // Refresh summary after generation
      await fetchSummary();

      setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
    } catch (err: unknown) {
      console.error("Error generating reconnaissance actions:", err);

      let errorMessage = "Nie udało się wygenerować akcji zwiadowczych";

      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: {
            status?: number;
            data?: { error?: string };
          };
        };

        if (axiosError.response?.status === 404) {
          errorMessage =
            "Rozpiska nie istnieje. Została prawdopodobnie usunięta. Odśwież stronę, aby zaktualizować listę rozpisek.";
        } else if (axiosError.response?.status === 400) {
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

  const hasEnemies = schedule?.enemyIds && schedule.enemyIds.length > 0;
  const canGenerate =
    hasEnemies && hasSettings && hasTroopsState && !isCheckingPrerequisites;

  const getWarningMessage = (): string | null => {
    if (!schedule) return "Ładowanie rozpiski...";
    if (isCheckingPrerequisites) return "Sprawdzanie wymaganych danych...";

    const missingItems: string[] = [];
    if (!hasEnemies)
      missingItems.push("Wybierz wrogów w ustawieniach rozpiski");
    if (!hasTroopsState)
      missingItems.push('Wgraj stan wojsk w zakładce "Stan Armii"');
    if (!hasSettings)
      missingItems.push(
        'Zapisz ustawienia zwiadowcze w zakładce "Ustawienia Zwiadowcze"',
      );

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

      {summary && (
        <div className={styles.info}>
          <p>
            <strong>Wygenerowana rozpiska</strong>
          </p>
          <ul>
            <li>
              Liczba komend: <strong>{summary.totalCount}</strong>
            </li>
            <li>
              Pierwszy minutm wysłania:{" "}
              <strong>{formatDate(summary.firstMinDepartureTime)}</strong>
            </li>
            <li>
              Ostatni minutm wysłania:{" "}
              <strong>{formatDate(summary.lastMinDepartureTime)}</strong>
            </li>
            <li>
              Komendy według typu:
              <ul>
                {Object.entries(summary.countByType).map(([type, count]) => (
                  <li key={type}>
                    {getCommandTypeLabel(type)}: <strong>{count}</strong>
                  </li>
                ))}
              </ul>
            </li>
            <li>
              Data wygenerowania:{" "}
              <strong>{formatDate(summary.generatedAt)}</strong>
            </li>
          </ul>
        </div>
      )}

      {warningMessage && (
        <div className={styles.info}>
          <p style={{ whiteSpace: "pre-line" }}>{warningMessage}</p>
        </div>
      )}

      {!warningMessage && !summary && (
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
          {isGenerating
            ? "Generowanie..."
            : summary
              ? "Przegeneruj Akcje Zwiadowcze"
              : "Generuj Akcje Zwiadowcze"}
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
