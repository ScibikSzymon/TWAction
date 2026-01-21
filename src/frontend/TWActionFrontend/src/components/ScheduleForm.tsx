import { useState } from "react";
import { WorldType, ScheduleType } from "../types/schedule";
import type {
  Schedule,
  CreateScheduleRequest,
  UpdateScheduleRequest,
} from "../types/schedule";
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
  const [name, setName] = useState(schedule?.name || "");
  const [world, setWorld] = useState<WorldType>(
    schedule?.world || WorldType.pl218,
  );
  const [scheduleType, setScheduleType] = useState<ScheduleType>(
    schedule?.scheduleType || ScheduleType.Main,
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      setError("Nazwa rozpiski jest wymagana");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const requestData = schedule
        ? { name, world, scheduleType }
        : { userId, name, world, scheduleType };

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
