import { useState, useEffect } from "react";
import { useAuth } from "../hooks/useAuth";
import type {
  Schedule,
  CreateScheduleRequest,
  UpdateScheduleRequest,
} from "../types/schedule";
import { scheduleService } from "../services/scheduleService";
import { ScheduleList } from "../components/ScheduleList";
import { ScheduleForm } from "../components/ScheduleForm";
import styles from "./HomePage.module.css";

const HomePage = () => {
  const {
    user,
    isLoading: authLoading,
    login,
    logout,
    isAuthenticated,
  } = useAuth();
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [isLoadingSchedules, setIsLoadingSchedules] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<Schedule | undefined>(
    undefined,
  );

  useEffect(() => {
    if (user?.id) {
      loadSchedules();
    }
  }, [user]);

  const loadSchedules = async () => {
    if (!user?.id) return;

    setIsLoadingSchedules(true);
    setError(null);
    try {
      const data = await scheduleService.getSchedulesByUser(user.id);
      setSchedules(data);
    } catch (err) {
      console.error("Error loading schedules:", err);
      setError("Nie udało się załadować rozpisek");
    } finally {
      setIsLoadingSchedules(false);
    }
  };

  const handleCreateSchedule = async (request: CreateScheduleRequest) => {
    try {
      const newSchedule = await scheduleService.createSchedule(request);
      setSchedules((prev) => [...prev, newSchedule]);
      setShowForm(false);
    } catch (err) {
      console.error("Error creating schedule:", err);
      throw err;
    }
  };

  const handleUpdateSchedule = async (request: UpdateScheduleRequest) => {
    if (!editingSchedule) return;

    try {
      const updatedSchedule = await scheduleService.updateSchedule(
        editingSchedule.id,
        request,
      );
      setSchedules((prev) =>
        prev.map((s) => (s.id === updatedSchedule.id ? updatedSchedule : s)),
      );
      setEditingSchedule(undefined);
      setShowForm(false);
    } catch (err) {
      console.error("Error updating schedule:", err);
      throw err;
    }
  };

  const handleDeleteSchedule = async (scheduleId: string) => {
    await scheduleService.deleteSchedule(scheduleId);
    setSchedules((prev) => prev.filter((s) => s.id !== scheduleId));
  };

  const handleEdit = (schedule: Schedule) => {
    setEditingSchedule(schedule);
    setShowForm(true);
  };

  const handleCancelForm = () => {
    setShowForm(false);
    setEditingSchedule(undefined);
  };

  const handleNewSchedule = () => {
    setEditingSchedule(undefined);
    setShowForm(true);
  };

  console.log("HomePage render:", { authLoading, isAuthenticated, user });

  if (authLoading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>Ładowanie...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className={styles.container}>
        <div className={styles.loginCard}>
          <h1>TWAction</h1>
          <p>Zarządzaj swoimi rozpiskami</p>
          <button onClick={login} className={styles.loginBtn}>
            Zaloguj się przez Google
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.userInfo}>
          <h1>Moje Rozpiski</h1>
          <p>Zalogowany jako: {user?.email}</p>
        </div>
        <button onClick={logout} className={styles.logoutBtn}>
          Wyloguj się
        </button>
      </header>

      {error && <div className={styles.error}>{error}</div>}

      {showForm ? (
        <ScheduleForm
          userId={user!.id}
          schedule={editingSchedule}
          onSubmit={
            editingSchedule ? handleUpdateSchedule : handleCreateSchedule
          }
          onCancel={handleCancelForm}
        />
      ) : (
        <>
          <div className={styles.actions}>
            <button onClick={handleNewSchedule} className={styles.newBtn}>
              + Nowa rozpiska
            </button>
          </div>

          {isLoadingSchedules ? (
            <div className={styles.loading}>Ładowanie rozpisek...</div>
          ) : (
            <ScheduleList
              schedules={schedules}
              onEdit={handleEdit}
              onDelete={handleDeleteSchedule}
            />
          )}
        </>
      )}
    </div>
  );
};

export default HomePage;
