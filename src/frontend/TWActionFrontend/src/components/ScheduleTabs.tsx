import { useState } from "react";
import type { Schedule } from "../types/schedule";
import { ScheduleType } from "../types/schedule";
import { TroopsStateManager } from "./TroopsStateManager";
import { ReconnaissanceSettings } from "./ReconnaissanceSettings";
import { ReconnaissanceActionsGenerator } from "./ReconnaissanceActionsGenerator";
import styles from "./ScheduleTabs.module.css";

interface ScheduleTabsProps {
  schedule: Schedule;
  onScheduleUpdate?: (updatedSchedule: Partial<Schedule>) => void;
}

type TabType = "troops" | "reconnaissance" | "generate";

export const ScheduleTabs = ({
  schedule,
  onScheduleUpdate,
}: ScheduleTabsProps) => {
  const [activeTab, setActiveTab] = useState<TabType>("troops");

  const tabs: { id: TabType; label: string; visible: boolean }[] = [
    { id: "troops", label: "Stan Armii", visible: true },
    {
      id: "reconnaissance",
      label: "Ustawienia Zwiadowcze",
      visible: schedule.scheduleType === ScheduleType.Reconnaissance,
    },
    {
      id: "generate",
      label: "Generuj Akcje",
      visible: schedule.scheduleType === ScheduleType.Reconnaissance,
    },
  ];

  const visibleTabs = tabs.filter((tab) => tab.visible);

  return (
    <div className={styles.tabsContainer}>
      <div className={styles.tabsHeader}>
        {visibleTabs.map((tab) => (
          <button
            key={tab.id}
            className={`${styles.tab} ${activeTab === tab.id ? styles.active : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className={styles.tabContent}>
        {activeTab === "troops" && (
          <TroopsStateManager scheduleId={schedule.id} />
        )}
        {activeTab === "reconnaissance" &&
          schedule.scheduleType === ScheduleType.Reconnaissance && (
            <ReconnaissanceSettings scheduleId={schedule.id} />
          )}
        {activeTab === "generate" &&
          schedule.scheduleType === ScheduleType.Reconnaissance && (
            <ReconnaissanceActionsGenerator
              scheduleId={schedule.id}
              schedule={schedule}
              onScheduleUpdate={onScheduleUpdate}
            />
          )}
      </div>
    </div>
  );
};
