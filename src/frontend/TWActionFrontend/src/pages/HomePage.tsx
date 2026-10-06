import { useState, useEffect, useCallback } from "react";
import { useAuth } from "../hooks/useAuth";
import { useActiveSchedule } from "../hooks/useActiveSchedule";
import type {
  Schedule,
  CreateScheduleRequest,
  UpdateScheduleRequest,
} from "../types/schedule";
import { scheduleService } from "../services/scheduleService";
import { userService } from "../services/userService";
import type { UserLimits } from "../types/user";
import { ScheduleList } from "../components/ScheduleList";
import { ScheduleForm } from "../components/ScheduleForm";
import { ScheduleTabs } from "../components/ScheduleTabs";
import styles from "./HomePage.module.css";

const HomePage = () => {
  const { user } = useAuth();
  const { activeScheduleId, setActive, clearActive } = useActiveSchedule();
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [limits, setLimits] = useState<UserLimits | null>(null);
  const [isLoadingSchedules, setIsLoadingSchedules] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<Schedule | undefined>(
    undefined,
  );
  const [searchQuery, setSearchQuery] = useState("");

  const loadSchedules = useCallback(async () => {
    if (!user?.id) return;

    setIsLoadingSchedules(true);
    setError(null);
    try {
      const data = await scheduleService.getSchedules();
      setSchedules(data);
      setLimits(await userService.getMyLimits());
    } catch (err) {
      console.error("Error loading schedules:", err);
      setError("Nie udało się załadować rozpisek");
    } finally {
      setIsLoadingSchedules(false);
    }
  }, [user?.id]);

  useEffect(() => {
    if (user?.id) {
      loadSchedules();
    }
  }, [user?.id, loadSchedules]);

  const handleCreateSchedule = async (request: CreateScheduleRequest) => {
    try {
      const newSchedule = await scheduleService.createSchedule(request);
      setSchedules((prev) => [...prev, newSchedule]);
      setLimits((prev) => prev ? { ...prev, scheduleCount: prev.scheduleCount + 1 } : prev);
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

  const handleSubmitSchedule = async (
    request: CreateScheduleRequest | UpdateScheduleRequest,
  ) => {
    if (editingSchedule) {
      await handleUpdateSchedule(request as UpdateScheduleRequest);
    } else {
      await handleCreateSchedule(request as CreateScheduleRequest);
    }
  };

  const handleDeleteSchedule = async (scheduleId: string) => {
    await scheduleService.deleteSchedule(scheduleId);
    setSchedules((prev) => prev.filter((s) => s.id !== scheduleId));
    setLimits((prev) => prev ? { ...prev, scheduleCount: Math.max(0, prev.scheduleCount - 1) } : prev);
    if (activeScheduleId === scheduleId) {
      clearActive();
    }
  };

  const handleEdit = async (schedule: Schedule) => {
    try {
      // Pobierz najnowsze dane rozpiski z bazy
      const freshSchedule = await scheduleService.getScheduleById(schedule.id);
      setEditingSchedule(freshSchedule);
      setShowForm(true);
    } catch (err) {
      console.error("Error loading schedule for edit:", err);
      setError("Nie udało się załadować rozpiski do edycji");
    }
  };

  const handleCancelForm = () => {
    setShowForm(false);
    setEditingSchedule(undefined);
  };

  const handleScheduleUpdate = (
    scheduleId: string,
    updates: Partial<Schedule>,
  ) => {
    setSchedules((prev) =>
      prev.map((s) => (s.id === scheduleId ? { ...s, ...updates } : s)),
    );
  };

  const handleNewSchedule = () => {
    setEditingSchedule(undefined);
    setShowForm(true);
  };

  const filteredSchedules = schedules.filter((schedule) =>
    schedule.name.toLowerCase().includes(searchQuery.trim().toLowerCase()),
  );

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.userInfo}>
          <h1>Moje Rozpiski</h1>
          {limits && <p className={styles.limitInfo}>
            Plan: {limits.subscriptionTier === "Premium" ? "Premium" : "Darmowy"} ·
            Rozpiski: {limits.scheduleCount}/{limits.scheduleLimit ?? "bez limitu"} ·
            Wgrania wojsk: {limits.troopsUploadLimit ?? "bez limitu"} na rozpiskę / {limits.troopsUploadWindowHours} h
          </p>}
        </div>
      </header>

      {error && <div className={styles.error}>{error}</div>}

      {showForm ? (
        <ScheduleForm
          schedule={editingSchedule}
          onSubmit={handleSubmitSchedule}
          onCancel={handleCancelForm}
        />
      ) : isLoadingSchedules ? (
        <div className={styles.loading}>Ładowanie rozpisek...</div>
      ) : (
        <div className={styles.body}>
          <div className={styles.listColumn}>
            <div className={styles.actions}>
              <button onClick={handleNewSchedule} className={styles.newBtn}
                disabled={limits?.scheduleLimit != null && limits.scheduleCount >= limits.scheduleLimit}
                title={limits?.scheduleLimit != null && limits.scheduleCount >= limits.scheduleLimit ? "Osiągnięto limit rozpisek" : undefined}>
                + Nowa
              </button>
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Szukaj rozpiski..."
                className={styles.searchInput}
              />
            </div>
            <div className={styles.listScroll}>
              <ScheduleList
                schedules={filteredSchedules}
                activeScheduleId={activeScheduleId}
                onEdit={handleEdit}
                onDelete={handleDeleteSchedule}
                onSetActive={setActive}
              />
            </div>
          </div>

          {activeScheduleId &&
            schedules.some((s) => s.id === activeScheduleId) && (
              <div className={styles.tabsColumn}>
                <ScheduleTabs
                  key={activeScheduleId}
                  schedule={schedules.find((s) => s.id === activeScheduleId)!}
                  onScheduleUpdate={(updates) =>
                    handleScheduleUpdate(activeScheduleId, updates)
                  }
                />
              </div>
            )}
        </div>
      )}
    </div>
  );
};

export default HomePage;
