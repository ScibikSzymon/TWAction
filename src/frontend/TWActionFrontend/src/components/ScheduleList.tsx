import { useState } from "react";
import type { Schedule } from "../types/schedule";
import styles from "./ScheduleList.module.css";

interface ScheduleListProps {
  schedules: Schedule[];
  activeScheduleId: string | null;
  onEdit: (schedule: Schedule) => void;
  onDelete: (scheduleId: string) => Promise<void>;
  onSetActive: (scheduleId: string) => void;
}

export const ScheduleList = ({
  schedules,
  activeScheduleId,
  onEdit,
  onDelete,
  onSetActive,
}: ScheduleListProps) => {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleDelete = async (schedule: Schedule) => {
    let confirmMessage = "Czy na pewno chcesz usunąć tę rozpiskę?";

    if (schedule.sentToPlemionaRozpiskiAt) {
      confirmMessage =
        "Ta rozpiska została wysłana na plemionarozpiski.pl.\n\n" +
        "Usunięcie jej spowoduje również usunięcie rozpiski na plemionarozpiski.pl.\n\n" +
        "Czy na pewno chcesz kontynuować?";
    }

    if (!window.confirm(confirmMessage)) {
      return;
    }

    setDeletingId(schedule.id);
    try {
      await onDelete(schedule.id);
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
      {schedules.map((schedule) => {
        const isActive = activeScheduleId === schedule.id;
        return (
          <div
            key={schedule.id}
            className={`${styles.item} ${isActive ? styles.active : ""}`}
          >
            <div className={styles.info}>
              <div className={styles.header}>
                <h3>{schedule.name}</h3>
                {isActive && (
                  <span className={styles.activeBadge}>Aktywna</span>
                )}
              </div>
              <div className={styles.details}>
                <span className={styles.badge}>{schedule.world}</span>
                <span className={styles.badge}>{schedule.scheduleType}</span>
              </div>
              <p className={styles.date}>
                Utworzono: {formatDate(schedule.creationDate)}
              </p>
            </div>
            <div className={styles.actions}>
              {!isActive && (
                <button
                  onClick={() => onSetActive(schedule.id)}
                  disabled={deletingId === schedule.id}
                  className={styles.activeBtn}
                >
                  Ustaw jako aktywną
                </button>
              )}
              <button
                onClick={() => onEdit(schedule)}
                disabled={deletingId === schedule.id}
                className={styles.editBtn}
              >
                Edytuj
              </button>
              <button
                onClick={() => handleDelete(schedule)}
                disabled={deletingId === schedule.id}
                className={styles.deleteBtn}
              >
                {deletingId === schedule.id ? "Usuwanie..." : "Usuń"}
              </button>
            </div>
          </div>
        );
      })}
    </div>
  );
};
