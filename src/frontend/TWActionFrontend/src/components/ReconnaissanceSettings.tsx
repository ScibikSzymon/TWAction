import { useState, useEffect } from "react";
import type { SaveReconnaissanceSettingsRequest } from "../types/reconnaissanceSettings";
import { getDefaultReconnaissanceSettings } from "../types/reconnaissanceSettings";
import { reconnaissanceSettingsService } from "../services/reconnaissanceSettingsService";
import styles from "./ScheduleForm.module.css";

interface ReconnaissanceSettingsProps {
  scheduleId: string;
}

export const ReconnaissanceSettings = ({
  scheduleId,
}: ReconnaissanceSettingsProps) => {
  const defaults = getDefaultReconnaissanceSettings();

  const [settings, setSettings] = useState<SaveReconnaissanceSettingsRequest>({
    minDepartureTime: defaults.minDepartureTime.toISOString(),
    minArrivalTime: defaults.minArrivalTime.toISOString(),
    maxArrivalTime: defaults.maxArrivalTime.toISOString(),
    minDistanceToFront: defaults.minDistanceToFront,
    minSpyCount: defaults.minSpyCount,
    maxPopulationInSourceVillage: defaults.maxPopulationInSourceVillage,
    skipNightSendings: defaults.skipNightSendings,
  });

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    const loadSettings = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data =
          await reconnaissanceSettingsService.getReconnaissanceSettings(
            scheduleId,
          );
        setSettings({
          minDepartureTime: data.minDepartureTime,
          minArrivalTime: data.minArrivalTime,
          maxArrivalTime: data.maxArrivalTime,
          minDistanceToFront: data.minDistanceToFront,
          minSpyCount: data.minSpyCount,
          maxPopulationInSourceVillage: data.maxPopulationInSourceVillage,
          skipNightSendings: data.skipNightSendings,
        });
      } catch (err: any) {
        // Jeśli ustawienia nie istnieją, resetuj do domyślnych
        if (err?.response?.status === 404) {
          const defaults = getDefaultReconnaissanceSettings();
          setSettings({
            minDepartureTime: defaults.minDepartureTime.toISOString(),
            minArrivalTime: defaults.minArrivalTime.toISOString(),
            maxArrivalTime: defaults.maxArrivalTime.toISOString(),
            minDistanceToFront: defaults.minDistanceToFront,
            minSpyCount: defaults.minSpyCount,
            maxPopulationInSourceVillage: defaults.maxPopulationInSourceVillage,
            skipNightSendings: defaults.skipNightSendings,
          });
        } else {
          setError("Nie udało się wczytać ustawień rozpiski zwiadowczej");
          console.error("Error loading reconnaissance settings:", err);
        }
      } finally {
        setIsLoading(false);
      }
    };

    loadSettings();
  }, [scheduleId]);

  const handleSave = async () => {
    try {
      setIsSaving(true);
      setError(null);
      setSaveSuccess(false);

      await reconnaissanceSettingsService.saveReconnaissanceSettings(
        scheduleId,
        settings,
      );

      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err) {
      setError("Wystąpił błąd podczas zapisywania ustawień");
      console.error("Error saving reconnaissance settings:", err);
    } finally {
      setIsSaving(false);
    }
  };

  const formatDateTimeLocal = (isoString: string): string => {
    const date = new Date(isoString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const handleDateTimeChange = (
    field: "minDepartureTime" | "minArrivalTime" | "maxArrivalTime",
    value: string,
  ) => {
    const newDate = new Date(value);

    setSettings((prev) => {
      const newSettings = {
        ...prev,
        [field]: newDate.toISOString(),
      };

      // Jeśli zmieniono minDepartureTime i jest większy lub równy czasom przyjazdu,
      // automatycznie dostosuj czasy przyjazdu
      if (field === "minDepartureTime") {
        const departureTime = newDate.getTime();
        const minArrivalTime = new Date(prev.minArrivalTime).getTime();
        const maxArrivalTime = new Date(prev.maxArrivalTime).getTime();

        // Jeśli czas wyjazdu >= czas minimalnego przyjazdu
        if (departureTime >= minArrivalTime) {
          // Ustaw minArrivalTime na 1 dzień po wyjeździe
          const newMinArrival = new Date(departureTime + 24 * 60 * 60 * 1000);
          newSettings.minArrivalTime = newMinArrival.toISOString();

          // Jeśli maxArrivalTime jest teraz mniejszy niż nowy minArrivalTime,
          // ustaw go na 2 dni po wyjeździe
          if (
            departureTime >= maxArrivalTime ||
            maxArrivalTime <= newMinArrival.getTime()
          ) {
            const newMaxArrival = new Date(departureTime + 2 * 24 * 60 * 60 * 1000);
            newSettings.maxArrivalTime = newMaxArrival.toISOString();
          }
        }
      }

      // Jeśli zmieniono minArrivalTime
      if (field === "minArrivalTime") {
        const minArrivalTime = newDate.getTime();
        const departureTime = new Date(prev.minDepartureTime).getTime();
        const maxArrivalTime = new Date(prev.maxArrivalTime).getTime();

        // Jeśli minArrivalTime jest przed lub równy departureTime,
        // przesuń departureTime na dzień wcześniej
        if (minArrivalTime <= departureTime) {
          const newDeparture = new Date(minArrivalTime - 24 * 60 * 60 * 1000);
          newSettings.minDepartureTime = newDeparture.toISOString();
        }

        // Jeśli minArrivalTime >= maxArrivalTime, dostosuj maxArrivalTime
        if (minArrivalTime >= maxArrivalTime) {
          // Ustaw maxArrivalTime na 2 godziny po minArrivalTime
          const newMaxArrival = new Date(minArrivalTime + 2 * 60 * 60 * 1000);
          newSettings.maxArrivalTime = newMaxArrival.toISOString();
        }
      }

      // Jeśli zmieniono maxArrivalTime
      if (field === "maxArrivalTime") {
        const maxArrivalTime = newDate.getTime();
        const minArrivalTime = new Date(prev.minArrivalTime).getTime();

        // Jeśli maxArrivalTime <= minArrivalTime, dostosuj minArrivalTime
        if (maxArrivalTime <= minArrivalTime) {
          // Ustaw minArrivalTime na 2 godziny przed maxArrivalTime
          const newMinArrival = new Date(maxArrivalTime - 2 * 60 * 60 * 1000);
          newSettings.minArrivalTime = newMinArrival.toISOString();
        }
      }

      return newSettings;
    });
  };

  const getMinDateTime = (): string => {
    return formatDateTimeLocal(new Date().toISOString());
  };

  const getMaxDateTime = (): string => {
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 7);
    return formatDateTimeLocal(maxDate.toISOString());
  };

  const getMaxDepartureDateTime = (): string => {
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 6); // tydzień - 1 dzień
    return formatDateTimeLocal(maxDate.toISOString());
  };

  if (isLoading) {
    return (
      <div className={styles.settingsSection}>
        <h4>Ustawienia rozpiski zwiadowczej</h4>
        <p className={styles.loadingText}>Ładowanie ustawień...</p>
      </div>
    );
  }

  return (
    <div className={styles.settingsSection}>
      <h4>Ustawienia rozpiski zwiadowczej</h4>

      {error && <div className={styles.error}>{error}</div>}
      {saveSuccess && (
        <div className={styles.success}>Ustawienia zapisane pomyślnie!</div>
      )}

      <div className={styles.formGroup}>
        <label htmlFor="minDepartureTime">Czas rozpoczęcia wysyłki:</label>
        <input
          id="minDepartureTime"
          type="datetime-local"
          value={formatDateTimeLocal(settings.minDepartureTime)}
          onChange={(e) =>
            handleDateTimeChange("minDepartureTime", e.target.value)
          }
          min={getMinDateTime()}
          max={getMaxDepartureDateTime()}
          disabled={isSaving}
        />
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="minArrivalTime">Minimalny czas dotarcia ataków:</label>
        <input
          id="minArrivalTime"
          type="datetime-local"
          value={formatDateTimeLocal(settings.minArrivalTime)}
          onChange={(e) =>
            handleDateTimeChange("minArrivalTime", e.target.value)
          }
          min={getMinDateTime()}
          max={getMaxDateTime()}
          disabled={isSaving}
        />
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="maxArrivalTime">Maksymalny czas dotarcia ataków:</label>
        <input
          id="maxArrivalTime"
          type="datetime-local"
          value={formatDateTimeLocal(settings.maxArrivalTime)}
          onChange={(e) =>
            handleDateTimeChange("maxArrivalTime", e.target.value)
          }
          min={getMinDateTime()}
          max={getMaxDateTime()}
          disabled={isSaving}
        />
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="minDistanceToFront">
          Minimalna odległość do frontu: {settings.minDistanceToFront}
        </label>
        <input
          id="minDistanceToFront"
          type="range"
          min="1"
          max="50"
          value={settings.minDistanceToFront}
          onChange={(e) =>
            setSettings((prev) => ({
              ...prev,
              minDistanceToFront: parseInt(e.target.value),
            }))
          }
          disabled={isSaving}
          className={styles.slider}
        />
        <div className={styles.sliderLabels}>
          <span>1</span>
          <span>50</span>
        </div>
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="minSpyCount">
          Minimalna liczba zwiadowców: {settings.minSpyCount}
        </label>
        <input
          id="minSpyCount"
          type="range"
          min="5"
          max="1000"
          step="5"
          value={settings.minSpyCount}
          onChange={(e) =>
            setSettings((prev) => ({
              ...prev,
              minSpyCount: parseInt(e.target.value),
            }))
          }
          disabled={isSaving}
          className={styles.slider}
        />
        <div className={styles.sliderLabels}>
          <span>5</span>
          <span>1000</span>
        </div>
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="maxPopulationInSourceVillage">
          Maksymalna populacja w wiosce źródłowej:{" "}
          {settings.maxPopulationInSourceVillage}
        </label>
        <input
          id="maxPopulationInSourceVillage"
          type="range"
          min="100"
          max="50000"
          step="100"
          value={settings.maxPopulationInSourceVillage}
          onChange={(e) =>
            setSettings((prev) => ({
              ...prev,
              maxPopulationInSourceVillage: parseInt(e.target.value),
            }))
          }
          disabled={isSaving}
          className={styles.slider}
        />
        <div className={styles.sliderLabels}>
          <span>100</span>
          <span>50000</span>
        </div>
      </div>

      <div className={styles.formGroup}>
        <label className={styles.checkboxLabel}>
          <input
            id="skipNightSendings"
            type="checkbox"
            checked={settings.skipNightSendings}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                skipNightSendings: e.target.checked,
              }))
            }
            disabled={isSaving}
          />
          <span>Pomiń wysyłki nocne</span>
        </label>
      </div>

      <button
        type="button"
        onClick={handleSave}
        disabled={isSaving}
        className={styles.submitBtn}
      >
        {isSaving ? "Zapisywanie..." : "Zapisz ustawienia"}
      </button>
    </div>
  );
};
