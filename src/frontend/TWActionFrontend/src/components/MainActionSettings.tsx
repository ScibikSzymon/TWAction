import { useState, useEffect } from "react";
import type { SaveMainActionSettingsRequest } from "../types/mainActionSettings";
import { getDefaultMainActionSettings } from "../types/mainActionSettings";
import { mainActionSettingsService } from "../services/mainActionSettingsService";
import styles from "./ScheduleForm.module.css";

interface MainActionSettingsProps {
  scheduleId: string;
}

export const MainActionSettings = ({ scheduleId }: MainActionSettingsProps) => {
  const defaults = getDefaultMainActionSettings();

  const [settings, setSettings] = useState<SaveMainActionSettingsRequest>({
    minDepartureTime: defaults.minDepartureTime.toISOString(),
    skipNightSendings: defaults.skipNightSendings,
    maxNobleDistance: defaults.maxNobleDistance,
    actionDate: defaults.actionDate,
    offSettings: defaults.offSettings,
    catasSettings: defaults.catasSettings,
    fakeOffSettings: defaults.fakeOffSettings,
    fakeDeffSettings: defaults.fakeDeffSettings,
    nobleSettings: defaults.nobleSettings,
    playerNobleBudgets: defaults.playerNobleBudgets,
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
          await mainActionSettingsService.getMainActionSettings(scheduleId);
        setSettings({
          minDepartureTime: data.minDepartureTime,
          skipNightSendings: data.skipNightSendings,
          maxNobleDistance: data.maxNobleDistance,
          actionDate: data.actionDate,
          offSettings: data.offSettings,
          catasSettings: data.catasSettings,
          fakeOffSettings: data.fakeOffSettings,
          fakeDeffSettings: data.fakeDeffSettings,
          nobleSettings: data.nobleSettings,
          playerNobleBudgets: data.playerNobleBudgets,
        });
      } catch (err: unknown) {
        // Jeśli ustawienia nie istnieją, resetuj do domyślnych
        if (
          (err as { response?: { status?: number } })?.response?.status === 404
        ) {
          const defaults = getDefaultMainActionSettings();
          setSettings({
            minDepartureTime: defaults.minDepartureTime.toISOString(),
            skipNightSendings: defaults.skipNightSendings,
            maxNobleDistance: defaults.maxNobleDistance,
            actionDate: defaults.actionDate,
            offSettings: defaults.offSettings,
            catasSettings: defaults.catasSettings,
            fakeOffSettings: defaults.fakeOffSettings,
            fakeDeffSettings: defaults.fakeDeffSettings,
            nobleSettings: defaults.nobleSettings,
            playerNobleBudgets: defaults.playerNobleBudgets,
          });
        } else {
          setError("Nie udało się wczytać ustawień głównej akcji");
          console.error("Error loading main action settings:", err);
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

      await mainActionSettingsService.saveMainActionSettings(
        scheduleId,
        settings,
      );

      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err) {
      setError("Wystąpił błąd podczas zapisywania ustawień");
      console.error("Error saving main action settings:", err);
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

  const handleDateTimeChange = (value: string) => {
    const newDate = new Date(value);
    setSettings((prev) => ({
      ...prev,
      minDepartureTime: newDate.toISOString(),
    }));
  };

  const getMinDateTime = (): string => {
    return formatDateTimeLocal(new Date().toISOString());
  };

  const getMaxDepartureDateTime = (): string => {
    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 7);
    return formatDateTimeLocal(maxDate.toISOString());
  };

  if (isLoading) {
    return (
      <div className={styles.settingsSection}>
        <h4>Ustawienia głównej akcji</h4>
        <p className={styles.loadingText}>Ładowanie ustawień...</p>
      </div>
    );
  }

  return (
    <div className={styles.settingsSection}>
      <h4>Ustawienia głównej akcji</h4>

      {error && <div className={styles.error}>{error}</div>}
      {saveSuccess && (
        <div className={styles.success}>Ustawienia zapisane pomyślnie!</div>
      )}

      {/* 1. Globalne ustawienia */}
      <details open>
        <summary
          style={{
            cursor: "pointer",
            fontWeight: "bold",
            fontSize: "1.1rem",
            marginBottom: "1rem",
            color: "#2c5aa0",
          }}
        >
          ⚙️ Globalne ustawienia
        </summary>

        <div className={styles.formGroup}>
          <label htmlFor="actionDate">Dzień wejścia akcji:</label>
          <input
            id="actionDate"
            type="date"
            value={settings.actionDate}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                actionDate: e.target.value,
              }))
            }
            min={new Date().toISOString().slice(0, 10)}
            disabled={isSaving}
          />
        </div>

        <div className={styles.formGroup}>
          <label htmlFor="minDepartureTime">Czas rozpoczęcia wysyłki:</label>
          <input
            id="minDepartureTime"
            type="datetime-local"
            value={formatDateTimeLocal(settings.minDepartureTime)}
            onChange={(e) => handleDateTimeChange(e.target.value)}
            min={getMinDateTime()}
            max={getMaxDepartureDateTime()}
            disabled={isSaving}
          />
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
            Pomiń nocne wysyłki
          </label>
        </div>
      </details>

      {/* 2. Ustawienia Szlachty */}
      <details open>
        <summary
          style={{
            cursor: "pointer",
            fontWeight: "bold",
            fontSize: "1.1rem",
            marginTop: "1.5rem",
            marginBottom: "1rem",
            color: "#8b6914",
          }}
        >
          👑 Ustawienia szlachty
        </summary>

        <div className={styles.formGroup}>
          <label htmlFor="maxNobleDistance">
            Maksymalny dystans szlachcica: {settings.maxNobleDistance}
          </label>
          <input
            id="maxNobleDistance"
            type="range"
            min="10"
            max="99"
            value={settings.maxNobleDistance}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                maxNobleDistance: parseInt(e.target.value),
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>10</span>
            <span>99</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label htmlFor="nobleMinDistanceFromFront">
            Minimalny dystans od frontu:{" "}
            {settings.nobleSettings.minDistanceFromFront}
          </label>
          <input
            id="nobleMinDistanceFromFront"
            type="range"
            min="0"
            max="30"
            value={settings.nobleSettings.minDistanceFromFront}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                nobleSettings: {
                  ...prev.nobleSettings,
                  minDistanceFromFront: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>0</span>
            <span>30</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label
            htmlFor="nobleMinOffUnitsForOffNoble"
            title="Szlachcic z pełnym offem (pełna armia)"
          >
            Min. populacja offensywna dla offoszlachty:{" "}
            {settings.nobleSettings.minOffUnitsForOffNoble}
          </label>
          <input
            id="nobleMinOffUnitsForOffNoble"
            type="range"
            min="1000"
            max="21000"
            step="100"
            value={settings.nobleSettings.minOffUnitsForOffNoble}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                nobleSettings: {
                  ...prev.nobleSettings,
                  minOffUnitsForOffNoble: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>1,000</span>
            <span>21,000</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label
            htmlFor="nobleMinOffUnitsForFakeOffNoble"
            title="Szlachcic z 150 toporami (lekki atak)"
          >
            Min. populacja offensywna dla fejk offoszlachty:{" "}
            {settings.nobleSettings.minOffUnitsForFakeOffNoble}
          </label>
          <input
            id="nobleMinOffUnitsForFakeOffNoble"
            type="range"
            min="1000"
            max="21000"
            step="100"
            value={settings.nobleSettings.minOffUnitsForFakeOffNoble}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                nobleSettings: {
                  ...prev.nobleSettings,
                  minOffUnitsForFakeOffNoble: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>1,000</span>
            <span>21,000</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label
            htmlFor="nobleMaxOffUnitsForDefNoble"
            title="Szlachcic z ciężką kawalerią (z wioski deffowej)"
          >
            Max. populacja offensywna dla deffoszlachty:{" "}
            {settings.nobleSettings.maxOffUnitsForDefNoble}
          </label>
          <input
            id="nobleMaxOffUnitsForDefNoble"
            type="range"
            min="1000"
            max="21000"
            step="100"
            value={settings.nobleSettings.maxOffUnitsForDefNoble}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                nobleSettings: {
                  ...prev.nobleSettings,
                  maxOffUnitsForDefNoble: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>1,000</span>
            <span>21,000</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label
            htmlFor="nobleMinDeffUnitsForDefNoble"
            title="Wymagane jednostki deffowe dla szlachcica deffowego"
          >
            Min. populacja defensywna dla deffoszlachty:{" "}
            {settings.nobleSettings.minDeffUnitsForDefNoble}
          </label>
          <input
            id="nobleMinDeffUnitsForDefNoble"
            type="range"
            min="1000"
            max="21000"
            step="100"
            value={settings.nobleSettings.minDeffUnitsForDefNoble}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                nobleSettings: {
                  ...prev.nobleSettings,
                  minDeffUnitsForDefNoble: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>1,000</span>
            <span>21,000</span>
          </div>
        </div>
      </details>

      {/* 3. Ustawienia Ataków Offowych */}
      <details open>
        <summary
          style={{
            cursor: "pointer",
            fontWeight: "bold",
            fontSize: "1.1rem",
            marginTop: "1.5rem",
            marginBottom: "1rem",
            color: "#c41e1e",
          }}
        >
          ⚔️ Ustawienia ataków offowych
        </summary>

        <div className={styles.formGroup}>
          <label htmlFor="offMinOffUnits">
            Minimalna siła offu: {settings.offSettings.minOffUnits}
          </label>
          <input
            id="offMinOffUnits"
            type="range"
            min="1000"
            max="21000"
            step="100"
            value={settings.offSettings.minOffUnits}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                offSettings: {
                  ...prev.offSettings,
                  minOffUnits: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>1,000</span>
            <span>21,000</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label htmlFor="offMinDistanceFromFront">
            Minimalny dystans od frontu:{" "}
            {settings.offSettings.minDistanceFromFront}
          </label>
          <input
            id="offMinDistanceFromFront"
            type="range"
            min="0"
            max="100"
            value={settings.offSettings.minDistanceFromFront}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                offSettings: {
                  ...prev.offSettings,
                  minDistanceFromFront: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>0</span>
            <span>100</span>
          </div>
        </div>
      </details>

      {/* 4. Ustawienia Burzenia */}
      <details open>
        <summary
          style={{
            cursor: "pointer",
            fontWeight: "bold",
            fontSize: "1.1rem",
            marginTop: "1.5rem",
            marginBottom: "1rem",
            color: "#7d3c98",
          }}
        >
          🏰 Ustawienia burzenia (katapulty)
        </summary>

        <div className={styles.formGroup}>
          <label htmlFor="catasMinCatasNumber">
            Minimalna liczba katapult: {settings.catasSettings.minCatasNumber}
          </label>
          <input
            id="catasMinCatasNumber"
            type="range"
            min="10"
            max="2500"
            step="10"
            value={settings.catasSettings.minCatasNumber}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                catasSettings: {
                  ...prev.catasSettings,
                  minCatasNumber: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>10</span>
            <span>2,500</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label htmlFor="catasMaxOffUnits">
            Maksymalna siła offu: {settings.catasSettings.maxOffUnits}
          </label>
          <input
            id="catasMaxOffUnits"
            type="range"
            min="0"
            max="25000"
            step="100"
            value={settings.catasSettings.maxOffUnits}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                catasSettings: {
                  ...prev.catasSettings,
                  maxOffUnits: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>0</span>
            <span>25,000</span>
          </div>
        </div>

        <div className={styles.formGroup}>
          <label htmlFor="catasMinDistanceFromFront">
            Minimalny dystans od frontu:{" "}
            {settings.catasSettings.minDistanceFromFront}
          </label>
          <input
            id="catasMinDistanceFromFront"
            type="range"
            min="0"
            max="100"
            value={settings.catasSettings.minDistanceFromFront}
            onChange={(e) =>
              setSettings((prev) => ({
                ...prev,
                catasSettings: {
                  ...prev.catasSettings,
                  minDistanceFromFront: parseInt(e.target.value),
                },
              }))
            }
            disabled={isSaving}
            className={styles.slider}
          />
          <div className={styles.sliderLabels}>
            <span>0</span>
            <span>100</span>
          </div>
        </div>
      </details>

      {/* 5. Ustawienia Fejków */}
      <details open>
        <summary
          style={{
            cursor: "pointer",
            fontWeight: "bold",
            fontSize: "1.1rem",
            marginTop: "1.5rem",
            marginBottom: "1rem",
            color: "#148f77",
          }}
        >
          🎭 Ustawienia fejków
        </summary>

        <div style={{ marginBottom: "1.5rem" }}>
          <h5 style={{ color: "#666", marginBottom: "0.5rem" }}>
            Fejki offowe (ataki fejkowe z offówek)
          </h5>

          <div className={styles.formGroup}>
            <label htmlFor="fakeOffMinOffUnits">
              Minimalna siła offu: {settings.fakeOffSettings.minOffUnits}
            </label>
            <input
              id="fakeOffMinOffUnits"
              type="range"
              min="1000"
              max="21000"
              step="100"
              value={settings.fakeOffSettings.minOffUnits}
              onChange={(e) =>
                setSettings((prev) => ({
                  ...prev,
                  fakeOffSettings: {
                    ...prev.fakeOffSettings,
                    minOffUnits: parseInt(e.target.value),
                  },
                }))
              }
              disabled={isSaving}
              className={styles.slider}
            />
            <div className={styles.sliderLabels}>
              <span>1,000</span>
              <span>21,000</span>
            </div>
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="fakeOffMinDistanceFromFront">
              Minimalny dystans od frontu:{" "}
              {settings.fakeOffSettings.minDistanceFromFront}
            </label>
            <input
              id="fakeOffMinDistanceFromFront"
              type="range"
              min="0"
              max="100"
              value={settings.fakeOffSettings.minDistanceFromFront}
              onChange={(e) =>
                setSettings((prev) => ({
                  ...prev,
                  fakeOffSettings: {
                    ...prev.fakeOffSettings,
                    minDistanceFromFront: parseInt(e.target.value),
                  },
                }))
              }
              disabled={isSaving}
              className={styles.slider}
            />
            <div className={styles.sliderLabels}>
              <span>0</span>
              <span>100</span>
            </div>
          </div>
        </div>

        <div>
          <h5 style={{ color: "#666", marginBottom: "0.5rem" }}>
            Fejki deffowe (ataki fejkowe z wioski deffowej)
          </h5>

          <div className={styles.formGroup}>
            <label htmlFor="fakeDeffMaxOffUnits">
              Maksymalna siła offu: {settings.fakeDeffSettings.maxOffUnits}
            </label>
            <input
              id="fakeDeffMaxOffUnits"
              type="range"
              min="0"
              max="21000"
              step="100"
              value={settings.fakeDeffSettings.maxOffUnits}
              onChange={(e) =>
                setSettings((prev) => ({
                  ...prev,
                  fakeDeffSettings: {
                    ...prev.fakeDeffSettings,
                    maxOffUnits: parseInt(e.target.value),
                  },
                }))
              }
              disabled={isSaving}
              className={styles.slider}
            />
            <div className={styles.sliderLabels}>
              <span>0</span>
              <span>21,000</span>
            </div>
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="fakeDeffMinDistanceFromFront">
              Minimalny dystans od frontu:{" "}
              {settings.fakeDeffSettings.minDistanceFromFront}
            </label>
            <input
              id="fakeDeffMinDistanceFromFront"
              type="range"
              min="0"
              max="100"
              value={settings.fakeDeffSettings.minDistanceFromFront}
              onChange={(e) =>
                setSettings((prev) => ({
                  ...prev,
                  fakeDeffSettings: {
                    ...prev.fakeDeffSettings,
                    minDistanceFromFront: parseInt(e.target.value),
                  },
                }))
              }
              disabled={isSaving}
              className={styles.slider}
            />
            <div className={styles.sliderLabels}>
              <span>0</span>
              <span>100</span>
            </div>
          </div>
        </div>
      </details>

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
