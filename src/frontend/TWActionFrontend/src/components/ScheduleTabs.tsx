import { useState } from "react";
import type { Schedule } from "../types/schedule";
import { ScheduleType } from "../types/schedule";
import { TroopsStateManager } from "./TroopsStateManager";
import { NobleBudgetManager } from "./NobleBudgetManager";
import { ReconnaissanceSettings } from "./ReconnaissanceSettings";
import { ReconnaissanceActionsGenerator } from "./ReconnaissanceActionsGenerator";
import { MainActionSettings } from "./MainActionSettings";
import { TargetGroupsManager } from "./TargetGroupsManager";
import styles from "./ScheduleTabs.module.css";

interface ScheduleTabsProps {
  schedule: Schedule;
  onScheduleUpdate?: (updatedSchedule: Partial<Schedule>) => void;
}

type TabType =
  | "troops"
  | "nobleBudget"
  | "mainActionSettings"
  | "targetGroups"
  | "reconnaissance"
  | "generate";

export const ScheduleTabs = ({
  schedule,
  onScheduleUpdate,
}: ScheduleTabsProps) => {
  const [activeTab, setActiveTab] = useState<TabType>("troops");

  const tabs: { id: TabType; label: string; visible: boolean }[] = [
    { id: "troops", label: "Stan Armii", visible: true },
    {
      id: "nobleBudget",
      label: "Limity Szlachciców",
      visible: schedule.scheduleType === ScheduleType.Main,
    },
    {
      id: "mainActionSettings",
      label: "Ustawienia Głównej Akcji",
      visible: schedule.scheduleType === ScheduleType.Main,
    },
    {
      id: "targetGroups",
      label: "Grupy Celi",
      visible: schedule.scheduleType === ScheduleType.Main,
    },
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
        {activeTab === "nobleBudget" &&
          schedule.scheduleType === ScheduleType.Main && (
            <NobleBudgetManager scheduleId={schedule.id} />
          )}
        {activeTab === "mainActionSettings" &&
          schedule.scheduleType === ScheduleType.Main && (
            <MainActionSettings scheduleId={schedule.id} />
          )}
        {activeTab === "targetGroups" &&
          schedule.scheduleType === ScheduleType.Main && (
            <TargetGroupsManager scheduleId={schedule.id} />
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
