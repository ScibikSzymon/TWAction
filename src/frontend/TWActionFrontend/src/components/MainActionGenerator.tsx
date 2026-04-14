import { useState, useEffect, useCallback } from "react";
import { mainActionGeneratorService } from "../services/mainActionGeneratorService";
import { mainActionSettingsService } from "../services/mainActionSettingsService";
import { troopsStateService } from "../services/troopsStateService";
import { targetGroupService } from "../services/targetGroupService";
import { attackCommandsService } from "../services/attackCommandsService";
import type { Schedule } from "../types/schedule";
import type { AttackCommandsSummary } from "../types/attackCommands";
import styles from "./TroopsStateManager.module.css";

interface MainActionGeneratorProps {
  scheduleId: string;
  schedule: Schedule | null;
  onScheduleUpdate?: (updatedSchedule: Partial<Schedule>) => void;
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
  Off: "OFF (atak offowy)",
  FakeOffensive: "Fejk OFF",
  FakeDefensive: "Fejk DEFF",
  Catapults: "Burzenie",
  NobleWithDeff: "Szlachcic z zagrodą deffa",
  NobleWithFullOff: "Szlachcic z pełnym offem",
  NobleWithHalfOff: "Szlachcic z połową offa",
  NobleWithFakeOff: "Szlachcic z fejk offem",
  NobleWithFakeDeff: "Szlachcic z fejk deffem",
};

const getCommandTypeLabel = (type: string): string =>
  COMMAND_TYPE_LABELS[type] ?? type;

