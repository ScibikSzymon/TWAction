import { useState, useEffect } from "react";

const ACTIVE_SCHEDULE_KEY = "activeScheduleId";

export const useActiveSchedule = () => {
  const [activeScheduleId, setActiveScheduleId] = useState<string | null>(
    () => {
      return localStorage.getItem(ACTIVE_SCHEDULE_KEY);
    },
  );

  useEffect(() => {
    if (activeScheduleId) {
      localStorage.setItem(ACTIVE_SCHEDULE_KEY, activeScheduleId);
    } else {
      localStorage.removeItem(ACTIVE_SCHEDULE_KEY);
    }
  }, [activeScheduleId]);

  const setActive = (scheduleId: string) => {
    setActiveScheduleId(scheduleId);
  };

  const clearActive = () => {
    setActiveScheduleId(null);
  };

  const isActive = (scheduleId: string) => {
    return activeScheduleId === scheduleId;
  };

  return {
    activeScheduleId,
    setActive,
    clearActive,
    isActive,
  };
};
