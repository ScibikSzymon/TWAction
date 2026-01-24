import { useState, useEffect, useRef, useCallback } from "react";
import { troopsStateService } from "../services/troopsStateService";
import type { TroopsState } from "../types/troopsState";
import styles from "./TroopsStateManager.module.css";

interface TroopsStateManagerProps {
  scheduleId: string | null;
}

export const TroopsStateManager = ({ scheduleId }: TroopsStateManagerProps) => {
  const [troopsState, setTroopsState] = useState<TroopsState | null>(null);
  const [rawData, setRawData] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const successTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const loadTroopsState = useCallback(async () => {
    if (!scheduleId) return;

    setIsLoading(true);
    setError(null);
    try {
      const data = await troopsStateService.getTroopsState(scheduleId);
      setTroopsState(data);
    } catch (err: unknown) {
      // Not found is expected when there's no troops state yet
      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as { response?: { status: number } };
        if (axiosError.response?.status === 404) {
          setTroopsState(null);
        } else {
          console.error("Error loading troops state:", err);
          setError("Nie udało się załadować stanu wojsk");
        }
      } else {
        console.error("Error loading troops state:", err);
        setError("Nie udało się załadować stanu wojsk");
      }
    } finally {
      setIsLoading(false);
    }
  }, [scheduleId]);

  useEffect(() => {
    if (scheduleId) {
      loadTroopsState();
    } else {
      setTroopsState(null);
    }
  }, [scheduleId, loadTroopsState]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (successTimeoutRef.current !== null) {
        clearTimeout(successTimeoutRef.current);
      }
    };
  }, []);

  const handleUpload = async () => {
    if (!scheduleId || !rawData.trim()) {
      setError("Wprowadź dane stanu wojsk");
      return;
    }

    setIsUploading(true);
    setError(null);
    setSuccessMessage(null);
    try {
      const data = await troopsStateService.uploadTroopsState(scheduleId, {
        rawData: rawData.trim(),
      });
      setTroopsState(data);
      setRawData("");
      setSuccessMessage(
        `Stan wojsk został pomyślnie wgrany! Znaleziono ${data.villageCount} wiosek i ${data.playerCount} graczy.`,
      );
      // Clear any existing timeout before setting a new one
      if (successTimeoutRef.current !== null) {
        clearTimeout(successTimeoutRef.current);
      }
      successTimeoutRef.current = setTimeout(() => {
        setSuccessMessage(null);
        successTimeoutRef.current = null;
      }, 5000);
    } catch (err: unknown) {
      console.error("Error uploading troops state:", err);

      // Extract error message from backend response
      let errorMessage = "Nie udało się wgrać stanu wojsk";
      if (err && typeof err === "object" && "response" in err) {
        const axiosError = err as {
          response?: {
            status?: number;
            data?: { error?: string };
          };
        };
        
        // Handle 404 - schedule not found
        if (axiosError.response?.status === 404) {
          errorMessage = "Rozpiska nie istnieje. Została prawdopodobnie usunięta. Odśwież stronę, aby zaktualizować listę rozpisek.";
        } else if (axiosError.response?.data?.error) {
          // Handle other errors (like 400 validation errors)
          errorMessage = axiosError.response.data.error;
        }
      }
      setError(errorMessage);
    } finally {
      setIsUploading(false);
    }
  };

  if (!scheduleId) {
    return (
      <div className={styles.container}>
        <h3>Stan wojsk</h3>
        <p className={styles.noActiveSchedule}>
          Wybierz aktywną rozpiskę, aby zarządzać stanem wojsk
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className={styles.container}>
        <h3>Stan wojsk</h3>
        <div className={styles.loading}>Ładowanie stanu wojsk...</div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <h3>Stan wojsk</h3>

      {error && <div className={styles.error}>{error}</div>}
      {successMessage && <div className={styles.success}>{successMessage}</div>}

      {troopsState && (
        <div className={styles.info}>
          <p>
            <strong>Ostatnia aktualizacja:</strong>{" "}
            {new Date(troopsState.updatedAt).toLocaleString("pl-PL")}
          </p>
          <p>
            <strong>Liczba graczy:</strong> {troopsState.playerCount}
          </p>
          <p>
            <strong>Liczba wiosek:</strong> {troopsState.villageCount}
          </p>
          <p>
            <strong>Data utworzenia:</strong>{" "}
            {new Date(troopsState.createdAt).toLocaleString("pl-PL")}
          </p>
        </div>
      )}

      <div className={styles.uploadSection}>
        <h4>{troopsState ? "Aktualizuj stan wojsk" : "Wgraj stan wojsk"}</h4>
        <div className={styles.textareaWrapper}>
          <label htmlFor="rawData">
            Wklej dane stanu wojsk (CSV ze statystyk gry)
          </label>
          <textarea
            id="rawData"
            className={styles.textarea}
            value={rawData}
            onChange={(e) => setRawData(e.target.value)}
            placeholder={`Nazwa gracza,Wioska,Piki,Miecze,Zwiad,CK,Katasy,Topory,LK,Tarany,Grube
wwwwwwQ,492|577,140,140,345,0,45,5505,2194,298,4
Zennirox,505|571,0,54,50,0,0,0,0,0,0`}
            disabled={isUploading}
          />
          <p className={styles.hint}>
            Skopiuj i wklej dane bezpośrednio ze statystyk gry. Pierwsza linia
            powinna zawierać nagłówki kolumn.
          </p>
        </div>
        <button
          className={styles.uploadBtn}
          onClick={handleUpload}
          disabled={isUploading || !rawData.trim()}
        >
          {isUploading ? "Wgrywanie..." : troopsState ? "Aktualizuj" : "Wgraj"}
        </button>
      </div>
    </div>
  );
};
