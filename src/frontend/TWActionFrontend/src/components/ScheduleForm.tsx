import { useState, useEffect } from "react";
import { WorldType, ScheduleType } from "../types/schedule";
import type {
  Schedule,
  CreateScheduleRequest,
  UpdateScheduleRequest,
} from "../types/schedule";
import { TribeSelector } from "./TribeSelector";
import type { EnemyTribeSnapshot } from "../types/tribe";
import styles from "./ScheduleForm.module.css";

interface ScheduleFormProps {
  userId: string;
  schedule?: Schedule;
  onSubmit: (
    request: CreateScheduleRequest | UpdateScheduleRequest,
  ) => Promise<void>;
  onCancel: () => void;
}

export const ScheduleForm = ({
  userId,
  schedule,
  onSubmit,
  onCancel,
}: ScheduleFormProps) => {
  const [name, setName] = useState("");
  const [world, setWorld] = useState<WorldType>(WorldType.pl218);
  const [scheduleType, setScheduleType] = useState<ScheduleType>(
    ScheduleType.Main,
  );
  const [enemies, setEnemies] = useState<EnemyTribeSnapshot[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tribesLoaded, setTribesLoaded] = useState(false);
  const [previousWorld, setPreviousWorld] = useState<WorldType | null>(null);

  useEffect(() => {
    if (schedule) {
      setName(schedule.name);
      setWorld(schedule.world);
      setPreviousWorld(schedule.world);
      setScheduleType(schedule.scheduleType);
      setTribesLoaded(false);
      // enemyIds będą przekonwertowane na pełne obiekty po załadowaniu plemion
    } else {
      setName("");
      setWorld(WorldType.pl218);
      setPreviousWorld(WorldType.pl218);
      setScheduleType(ScheduleType.Main);
      setEnemies([]);
      setTribesLoaded(false);
    }
  }, [schedule]);

  // Wyczyść listę wrogich plemion gdy użytkownik zmieni świat
  useEffect(() => {
    if (previousWorld !== null && previousWorld !== world) {
      setEnemies([]);
      setTribesLoaded(false);
    }
    setPreviousWorld(world);
  }, [world]);

  const handleTribesLoaded = (loadedTribes: EnemyTribeSnapshot[]) => {
    if (!tribesLoaded && schedule?.enemyIds && schedule.enemyIds.length > 0) {
      // Konwertuj enemyIds na pełne obiekty plemion
      const selectedTribes = loadedTribes.filter((tribe) =>
        schedule.enemyIds!.includes(tribe.tribalWarsId),
      );
      setEnemies(selectedTribes);
      setTribesLoaded(true);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      setError("Nazwa rozpiski jest wymagana");
      return;
    }

    if (enemies.length > 10) {
      setError("Możesz wybrać maksymalnie 10 wrogich plemion");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const enemyTribalWarsIds = enemies.map((e) => e.tribalWarsId);

      const requestData = schedule
        ? { name, world, scheduleType, enemyTribalWarsIds }
        : { userId, name, world, scheduleType, enemyTribalWarsIds };

      if (schedule) {
        await onSubmit(requestData as UpdateScheduleRequest);
      } else {
        await onSubmit(requestData as CreateScheduleRequest);
      }
    } catch (err) {
      setError("Wystąpił błąd podczas zapisywania rozpiski");
      console.error("Error submitting schedule:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className={styles.form}>
      <h3>{schedule ? "Edytuj rozpiskę" : "Nowa rozpiska"}</h3>

      {error && <div className={styles.error}>{error}</div>}

      <div className={styles.formGroup}>
        <label htmlFor="name">Nazwa:</label>
        <input
          id="name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Wprowadź nazwę rozpiski"
          disabled={isSubmitting}
          required
        />
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="world">Świat:</label>
        <select
          id="world"
          value={world}
          onChange={(e) => setWorld(e.target.value as WorldType)}
          disabled={isSubmitting}
        >
          {(Object.keys(WorldType) as Array<keyof typeof WorldType>).map(
            (key) => (
              <option key={WorldType[key]} value={WorldType[key]}>
                {WorldType[key]}
              </option>
            ),
          )}
        </select>
      </div>

      <div className={styles.formGroup}>
        <label htmlFor="scheduleType">Typ:</label>
        <select
          id="scheduleType"
          value={scheduleType}
          onChange={(e) => setScheduleType(e.target.value as ScheduleType)}
          disabled={isSubmitting}
        >
          {(Object.keys(ScheduleType) as Array<keyof typeof ScheduleType>).map(
            (key) => (
              <option key={ScheduleType[key]} value={ScheduleType[key]}>
                {ScheduleType[key]}
              </option>
            ),
          )}
        </select>
      </div>

      <TribeSelector
        key={schedule?.id || "new"}
        world={world}
        selectedTribes={enemies}
        onSelectionChange={setEnemies}
        onTribesLoaded={handleTribesLoaded}
        maxTribes={10}
      />

      <div className={styles.actions}>
        <button
          type="submit"
          disabled={isSubmitting}
          className={styles.submitBtn}
        >
          {isSubmitting ? "Zapisywanie..." : schedule ? "Zapisz" : "Utwórz"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          className={styles.cancelBtn}
        >
          Anuluj
        </button>
      </div>
    </form>
  );
};