export const MainActionGenerator = ({
  scheduleId,
  schedule,
  onScheduleUpdate,
}: MainActionGeneratorProps) => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [hasSettings, setHasSettings] = useState(false);
  const [hasTroopsState, setHasTroopsState] = useState(false);
  const [hasTargetGroups, setHasTargetGroups] = useState(false);
  const [targetGroupCount, setTargetGroupCount] = useState(0);
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

  useEffect(() => {
    const checkPrerequisites = async () => {
      setIsCheckingPrerequisites(true);

      try {
        try {
          await mainActionSettingsService.getMainActionSettings(scheduleId);
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

        try {
          const groups = await targetGroupService.getGroups(scheduleId);
          setTargetGroupCount(groups.length);
          setHasTargetGroups(groups.length > 0);
        } catch {
          setTargetGroupCount(0);
          setHasTargetGroups(false);
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
        await mainActionGeneratorService.generateMainActions(scheduleId);

      setSuccessMessage(
        `Pomyślnie wygenerowano ${response.generatedCommandsCount} komend! ` +
          `(${response.targetGroupCount} grup celi, ${response.targetVillageCount} wiosek docelowych)`,
      );

      await fetchSummary();

      setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
    } catch (err: unknown) {
      console.error("Error generating main actions:", err);

      let errorMessage = "Nie udało się wygenerować głównej akcji";

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

  const handleSendToPlemionaRozpiski = async (
    forceOverwrite: boolean = false,
  ) => {
    if (schedule?.sentToPlemionaRozpiskiAt && !forceOverwrite) {
      setShowConfirmDialog(true);
      return;
    }

    setIsSending(true);
    setError(null);
    setSuccessMessage(null);
    setShowConfirmDialog(false);

    try {
      const response = await attackCommandsService.sendToPlemionaRozpiski(
        scheduleId,
        forceOverwrite,
      );

      setSuccessMessage(
        `Pomyślnie wysłano ${response.commandsSentCount} komend na plemionarozpiski.pl!`,
      );

      onScheduleUpdate?.({ sentToPlemionaRozpiskiAt: response.sentAt });

      setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
    } catch (err: unknown) {
      console.error("Error sending to plemionarozpiski.pl:", err);

      let errorMessage = "Nie udało się wysłać rozpiski na plemionarozpiski.pl";

      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: {
            status?: number;
            data?: { error?: string };
          };
        };

        if (axiosError.response?.data?.error) {
          errorMessage = axiosError.response.data.error;
        }
      }

      setError(errorMessage);
    } finally {
      setIsSending(false);
    }
  };

  const canGenerate =
    hasSettings &&
    hasTroopsState &&
    hasTargetGroups &&
    !isCheckingPrerequisites;

  const getWarningMessage = (): string | null => {
    if (!schedule) return "Ładowanie rozpiski...";
    if (isCheckingPrerequisites) return "Sprawdzanie wymaganych danych...";

    const missingItems: string[] = [];
    if (!hasTroopsState)
      missingItems.push('Wgraj stan wojsk w zakładce "Stan Armii"');
    if (!hasSettings)
      missingItems.push(
        'Zapisz ustawienia głównej akcji w zakładce "Ustawienia Głównej Akcji"',
      );
    if (!hasTargetGroups)
      missingItems.push(
        'Utwórz przynajmniej jedną grupę celi w zakładce "Grupy Celi"',
      );

    if (missingItems.length > 0) {
      return (
        "Aby wygenerować główną akcję, musisz:\n" +
        missingItems.map((item) => `• ${item}`).join("\n")
      );
    }

    return null;
  };

  const warningMessage = getWarningMessage();

  return (
    <div className={styles.container}>
      <h3>Generuj Główną Akcję</h3>

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
              Pierwszy czas wysłania:{" "}
              <strong>{formatDate(summary.firstMinDepartureTime)}</strong>
            </li>
            <li>
              Ostatni czas wysłania:{" "}
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
            {schedule?.sentToPlemionaRozpiskiAt && (
              <li>
                Wysłano na plemionarozpiski.pl:{" "}
                <strong>{formatDate(schedule.sentToPlemionaRozpiskiAt)}</strong>
              </li>
            )}
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
            <li>✓ Wgrany jest stan wojsk</li>
            <li>✓ Zapisane są ustawienia głównej akcji</li>
            <li>✓ Zdefiniowane są grupy celi ({targetGroupCount})</li>
          </ul>
          <p>
            System automatycznie wygeneruje optymalną rozpiskę głównej akcji na
            podstawie wprowadzonych danych.
          </p>
        </div>
      )}

      {!warningMessage && summary && (
        <div className={styles.info}>
          <p>
            Grupy celi: <strong>{targetGroupCount}</strong>
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
              ? "Przegeneruj Główną Akcję"
              : "Generuj Główną Akcję"}
        </button>

        {summary && (
          <button
            className={styles.uploadBtn}
            onClick={() => handleSendToPlemionaRozpiski(false)}
            disabled={isSending}
            style={{ marginLeft: "10px" }}
          >
            {isSending
              ? "Wysyłanie..."
              : schedule?.sentToPlemionaRozpiskiAt
                ? "Wyślij ponownie na plemionarozpiski.pl"
                : "Wyślij na plemionarozpiski.pl"}
          </button>
        )}

        {!canGenerate && !isCheckingPrerequisites && (
          <p className={styles.hint}>
            Wypełnij wszystkie wymagane dane, aby móc wygenerować główną akcję.
          </p>
        )}
      </div>

      {showConfirmDialog && (
        <div className={styles.confirmDialog}>
          <div className={styles.confirmDialogContent}>
            <p>
              Ta rozpiska została już wysłana na plemionarozpiski.pl dnia{" "}
              <strong>
                {schedule?.sentToPlemionaRozpiskiAt &&
                  formatDate(schedule.sentToPlemionaRozpiskiAt)}
              </strong>
              .
            </p>
            <p>Czy na pewno chcesz nadpisać istniejącą rozpiskę?</p>
            <div className={styles.confirmDialogButtons}>
              <button
                className={styles.uploadBtn}
                onClick={() => handleSendToPlemionaRozpiski(true)}
              >
                Tak, nadpisz
              </button>
              <button
                className={styles.cancelBtn}
                onClick={() => setShowConfirmDialog(false)}
              >
                Anuluj
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
