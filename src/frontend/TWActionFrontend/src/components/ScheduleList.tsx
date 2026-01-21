import { useState } from "react";
import type { Schedule } from "../types/schedule";
import styles from "./ScheduleList.module.css";

interface ScheduleListProps {
  schedules: Schedule[];
  onEdit: (schedule: Schedule) => void;
  onDelete: (scheduleId: string) => Promise<void>;
}

export const ScheduleList = ({
  schedules,
  onEdit,
  onDelete,
}: ScheduleListProps) => {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (scheduleId: string) => {
    if (!window.confirm("Czy na pewno chcesz usunąć tę rozpiskę?")) {
      return;
    }

    setDeletingId(scheduleId);
    try {
      await onDelete(scheduleId);
    } catch (err) {
      console.error("Error deleting schedule:", err);
      alert("Wystąpił błąd podczas usuwania rozpiski");
    } finally {
      setDeletingId(null);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("pl-PL", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (schedules.length === 0) {
    return (
      <div className={styles.empty}>
        <p>Brak rozpisek. Utwórz swoją pierwszą rozpiskę!</p>
      </div>
    );
  }

  return (
    <div className={styles.list}>
      {schedules.map((schedule) => (
        <div key={schedule.id} className={styles.item}>
          <div className={styles.info}>
            <h3>{schedule.name}</h3>
            <div className={styles.details}>
              <span className={styles.badge}>{schedule.world}</span>
              <span className={styles.badge}>{schedule.scheduleType}</span>
            </div>
            <p className={styles.date}>
              Utworzono: {formatDate(schedule.creationDate)}
            </p>
          </div>
          <div className={styles.actions}>
            <button
              onClick={() => onEdit(schedule)}
              disabled={deletingId === schedule.id}
              className={styles.editBtn}
            >
              Edytuj
            </button>
            <button
              onClick={() => handleDelete(schedule.id)}
              disabled={deletingId === schedule.id}
              className={styles.deleteBtn}
            >
              {deletingId === schedule.id ? "Usuwanie..." : "Usuń"}
            </button>
          </div>
        </div>
      ))}
    </div>
  );
};
